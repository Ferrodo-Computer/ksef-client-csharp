using KSeF.Client.Core.Models.CollectiveIdentifiers;
using System.Threading;
using System.Threading.Tasks;

namespace KSeF.Client.Core.Interfaces.Clients
{
    /// <summary>
    /// Operacje sekcji „Identyfikatory zbiorcze".
    /// </summary>
    public interface ICollectiveIdentifiersClient
    {
        /// <summary>
        /// POST /collective-identifiers — generowanie identyfikatora zbiorczego dla wskazanych faktur.
        /// </summary>
        Task<GenerateCollectiveIdentifierResponse> GenerateCollectiveIdentifierAsync(GenerateCollectiveIdentifierRequest request, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// POST /collective-identifiers/query — pobranie listy identyfikatorów zbiorczych wygenerowanych w kontekście.
        /// </summary>
        Task<CollectiveIdentifiersQueryResponse> QueryCollectiveIdentifiersAsync(
            CollectiveIdentifiersQueryRequest request,
            string accessToken,
            int? pageSize = null,
            string continuationToken = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// GET /collective-identifiers/ksef/{ksefNumber} — pobranie listy identyfikatorów zbiorczych po numerze KSeF faktury.
        /// </summary>
        Task<CollectiveIdentifiersByKsefNumberQueryResponse> GetCollectiveIdentifiersByKsefNumberAsync(
            string ksefNumber,
            string accessToken,
            string continuationToken = null,
            int? pageSize = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// POST /collective-identifiers/invoices — pobranie listy faktur wchodzących w skład wskazanych identyfikatorów zbiorczych (maksymalnie 10 na żądanie).
        /// </summary>
        Task<CollectiveIdentifierInvoicesQueryResponse> GetCollectiveIdentifierInvoicesAsync(
            CollectiveIdentifierInvoicesQueryRequest request,
            string accessToken,
            string continuationToken = null,
            int? pageSize = null,
            CancellationToken cancellationToken = default);
    }
}
