using KSeF.Client.Core.Exceptions;
using KSeF.Client.Core.Models.Certificates;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace KSeF.Client.Core.Interfaces.Clients
{
    public interface ICryptographyClient
    {
        /// <summary>
        /// Zwraca informacje o kluczach publicznych używanych do szyfrowania danych przesyłanych do systemu KSeF.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="KsefApiException">Nieprawidłowe żądanie. (400 Bad request)</exception>
        Task<ICollection<PemCertificateInfo>> GetPublicCertificatesAsync(CancellationToken cancellationToken = default);
    }
}