# 🇧🇷 SolidSign API - Exemplo de Integração: Assinatura XAdES (XML) com PKCS#12 (certificado pré-importado) (C#)

## ⚠️ Disponibilidade

Este método (importação de certificado PKCS#12 direto no servidor) só está disponível em instâncias do SolidSign API rodando **on-premises** (localmente, na infraestrutura do próprio cliente). **Não está disponível na versão SaaS pública** do SolidSign.

Se você usa o SaaS público, use `sign-hsm-cloud` (com o seu próprio PSC) ou a custódia KMS SolidSign em vez deste método.

## Requisitos

- ASP.NET Minimal API
- Um token JWT válido (`POST /solidsign/auth/token`)
- Um certificado PKCS#12 já importado (`POST /solidsign/dsig/certificates/pkcs12/import`) — o `id` retornado é o `pfxCode`
- Front-end de referência (opcional): [`exemplo-react-xml-pkcs12`](https://github.com/SolidTechSolutions/exemplo-react-xml-pkcs12)

## Como rodar

```bash
dotnet run
```

O serviço sobe em `http://localhost:5093`.

## Como funciona

Este back-end expõe dois endpoints:

- `POST /api/xml/sign-pkcs12` — modo lote, assina todos os `.xml` de `SolidSign:Batch:InputPath`.
- `POST /api/xml/sign/form` — recebe um formulário (`multipart/form-data`, CORS liberado) e repassa os dados pra SolidSign API real, devolvendo o ZIP assinado.

## Variáveis do formulário

| Campo | Significado | Default |
|---|---|---|
| `document[i]` | Documento(s) XML a assinar | — |
| `authorization` | Token JWT (Bearer) | — |
| `baseUrl` | URL base da SolidSign API | `https://www.solidsign.com.br` |
| `pfxCode` | ID do certificado PKCS#12 importado | — |
| `profile` | Perfil de assinatura XAdES (PBAD/ETSI) | `ADRB` |
| `hashAlgorithm` | Algoritmo de hash | `SHA256` |
| `signaturePackaging` | Empacotamento da assinatura (ENVELOPED/ENVELOPING/DETACHED) | `ENVELOPED` |
| `signatureNodeName` | Nome local do nó XML a assinar | — |
| `signatureNodeNamespace` | Namespace do nó XML a assinar | — |
| `canonicalizationMethod` | Algoritmo de canonicalização (C14N) | `EXCLUSIVE` |
| `isRemoveXPathExclusionFilter` | Remove filtro XPath de exclusão | `false` |
| `isRemoveNamespacePrefixFromNodeNames` | Remove prefixo de namespace dos nomes de nó | `false` |
| `isSignKeyInfo` | Inclui elemento KeyInfo na assinatura | `false` |
| `signatureNodeId` | ID do nó XML alvo (opcional, múltiplos elementos elegíveis) | — |
| `policyVersion` | OID da política de assinatura (opcional) | (padrão da API) |

---

# 🇬🇧 SolidSign API - Integration Example: XAdES (XML) Signing with PKCS#12 (pre-imported certificate) (C#)

## ⚠️ Availability

This method (server-side PKCS#12 certificate import) is only available on **on-premises** SolidSign API instances (running locally, on the customer's own infrastructure). **It is not available on the public SaaS** version of SolidSign.

Why: PKCS#12 import keeps the decrypted private key cached on the server for up to 2 hours — an acceptable risk on your own on-premises instance, but not on a shared multi-tenant SaaS instance. If you use the public SaaS, use `sign-hsm-cloud` (your own PSC) or KMS SolidSign custody instead of this method.

## Requirements

- ASP.NET Minimal API
- A valid JWT token (`POST /solidsign/auth/token`)
- A PKCS#12 certificate already imported (`POST /solidsign/dsig/certificates/pkcs12/import`) — the returned `id` is the `pfxCode`
- Reference front-end (optional): [`exemplo-react-xml-pkcs12`](https://github.com/SolidTechSolutions/exemplo-react-xml-pkcs12)

## Running

```bash
dotnet run
```

The service starts on `http://localhost:5093`.

## How it works

This backend exposes two endpoints:

- `POST /api/xml/sign-pkcs12` — batch mode, signs every `.xml` file in `SolidSign:Batch:InputPath`.
- `POST /api/xml/sign/form` — accepts a form (`multipart/form-data`, CORS-enabled) and forwards the data to the real SolidSign API, returning the signed ZIP.

## Form fields

| Field | Meaning | Default |
|---|---|---|
| `document[i]` | XML document(s) to sign | — |
| `authorization` | JWT (Bearer) token | — |
| `baseUrl` | SolidSign API base URL | `https://www.solidsign.com.br` |
| `pfxCode` | ID of the imported PKCS#12 certificate | — |
| `profile` | XAdES signature profile (PBAD/ETSI) | `ADRB` |
| `hashAlgorithm` | Hash algorithm | `SHA256` |
| `signaturePackaging` | Signature packaging (ENVELOPED/ENVELOPING/DETACHED) | `ENVELOPED` |
| `signatureNodeName` | Local name of the XML node to sign | — |
| `signatureNodeNamespace` | Namespace of the XML node to sign | — |
| `canonicalizationMethod` | Canonicalization algorithm (C14N) | `EXCLUSIVE` |
| `isRemoveXPathExclusionFilter` | Removes the XPath exclusion filter | `false` |
| `isRemoveNamespacePrefixFromNodeNames` | Removes namespace prefix from node names | `false` |
| `isSignKeyInfo` | Includes the KeyInfo element in the signature | `false` |
| `signatureNodeId` | Target XML node ID (optional, multiple eligible elements) | — |
| `policyVersion` | Signature policy OID (optional) | (API default) |

---

# 🇪🇸 SolidSign API - Ejemplo de Integración: Firma XAdES (XML) con PKCS#12 (certificado pre-importado) (C#)

## ⚠️ Disponibilidad

Este método (importación de certificado PKCS#12 directo en el servidor) solo está disponible en instancias del SolidSign API que corren **on-premises** (localmente, en la infraestructura propia del cliente). **No está disponible en la versión SaaS pública** de SolidSign.

Si usa el SaaS público, use `sign-hsm-cloud` (con su propio PSC) o la custodia KMS SolidSign en lugar de este método.

## Requisitos

- ASP.NET Minimal API
- Un token JWT válido (`POST /solidsign/auth/token`)
- Un certificado PKCS#12 ya importado (`POST /solidsign/dsig/certificates/pkcs12/import`) — el `id` devuelto es el `pfxCode`
- Front-end de referencia (opcional): [`exemplo-react-xml-pkcs12`](https://github.com/SolidTechSolutions/exemplo-react-xml-pkcs12)

## Cómo ejecutar

```bash
dotnet run
```

El servicio arranca en `http://localhost:5093`.

## Cómo funciona

Este backend expone dos endpoints:

- `POST /api/xml/sign-pkcs12` — modo lote, firma todos los `.xml` de `SolidSign:Batch:InputPath`.
- `POST /api/xml/sign/form` — recibe un formulario (`multipart/form-data`, CORS habilitado) y reenvía los datos a la API SolidSign real, devolviendo el ZIP firmado.

## Campos del formulario

| Campo | Significado | Default |
|---|---|---|
| `document[i]` | Documento(s) XML a firmar | — |
| `authorization` | Token JWT (Bearer) | — |
| `baseUrl` | URL base de la API SolidSign | `https://www.solidsign.com.br` |
| `pfxCode` | ID del certificado PKCS#12 importado | — |
| `profile` | Perfil de firma XAdES (PBAD/ETSI) | `ADRB` |
| `hashAlgorithm` | Algoritmo de hash | `SHA256` |
| `signaturePackaging` | Empaquetado de la firma (ENVELOPED/ENVELOPING/DETACHED) | `ENVELOPED` |
| `signatureNodeName` | Nombre local del nodo XML a firmar | — |
| `signatureNodeNamespace` | Namespace del nodo XML a firmar | — |
| `canonicalizationMethod` | Algoritmo de canonicalización (C14N) | `EXCLUSIVE` |
| `isRemoveXPathExclusionFilter` | Elimina el filtro de exclusión XPath | `false` |
| `isRemoveNamespacePrefixFromNodeNames` | Elimina el prefijo de namespace de los nombres de nodo | `false` |
| `isSignKeyInfo` | Incluye el elemento KeyInfo en la firma | `false` |
| `signatureNodeId` | ID del nodo XML objetivo (opcional, múltiples elementos elegibles) | — |
| `policyVersion` | OID de la política de firma (opcional) | (default de la API) |
