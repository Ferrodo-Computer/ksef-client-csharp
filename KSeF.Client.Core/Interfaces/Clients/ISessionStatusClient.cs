using KSeF.Client.Core.Exceptions;
using KSeF.Client.Core.Models.Sessions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace KSeF.Client.Core.Interfaces.Clients
{
    /// <summary>
    /// Klient do obsługi operacji związanych ze statusami sesji oraz pobieraniem UPO.
    /// </summary>
    public interface ISessionStatusClient
    {
		/// <summary>
		/// Zwraca listę sesji spełniających podane kryteria wyszukiwania.
		/// </summary>
		/// <param name="sessionType">Rodzaj sesji</param>
		/// <param name="accessToken">Access token</param>
		/// <param name="pageSize">Rozmiar strony wyników.</param>
		/// <param name="continuationToken">Token kontynuacji, jeśli jest dostępny.</param>
		/// <param name="sessionsFilter">Filtry sesji</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		/// <returns><see cref="SessionsListResponse"/></returns>
		/// <exception cref="KsefApiException">Nieprawidłowe żądanie. (400 Bad request)</exception>
		/// <exception cref="KsefApiException">Brak autoryzacji. (401 Unauthorized)</exception>
		Task<SessionsListResponse> GetSessionsAsync(SessionType sessionType, string accessToken, int? pageSize, string continuationToken, SessionsFilter sessionsFilter = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Pobranie statusu sesji
        /// </summary>
        /// <remarks>
        /// Zwraca informacje o statusie sesji.
        /// </remarks>
        /// <param name="sessionReferenceNumber">Numer referencyjny sesji</param>
        /// <param name="accessToken">Access token</param>
        /// <param name="cancellationToken">Cancellation token.</param>"
        /// <returns><see cref="SessionStatusResponse"/></returns>
        /// <exception cref="KsefApiException">Nieprawidłowe żądanie. (400 Bad request)</exception>
        /// <exception cref="KsefApiException">Brak autoryzacji. (401 Unauthorized)</exception>
        Task<SessionStatusResponse> GetSessionStatusAsync(string sessionReferenceNumber, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Pobranie statusów faktur sesji
        /// </summary>
        /// <remarks>
        /// Zwraca listę faktur przesłanych w sesji wraz z ich statusami, oraz informacje na temat ilości poprawnie i niepoprawnie przetworzonych faktur.
        /// </remarks>
        /// <param name="sessionReferenceNumber">Numer referencyjny sesji</param>
        /// <param name="accessToken">Access token.</param>
        /// <param name="pageSize">Rozmiar strony wyników.</param>
        /// <param name="continuationToken">Token kontynuacji, jeśli jest dostępny.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns><see cref="SessionInvoicesResponse"/></returns>
        /// <exception cref="KsefApiException">Nieprawidłowe żądanie. (400 Bad request)</exception>
        /// <exception cref="KsefApiException">Brak autoryzacji. (401 Unauthorized)</exception>
        /// <exception cref="KsefApiException">Brak uprawnień. (403 Forbidden)</exception>
        Task<SessionInvoicesResponse> GetSessionInvoicesAsync(string sessionReferenceNumber, string accessToken, int? pageSize = null, string continuationToken = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Pobranie statusu faktury z sesji
        /// </summary>
        /// <remarks>Zwraca fakturę przesłaną w sesji wraz ze statusem.</remarks>
        /// <param name="sessionReferenceNumber">Numer referencyjny sesji.</param>
        /// <param name="invoiceReferenceNumber">Numer referencyjny faktury.</param>
        /// <param name="accessToken">Access token.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns><see cref="SessionInvoice"/></returns>
        /// <exception cref="KsefApiException">Nieprawidłowe żądanie. (400 Bad request)</exception>
        /// <exception cref="KsefApiException">Brak autoryzacji. (401 Unauthorized)</exception>
        Task<SessionInvoice> GetSessionInvoiceAsync(string sessionReferenceNumber, string invoiceReferenceNumber, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Pobranie niepoprawnie przetworzonych dokumentów sesji
        /// </summary>
        /// <remarks>
        /// Zwraca listę niepoprawnie przetworzonych dokumentów przesłanych w sesji wraz z ich statusami.
        /// </remarks>
        /// <param name="sessionReferenceNumber">Numer referencyjny sesji</param>
        /// <param name="accessToken">Access token</param>
        /// <param name="pageSize">Rozmiar strony wyników.</param>
        /// <param name="continuationToken">Token kontynuacji, jeśli jest dostępny.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns><see cref="SessionInvoicesResponse"/></returns>
        /// <exception cref="KsefApiException">Nieprawidłowe żądanie. (400 Bad request)</exception>
        /// <exception cref="KsefApiException">Brak autoryzacji. (401 Unauthorized)</exception>
        Task<SessionInvoicesResponse> GetSessionFailedInvoicesAsync(string sessionReferenceNumber, string accessToken, int? pageSize, string continuationToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Pobranie UPO faktury z sesji na podstawie numeru KSeF
        /// </summary>
        /// <remarks>
        /// Zwraca UPO faktury przesłanej w sesji na podstawie jego numeru KSeF.
        /// </remarks>
        /// <param name="sessionReferenceNumber">Numer referencyjny sesji</param>
        /// <param name="ksefNumber">Numer KSeF faktury</param>
        /// <param name="accessToken">Access token</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>UPO w formie XML</returns>
        /// <exception cref="KsefApiException">Nieprawidłowe żądanie. (400 Bad request)</exception>
        /// <exception cref="KsefApiException">Brak autoryzacji. (401 Unauthorized)</exception>
        Task<string> GetSessionInvoiceUpoByKsefNumberAsync(string sessionReferenceNumber, string ksefNumber, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Pobranie UPO faktury z sesji na podstawie numeru referencyjnego faktury.
        /// </summary>
        /// <remarks>
        /// Zwraca UPO faktury przesłanego w sesji na podstawie jego numeru KSeF.
        /// </remarks>
        /// <param name="sessionReferenceNumber">Numer referencyjny sesji</param>
        /// <param name="invoiceReferenceNumber">Numer referencyjny faktury</param>
        /// <param name="accessToken">Access token</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>UPO w formie XML</returns>
        /// <exception cref="KsefApiException">Nieprawidłowe żądanie. (400 Bad request)</exception>
        /// <exception cref="KsefApiException">Brak autoryzacji. (401 Unauthorized)</exception>
        Task<string> GetSessionInvoiceUpoByReferenceNumberAsync(string sessionReferenceNumber, string invoiceReferenceNumber, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Pobranie UPO sesji
        /// </summary>
        /// <remarks>
        /// Zwraca XML zawierający zbiorcze UPO sesji.
        /// </remarks>
        /// <param name="sessionReferenceNumber">Numer referencyjny sesji</param>
        /// <param name="upoReferenceNumber">Numer referencyjny UPO</param>
        /// <param name="accessToken">Access token</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Zbiorcze UPO w formie XML</returns>
        /// <exception cref="KsefApiException">Nieprawidłowe żądanie. (400 Bad request)</exception>
        /// <exception cref="KsefApiException">Brak autoryzacji. (401 Unauthorized)</exception>
        Task<string> GetSessionUpoAsync(string sessionReferenceNumber, string upoReferenceNumber, string accessToken, CancellationToken cancellationToken = default);

		/// <summary>
		/// Pobiera UPO z adresu Uri.
		/// </summary>
		/// <param name="uri">Adres Uri.</param>
		/// <param name="cancellationToken">Cancellation token.</param>
		Task<string> GetUpoAsync(Uri uri, CancellationToken cancellationToken = default);
    }
}
