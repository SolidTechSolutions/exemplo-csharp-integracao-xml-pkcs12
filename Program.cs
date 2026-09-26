/**
 * [EN]    XAdES (XML) signing example — PKCS#12 pre-imported certificate.
 *         Run: dotnet run
 *         Batch: POST http://localhost:5093/api/xml/sign-pkcs12
 *         Form:  POST http://localhost:5093/api/xml/sign/form
 *
 * [PT-BR] Exemplo de assinatura XAdES (XML) — certificado PKCS#12 pré-importado.
 *         Executar: dotnet run
 */
using SolidSign.Examples;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient<XmlPkcs12Service>();
var app = builder.Build();

// ── Batch endpoint ──────────────────────────────────────────────────────────
app.MapPost("/api/xml/sign-pkcs12", async (XmlPkcs12Service svc, IConfiguration cfg) =>
{
    var inputPath  = cfg["SolidSign:Batch:InputPath"] ?? "";
    var outputPath = cfg["SolidSign:Batch:OutputPath"] ?? "";
    var certId     = cfg["SolidSign:Cert:Id"] ?? "";

    if (!Directory.Exists(inputPath)) return Results.BadRequest(new { error = $"Invalid input path: {inputPath}" });

    var xmlFiles = Directory.GetFiles(inputPath, "*.xml");
    if (xmlFiles.Length == 0) return Results.Ok(new { message = $"No XML files found in {inputPath}" });

    Console.WriteLine($"Found {xmlFiles.Length} files for local processing.");
    var result = await svc.SignPkcs12Async(xmlFiles, certId, outputPath);

    return result is not null
        ? Results.Ok(new { message = $"Processing completed! ZIP generated at: {result}" })
        : Results.Problem("Processing failed. Check logs.");
});

// ── Form endpoint ───────────────────────────────────────────────────────────
app.MapPost("/api/xml/sign/form", async (HttpRequest req, XmlPkcs12Service svc) =>
{
    var form = await req.ReadFormAsync();
    var documents = form.Files.GetFiles("document");
    string? G(string k) => form.TryGetValue(k, out var v) ? v.ToString() : null;

    var zip = await svc.SignPkcs12FormAsync(
        authorization: G("authorization")!, baseUrl: G("baseUrl")!, pfxCode: G("pfxCode")!,
        documents: documents,
        profile: G("profile"), hashAlgorithm: G("hashAlgorithm"), policyVersion: G("policyVersion"),
        signaturePackaging: G("signaturePackaging"),
        signatureNodeName: G("signatureNodeName"), signatureNodeNamespace: G("signatureNodeNamespace"),
        canonicalizationMethod: G("canonicalizationMethod"),
        isRemoveXPathExclusionFilter: G("isRemoveXPathExclusionFilter"),
        isRemoveNamespacePrefixFromNodeNames: G("isRemoveNamespacePrefixFromNodeNames"),
        isSignKeyInfo: G("isSignKeyInfo"), signatureNodeId: G("signatureNodeId"));

    return zip is not null
        ? Results.File(zip, "application/zip", "signed_xml.zip")
        : Results.Problem("Processing failed. Check logs.");
});

app.Run("http://0.0.0.0:5093");
