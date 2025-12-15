using KSeF.Client.Api.Builders.EuEntityPermissions;
using KSeF.Client.Core.Models;
using KSeF.Client.Core.Models.Authorization;
using KSeF.Client.Core.Models.Permissions;
using KSeF.Client.Core.Models.Permissions.EUEntity;
using KSeF.Client.Core.Models.Permissions.Identifiers;
using KSeF.Client.Tests.Utils;
using System.Security.Cryptography.X509Certificates;

namespace KSeF.Client.Tests.Core.E2E.Permissions.EuEntityPermission;

/// <summary>
/// Pobranie listy administratorów podmiotów unijnych jako właściciel.
/// Scenariusz: właściciel nadaje uprawnienia administracyjne jednostce UE w swoim kontekście NIP-VAT UE,
/// następnie pobiera listę administratorów poprzez SearchGrantedEuEntityPermissionsAsync.
/// </summary>
public class EuEntityAdminsListAsOwnerE2ETests : TestBase
{
    [Fact]
    public async Task EuEntityAdmins_AsOwner_ShouldReturnList()
    {
        // Arrange: przygotuj kontekst właściciela (NIP oraz NIP-VAT UE)
        string ownerNip = MiscellaneousUtils.GetRandomNip();
        string ownerVatEu = MiscellaneousUtils.GetRandomNipVatEU(ownerNip,CountryCode.ES);

        // Uwierzytelnij właściciela w kontekście NIP
        AuthenticationOperationStatusResponse ownerAuth = await AuthenticationUtils.AuthenticateAsync(
            AuthorizationClient,
            ownerNip);

        Assert.NotNull(ownerAuth);
        Assert.False(string.IsNullOrWhiteSpace(ownerAuth.AccessToken?.Token));

        // Przygotuj "administratora" jednostki UE (fingerprint certyfikatu osobistego)
        X509Certificate2 euAdminCertificate = CertificateUtils.GetPersonalCertificate(
            givenName: "EU",
            surname: "Admin",
            serialNumberPrefix: "TINPL",
            serialNumber: ownerNip,
            commonName: "EU Admin");
        string euAdminFingerprint = CertificateUtils.GetSha256Fingerprint(euAdminCertificate);

        // Nadaj uprawnienia administracyjne jednostce UE w kontekście właściciela (NipVatUe)
        PermissionsEuEntitySubjectDetails subjectDetails = new PermissionsEuEntitySubjectDetails
        {
            SubjectDetailsType = PermissionsEuEntitySubjectDetailsType.EntityByFingerprint,
             EntityByFp = new PermissionsEuEntityEntityByFp
             {
                 Address = "EU Admin Address",
                 FullName = "EU Admin Full Name"
             }
        };

        GrantPermissionsEuEntityRequest grantRequest = GrantEuEntityPermissionsRequestBuilder
            .Create()
            .WithSubject(new EuEntitySubjectIdentifier
            {
                Type = EuEntitySubjectIdentifierType.Fingerprint,
                Value = euAdminFingerprint
            })
            .WithSubjectName("EU Admin Entity")
            .WithContext(new EuEntityContextIdentifier
            {
                Type = EuEntityContextIdentifierType.NipVatUe,
                Value = ownerVatEu
            })
            .WithDescription("Grant admin for EU Entity context")
            .WithSubjectDetails(subjectDetails)
            .Build();

        OperationResponse grantResponse = await KsefClient
            .GrantsPermissionEUEntityAsync(grantRequest, ownerAuth.AccessToken.Token, CancellationToken);

        Assert.NotNull(grantResponse);
        Assert.False(string.IsNullOrWhiteSpace(grantResponse.ReferenceNumber));

        // Opcjonalnie: poczekaj aż operacja nadania będzie w statusie 200
        PermissionsOperationStatusResponse grantStatus = await AsyncPollingUtils.PollAsync(
            async () => await KsefClient.OperationsStatusAsync(grantResponse.ReferenceNumber, ownerAuth.AccessToken.Token).ConfigureAwait(false),
            result => result is not null && result.Status is not null && result.Status.Code == 200,
            delay: TimeSpan.FromMilliseconds(SleepTime),
            maxAttempts: 60,
            cancellationToken: CancellationToken);

        Assert.NotNull(grantStatus);
        Assert.Equal(200, grantStatus.Status.Code);

        // Act: pobierz listę administratorów podmiotów unijnych jako właściciel
        EuEntityPermissionsQueryRequest query = new()
        {
            PermissionTypes = new List<EuEntityPermissionType>
            {
                EuEntityPermissionType.VatUeManage
            }
        };

        PagedPermissionsResponse<Client.Core.Models.Permissions.EuEntityPermission> admins = await AsyncPollingUtils.PollAsync(
            action: async () => await KsefClient.SearchGrantedEuEntityPermissionsAsync(
                query,
                ownerAuth.AccessToken.Token,
                pageOffset: 0,
                pageSize: 10,
                CancellationToken).ConfigureAwait(false),
            condition: r => r is not null && r.Permissions is { Count: > 0 },
            delay: TimeSpan.FromMilliseconds(SleepTime),
            maxAttempts: 60,
            cancellationToken: CancellationToken);

        // Assert
        Assert.NotNull(admins);
        Assert.NotEmpty(admins.Permissions);
        Assert.Contains(admins.Permissions, x => x.SubjectEntityDetails.Address == subjectDetails.EntityByFp.Address && x.SubjectEntityDetails.FullName == subjectDetails.EntityByFp.FullName);
    }
}
