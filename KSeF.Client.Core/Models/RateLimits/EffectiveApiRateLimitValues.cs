namespace KSeF.Client.Core.Models.RateLimits
{
    /// <summary>
    /// Wartości limitów wyrażone w sekundach, minutach oraz godzinach.
    /// </summary>
    public class EffectiveApiRateLimitValues
    {
        /// <summary>
        /// Ilość żądań na sekundę.
        /// </summary>
        public int PerSecond { get; set; }

        /// <summary>
        /// Ilość żądań na minutę.
        /// </summary>
        public int PerMinute { get; set; }

        /// <summary>
        /// Ilość żądań na godzinę.
        /// </summary>
        public int PerHour { get; set; }
    }
}
