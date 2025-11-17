using KSeF.Client.Core.Exceptions;
using KSeF.Client.Core.Models;
using KSeF.Client.Core.Models.Permissions.Authorizations;
using KSeF.Client.Core.Models.Permissions.Entity;
using KSeF.Client.Core.Models.Permissions.EUEntity;
using KSeF.Client.Core.Models.Permissions.EuEntityRepresentative;
using KSeF.Client.Core.Models.Permissions.IndirectEntity;
using KSeF.Client.Core.Models.Permissions.Person;
using KSeF.Client.Core.Models.Permissions.SubUnit;
using System.Threading;
using System.Threading.Tasks;

namespace KSeF.Client.Core.Interfaces.Clients
{
    /// <summary>
    /// Klient służący do nadawania uprawnień.
    /// </summary>
    public interface IGrantPermissionClient
    {
        /// <summary>
        /// Nadanie osobom fizycznym uprawnień do pracy w KSeF
        /// </summary>
        /// <param name="requestPayload"><see cref="GrantPermissionsPersonRequest"/></param>
        /// <param name="accessToken">Access token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns><see cref="OperationResponse"/></returns>
        /// <exception cref="KsefApiException">Nieprawidłowe żądanie. (400 Bad request)</exception>
        /// <exception cref="KsefApiException">Brak autoryzacji. (401 Unauthorized)</exception>
        Task<OperationResponse> GrantsPermissionPersonAsync(GrantPermissionsPersonRequest requestPayload, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Nadanie podmiotom uprawnień do pracy w KSeF
        /// </summary>
        /// <param name="requestPayload"><see cref="GrantPermissionsEntityRequest"/></param>
        /// <param name="accessToken">Access token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns><see cref="OperationResponse"/></returns>
        /// <exception cref="KsefApiException">Nieprawidłowe żądanie. (400 Bad request)</exception>
        /// <exception cref="KsefApiException">Brak autoryzacji. (401 Unauthorized)</exception>
        Task<OperationResponse> GrantsPermissionEntityAsync(GrantPermissionsEntityRequest requestPayload, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Rozpoczyna asynchroniczną operację nadawania uprawnień podmiotowych.
        /// </summary>
        /// <param name="requestPayload"><see cref="GrantPermissionsAuthorizationRequest"/></param>
        /// <param name="accessToken">Access token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns><see cref="OperationResponse"/></returns>
        /// <exception cref="KsefApiException">Nieprawidłowe żądanie. (400 Bad request)</exception>
        /// <exception cref="KsefApiException">Brak autoryzacji. (401 Unauthorized)</exception>
        Task<OperationResponse> GrantsAuthorizationPermissionAsync(GrantPermissionsAuthorizationRequest requestPayload, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Nadanie uprawnień w sposób pośredni
        /// </summary>
        /// <param name="requestPayload"><see cref="GrantPermissionsIndirectEntityRequest"/></param>
        /// <param name="accessToken">Access token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns><see cref="OperationResponse"/></returns>
        /// <exception cref="KsefApiException">Nieprawidłowe żądanie. (400 Bad request)</exception>
        /// <exception cref="KsefApiException">Brak autoryzacji. (401 Unauthorized)</exception>
        Task<OperationResponse> GrantsPermissionIndirectEntityAsync(GrantPermissionsIndirectEntityRequest requestPayload, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Nadanie uprawnień administratora podmiotu podrzędnego
        /// </summary>
        /// <param name="requestPayload"><see cref="GrantPermissionsSubunitRequest"/></param>
        /// <param name="accessToken">Access token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns><see cref="OperationResponse"/></returns>
        /// <exception cref="KsefApiException">Nieprawidłowe żądanie. (400 Bad request)</exception>
        /// <exception cref="KsefApiException">Brak autoryzacji. (401 Unauthorized)</exception>
        Task<OperationResponse> GrantsPermissionSubUnitAsync(GrantPermissionsSubunitRequest requestPayload, string accessToken, CancellationToken cancellationToken = default);


        /// <summary>
        /// Nadanie uprawnień administratora podmiotu unijnego
        /// </summary>
        /// <param name="requestPayload"><see cref="GrantPermissionsEuEntityRequest"/></param>
        /// <param name="accessToken">Access token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns><see cref="OperationResponse"/></returns>
        /// <exception cref="KsefApiException">Nieprawidłowe żądanie. (400 Bad request)</exception>
        /// <exception cref="KsefApiException">Brak autoryzacji. (401 Unauthorized)</exception>
        Task<OperationResponse> GrantsPermissionEUEntityAsync(GrantPermissionsEuEntityRequest requestPayload, string accessToken, CancellationToken cancellationToken = default);


        /// <summary>
        /// Nadanie uprawnień administratora podmiotu unijnego
        /// </summary>
        /// <param name="requestPayload"><see cref="GrantPermissionsEuEntityRepresentativeRequest"/></param>
        /// <param name="accessToken">Access token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns><see cref="OperationResponse"/></returns>
        /// <exception cref="KsefApiException">Nieprawidłowe żądanie. (400 Bad request)</exception>
        /// <exception cref="KsefApiException">Brak autoryzacji. (401 Unauthorized)</exception>
        Task<OperationResponse> GrantsPermissionEUEntityRepresentativeAsync(GrantPermissionsEuEntityRepresentativeRequest requestPayload, string accessToken, CancellationToken cancellationToken = default);
    }
}
