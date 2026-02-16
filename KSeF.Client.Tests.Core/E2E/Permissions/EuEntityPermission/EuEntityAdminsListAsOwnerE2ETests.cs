using KSeF.Client.Api.Builders.EuEntityPermissions;
using KSeF.Client.Core.Models;
using KSeF.Client.Core.Models.ApiResponses;
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
    private const string EuFirstName = "EU";
    private const string EuLastName = "Admin";
    private const string EuBirthDate = "1990-01-01";
    private const string IdDocType = "ID";
    private const string IdDocNumber = "123456";
    private const string IdDocCountry = "PL";
    private const string SerialNumberPrefixTinPl = "TINPL";
    private const string EuAdminCommonName = "EU Admin";

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
            givenName: EuFirstName,
            surname: EuLastName,
            serialNumberPrefix: SerialNumberPrefixTinPl,
            serialNumber: ownerNip,
            commonName: EuAdminCommonName);
        string euAdminFingerprint = CertificateUtils.GetSha256Fingerprint(euAdminCertificate);

        // Nadaj uprawnienia administracyjne jednostce UE w kontekście właściciela (NipVatUe)
        PermissionsEuEntitySubjectDetails subjectDetails = new PermissionsEuEntitySubjectDetails
        {
            SubjectDetailsType = PermissionsEuEntitySubjectDetailsType.PersonByFingerprintWithoutIdentifier,
            PersonByFpNoId = new PermissionsEuEntityPersonByFpNoId
            {
                FirstName = EuFirstName,
                LastName = EuLastName,
                BirthDate = EuBirthDate,
                IdDocument = new PermissionsEuEntityIdentityDocument
                {
                    Type = IdDocType,
                    Number = IdDocNumber,
                    Country = IdDocCountry
                }
            }
        };

        GrantPermissionsEuEntityRequest grantRequest = GrantEuEntityPermissionsRequestBuilder
            .Create()
            .WithSubject(new EuEntitySubjectIdentifier
            {
                Type = EuEntitySubjectIdentifierType.Fingerprint,
                Value = euAdminFingerprint
            })
            .WithSubjectName("Administrator jednostki UE")
            .WithContext(new EuEntityContextIdentifier
            {
                Type = EuEntityContextIdentifierType.NipVatUe,
                Value = ownerVatEu
            })
            .WithDescription("Nadanie uprawnień administratora dla kontekstu jednostki UE")
            .WithSubjectDetails(subjectDetails)
			.WithEuEntityDetails(new PermissionsEuEntityDetails
			{
				Address = "ul. Testowa 1, 00-000 Miasto",
				FullName = "Podmiot Testowy 1"
			})
			.Build();

        OperationResponse grantResponse = await KsefClient
            .GrantsPermissionEUEntityAsync(grantRequest, ownerAuth.AccessToken.Token, CancellationToken);

        Assert.NotNull(grantResponse);
        Assert.False(string.IsNullOrWhiteSpace(grantResponse.ReferenceNumber));

        // Opcjonalnie: poczekaj aż operacja nadania będzie w statusie 200
        PermissionsOperationStatusResponse grantStatus = await AsyncPollingUtils.PollAsync(
            async () => await KsefClient.OperationsStatusAsync(grantResponse.ReferenceNumber, ownerAuth.AccessToken.Token).ConfigureAwait(false),
            result => result is not null && result.Status is not null && result.Status.Code == OperationStatusCodeResponse.Success,
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
            cancellationToken: CancellationToken);

        // Assert
        Assert.NotNull(admins);
        Assert.NotEmpty(admins.Permissions);
        Assert.Contains(admins.Permissions, x => x.SubjectPersonDetails is not null && x.SubjectPersonDetails.FirstName == subjectDetails.PersonByFpNoId.FirstName && x.SubjectPersonDetails.LastName == subjectDetails.PersonByFpNoId.LastName);
    }
}
