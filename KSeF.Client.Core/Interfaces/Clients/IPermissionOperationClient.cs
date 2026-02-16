using KSeF.Client.Core.Exceptions;
using KSeF.Client.Core.Models.Permissions;
using System.Threading;
using System.Threading.Tasks;

namespace KSeF.Client.Core.Interfaces.Clients
{
    /// <summary>
    /// Klient umożliwiający pobranie statusu operacji związanej z nadaniem lub odebraniem uprawnień oraz sprawdzenie statusu zgody na wystawianie faktury z załącznikiem.
    /// </summary>
    public interface IPermissionOperationClient
    {
        /// <summary>
        /// Pobranie statusu operacji - uprawnienia
        /// </summary>
        /// <param name="operationReferenceNumber">Numer referencyjny operacji.</param>
        /// <param name="accessToken">Access token.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns><see cref="PermissionsOperationStatusResponse"/></returns>
        /// <exception cref="KsefApiException">Nieprawidłowe żądanie. (400 Bad request)</exception>
        /// <exception cref="KsefApiException">Brak autoryzacji. (401 Unauthorized)</exception>
        Task<PermissionsOperationStatusResponse> OperationsStatusAsync(string operationReferenceNumber, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Sprawdzenie czy obecny kontekst posiada zgodę na wystawianie faktur z załącznikiem.
        /// Wymagane uprawnienia: CredentialsManage, CredentialsRead.
        /// </summary>    
        /// <param name="accessToken">Token dostępu.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns><see cref="PermissionsAttachmentAllowedResponse"/></returns>
        /// <exception cref="KsefApiException">Nieprawidłowe żądanie. (400 Bad request)</exception>
        /// <exception cref="KsefApiException">Brak autoryzacji. (401 Unauthorized)</exception>
        Task<PermissionsAttachmentAllowedResponse> GetAttachmentPermissionStatusAsync(string accessToken, CancellationToken cancellationToken = default);
    }
}
