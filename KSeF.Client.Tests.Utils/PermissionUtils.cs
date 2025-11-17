using KSeF.Client.Api.Builders.IndirectEntityPermissions;
using KSeF.Client.Api.Builders.PersonPermissions;
using KSeF.Client.Core.Interfaces.Clients;
using KSeF.Client.Core.Models;
using KSeF.Client.Core.Models.Permissions;
using KSeF.Client.Core.Models.Permissions.Identifiers;
using KSeF.Client.Core.Models.Permissions.IndirectEntity;
using KSeF.Client.Core.Models.Permissions.Person;

namespace KSeF.Client.Tests.Utils;

/// <summary>
/// Zestaw metod pomocniczych do zarządzania uprawnieniami w systemie KSeF.
/// </summary>
public static class PermissionsUtils
{
    /// <summary>
    /// Wyszukuje przyznane uprawnienia osoby.
    /// </summary>
    /// <param name="ksefClient">Klient KSeF.</param>
    /// <param name="accessToken">Token dostępu autoryzujący zapytanie.</param>
    /// <param name="queryType">Rodzaj zapytania (np. uprawnienia bież).</param>
    /// <param name="state">Stan uprawnienia (aktywne, nieaktywne).</param>
    /// <param name="pageOffset">Indeks strony (offset).</param>
    /// <param name="pageSize">Rozmiar strony wyników.</param>
    /// <returns>Lista uprawnień osoby.</returns>
    public static async Task<IReadOnlyList<PersonPermission>> SearchPersonPermissionsAsync(
        IKSeFClient ksefClient,
        string accessToken,
        PersonQueryType queryType,
        PersonPermissionState state,
        int pageOffset = 0, int pageSize = 10)
    {
        PersonPermissionsQueryRequest query = new PersonPermissionsQueryRequest
        {
            QueryType = queryType,
            PermissionState = state
        };

        PagedPermissionsResponse<PersonPermission> searchResult = await ksefClient.SearchGrantedPersonPermissionsAsync(query, accessToken, pageOffset: pageOffset, pageSize: pageSize);
        return searchResult?.Permissions ?? [];
    }

    /// <summary>
    /// Pobiera status operacji związanej z uprawnieniami.
    /// </summary>
    /// <param name="ksefClient">Klient KSeF.</param>
    /// <param name="operationReferenceNumber">Numer referencyjny operacji.</param>
    /// <param name="accessToken">Token dostępu.</param>
    /// <returns>Odpowiedź ze statusem operacji.</returns>
    public static async Task<PermissionsOperationStatusResponse> GetPermissionsOperationStatusAsync(
        IKSeFClient ksefClient, string operationReferenceNumber, string accessToken)
        => await ksefClient.OperationsStatusAsync(operationReferenceNumber, accessToken);

    /// <summary>
    /// Wycofuje (odwołuje) istniejące uprawnienie osoby.
    /// </summary>
    /// <param name="ksefClient">Klient KSeF.</param>
    /// <param name="accessToken">Token dostępu.</param>
    /// <param name="permissionId">Identyfikator uprawnienia do odwołania.</param>
    /// <returns>Odpowiedź operacji.</returns>
    public static async Task<OperationResponse> RevokePersonPermissionAsync(
        IKSeFClient ksefClient, string accessToken, string permissionId)
        => await ksefClient.RevokeCommonPermissionAsync(permissionId, accessToken);

    /// <summary>
    /// Nadaje osobie wskazane uprawnienia.
    /// </summary>
    /// <param name="client">Klient KSeF.</param>
    /// <param name="accessToken">Token dostępu.</param>
    /// <param name="subject">Podmiot (np. NIP, PESEL).</param>
    /// <param name="permissions">Tablica uprawnień do nadania.</param>
    /// <param name="description">Opcjonalny opis operacji.</param>
    /// <returns>Odpowiedź operacji.</returns>
    public static async Task<OperationResponse> GrantPersonPermissionsAsync(
        IKSeFClient client,
        string accessToken,
        GrantPermissionsPersonSubjectIdentifier subject,
        PersonPermissionType[] permissions,
        string? description = null)
    {
        GrantPermissionsPersonRequest request = GrantPersonPermissionsRequestBuilder
            .Create()
            .WithSubject(subject)
            .WithPermissions(permissions)
            .WithDescription(description ?? $"Grant {string.Join(", ", permissions)} to {subject.Type}:{subject.Value}")
            .Build();

        return await client.GrantsPermissionPersonAsync(request, accessToken);
    }

    /// <summary>
    /// Nadaje uprawnienia w kontekście innego podmiotu (uprawnienia pośrednie).
    /// </summary>
    /// <param name="client">Klient KSeF.</param>
    /// <param name="accessToken">Token dostępu.</param>
    /// <param name="subject">Identyfikator osoby.</param>
    /// <param name="context">Identyfikator podmiotu (np. NIP właściciela).</param>
    /// <param name="permissions">Tablica uprawnień do nadania.</param>
    /// <param name="description">Opcjonalny opis operacji.</param>
    /// <returns>Odpowiedź operacji.</returns>
    public static async Task<OperationResponse> GrantIndirectPermissionsAsync(
        IKSeFClient client,
        string accessToken,
        IndirectEntitySubjectIdentifier subject,
        IndirectEntityTargetIdentifier context,
        IndirectEntityStandardPermissionType[] permissions,
        string? description = null)
    {
        GrantPermissionsIndirectEntityRequest request = GrantIndirectEntityPermissionsRequestBuilder
            .Create()
            .WithSubject(subject)
            .WithContext(context)
            .WithPermissions(permissions)
            .WithDescription(description ?? $"Grant {string.Join(", ", permissions)} to {subject.Type}:{subject.Value} @ {context.Value}")
            .Build();

        return await client.GrantsPermissionIndirectEntityAsync(request, accessToken);
    }

    /// <summary>
    /// Wyszukuje aktywne uprawnienia w bieżącym kontekście.
    /// </summary>
    /// <param name="client">Klient KSeF.</param>
    /// <param name="accessToken">Token dostępu.</param>
    /// <param name="state">Stan uprawnienia.</param>
    /// <returns>Lista uprawnień osoby.</returns>a
    public static async Task<IReadOnlyList<PersonPermission>> SearchPersonPermissionsAsync(
        IKSeFClient client, string accessToken, PersonPermissionState state)
        => await SearchPersonPermissionsAsync(client, accessToken, PersonQueryType.PermissionsGrantedInCurrentContext, state);

    /// <summary>
    /// Sprawdza, czy operacja zakończyła się sukcesem, oczekując na wynik jej statusu.
    /// </summary>
    /// <param name="client">Klient KSeF.</param>
    /// <param name="operationResponse">Odpowiedź operacji do sprawdzenia.</param>
    /// <param name="accessToken">Token dostępu.</param>
    /// <returns>true, jeśli status operacji wskazuje powodzenie.</returns>
    public static async Task<bool> ConfirmOperationSuccessAsync(
        IKSeFClient client, OperationResponse operationResponse, string accessToken)
    {
        if (string.IsNullOrWhiteSpace(operationResponse?.ReferenceNumber))
            return false;

        await Task.Delay(2000);

        PermissionsOperationStatusResponse status = await GetPermissionsOperationStatusAsync(client, operationResponse.ReferenceNumber!, accessToken);
        return status?.Status?.Code == 200;
    }
}
