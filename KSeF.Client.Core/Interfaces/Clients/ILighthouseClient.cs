using System.Threading;
using System.Threading.Tasks;
using KSeF.Client.Core.Models.Lighthouse;

namespace KSeF.Client.Core.Interfaces.Clients
{
    /// <summary>
    /// Klient do odczytu statusu i komunikatów Latarni.
    /// </summary>
    public interface ILighthouseClient
    {
        /// <summary>
        /// Pobiera aktualny status systemu KSeF wraz z ewentualnymi komunikatami.
        /// </summary>
        Task<KsefStatusResponse> GetStatusAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Pobiera bieżące komunikaty Latarni.
        /// </summary>
        Task<KsefMessagesResponse> GetMessagesAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
