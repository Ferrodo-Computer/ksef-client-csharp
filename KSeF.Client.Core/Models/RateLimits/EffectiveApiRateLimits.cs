namespace KSeF.Client.Core.Models.RateLimits
{
    /// <summary>
    /// Aktualnie obowiązujące limity ilości żądań przesyłanych do API.
    /// </summary>
    public class EffectiveApiRateLimits
    {
        /// <summary>
        /// Limity dla otwierania/zamykania sesji interaktywnych.
        /// </summary>
        public EffectiveApiRateLimitValues OnlineSession { get; set; }

        /// <summary>
        /// Limity dla otwierania/zamykania sesji wsadowych.
        /// </summary>
        public EffectiveApiRateLimitValues BatchSession { get; set; }

        /// <summary>
        /// Limity dla wysyłki faktur.
        /// </summary>
        public EffectiveApiRateLimitValues InvoiceSend { get; set; }

        /// <summary>
        /// Limity dla pobierania statusu faktury z sesji.
        /// </summary>
        public EffectiveApiRateLimitValues InvoiceStatus { get; set; }

        /// <summary>
        /// Limity dla pobierania listy sesji.
        /// </summary>
        public EffectiveApiRateLimitValues SessionList { get; set; }

        /// <summary>
        /// Limity dla pobierania listy faktur w sesji.
        /// </summary>
        public EffectiveApiRateLimitValues SessionInvoiceList { get; set; }

        /// <summary>
        /// Limity dla pozostałych operacji w ramach sesji.
        /// </summary>
        public EffectiveApiRateLimitValues SessionMisc { get; set; }

        /// <summary>
        /// Limity dla pobierania metadanych faktur.
        /// </summary>
        public EffectiveApiRateLimitValues InvoiceMetadata { get; set; }

        /// <summary>
        /// Limity dla eksportu paczki faktur.
        /// </summary>
        public EffectiveApiRateLimitValues InvoiceExport { get; set; }

        /// <summary>
        /// Limity dla pobierania faktur po numerze KSeF.
        /// </summary>
        public EffectiveApiRateLimitValues InvoiceDownload { get; set; }

        /// <summary>
        /// Limity dla pozostałych operacji API.
        /// </summary>
        public EffectiveApiRateLimitValues Other { get; set; }
    }
}
