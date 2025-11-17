
namespace KSeF.Client.Core.Models.RateLimits
{
    /// <summary>
    /// Limity żądań przesyłanych do API.
    /// </summary>
    public class EffectiveApiRateLimitsRequest
    {
        /// <summary>
        /// Lista limitów żądań API.
        /// </summary>
        public EffectiveApiRateLimits RateLimits { get; set; }
    }
}
