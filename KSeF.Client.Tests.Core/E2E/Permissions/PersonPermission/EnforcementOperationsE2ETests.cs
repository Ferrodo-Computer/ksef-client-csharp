using KSeF.Client.Api.Builders.PersonPermissions;
using KSeF.Client.Core.Models;
using KSeF.Client.Core.Models.ApiResponses;
using KSeF.Client.Core.Models.Authorization;
using KSeF.Client.Core.Models.Permissions;
using KSeF.Client.Core.Models.Permissions.Identifiers;
using KSeF.Client.Core.Models.Permissions.Person;
using KSeF.Client.Core.Models.TestData;
using KSeF.Client.Tests.Utils;

namespace KSeF.Client.Tests.Core.E2E.Permissions.PersonPermission;

/// <summary>
/// Nadawanie uprawnień do wykonywania operacji komorniczych w kontekście, który na to zezwala
/// (EnforcementAuthority oraz CourtBailiff).
/// </summary>
public class EnforcementOperationsE2ETests : TestBase
{
    private const string PermissionDescription = "E2E grant EnforcementOperations";

    /// <summary>
    /// Scenariusz dla EnforcementAuthority:
    /// 1) Utworzenie podmiotu typu EnforcementAuthority (kontekst dozwolony)
    /// 2) Uwierzytelnienie w kontekście EnforcementAuthority
    /// 3) Nadanie osobie uprawnienia EnforcementOperations
    /// 4) Weryfikacja, że uprawnienie pojawiło się w wyszukiwaniu
    /// 5) Cofnięcie uprawnienia i weryfikacja, że zniknęło
    /// 6) Sprzątanie środowiska testowego (usunięcie podmiotu)
    /// </summary>
    [Fact]
    public async Task GrantEnforcementOperationsAsEnforcementAuthorityE2EGrantSearchRevokeSearch()
    {
        // Arrange: Utworzenie kontekstu EnforcementAuthority oraz uwierzytelnienie
        string enforcementAuthorityNip = MiscellaneousUtils.GetRandomNip();
        string authorizedNip = MiscellaneousUtils.GetRandomNip();

        await TestDataClient.CreateSubjectAsync(new SubjectCreateRequest
        {
            SubjectNip = enforcementAuthorityNip,
            SubjectType = SubjectType.EnforcementAuthority,
            Description = "E2E EnforcementAuthority"
        });

        AuthenticationOperationStatusResponse authorizationInfo = await AuthenticationUtils
            .AuthenticateAsync(AuthorizationClient, enforcementAuthorityNip);

        string accessToken = authorizationInfo.AccessToken.Token;

        // Act: Nadawanie uprawnienia EnforcementOperations osobie uwierzytelniającej się nipem `authorizedNip`
        OperationResponse grantResponse = await GrantEnforcementOperationsAsync(authorizedNip, accessToken);

        Assert.NotNull(grantResponse);
        Assert.False(string.IsNullOrEmpty(grantResponse.ReferenceNumber));

        // Wyszukiwanie - trwa do czasu aż pojawi się nadane uprawnienie
        PagedPermissionsResponse<Client.Core.Models.Permissions.PersonPermission> searchAfterGrant = await AsyncPollingUtils.PollAsync(
            action: async () => await SearchGrantedPersonPermissionsAsync(accessToken).ConfigureAwait(false),
            condition: result =>
            {
                if (result is null || result.Permissions is null)
                {
                    return false;
                }

                List<Client.Core.Models.Permissions.PersonPermission> matches = [.. result.Permissions.Where(p => p.Description == PermissionDescription)];

                return matches.Any(x =>
                    x.PermissionScope == PersonPermissionType.EnforcementOperations);
            },
            delay: TimeSpan.FromMilliseconds(SleepTime),
            maxAttempts: 60,
            cancellationToken: CancellationToken);

        // Assert: Weryfikacja, że uprawnienie jest widoczne i zawiera oczekiwany zakres
        Assert.NotNull(searchAfterGrant);
        Assert.NotEmpty(searchAfterGrant.Permissions);
        Assert.Contains(searchAfterGrant.Permissions,
            x => x.Description == PermissionDescription &&
                 x.PermissionScope == PersonPermissionType.EnforcementOperations);

        // Wycofanie wszystkich znalezionych uprawnień (po opisie)
        List<Client.Core.Models.Permissions.PersonPermission> toRevoke = [.. searchAfterGrant.Permissions.Where(p => p.Description == PermissionDescription)];

        List<PermissionsOperationStatusResponse> revokeStatuses = await RevokePermissionsAsync(toRevoke, accessToken);

        Assert.NotNull(revokeStatuses);
        Assert.NotEmpty(revokeStatuses);
        Assert.Equal(toRevoke.Count, revokeStatuses.Count);
        Assert.All(revokeStatuses, s =>
            Assert.Equal(OperationStatusCodeResponse.Success, s.Status.Code));

        // Ponowne wyszukiwanie — aż uprawnienie zniknie
        PagedPermissionsResponse<Client.Core.Models.Permissions.PersonPermission> searchAfterRevoke = await AsyncPollingUtils.PollAsync(
            action: async () => await SearchGrantedPersonPermissionsAsync(accessToken).ConfigureAwait(false),
            condition: result =>
            {
                if (result is null || result.Permissions is null)
                {
                    return false;
                }

                List<Client.Core.Models.Permissions.PersonPermission> remaining = [.. result.Permissions.Where(p => p.Description == PermissionDescription)];

                return remaining.Count == 0;
            },
            delay: TimeSpan.FromMilliseconds(SleepTime),
            maxAttempts: 60,
            cancellationToken: CancellationToken);

        // Assert: Uprawnienie zostało usunięte
        Assert.NotNull(searchAfterRevoke);
        Assert.True(searchAfterRevoke.Permissions is null ||
                    !searchAfterRevoke.Permissions.Any(p => p.Description == PermissionDescription));

        // Cleanup: Usunięcie kontekstu EnforcementAuthority
        await TestDataClient.RemoveSubjectAsync(new SubjectRemoveRequest
            { SubjectNip = enforcementAuthorityNip });
    }

