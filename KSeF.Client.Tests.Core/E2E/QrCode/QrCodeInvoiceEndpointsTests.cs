using KSeF.Client.Api.Services;
using KSeF.Client.Core.Interfaces.Services;
using KSeF.Client.Core.Models.QRCode;
using KSeF.Client.Extensions;
using System.Security.Cryptography.X509Certificates;
using Xunit.Abstractions;

namespace KSeF.Client.Tests.Core.E2E.QrCode;

[Collection("QrCodeScenario")]
public class QrCodeControllerMethodsE2ETests : TestBase
{
    private const string Base64PngPrefix = "iVBOR";

    private readonly IVerificationLinkService VerificationLinkService;
    private readonly ITestOutputHelper Output;

    private const string CertBase64UrlEncoded =
        "MIIDYTCCAuagAwIBAgIIAfIKXTUq5ZAwDAYIKoZIzj0EAwQFADBtMQswCQYDVQQGEwJQTDEfMB0GA1UECgwWTWluaXN0ZXJzdHdvIEZpbmFuc8OzdzEnMCUGA1UECwweS3Jham93YSBBZG1pbmlzdHJhY2phIFNrYXJib3dhMRQwEgYDVQQDDAtUSSBDQ0sgS1NlRjAeFw0yNTA3MjkwOTI4MzdaFw0yNzA3MjkwOTI4MzdaMEwxCzAJBgNVBAYTAlBMMRkwFwYDVQRhDBBWQVRQTC01MjY1ODc3NjM1MQ0wCwYDVQQKDARCZXRhMRMwEQYDVQQDDAo1MjY1ODc3NjM1MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEArf6b3J8GpbzWx90Xic7zQX4rrSkKgmNu2oaxqp8Y7F+NypawebUD3nyGl3uaaHMg5oR9dwlDxRM3y9QdCwq98HLtGNOoETCtlMRxkjJHQQXMS4Y+3b+L5ot23PBMFibx0syblJvE7i3UhbXoD5BLu9wWxkMHKli7rk+6IOCR64qmee3+PorQbcjJE/K4E5TV3tp7ZPjtreCbfo/M8obICSpjgLVyI8VvgAQqLTWCotzVlT13/QBtVylcGxTJeHXSKeYBzMoUe+YHjFdv/HP0SeGbngEDNZJ+IyxlPvNJsKvzfoPT6xNbKOD6YYKa/TzGqbvlMZADhjgUoDUJHT1J0wIDAQABo4HDMIHAMB8GA1UdIwQYMBaAFLLUPCZi2SvB4S1Svv2oHjU0v3ywMAwGA1UdEwEB/wQCMAAwDgYDVR0PAQH/BAQDAgeAMB0GA1UdDgQWBBScmYgyDr7ZQ8sYeVL5Bx6r5z3ltjA7BgNVHR8ENDAyMDCgLqAshipodHRwczovL3B1ZXNjLmdvdi5wbC9wa2kvY3JsL21ma3NlZnRpMS5jcmwwIwYIKwYBBQUHAQMEFzAVMBMGBgQAjkYBBjAJBgcEAI5GAQYCMAwGCCqGSM49BAMEBQADZwAwZAIwfH0op1zHItXmZIZNnb4bI6iCqvXe9YUiO9YX9zdjOQ0x6iaGoiN1ZPyYMVrWENenAjBe5/LegAjY71zzHz9Qdnw6GbfH+XdQlRNLeYEjqFXevGywZZ4j1p+13HgqLe3ut4E=";

