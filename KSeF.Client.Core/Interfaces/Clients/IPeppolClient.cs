using KSeF.Client.Core.Exceptions;
using KSeF.Client.Core.Models.Peppol;
using System.Threading;
using System.Threading.Tasks;

namespace KSeF.Client.Core.Interfaces.Clients
{
    /// <summary>
    /// Klient do zarządzania dostawcami usług Peppol.
    /// </summary>
    public interface IPeppolClient
    {
        /// <summary>
        /// Pobranie listy dostawców usług Peppol.
        /// </summary>
        /// <param name="accessToken">Bearer access token.</param>
        /// <param name="pageOffset">Numer strony wyników (opcjonalny).</param>
        /// <param name="pageSize">Rozmiar strony wyników (opcjonalny).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns><see cref="QueryPeppolProvidersResponse"/></returns>
        /// <exception cref="KsefApiException">Nieprawidłowe żądanie. (400 Bad request)</exception>
        /// <exception cref="KsefApiException">Brak autoryzacji. (401 Unauthorized)</exception>
        Task<QueryPeppolProvidersResponse> QueryPeppolProvidersAsync(
            string accessToken,
            int? pageOffset = null,
            int? pageSize = null,
            CancellationToken cancellationToken = default);
    }
}
