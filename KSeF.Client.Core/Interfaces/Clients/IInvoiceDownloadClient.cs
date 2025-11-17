
using KSeF.Client.Core.Exceptions;
using KSeF.Client.Core.Models;
using KSeF.Client.Core.Models.Invoices;
using System.Threading;
using System.Threading.Tasks;

namespace KSeF.Client.Core.Interfaces.Clients
{
    /// <summary>
    /// Klient do obsługi operacji związanych z pobieraniem faktur oraz metadanych.
    /// </summary>
    public interface IInvoiceDownloadClient
    {
        /// <summary>
        /// Pobranie faktury po numerze KSeF
        /// </summary>
        /// <param name="ksefNumber">Numer KSeF faktury</param>
        /// <param name="accessToken">Access token</param>
        /// <param name="cancellationToken">Cancellation token./param>
        /// <returns>Faktura w formie XML.</returns>
        /// <exception cref="KsefApiException">Nieprawidłowe żądanie. (400 Bad request)</exception>
        /// <exception cref="KsefApiException">Brak autoryzacji. (401 Unauthorized)</exception>
        Task<string> GetInvoiceAsync(string ksefNumber, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Zwraca listę metadanych faktur spełniające podane kryteria wyszukiwania.
        /// </summary>
        /// <param name="requestPayload"><see cref="InvoiceQueryFilters"/>zestaw filtrów</param>
        /// <param name="accessToken">Access token.</param>
        /// <param name="pageOffset">Numer strony wyników.</param>
        /// <param name="pageSize">Rozmiar strony wyników.</param>
        /// <param name="cancellationToken">Cancellation token./param>
        /// <returns><see cref="PagedInvoiceResponse"/></returns>
        /// <exception cref="KsefApiException">Nieprawidłowe żądanie. (400 Bad request)</exception>
        /// <exception cref="KsefApiException">Brak autoryzacji. (401 Unauthorized)</exception>
        Task<PagedInvoiceResponse> QueryInvoiceMetadataAsync(InvoiceQueryFilters requestPayload, string accessToken, int? pageOffset = null, int? pageSize = null, SortOrder sortOrder = SortOrder.Asc, CancellationToken cancellationToken = default);

        /// <summary>
        /// Inicjuje eksport paczki faktur zgodnie z podanymi filtrami.
        /// </summary>
        /// <param name="requestPayload">Żądanie eksportu faktur (filtry + szyfrowanie).</param>
        /// <param name="accessToken">Access token.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns><see cref="OperationResponse"/> zawierający numer referencyjny operacji.</returns>
        /// <exception cref="KsefApiException">Nieprawidłowe żądanie. (400 Bad request)</exception>
        /// <exception cref="KsefApiException">Brak autoryzacji. (401 Unauthorized)</exception>
        Task<OperationResponse> ExportInvoicesAsync(
            InvoiceExportRequest requestPayload,
            string accessToken,
            CancellationToken cancellationToken = default,
            bool includeMetadata = true);

        /// <summary>
        /// Pobiera status operacji eksportu paczki faktur.
        /// </summary>
        /// <param name="referenceNumber">Numer referencyjny operacji eksportu.</param>
        /// <param name="accessToken">Access token.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns><see cref="InvoiceExportStatusResponse"/> zawierający status oraz paczkę faktur (jeśli dostępna).</returns>
        /// <exception cref="KsefApiException">Nieprawidłowe żądanie. (400 Bad request)</exception>
        /// <exception cref="KsefApiException">Brak autoryzacji. (401 Unauthorized)</exception>
        Task<InvoiceExportStatusResponse> GetInvoiceExportStatusAsync(
            string referenceNumber,
            string accessToken,
            CancellationToken cancellationToken = default);
    }
}
