using KSeF.Client.Api.Builders.Certificates;
using KSeF.Client.Core.Models.Authorization;
using KSeF.Client.Core.Models.Certificates;
using KSeF.Client.Core.Models.TestData;
using KSeF.Client.Tests.Utils;

namespace KSeF.Client.Tests.Core.E2E.TestData;

[Collection("CertificateScenarioE2ECollection")]
public class TestDataUpdateCertificateE2ETests : TestBase
{
    private const string CertificateName = "E2E UpdateCertificate Test";

    private readonly string _accessToken;

    public TestDataUpdateCertificateE2ETests()
    {
        AuthenticationOperationStatusResponse auth = AuthenticationUtils
            .AuthenticateAsync(AuthorizationClient, MiscellaneousUtils.GetRandomNip())
            .GetAwaiter()
            .GetResult();

        _accessToken = auth.AccessToken.Token;
    }

    /// <summary>
    /// Weryfikuje, że PUT /testdata/certificates/{serialNumber} pozwala zaktualizować datę ważności certyfikatu.
    /// Kroki:
    /// 1) Wystawienie certyfikatu (enrollment)
    /// 2) Odczytanie oryginalnej daty ważności z metadanych
    /// 3) Aktualizacja daty ważności na wcześniejszą (test-data, tylko środowiska testowe)
    /// 4) Weryfikacja, że metadane certyfikatu odzwierciedlają nową datę ważności
    /// 5) Sprzątanie: unieważnienie certyfikatu
    /// </summary>
    [Fact]
    public async Task UpdateCertificate_ChangesValidTo()
    {
        CertificateEnrollmentsInfoResponse enrollmentInfo = await KsefClient.GetCertificateEnrollmentDataAsync(_accessToken, CancellationToken);
        (string csr, string _) = CryptographyService.GenerateCsrWithEcdsa(enrollmentInfo);

        string certificateName = $"{CertificateName} {Guid.NewGuid():N}";

        SendCertificateEnrollmentRequest enrollmentRequest = SendCertificateEnrollmentRequestBuilder
            .Create()
            .WithCertificateName(certificateName)
            .WithCertificateType(CertificateType.Authentication)
            .WithCsr(csr)
            .Build();

        CertificateEnrollmentResponse enrollmentResponse = await KsefClient.SendCertificateEnrollmentAsync(enrollmentRequest, _accessToken, CancellationToken);
        Assert.NotNull(enrollmentResponse);
        Assert.False(string.IsNullOrWhiteSpace(enrollmentResponse.ReferenceNumber));

        CertificateEnrollmentStatusResponse status = await AsyncPollingUtils.PollAsync(
            action: () => KsefClient.GetCertificateEnrollmentStatusAsync(enrollmentResponse.ReferenceNumber, _accessToken, CancellationToken),
            condition: result => result is not null && !string.IsNullOrWhiteSpace(result.CertificateSerialNumber),
            cancellationToken: CancellationToken);

        Assert.NotNull(status);
        Assert.False(string.IsNullOrWhiteSpace(status.CertificateSerialNumber));
        string serialNumber = status.CertificateSerialNumber;

        CertificateMetadataListRequest metadataQuery = GetCertificateMetadataListRequestBuilder
            .Create()
            .WithCertificateSerialNumber(serialNumber)
            .Build();

        CertificateMetadataListResponse originalMetadata = await AsyncPollingUtils.PollAsync(
            action: () => KsefClient.GetCertificateMetadataListAsync(_accessToken, metadataQuery, pageSize: 20, pageOffset: 0, cancellationToken: CancellationToken),
            condition: r => r?.Certificates is not null && r.Certificates.Any(c => c.CertificateSerialNumber == serialNumber),
            cancellationToken: CancellationToken);

        DateTimeOffset originalValidTo = originalMetadata.Certificates.First(c => c.CertificateSerialNumber == serialNumber).ValidTo;
        DateTimeOffset newValidTo = originalValidTo.AddDays(-1);

        await TestDataClient.UpdateCertificateAsync(
            serialNumber,
            new TestDataUpdateCertificateRequest { ValidTo = newValidTo },
            _accessToken, CancellationToken);

        CertificateMetadataListResponse updatedMetadata = await AsyncPollingUtils.PollAsync(
            action: () => KsefClient.GetCertificateMetadataListAsync(_accessToken, metadataQuery, pageSize: 20, pageOffset: 0, cancellationToken: CancellationToken),
            condition: r => r?.Certificates is not null
                && r.Certificates.Any(c => c.CertificateSerialNumber == serialNumber && c.ValidTo == newValidTo),
            cancellationToken: CancellationToken);

        Assert.Contains(updatedMetadata.Certificates, c => c.CertificateSerialNumber == serialNumber && c.ValidTo == newValidTo);

        await KsefClient.RevokeCertificateAsync(
            RevokeCertificateRequestBuilder
                .Create()
                .WithRevocationReason(CertificateRevocationReason.KeyCompromise)
                .Build(),
            serialNumber,
            _accessToken,
            CancellationToken);
    }
}
