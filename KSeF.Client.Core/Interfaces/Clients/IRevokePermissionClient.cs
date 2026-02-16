using KSeF.Client.Core.Exceptions;
using KSeF.Client.Core.Models;
using System.Threading;
using System.Threading.Tasks;

namespace KSeF.Client.Core.Interfaces.Clients
{
    /// <summary>
    /// Klient służący do cofania uprawnień.
    /// </summary>
    public interface IRevokePermissionClient
    {
        /// <summary>
        /// Rozpoczyna asynchroniczną operację odbierania uprawnienia o podanym identyfikatorze.
        /// </summary>
        /// <param name="permissionId">Id uprawnienia.</param>
        /// <param name="accessToken">Token dostępu.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns><see cref="OperationResponse"/></returns>
        /// <exception cref="KsefApiException">Nieprawidłowe żądanie. (400 Bad request)</exception>
        /// <exception cref="KsefApiException">Brak autoryzacji. (401 Unauthorized)</exception>
        Task<OperationResponse> RevokeCommonPermissionAsync(string permissionId, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Rozpoczyna asynchroniczną operacje odbierania uprawnienia o podanym identyfikatorze. 
        /// Ta metoda służy do odbierania uprawnień o charakterze upoważnień.
        /// </summary>
        /// <param name="permissionId">Id uprawnienia.</param>
        /// <param name="accessToken">Token dostępu.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns><see cref="OperationResponse"/></returns>
        /// <exception cref="KsefApiException">Nieprawidłowe żądanie. (400 Bad request)</exception>
        /// <exception cref="KsefApiException">Brak autoryzacji. (401 Unauthorized)</exception>
        Task<OperationResponse> RevokeAuthorizationsPermissionAsync(string permissionId, string accessToken, CancellationToken cancellationToken = default);

    }
}
