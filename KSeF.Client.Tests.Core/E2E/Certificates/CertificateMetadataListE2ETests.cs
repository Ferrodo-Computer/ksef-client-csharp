using KSeF.Client.Api.Builders.Certificates;
using KSeF.Client.Core.Models.Authorization;
using KSeF.Client.Core.Models.Certificates;
using KSeF.Client.Tests.Utils;

namespace KSeF.Client.Tests.Core.E2E.Certificates;

[Collection("CertificateScenarioE2ECollection")]
public class CertificateMetadataListE2ETests : TestBase
{
    private const string BaseCertificateName = "E2E Meta Cert";

    /// <summary>
    /// E2E: Wystaw certyfikat i odczytaj jego metadane filtrowane builderem.
    /// Kroki:
    /// 1) Logowanie i pobranie tokenu dostępowego dla losowego NIP
    /// 2) Pobranie danych do CSR i wygenerowanie CSR (ECDSA)
    /// 3) Wysłanie wniosku o certyfikat (enrollment)
    /// 4) Polling statusu do momentu uzyskania numeru seryjnego certyfikatu
    /// 5) Zbudowanie zapytania metadanych przy użyciu GetCertificateMetadataListRequestBuilder (serial, name, type)
    /// 6) Wywołanie API i weryfikacja, że na liście znajduje się wystawiony certyfikat
    /// 7) Sprzątanie: unieważnienie (revoke) utworzonego certyfikatu
    /// </summary>
    [Fact]
    public async Task GivenIssuedCertificate_WhenQueryMetadataWithBuilder_ThenReturnsMatchingEntry()
    {
        // Krok 1) Logowanie i pobranie access token dla losowego NIP
        string nip = MiscellaneousUtils.GetRandomNip();
        AuthenticationOperationStatusResponse auth = await AuthenticationUtils.AuthenticateAsync(AuthorizationClient, nip);
        string accessToken = auth.AccessToken.Token;

        // Krok 2) Pobranie informacji do CSR i wygenerowanie CSR (ECDSA)
        CertificateEnrollmentsInfoResponse enrollmentInfo = await KsefClient.GetCertificateEnrollmentDataAsync(accessToken, CancellationToken);
        (string csr, string _) = CryptographyService.GenerateCsrWithEcdsa(enrollmentInfo);

        string certificateName = $"{BaseCertificateName} {Guid.NewGuid():N}";

        // Krok 3) Złożenie wniosku o certyfikat (enrollment)
        SendCertificateEnrollmentRequest request = SendCertificateEnrollmentRequestBuilder
            .Create()
            .WithCertificateName(certificateName)
            .WithCertificateType(CertificateType.Authentication)
            .WithCsr(csr)
            .Build();

        CertificateEnrollmentResponse enrollmentResponse = await KsefClient.SendCertificateEnrollmentAsync(request, accessToken, CancellationToken);
        Assert.NotNull(enrollmentResponse);
        Assert.False(string.IsNullOrWhiteSpace(enrollmentResponse.ReferenceNumber));

        // Krok 4) Polling statusu rejestracji do momentu uzyskania numeru seryjnego
        CertificateEnrollmentStatusResponse status = await AsyncPollingUtils.PollAsync(
            async () => await KsefClient.GetCertificateEnrollmentStatusAsync(enrollmentResponse.ReferenceNumber, accessToken, CancellationToken).ConfigureAwait(false),
            result => result is not null && !string.IsNullOrWhiteSpace(result.CertificateSerialNumber),
            cancellationToken: CancellationToken);

        Assert.NotNull(status);
        Assert.False(string.IsNullOrWhiteSpace(status.CertificateSerialNumber));
        string serialNumber = status.CertificateSerialNumber;

        // Krok 5) Zbudowanie zapytania o metadane przy użyciu buildera (filtry: serial, name, type)
        CertificateMetadataListRequest metadataQuery = GetCertificateMetadataListRequestBuilder
            .Create()
            .WithCertificateSerialNumber(serialNumber)
            .WithName(certificateName)
            .WithCertificateType(CertificateType.Authentication)
            .Build();

        // Krok 6) Wywołanie API i asercja, że certyfikat znajduje się w wynikach
        CertificateMetadataListResponse metadataList = await KsefClient.GetCertificateMetadataListAsync(
            accessToken,
            metadataQuery,
            pageSize: 20,
            pageOffset: 0,
            cancellationToken: CancellationToken);

        Assert.NotNull(metadataList);
        Assert.NotNull(metadataList.Certificates);
        Assert.Contains(metadataList.Certificates, c => c.CertificateSerialNumber == serialNumber && c.Name == certificateName);

        // Krok 7) Sprzątanie: unieważnienie utworzonego certyfikatu
        await KsefClient.RevokeCertificateAsync(
            RevokeCertificateRequestBuilder
                .Create()
                .WithRevocationReason(CertificateRevocationReason.KeyCompromise)
                .Build(),
            serialNumber,
            accessToken,
            CancellationToken);
    }
}