    /// <summary>
    /// Scenariusz dla CourtBailiff:
    /// 1) Utworzenie osoby fizycznej z flagą komornika (IsBailiff = true)
    /// 2) Uwierzytelnienie i weryfikacja roli CourtBailiff
    /// 3) Nadanie osobie uprawnienia EnforcementOperations
    /// 4) Weryfikacja, że uprawnienie pojawiło się w wyszukiwaniu
    /// 5) Cofnięcie uprawnienia i weryfikacja, że zniknęło
    /// 6) Sprzątanie środowiska testowego (usunięcie osoby fizycznej)
    /// </summary>
    [Fact]
    public async Task GrantEnforcementOperationsAsCourtBailiffE2EGrantSearchRevokeSearch()
    {
        // Arrange: Utworzenie osoby fizycznej z flagą komornika oraz uwierzytelnienie
        string bailiffNip = MiscellaneousUtils.GetRandomNip();
        string bailiffPesel = MiscellaneousUtils.GetRandomPesel();
        string granteeNip = MiscellaneousUtils.GetRandomNip();

        await TestDataClient.CreatePersonAsync(new PersonCreateRequest
        {
            Nip = bailiffNip,
            Pesel = bailiffPesel,
            IsBailiff = true,
            Description = "E2E CourtBailiff"
        });

        AuthenticationOperationStatusResponse auth = await AuthenticationUtils
            .AuthenticateAsync(AuthorizationClient, bailiffNip);

        string accessToken = auth.AccessToken.Token;

        // Act: Nadanie uprawnienia EnforcementOperations osobie granteeNip
        OperationResponse grantResponse = await GrantEnforcementOperationsAsync(granteeNip, accessToken);

        Assert.NotNull(grantResponse);
        Assert.False(string.IsNullOrEmpty(grantResponse.ReferenceNumber));

        // Wyszukiwanie — aż pojawi się nadane uprawnienie
        PagedPermissionsResponse<Client.Core.Models.Permissions.PersonPermission> searchAfterGrant = await AsyncPollingUtils.PollAsync(
            action: async () => await SearchGrantedPersonPermissionsAsync(accessToken).ConfigureAwait(false),
            condition: result =>
            {
                if (result is null || result.Permissions is null)
                {
                    return false;
                }

                List<Client.Core.Models.Permissions.PersonPermission> matches = [.. result.Permissions.Where(p => p.Description == PermissionDescription)];

                return matches.Any(x =>
                    x.PermissionScope == PersonPermissionType.EnforcementOperations);
            },
            delay: TimeSpan.FromMilliseconds(SleepTime),
            maxAttempts: 60,
            cancellationToken: CancellationToken);

        // Assert: Weryfikacja, że uprawnienie jest widoczne i zawiera oczekiwany zakres
        Assert.NotNull(searchAfterGrant);
        Assert.NotEmpty(searchAfterGrant.Permissions);
        Assert.Contains(searchAfterGrant.Permissions,
            x => x.Description == PermissionDescription &&
                 x.PermissionScope == PersonPermissionType.EnforcementOperations);

        // Wycofanie wszystkich znalezionych uprawnień (po opisie)
        List<Client.Core.Models.Permissions.PersonPermission> toRevoke = [.. searchAfterGrant.Permissions.Where(p => p.Description == PermissionDescription)];

        List<PermissionsOperationStatusResponse> revokeStatuses = await RevokePermissionsAsync(toRevoke, accessToken);

        Assert.NotNull(revokeStatuses);
        Assert.NotEmpty(revokeStatuses);
        Assert.Equal(toRevoke.Count, revokeStatuses.Count);
        Assert.All(revokeStatuses, s =>
            Assert.Equal(OperationStatusCodeResponse.Success, s.Status.Code));

        // Ponowne wyszukiwanie — aż uprawnienie zniknie
        PagedPermissionsResponse<Client.Core.Models.Permissions.PersonPermission> searchAfterRevoke = await AsyncPollingUtils.PollAsync(
            action: async () => await SearchGrantedPersonPermissionsAsync(accessToken).ConfigureAwait(false),
            condition: result =>
            {
                if (result is null || result.Permissions is null)
                {
                    return false;
                }

                List<Client.Core.Models.Permissions.PersonPermission> remaining = [.. result.Permissions.Where(p => p.Description == PermissionDescription)];

                return remaining.Count == 0;
            },
            delay: TimeSpan.FromMilliseconds(SleepTime),
            maxAttempts: 60,
            cancellationToken: CancellationToken);

        // Assert: Uprawnienie zostało usunięte
        Assert.NotNull(searchAfterRevoke);
        Assert.True(searchAfterRevoke.Permissions is null ||
                    !searchAfterRevoke.Permissions.Any(p => p.Description == PermissionDescription));

        // Cleanup: Usunięcie osoby fizycznej z flagą komornika
        await TestDataClient.RemovePersonAsync(new PersonRemoveRequest
        { Nip = bailiffNip });
    }

