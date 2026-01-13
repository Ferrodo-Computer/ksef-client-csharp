using KSeF.Client.Api.Builders.PersonPermissions;
using KSeF.Client.Core.Models;
using KSeF.Client.Core.Models.ApiResponses;
using KSeF.Client.Core.Models.Permissions;
using KSeF.Client.Core.Models.Permissions.Identifiers;
using KSeF.Client.Core.Models.Permissions.Person;
using KSeF.Client.Tests.Utils;

namespace KSeF.Client.Tests.Core.E2E.Permissions.PersonPermission;

public class PersonPermissionE2ETests : TestBase
{
    private const string PermissionDescription = "E2E test grant";

    private readonly string accessToken = string.Empty;
    private GrantPermissionsPersonSubjectIdentifier Person { get; } = new();

    public PersonPermissionE2ETests()
    {
        // Arrange: uwierzytelnienie i przygotowanie danych testowych
        Client.Core.Models.Authorization.AuthenticationOperationStatusResponse auth = AuthenticationUtils
            .AuthenticateAsync(AuthorizationClient)
            .GetAwaiter().GetResult();

        accessToken = auth.AccessToken.Token;

        // Ustaw dane osoby testowej (PESEL)
        Person.Value = MiscellaneousUtils.GetRandomPesel();
        Person.Type = GrantPermissionsPersonSubjectIdentifierType.Pesel;
    }

    /// <summary>
    /// Testy E2E nadawania i cofania uprawnień osobom:
    /// - nadanie uprawnień
    /// - wyszukanie nadanych uprawnień
    /// - cofnięcie uprawnień
    /// - ponowne wyszukanie (weryfikacja, że zostały cofnięte)
    /// </summary>
    [Fact]
    public async Task PersonPermissionsFullFlowGrantSearchRevokeSearch()
    {
        // Arrange: dane wejściowe i oczekiwane typy uprawnień
        string description = PermissionDescription;

        // Act: nadaj uprawnienia osobie
        OperationResponse grantResponse =
            await GrantPersonPermissionsAsync(Person, description, accessToken);

        // Assert: weryfikacja poprawności odpowiedzi operacji nadania
        Assert.NotNull(grantResponse);
        Assert.False(string.IsNullOrEmpty(grantResponse.ReferenceNumber));

        // Act: odpytywanie do momentu, aż nadane uprawnienia będą widoczne (obie pule: Read i Write)
        PagedPermissionsResponse<Client.Core.Models.Permissions.PersonPermission> searchAfterGrant =
            await AsyncPollingUtils.PollAsync(
                async () => await SearchGrantedPersonPermissionsAsync(accessToken).ConfigureAwait(false),
                result =>
                {
                    if (result is null || result.Permissions is null)
                    {
                        return false;
                    }

                    List<Client.Core.Models.Permissions.PersonPermission> byDescription =
                        [.. result.Permissions.Where(p => p.Description == description)];

                    bool hasRead = byDescription.Any(x => x.PermissionScope == PersonPermissionType.InvoiceRead);
                    bool hasWrite = byDescription.Any(x => x.PermissionScope == PersonPermissionType.InvoiceWrite);

                    return byDescription.Count > 0 && hasRead && hasWrite;
                },
                delay: TimeSpan.FromMilliseconds(SleepTime),
                maxAttempts: 60,
                cancellationToken: CancellationToken);

        // Assert: upewnij się, że uprawnienia są widoczne i zawierają oczekiwane zakresy
        Assert.NotNull(searchAfterGrant);
        Assert.NotEmpty(searchAfterGrant.Permissions);
		Assert.All(searchAfterGrant.Permissions, permission =>
        {
            Assert.False(string.IsNullOrEmpty(permission.Id));

            Assert.NotNull(permission.AuthorizedIdentifier);
			Assert.Equal(PersonPermissionAuthorizedIdentifierType.Pesel, permission.AuthorizedIdentifier.Type); 
            Assert.False(string.IsNullOrEmpty(permission.AuthorizedIdentifier.Value));

			Assert.NotNull(permission.AuthorIdentifier);
			Assert.Equal(AuthorIdentifierType.Nip, permission.AuthorIdentifier.Type);
			Assert.False(string.IsNullOrEmpty(permission.AuthorIdentifier.Value));

			Assert.False(string.IsNullOrEmpty(permission.Description));
			Assert.Equal(PersonPermissionState.Active, permission.PermissionState);
			Assert.NotEqual(default, permission.StartDate);
		});

		List<Client.Core.Models.Permissions.PersonPermission> grantedNow =
            [.. searchAfterGrant.Permissions.Where(p => p.Description == description)];

        Assert.NotEmpty(grantedNow);
        Assert.Contains(grantedNow, x => x.PermissionScope == PersonPermissionType.InvoiceRead);
        Assert.Contains(grantedNow, x => x.PermissionScope == PersonPermissionType.InvoiceWrite);

        // Act: cofnij nadane uprawnienia
        List<PermissionsOperationStatusResponse> revokeResult = await RevokePermissionsAsync(searchAfterGrant.Permissions, accessToken);

        // Assert: weryfikacja wyników cofania
        Assert.NotNull(revokeResult);
        Assert.NotEmpty(revokeResult);
        Assert.Equal(searchAfterGrant.Permissions.Count, revokeResult.Count);
        Assert.All(revokeResult, r =>
            Assert.True(r.Status.Code == OperationStatusCodeResponse.Success,
                $"Operacja cofnięcia uprawnień nie powiodła się: {r.Status.Description}, szczegóły: [{string.Join(",", r.Status.Details ?? Array.Empty<string>())}]")
        );

        // Act: odpytywanie do momentu, aż uprawnienia o danym opisie znikną
        PagedPermissionsResponse<Client.Core.Models.Permissions.PersonPermission> searchAfterRevoke =
            await AsyncPollingUtils.PollAsync(
                async () => await SearchGrantedPersonPermissionsAsync(accessToken).ConfigureAwait(false),
                result =>
                {
                    if (result is null || result.Permissions is null)
                    {
                        return false;
                    }

                    List<Client.Core.Models.Permissions.PersonPermission> remainingLocal =
                        [.. result.Permissions.Where(p => p.Description == description)];

                    return remainingLocal.Count == 0;
                },
                delay: TimeSpan.FromMilliseconds(SleepTime),
                maxAttempts: 60,
                cancellationToken: CancellationToken);

        // Assert: upewnij się, że nie pozostały wpisy z danym opisem
        Assert.NotNull(searchAfterRevoke);

        List<Client.Core.Models.Permissions.PersonPermission> remaining =
            [.. searchAfterRevoke.Permissions.Where(p => p.Description == description)];

        Assert.Empty(remaining);
    }

