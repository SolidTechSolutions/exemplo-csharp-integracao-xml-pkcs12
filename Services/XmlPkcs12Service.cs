using System.IO.Compression;
using System.Text.Json;

namespace SolidSign.Examples;

/// <summary>
/// [EN]    XAdES (XML) signing service — PKCS#12 certificate pre-imported into SolidSign cache.
///         Import once via POST /solidsign/dsig/certificates/pkcs12/import, then set SolidSign:Cert:Id.
/// [PT-BR] Serviço de assinatura XAdES (XML) — certificado PKCS#12 pré-importado na cache do SolidSign.
///         Importe uma vez via POST /solidsign/dsig/certificates/pkcs12/import, depois configure SolidSign:Cert:Id.
/// </summary>
public class XmlPkcs12Service(IConfiguration cfg, HttpClient http)
{
    private string BaseUrl        => cfg["SolidSign:Api:BaseUrl"]!.TrimEnd('/');
    private string Auth           => cfg["SolidSign:Api:Authorization"]!;
    private string Profile        => cfg["SolidSign:Sig:Profile"] ?? "ADRB";
    private string HashAlg        => cfg["SolidSign:Sig:HashAlgorithm"] ?? "SHA256";
    private string PolicyVer      => cfg["SolidSign:Sig:PolicyVersion"] ?? "";
    private string Packaging      => cfg["SolidSign:Sig:SignaturePackaging"] ?? "ENVELOPED";
    private string NodeName       => cfg["SolidSign:Sig:SignatureNodeName"] ?? "";
    private string NodeNamespace  => cfg["SolidSign:Sig:SignatureNodeNamespace"] ?? "";
    private string Canon          => cfg["SolidSign:Sig:CanonicalizationMethod"] ?? "EXCLUSIVE";
    private string RemoveXPath    => cfg["SolidSign:Sig:IsRemoveXPathExclusionFilter"] ?? "false";
    private string RemoveNsPrefix => cfg["SolidSign:Sig:IsRemoveNamespacePrefixFromNodeNames"] ?? "false";
    private string SignKeyInfo    => cfg["SolidSign:Sig:IsSignKeyInfo"] ?? "false";

    // ── Batch endpoint ────────────────────────────────────────────────────────