    private const string PrivateKeyUrlEncoded =
        "MIIEowIBAAKCAQEArf6b3J8GpbzWx90Xic7zQX4rrSkKgmNu2oaxqp8Y7F+NypawebUD3nyGl3uaaHMg5oR9dwlDxRM3y9QdCwq98HLtGNOoETCtlMRxkjJHQQXMS4Y+3b+L5ot23PBMFibx0syblJvE7i3UhbXoD5BLu9wWxkMHKli7rk+6IOCR64qmee3+PorQbcjJE/K4E5TV3tp7ZPjtreCbfo/M8obICSpjgLVyI8VvgAQqLTWCotzVlT13/QBtVylcGxTJeHXSKeYBzMoUe+YHjFdv/HP0SeGbngEDNZJ+IyxlPvNJsKvzfoPT6xNbKOD6YYKa/TzGqbvlMZADhjgUoDUJHT1J0wIDAQABAoIBAA2I4sL0s+WsnOCLOEuGB7IuiGM98A1YgsUI+UUWfy/T9wmtUykEhbqG4UljWg2J9yM3ZzMdS2JHLm3yoBe9zCyqI/tsa4R6zuXlqhf/RT+vnca6OKWzQsS6UJK7No/6k5EcTXXv8A+/DOshzV14kguZAUSG7kXDBUZ3+TiZf4Bc9eMVXaubTRCJjM0UzwfNf0GmStRKxX6xZ4/5jp8/mXyMQobBXnlFcZRVNHJfxfbmSrUu1opGFqe3gNEP7N/HTiJsiTQVjdYQtrkfw0HoOSMnVSdtzzxZSc502nP2UT6PR/R/xqCNzAo/1pnTVI+lJDGfz2ogXRNRcGIBJ6K48zECgYEA1vDYDzZsRdb2SBacHTQk4e0AorSk7OcO94avf5PSehB9NuFODI8z5MkzhRda6kn4smJqEb5gwBO4u0qYzcop9MsZUSdXj1oc5UrxTSMAQrTn3GuSBoZGepdvBm8GZmk6AvNFpnqhAIiDcMxwtbRuQFyBszMSm1jFqzRZjAZSH/UCgYEAzztjoT+1qDl9km7YdRRwTSu44X9WbJEK+V3CCtgDtYJHyPGP6M/tfpK4gIynQfE7ohHoM99iEM0F8W66J0hzI0jFekgbwE4mGAUD5RnBEu5vvQy5LC5RBxLjL/hcTmU/MiMCKc6hU35TQC8I8BT/voFAK0Sz4YPKznu/r+n/DacCgYEAmvBeLwkiH24Hdoul2X9fHuUDUkY5pPQiW9fg5mweixMbz9W1t2P7Gm7XDpd5V+4esigzIbtEbvFIduodICsc93L4OwHLInDo53iQXPRgGbXidYetabqdT32d8NtTl7s+sCXBDXLUYFgHt+YHUVRRLWABtrWYMhdZ1kIUUtWzmYkCgYBQRAiK5EpQJjRlC1n7vzbgLRcnAFNRKby+aXpHCPQm0ZdMVYQQALlUVS/xWolOGUmntJfjv5oUN9Uddm3T2VP/TqhufI+DJMHMe+TOT/Ngicntx4fRfP8VZlNouSHHm5+mo7iqyMXjuQI10gH8O6Xy+80G9U1XA90BrRzJ3jBT8QKBgAYcjBRoJFEwPs0dThiqMjhkZln94olizGHhe5VxDRnUd26PVKUaZGQO1bp3rnQBUBbfxOyXUsuJex6AFxI1wcJdVdjCf6SeXGAFvpqsGmBgT7Xo9VBhmH2siJkVP8QxNX6tfXaVezHNmZMs6a8jJiV45sF6skBqgiaw8GcvOH7r";

    public QrCodeControllerMethodsE2ETests(ITestOutputHelper output)
    {
        Output = output;                
        VerificationLinkService = Get<IVerificationLinkService>();
    }

    [Theory]
    [InlineData(
        "1111111111",
        2026, 2, 01,
        "UtQp9Gpc51y-u3xApZjIjgkpZ01js-J8KflSPW8WzIE",
        "5265877635-20250729-01004014E040-00")]
    public void InvoiceQrWithKsefShouldReturnExpectedUrlAndBase64Png(
        string nip,
        int issueYear,
        int issueMonth,
        int issueDay,
        string invoiceHash,
        string ksefNumber)
    {
        DateTime issueDate = new(issueYear, issueMonth, issueDay);

        QrCodeResult payload = InvoiceQrWithKsef(nip, issueDate, invoiceHash, ksefNumber);

        string expectedUrl = VerificationLinkService.BuildInvoiceVerificationUrl(nip, issueDate, invoiceHash);
        Assert.Equal(expectedUrl, payload.Url);
        AssertBase64Png(payload.QrCode);

        Output.WriteLine($"URL: {payload.Url}");
        Output.WriteLine($"PNG(Base64): {payload.QrCode}");
    }

