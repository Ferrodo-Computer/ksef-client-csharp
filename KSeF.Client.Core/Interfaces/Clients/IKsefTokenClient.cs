using KSeF.Client.Core.Exceptions;
using KSeF.Client.Core.Models.Authorization;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KSeF.Client.Core.Interfaces.Clients
{
    /// <summary>
    /// Klient do zarządzania tokenami KSeF.
    /// </summary>
    public interface IKsefTokenClient
    {
        /// <summary>
        /// Generuje nowy token KSeF.
        /// </summary>
        /// <param name="requestPayload"><see cref="KsefTokenRequest"/></param>
        /// <param name="accessToken">Access token</param>
        /// <param name="cancellationToken"></param>
        /// <returns><see cref="KsefTokenResponse"/></returns>
        /// <exception cref="KsefApiException">Nieprawidłowe żądanie. (400 Bad request)</exception>
        /// <exception cref="KsefApiException">Brak autoryzacji. (401 Unauthorized)</exception>
        Task<KsefTokenResponse> GenerateKsefTokenAsync(KsefTokenRequest requestPayload, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Pobranie listy wygenerowanych tokenów.
        /// </summary>
        /// <param name="accessToken">Access token.</param>
        /// <param name="statuses">Statusy tokenów do zwrócenia (można podać wielokrotnie).</param>
        /// <param name="authorIdentifier">Identyfikator uwierzytelnienia</param>
        /// <param name="authorIdentifierType">Typ identyfikatora</param>
        /// <param name="description">Opis</param>
        /// <param name="continuationToken">Continuation token.</param>
        /// <param name="pageSize">Ilość elementów na stronie (domyślnie 10)</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns><see cref="QueryKsefTokensResponse"/></returns>
        /// <exception cref="KsefApiException">Nieprawidłowe żądanie. (400 Bad request)</exception>
        /// <exception cref="KsefApiException">Brak autoryzacji. (401 Unauthorized)</exception>
        Task<QueryKsefTokensResponse> QueryKsefTokensAsync(
        string accessToken,
        ICollection<AuthenticationKsefTokenStatus> statuses = null,
        string authorIdentifier = null,
        Models.Token.TokenContextIdentifierType? authorIdentifierType = null,
        string description = null,
        string continuationToken = null,
        int? pageSize = null,
        CancellationToken cancellationToken = default);

        /// <summary>
        /// Pobranie statusu tokena
        /// </summary>
        /// <param name="tokenReferenceNumber">Numer referencyjny tokena.</param>
        /// <param name="accessToken">Access token.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns><see cref="AuthenticationKsefToken"/></returns>
        /// <exception cref="KsefApiException">Nieprawidłowe żądanie. (400 Bad request)</exception>
        /// <exception cref="KsefApiException">Brak autoryzacji. (401 Unauthorized)</exception>
        Task<AuthenticationKsefToken> GetKsefTokenAsync(string tokenReferenceNumber, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Unieważnienie tokena.
        /// </summary>
        /// <param name="tokenReferenceNumber">Numer referencyjny tokena.</param>
        /// <param name="accessToken">Access token.</param>
        /// <param name="cancellationToken">Cancellation toke.</param>
        /// <exception cref="KsefApiException">Nieprawidłowe żądanie. (400 Bad request)</exception>
        /// <exception cref="KsefApiException">Brak autoryzacji. (401 Unauthorized)</exception>
        Task RevokeKsefTokenAsync(string tokenReferenceNumber, string accessToken, CancellationToken cancellationToken = default);
    }
}
