using KSeF.Client.Core.Exceptions;
using KSeF.Client.Core.Models.Sessions.BatchSession;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KSeF.Client.Core.Interfaces.Clients
{
    /// <summary>
    /// Klient odpowiedzialny za operacje związane z sesją wsadową.
    /// </summary>
    public interface IBatchSessionClient
    {
		/// <summary>
		/// Otwarcie sesji wsadowej
		/// </summary>
		/// <remarks>
		/// Otwiera sesję do wysyłki wsadowej faktur.
		/// </remarks>
		/// <param name="requestPayload"><see cref="OpenBatchSessionRequest"/>schemat wysyłanych faktur, informacje o paczce faktur oraz informacje o kluczu używanym do szyfrowania.</param>
		/// <param name="accessToken">Access token</param>
		/// <param name="upoVersion">
		/// Opcjonalna wersja formatu UPO. Dostępne wartości: "upo-v4-3". 
		/// Generuje nagłówek X-KSeF-Feature z odpowiednią wartością. 
		/// Domyślnie: v4-2 (v4-3 od 05.01.2026).
		/// </param>		
		/// /// <param name="cancellationToken">Cancellation token</param>
		/// <returns><see cref="OpenBatchSessionResponse"/></returns>
		/// <exception cref="KsefApiException">Nieprawidłowe żądanie. (400 Bad request)</exception>
		/// <exception cref="KsefApiException">Brak autoryzacji. (401 Unauthorized)</exception>
		Task<OpenBatchSessionResponse> OpenBatchSessionAsync(OpenBatchSessionRequest requestPayload, string accessToken, string upoVersion = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Zamknięcie sesji wsadowej.
        /// </summary>
        /// <remarks>
        /// Zamyka sesję wsadową, rozpoczyna procesowanie paczki faktur i generowanie UPO dla prawidłowych faktur oraz zbiorczego UPO dla sesji.
        /// </remarks>
        /// <param name="batchSessionReferenceNumber">Numer referencyjny sesji wsadowej.</param>
        /// <param name="accessToken">Access token.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <exception cref="KsefApiException">Nieprawidłowe żądanie. (400 Bad request)</exception>
        /// <exception cref="KsefApiException">Brak autoryzacji. (401 Unauthorized)</exception>
        Task CloseBatchSessionAsync(string batchSessionReferenceNumber, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Wysłanie części paczki faktur.
        /// </summary>
        /// <param name="openBatchSessionResponse"><see cref="OpenBatchSessionResponse"/></param>
        /// <param name="parts">Kolekcja trzymająca informacje o partach</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="AggregateException"></exception>
        Task SendBatchPartsAsync(OpenBatchSessionResponse openBatchSessionResponse, ICollection<BatchPartSendingInfo> parts, CancellationToken cancellationToken = default);

        /// <summary>
        /// Wysłanie części paczki faktur z wykorzystaniem strumienia.
        /// </summary>
        /// <param name="openBatchSessionResponse"><see cref="OpenBatchSessionResponse"/></param>
        /// <param name="parts">Kolekcja trzymająca informacje o partach</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="AggregateException"></exception>
        Task SendBatchPartsWithStreamAsync(OpenBatchSessionResponse openBatchSessionResponse, ICollection<BatchPartStreamSendingInfo> parts, CancellationToken cancellationToken = default);
    }
}