    #region Shared Helper Methods

    /// <summary>
    /// Przyznanie osobie uprawnień typu EnforcementOperations.
    /// </summary>
    /// <param name="subjectNip"></param>
    /// <param name="accessToken"></param>
    /// <returns></returns>
    private async Task<OperationResponse> GrantEnforcementOperationsAsync(string subjectNip, string accessToken)
    {
        GrantPermissionsPersonRequest request = GrantPersonPermissionsRequestBuilder
            .Create()
            .WithSubject(new GrantPermissionsPersonSubjectIdentifier
            {
                Type = GrantPermissionsPersonSubjectIdentifierType.Nip,
                Value = subjectNip
            })
            .WithPermissions(PersonPermissionType.EnforcementOperations)
            .WithDescription(PermissionDescription)
            .Build();

        OperationResponse response = await KsefClient.GrantsPermissionPersonAsync(request, accessToken, CancellationToken).ConfigureAwait(false);
        return response;
    }

    /// <summary>
    /// Wyszukanie nadanych uprawnień osób o typie EnforcementOperations.
    /// </summary>
    /// <param name="accessToken"></param>
    /// <returns></returns>
    private async Task<PagedPermissionsResponse<Client.Core.Models.Permissions.PersonPermission>> SearchGrantedPersonPermissionsAsync(string accessToken)
    {
        PersonPermissionsQueryRequest query = new()
        {
            PermissionTypes =
            [
                PersonPermissionType.EnforcementOperations
            ]
        };

        PagedPermissionsResponse<Client.Core.Models.Permissions.PersonPermission> response = await KsefClient
            .SearchGrantedPersonPermissionsAsync(query, accessToken, pageOffset: 0, pageSize: 10, CancellationToken).ConfigureAwait(false);

        return response;
    }

    /// <summary>
    /// Cofnięcie wszystkich przekazanych uprawnień i zwrócenie statusów operacji.
    /// </summary>
    /// <param name="grantedPermissions"></param>
    /// <param name="accessToken"></param>
    /// <returns></returns>
    private async Task<List<PermissionsOperationStatusResponse>> RevokePermissionsAsync(
        IEnumerable<Client.Core.Models.Permissions.PersonPermission> grantedPermissions, string accessToken)
    {
        List<OperationResponse> revokeResponses = [];

        foreach (Client.Core.Models.Permissions.PersonPermission permission in grantedPermissions)
        {
            OperationResponse response = await KsefClient
                .RevokeCommonPermissionAsync(permission.Id, accessToken, CancellationToken.None).ConfigureAwait(false);
            revokeResponses.Add(response);
        }

        List<PermissionsOperationStatusResponse> statuses = [];
        foreach (OperationResponse revokeResponse in revokeResponses)
        {
            PermissionsOperationStatusResponse status = await AsyncPollingUtils.PollAsync(
                action: () => KsefClient.OperationsStatusAsync(revokeResponse.ReferenceNumber, accessToken),
                condition: s => s is not null && s.Status is not null && s.Status.Code == OperationStatusCodeResponse.Success,
                delay: TimeSpan.FromMilliseconds(SleepTime),
                maxAttempts: 60,
                cancellationToken: CancellationToken).ConfigureAwait(false);

            statuses.Add(status);
        }

        return statuses;
    }

    #endregion
}