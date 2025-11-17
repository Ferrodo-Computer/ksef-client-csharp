using KSeF.Client.Core.Models.RateLimits;
using KSeF.Client.Core.Models.TestData;
using System.Threading;
using System.Threading.Tasks;

namespace KSeF.Client.Core.Interfaces.Clients
{
    /// <summary>
    /// Operacje sekcji „Dane testowe” + pomocnicze „Query Grants” (weryfikacja efektów).
    /// </summary>
    public interface ILimitsClient
    {
        /// <summary>
        /// GET /api/v2/limits/context — pobiera aktualne limity dla bieżącego kontekstu.
        /// </summary>
        Task<SessionLimitsInCurrentContextResponse> GetLimitsForCurrentContextAsync(string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// GET /api/v2/limits/subject — pobiera aktualne limity dla bieżącego podmiotu.
        /// </summary>
        Task<CertificatesLimitInCurrentSubjectResponse> GetLimitsForCurrentSubjectAsync(string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// GET /api/v2/rate-limits — pobiera aktualne limity dla przesyłanych żądań do API.
        /// </summary>
        Task<EffectiveApiRateLimits> GetRateLimitsAsync(string accessToken, CancellationToken cancellationToken = default);
    }
    
}
