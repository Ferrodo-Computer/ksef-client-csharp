using KSeF.Client.Api.Services;
using KSeF.Client.Core.Interfaces.Services;
using KSeF.Client.Core.Models.QRCode;
using KSeF.Client.DI;
using KSeF.Client.Extensions;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace KSeF.Client.Tests;

public class VerificationLinkServiceTests
{
    private readonly IVerificationLinkService _svc = new VerificationLinkService(new KSeFClientOptions() { BaseUrl = KsefEnvironmentsUris.TEST }); 
    private readonly string BaseUrl = $"{KsefEnvironmentsUris.TEST}/client-app";


    // =============================================
    // Testy legacy (RSA) – tylko dla zgodności wstecznej; NIEZALECANE:
    // • Użycie RSA 2048-bit:
    //    - Większy rozmiar kluczy i linków
    //    - Wolniejsze operacje kryptograficzne
    //    - Dłuższe URL-e (gorszy UX)
    // =============================================
    [Theory]
    [InlineData("<root>test</root>")]
    [InlineData("<data>special & chars /?</data>")]
    public void BuildInvoiceVerificationUrl_EncodesHashCorrectly(string xml)
    {
        // Arrange
        string nip = "1234567890";
        DateTime issueDate = new DateTime(2026, 1, 5);

        byte[] sha;
        using (SHA256 sha256 = SHA256.Create())
            sha = sha256.ComputeHash(Encoding.UTF8.GetBytes(xml));

        string invoiceHash = Convert.ToBase64String(sha);
        string expectedHash = sha.EncodeBase64UrlToString();
        string expectedUrl = $"{BaseUrl}/invoice/{nip}/{issueDate:dd-MM-yyyy}/{expectedHash}";

        // Act
        string url = _svc.BuildInvoiceVerificationUrl(nip, issueDate, invoiceHash);

        // Assert
        Assert.Equal(expectedUrl, url);

        string[] segments = new Uri(url)
            .Segments
            .Select(s => s.Trim('/'))
            .ToArray();

        Assert.Equal("client-app", segments[1]);
        Assert.Equal("invoice", segments[2]);
        Assert.Equal(nip, segments[3]);
        Assert.Equal(issueDate.ToString("dd-MM-yyyy"), segments[4]);
        Assert.Equal(expectedHash, segments[5]);
    }