    /// <summary>
    /// Nadaje uprawnienia osobie i zwraca odpowiedź operacji.
    /// </summary>
    private async Task<OperationResponse> GrantPersonPermissionsAsync(
        GrantPermissionsPersonSubjectIdentifier subject,
        string description,
        string accessToken)
    {
        PersonPermissionSubjectDetails subjectDetails = new PersonPermissionSubjectDetails
        {
            SubjectDetailsType = PersonPermissionSubjectDetailsType.PersonByIdentifier,
            PersonById = new PersonPermissionPersonById
            {
                FirstName = "Anna",
                LastName = "Testowa"
            }
        };

        // Arrange: zbudowanie żądania nadania uprawnień
        GrantPermissionsPersonRequest request = GrantPersonPermissionsRequestBuilder
            .Create()
            .WithSubject(subject)
            .WithPermissions(
                PersonPermissionType.InvoiceRead,
                PersonPermissionType.InvoiceWrite)
            .WithDescription(description)
            .WithSubjectDetails(subjectDetails)
            .Build();

        // Act: wywołanie API nadawania uprawnień
        OperationResponse response =
            await KsefClient.GrantsPermissionPersonAsync(request, accessToken, CancellationToken).ConfigureAwait(false);

        // Assert: zwrócenie odpowiedzi (asercje są wykonywane w teście)
        return response;
    }

    /// <summary>
    /// Wyszukuje uprawnienia nadane osobom i zwraca wynik wyszukiwania.
    /// </summary>
    private async Task<PagedPermissionsResponse<Client.Core.Models.Permissions.PersonPermission>>
        SearchGrantedPersonPermissionsAsync(string accessToken)
    {
        // Arrange: budowa zapytania wyszukującego uprawnienia
        PersonPermissionsQueryRequest query = new()
        {
            PermissionTypes =
            [
                PersonPermissionType.InvoiceRead,
                PersonPermissionType.InvoiceWrite
            ]
        };

        // Act: wywołanie API wyszukiwania
        PagedPermissionsResponse<Client.Core.Models.Permissions.PersonPermission> response =
            await KsefClient.SearchGrantedPersonPermissionsAsync(query, accessToken, pageOffset: 0, pageSize: 10, CancellationToken).ConfigureAwait(false);

        // Assert: zwrócenie wyniku wyszukiwania
        return response;
    }

    /// <summary>
    /// Cofnięcie wszystkich przekazanych uprawnień i zwrócenie statusów operacji.
    /// </summary>
    private async Task<List<PermissionsOperationStatusResponse>> RevokePermissionsAsync(
        IEnumerable<Client.Core.Models.Permissions.PersonPermission> grantedPermissions,
        string accessToken)
    {
        // Arrange: lista odpowiedzi z operacji cofania
        List<OperationResponse> revokeResponses = [];

        // Act: uruchomienie operacji cofania każdej pozycji
        foreach (Client.Core.Models.Permissions.PersonPermission permission in grantedPermissions)
        {
            OperationResponse response = await KsefClient.RevokeCommonPermissionAsync(permission.Id, accessToken, CancellationToken.None).ConfigureAwait(false);
            revokeResponses.Add(response);
        }

        // Act: odpytywanie statusów do skutku (sukces) i zebranie wyników
        List<PermissionsOperationStatusResponse> statuses = [];

        foreach (OperationResponse revokeResponse in revokeResponses)
        {
            PermissionsOperationStatusResponse status =
                await AsyncPollingUtils.PollAsync(
                    async () => await KsefClient.OperationsStatusAsync(revokeResponse.ReferenceNumber, accessToken).ConfigureAwait(false),
                    result => result is not null && result.Status is not null && result.Status.Code == OperationStatusCodeResponse.Success,
                    delay: TimeSpan.FromMilliseconds(SleepTime),
                    maxAttempts: 60,
                    cancellationToken: CancellationToken).ConfigureAwait(false);

            statuses.Add(status);
        }

        // Assert: zwrócenie statusów (asercje w teście nadrzędnym)
        return statuses;
    }
}