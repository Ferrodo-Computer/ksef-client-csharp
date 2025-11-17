using KSeF.Client.Api.Services;
using KSeF.Client.Core.Models.QRCode;
using KSeF.Client.DI;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace KSeF.Client.Tests.Features;

public class QrCodeTests
{
    private readonly VerificationLinkService _linkSvc;

    public QrCodeTests()
    {
        _linkSvc = new VerificationLinkService(new KSeFClientOptions() { BaseUrl = KsefEnvironmentsUris.TEST });
    }

    [Theory]
    [InlineData("RSA", "TestRsaPublic")]
    [InlineData("ECC", "TestEccPublic")]
    [Trait("Category", "QRCode")]
    [Trait("Scenario", "Zbuduj link weryfikujący używając certyfikatu publicznego")]
    [Trait("Expected", "InvalidOperationException")]
    public void GivenPublicOnlyCertificate_WhenBuildingVerificationUrl_ThenThrowsInvalidOperationException(
        string keyType, string subjectName)
    {
        // Arrange
        X509Certificate2 publicCert;
        if (keyType == "RSA")
        {
            using RSA rsa = RSA.Create(2048);
            CertificateRequest req = new CertificateRequest($"CN={subjectName}", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            X509Certificate2 cert = req.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(1));
            publicCert = new X509Certificate2(cert.Export(X509ContentType.Cert));
        }
        else
        {
            using ECDsa ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
            CertificateRequest req = new CertificateRequest($"CN={subjectName}", ecdsa, HashAlgorithmName.SHA256);
            X509Certificate2 cert = req.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(1));
            publicCert = new X509Certificate2(cert.Export(X509ContentType.Cert));
        }

        string nip = "0000000000";
        string xml = "<x/>";
        string serial = Guid.NewGuid().ToString();
        string invoiceHash;
        using (SHA256 sha256 = SHA256.Create())
            invoiceHash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(xml)));

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            _linkSvc.BuildCertificateVerificationUrl(
                nip,
                QRCodeContextIdentifierType.Nip,
                nip,
                serial,
                invoiceHash,
                publicCert
            )
        );
    }

}