    [Fact]
    public void BuildCertificateVerificationUrl_WithRsaCertificate_ShouldMatchFormat()
    {
        // Arrange
        string nip = "0000000000";
        string xml = "<x/>";
        string serial = Guid.NewGuid().ToString();
        string invoiceHash;
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(xml));
            invoiceHash = Convert.ToBase64String(hashBytes);
        }

        // Create full self-signed RSA cert with private key
        using RSA rsa = RSA.Create(2048);
        CertificateRequest req = new CertificateRequest("CN=TestRSA", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
        X509Certificate2 fullCert = req.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddDays(1));
                    
        // Act
        string url = _svc.BuildCertificateVerificationUrl(nip, QRCodeContextIdentifierType.Nip,nip, serial.ToString(), invoiceHash, fullCert);

        // Assert
        string[] segments = new Uri(url)
            .Segments
            .Select(s => s.Trim('/'))
            .ToArray();

        Assert.Equal("client-app", segments[1]);
        Assert.Equal("certificate", segments[2]);
        Assert.Equal("Nip", segments[3]);
        Assert.Equal(nip, segments[4]);
        Assert.Equal(nip, segments[5]);
        Assert.Equal(serial.ToString(), segments[6]);
        Assert.False(string.IsNullOrWhiteSpace(segments[7])); // hash
        Assert.False(string.IsNullOrWhiteSpace(segments[8])); // signed hash
    }

    [Fact]
    public void BuildCertificateVerificationUrl_WithEcdsaCertificate_ShouldMatchFormat()
    {
        // Arrange
        string nip = "0000000000";
        string xml = "<x/>";
        string serial = Guid.NewGuid().ToString();
        string invoiceHash;
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(xml));
            invoiceHash = Convert.ToBase64String(hashBytes);
        }

        // Create full self-signed ECDsa cert with private key
        using ECDsa ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        CertificateRequest req = new CertificateRequest("CN=TestECDSA", ecdsa, HashAlgorithmName.SHA256);
        X509Certificate2 fullCert = req.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(1));

        // Act
        string url = _svc.BuildCertificateVerificationUrl(nip, QRCodeContextIdentifierType.Nip, nip, serial.ToString(), invoiceHash, fullCert,fullCert.GetRSAPrivateKey()?.ExportPkcs8PrivateKeyPem());

        // Assert
        string[] segments = new Uri(url)
            .Segments
            .Select(s => s.Trim('/'))
            .ToArray();

        Assert.Equal("client-app", segments[1]);
        Assert.Equal("certificate", segments[2]);
        Assert.Equal("Nip", segments[3]);
        Assert.Equal(nip, segments[4]);
        Assert.Equal(nip, segments[5]);
        Assert.Equal(serial.ToString(), segments[6]);
        Assert.False(string.IsNullOrWhiteSpace(segments[7])); // hash
        Assert.False(string.IsNullOrWhiteSpace(segments[8])); // signed hash
    }


    [Fact]
    public void BuildCertificateVerificationUrl_WithoutPrivateKey_ShouldThrow()
    {
        // Arrange: certyfikat z samym kluczem publicznym (bez prywatnego)
        using RSA rsa = RSA.Create(2048);
        CertificateRequest req = new CertificateRequest("CN=PublicOnly", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
        X509Certificate2 fullCert = req.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddDays(1));

        // Eksport tylko publicznego certyfikatu
        byte[] publicBytes = fullCert.Export(X509ContentType.Cert);
        X509Certificate2 pubOnly = new X509Certificate2(publicBytes); // brak prywatnego klucza

        string nip = "0000000000";
        string xml = "<x/>";
        string serial = Guid.NewGuid().ToString();
        string invoiceHash;
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(xml));
            invoiceHash = Convert.ToBase64String(hashBytes);
        }

        // Act & Assert: próba podpisania bez klucza prywatnego → wyjątek
        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
        {
            // przekazujemy pusty ciąg Base64 jako "brakujący" klucz prywatny
            return _svc.BuildCertificateVerificationUrl(nip, QRCodeContextIdentifierType.Nip, nip, serial.ToString(), invoiceHash, pubOnly);
        });

        Assert.Contains("nie wspiera RSA", ex.Message, StringComparison.OrdinalIgnoreCase);
    }


    [Fact]
    public void BuildCertificateVerificationUrl_WithEmbeddedPrivateKey_ShouldSucceed()
    {
        // Arrange: wygeneruj self-signed cert z kluczem RSA
        using RSA rsa = RSA.Create(2048);
        CertificateRequest req = new CertificateRequest(
            "CN=FullCert",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pss
        );
        X509Certificate2 fullCert = req.CreateSelfSigned(
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddDays(1)
        );

        // Zapisz PFX i zaimportuj z flagą Exportable — certyfikat ma teraz wbudowany klucz
        byte[] pfxBytes = fullCert.Export(X509ContentType.Pfx);
        X509Certificate2 certWithKey = new X509Certificate2(
            pfxBytes,
            (string?)null,
            X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet
        );

        string nip = "0000000000";
        string xml = "<x/>";
        string serial = Guid.NewGuid().ToString();
        string invoiceHash;
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] sha = sha256.ComputeHash(Encoding.UTF8.GetBytes(xml));
            invoiceHash = Convert.ToBase64String(sha);
        }

        // Act
        string url = _svc.BuildCertificateVerificationUrl(nip, QRCodeContextIdentifierType.Nip,
            nip,
            serial,
            invoiceHash,
            certWithKey
        );

        // Assert: URL powinien zawierać URL-encoded Base64 podpisu (końcówka "==" → "%3D%3D")
        Assert.NotNull(url);
        Uri uri = new Uri(url);
        string[] segments = uri.AbsolutePath.Split('/');
        string signedUrl = segments.Last();
        Assert.Matches("^[A-Za-z0-9_-]+$", signedUrl);
    }

    // =============================================
    // Rekomendowane testy ECC (ECDSA P-256):
    // • Bezpieczeństwo jak RSA-2048, ale mniejsze i szybsze klucze
    // • Krótsze podpisane URL-e → lepszy UX w QR i linkach
    // =============================================

    [Theory]
    [InlineData("<root>test</root>")]
    [InlineData("<data>special & chars /?</data>")]
    public void BuildInvoiceVerificationUrl_EncodesHashCorrectly_Ecc(string xml)
    {
        // Arrange – bez zmian, testuje enkodowanie hash
        string nip = "1234567890";
        DateTime issueDate = new DateTime(2026, 1, 5);

        byte[] sha;
        using (SHA256 sha256 = SHA256.Create())
            sha = sha256.ComputeHash(Encoding.UTF8.GetBytes(xml));

        string invoiceHash = Convert.ToBase64String(sha);            
        string expectedHash = sha.EncodeBase64UrlToString();

        string expectedUrl = $"{BaseUrl}/invoice/{nip}/{issueDate:dd-MM-yyyy}/{expectedHash}";

        // Act
        string url = _svc.BuildInvoiceVerificationUrl(nip, issueDate, invoiceHash);

        // Assert
        Assert.Equal(expectedUrl, url);
        string[] segments = new Uri(url).Segments.Select(s => s.Trim('/')).ToArray();
        Assert.Equal("invoice", segments[2]);
        Assert.Equal(expectedHash, segments[5]);
    }

    [Fact]
    public void BuildCertificateVerificationUrl_WithEcdsaCertificate_ShouldMatchFormat_Ecc()
    {
        // Arrange – generowanie ECDSA P-256
        string nip = "0000000000";
        string xml = "<x/>";
        string serial = Guid.NewGuid().ToString();
        string invoiceHash;
        using (SHA256 sha256 = SHA256.Create())
        {
            invoiceHash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(xml)));
        }

        using ECDsa ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        CertificateRequest req = new CertificateRequest("CN=TestECDSA", ecdsa, HashAlgorithmName.SHA256);
        X509Certificate2 fullCert = req.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddYears(1));

        // Act – jawnie przekazujemy prywatny klucz ECDSA
        string? privateKeyPem = fullCert.GetECDsaPrivateKey()?.ExportPkcs8PrivateKeyPem();
        string url = _svc.BuildCertificateVerificationUrl(nip, QRCodeContextIdentifierType.Nip, nip, serial, invoiceHash, fullCert, privateKeyPem);

        // Assert – format ścieżek
        string[] segments = new Uri(url).Segments.Select(s => s.Trim('/')).ToArray();
        Assert.Equal("client-app", segments[1]);
        Assert.Equal("certificate", segments[2]);
        Assert.Equal("Nip", segments[3]);
        Assert.Equal(nip, segments[4]);
        Assert.Equal(nip, segments[5]);
        Assert.Equal(serial.ToString(), segments[6]);
        Assert.False(string.IsNullOrWhiteSpace(segments[7])); // hash
        Assert.False(string.IsNullOrWhiteSpace(segments[8])); // signed hash
    }

    [Fact]
    public void BuildCertificateVerificationUrl_WithoutPrivateKey_ShouldThrow_Ecc()
    {
        // Arrange – public-only ECC powinno rzucić
        using ECDsa ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        CertificateRequest req = new CertificateRequest("CN=PublicOnly", ecdsa, HashAlgorithmName.SHA256);
        X509Certificate2 fullCert = req.CreateSelfSigned(DateTimeOffset.Now, DateTimeOffset.Now.AddDays(1));
        byte[] publicBytes = fullCert.Export(X509ContentType.Cert);
        X509Certificate2 pubOnly = new X509Certificate2(publicBytes);

        string nip = "0000000000";
        string xml = "<x/>";
        string serial = Guid.NewGuid().ToString();
        string invoiceHash;
        using (SHA256 sha256 = SHA256.Create())
        {
            invoiceHash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(xml)));
        }

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            _svc.BuildCertificateVerificationUrl(nip, QRCodeContextIdentifierType.Nip, nip, serial, invoiceHash, pubOnly)
        );
    }

    [Fact]
    public void BuildCertificateVerificationUrl_WithEmbeddedEcdsaKey_ShouldSucceed_Ecc()
    {
        // Arrange – certyfikat PFX ECDSA P-256 z flagą exportowalności
        using ECDsa ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        CertificateRequest req = new CertificateRequest("CN=FullEccCert", ecdsa, HashAlgorithmName.SHA256);
        X509Certificate2 fullCert = req.CreateSelfSigned(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(1));
        byte[] pfx = fullCert.Export(X509ContentType.Pfx);
        X509Certificate2 certWithKey = new X509Certificate2(pfx, string.Empty, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet);

        string nip = "0000000000";
        string xml = "<x/>";
        string serial = Guid.NewGuid().ToString();
        string invoiceHash;
        using (SHA256 sha256 = SHA256.Create())
            invoiceHash = Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(xml)));

        // Act
        string url = _svc.BuildCertificateVerificationUrl(nip, QRCodeContextIdentifierType.Nip,    nip, serial, invoiceHash, certWithKey);

        // Assert: URL zawiera poprawny ECDSA podpis kodowany w Base64
        Uri uri = new Uri(url);
        string[] segments = uri.AbsolutePath.Split('/');
        string signedUrl = segments.Last();
        Assert.Matches("^[A-Za-z0-9_-]+$", signedUrl);             
    }
}