    public async Task<string?> SignPkcs12Async(IEnumerable<string> xmlPaths, string certId, string outputDir)
    {
        using var form = new MultipartFormDataContent();
        int i = 0;
        foreach (var fp in xmlPaths)
            form.Add(new StreamContent(File.OpenRead(fp)), $"document[{i++}]", Path.GetFileName(fp));

        form.Add(new StringContent(certId),          "pfxCode");
        form.Add(new StringContent(Profile),         "profile");
        form.Add(new StringContent(HashAlg),         "hashAlgorithm");
        form.Add(new StringContent(Packaging),       "signaturePackaging");
        form.Add(new StringContent(NodeName),        "signatureNodeName");
        form.Add(new StringContent(NodeNamespace),   "signatureNodeNamespace");
        form.Add(new StringContent(Canon),           "canonicalizationMethod");
        form.Add(new StringContent(RemoveXPath),     "isRemoveXPathExclusionFilter");
        form.Add(new StringContent(RemoveNsPrefix),  "isRemoveNamespacePrefixFromNodeNames");
        form.Add(new StringContent(SignKeyInfo),     "isSignKeyInfo");
        if (!string.IsNullOrWhiteSpace(PolicyVer)) form.Add(new StringContent(PolicyVer), "policyVersion");
        // Optional — uncomment to target a specific node by ID:
        // form.Add(new StringContent("NFe35..."), "signatureNodeId");

        var req = new HttpRequestMessage(HttpMethod.Post, $"{BaseUrl}/solidsign/dsig/xml/sign-pkcs12") { Content = form };
        req.Headers.TryAddWithoutValidation("Authorization", Auth);
        var resp = await http.SendAsync(req);
        if (!resp.IsSuccessStatusCode) { Console.Error.WriteLine($"SolidSign error {(int)resp.StatusCode}: {await resp.Content.ReadAsStringAsync()}"); return null; }

        var signResp = JsonSerializer.Deserialize<SignResponse>(await resp.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var zipBytes = await DownloadAndZipAsync(signResp!, xmlPaths.Select(Path.GetFileName!).ToList(), Auth);

        Directory.CreateDirectory(outputDir);
        var outPath = Path.Combine(outputDir, $"signed_xml_pkcs12_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}.zip");
        await File.WriteAllBytesAsync(outPath, zipBytes);
        Console.WriteLine($"XAdES PKCS12 signing complete. Output: {outPath}");
        return outPath;
    }

    // ── Form endpoint ─────────────────────────────────────────────────────────

    public async Task<byte[]?> SignPkcs12FormAsync(
        string authorization, string baseUrl, string pfxCode,
        IEnumerable<IFormFile> documents,
        string? profile, string? hashAlgorithm, string? policyVersion,
        string? signaturePackaging, string? signatureNodeName, string? signatureNodeNamespace,
        string? canonicalizationMethod, string? isRemoveXPathExclusionFilter,
        string? isRemoveNamespacePrefixFromNodeNames, string? isSignKeyInfo,
        string? signatureNodeId = null)
    {
        using var form = new MultipartFormDataContent();
        int i = 0;
        foreach (var d in documents)
        {
            var ms = new MemoryStream(); await d.CopyToAsync(ms); ms.Position = 0;
            form.Add(new StreamContent(ms), $"document[{i++}]", d.FileName);
        }
        form.Add(new StringContent(pfxCode), "pfxCode");
        void Add(string? v, string k) { if (!string.IsNullOrWhiteSpace(v)) form.Add(new StringContent(v), k); }
        Add(profile, "profile"); Add(hashAlgorithm, "hashAlgorithm"); Add(policyVersion, "policyVersion");
        Add(signaturePackaging, "signaturePackaging");
        Add(signatureNodeName, "signatureNodeName"); Add(signatureNodeNamespace, "signatureNodeNamespace");
        Add(canonicalizationMethod, "canonicalizationMethod");
        Add(isRemoveXPathExclusionFilter, "isRemoveXPathExclusionFilter");
        Add(isRemoveNamespacePrefixFromNodeNames, "isRemoveNamespacePrefixFromNodeNames");
        Add(isSignKeyInfo, "isSignKeyInfo");
        Add(signatureNodeId, "signatureNodeId");

        var req = new HttpRequestMessage(HttpMethod.Post, $"{baseUrl.TrimEnd('/')}/solidsign/dsig/xml/sign-pkcs12") { Content = form };
        req.Headers.TryAddWithoutValidation("Authorization", authorization);
        var resp = await http.SendAsync(req);
        if (!resp.IsSuccessStatusCode) { Console.Error.WriteLine($"SolidSign error {(int)resp.StatusCode}"); return null; }

        var signResp = JsonSerializer.Deserialize<SignResponse>(await resp.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        var origNames = documents.Select(d => d.FileName).ToList();
        return await DownloadAndZipAsync(signResp!, origNames, authorization);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<byte[]> DownloadAndZipAsync(SignResponse signResp, List<string?> originalNames, string auth)
    {
        using var ms = new MemoryStream();
        using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, leaveOpen: true))
        {
            for (int i = 0; i < signResp.Documents.Count; i++)
            {
                var href = signResp.Documents[i].HalLinks?.Self?.Href
                        ?? signResp.Documents[i].Links?.FirstOrDefault(l => l.Rel == "self")?.Href;
                if (href is null) continue;
                var dlReq = new HttpRequestMessage(HttpMethod.Get, href);
                dlReq.Headers.TryAddWithoutValidation("Authorization", auth);
                var dlResp = await http.SendAsync(dlReq);
                if (!dlResp.IsSuccessStatusCode) continue;
                var entry = archive.CreateEntry($"signed_{originalNames[i]}");
                await using var entryStream = entry.Open();
                await (await dlResp.Content.ReadAsStreamAsync()).CopyToAsync(entryStream);
            }
        }
        return ms.ToArray();
    }
}