    [Theory]
    [InlineData(
        "1111111111",
        2026, 2, 01,
        "UtQp9Gpc51y-u3xApZjIjgkpZ01js-J8KflSPW8WzIE")]
    public void InvoiceQrOfflineShouldReturnExpectedUrlAndBase64Png(
        string nip,
        int issueYear,
        int issueMonth,
        int issueDay,
        string invoiceHash)
    {
        DateTime issueDate = new(issueYear, issueMonth, issueDay);

        QrCodeResult payload = InvoiceQrOffline(nip, issueDate, invoiceHash);

        string expectedUrl = VerificationLinkService.BuildInvoiceVerificationUrl(nip, issueDate, invoiceHash);
        Assert.Equal(expectedUrl, payload.Url);
        AssertBase64Png(payload.QrCode);

        Output.WriteLine($"URL: {payload.Url}");
        Output.WriteLine($"PNG(Base64): {payload.QrCode}");
    }

    [Theory]
    [InlineData(
        "1111111111",
        QRCodeContextIdentifierType.Nip,
        "1111111111",
        "01F20A5D352AE590",
        "UtQp9Gpc51y-u3xApZjIjgkpZ01js-J8KflSPW8WzIE")]
    public void CertificateQrUrlAndBase64Png(
        string sellerNip,
        QRCodeContextIdentifierType contextIdentifierType,
        string contextIdentifierValue,
        string certSerial,
        string invoiceHash)
    {
        string certBase64 = Uri.UnescapeDataString(CertBase64UrlEncoded);
        string privateKey = Uri.UnescapeDataString(PrivateKeyUrlEncoded);

        QrCodeResult payload = CertificateQr(
            sellerNip,
            contextIdentifierType,
            contextIdentifierValue,
            certSerial,
            invoiceHash,
            certBase64,
            privateKey);

        AssertBase64Png(payload.QrCode);

        Output.WriteLine($"URL: {payload.Url}");
        Output.WriteLine($"PNG(Base64): {payload.QrCode}");
    }

    private QrCodeResult InvoiceQrWithKsef(string nip, DateTime issueDate, string invoiceHash, string ksefNumber)
    {
        string url = VerificationLinkService.BuildInvoiceVerificationUrl(nip, issueDate, invoiceHash);
        byte[] qrCode = QrCodeService.GenerateQrCode(url);
        byte[] labeledQr = QrCodeService.AddLabelToQrCode(qrCode, ksefNumber);

        return new QrCodeResult(url, Convert.ToBase64String(labeledQr));
    }

    private QrCodeResult InvoiceQrOffline(string nip, DateTime issueDate, string invoiceHash)
    {
        string url = VerificationLinkService.BuildInvoiceVerificationUrl(nip, issueDate, invoiceHash);
        byte[] qrCode = QrCodeService.GenerateQrCode(url);
        byte[] labeledQr = QrCodeService.AddLabelToQrCode(qrCode, "OFFLINE");

        return new QrCodeResult(url, Convert.ToBase64String(labeledQr));
    }

    private QrCodeResult CertificateQr(
        string sellerNip,
        QRCodeContextIdentifierType contextIdentifierType,
        string contextIdentifierValue,
        string certSerial,
        string invoiceHash,
        string certbase64,
        string privateKey)
    {
        byte[] bytes = Convert.FromBase64String(certbase64);
        X509Certificate2 certificate = bytes.LoadCertificate();

        string url = VerificationLinkService.BuildCertificateVerificationUrl(
            sellerNip,
            contextIdentifierType,
            contextIdentifierValue,
            certSerial,
            invoiceHash,
            certificate,
            privateKey);

        byte[] qrCode = QrCodeService.GenerateQrCode(url);
        byte[] labeledQr = QrCodeService.AddLabelToQrCode(qrCode, "CERTYFIKAT");

        return new QrCodeResult(url, Convert.ToBase64String(labeledQr));
    }

    // ------------------ Helpers ------------------

    private static void AssertBase64Png(string base64)
    {
        Assert.False(string.IsNullOrWhiteSpace(base64));
        Assert.StartsWith(Base64PngPrefix, base64);

        byte[] bytes = Convert.FromBase64String(base64);
        Assert.True(bytes.Length > 8);

        // PNG signature: 89 50 4E 47 0D 0A 1A 0A
        Assert.Equal(0x89, bytes[0]);
        Assert.Equal(0x50, bytes[1]);
        Assert.Equal(0x4E, bytes[2]);
        Assert.Equal(0x47, bytes[3]);
        Assert.Equal(0x0D, bytes[4]);
        Assert.Equal(0x0A, bytes[5]);
        Assert.Equal(0x1A, bytes[6]);
        Assert.Equal(0x0A, bytes[7]);
    }
}
