namespace KSeF.Client.Core.Models.RateLimits
{
    /// <summary>
    /// Aktualnie obowiązujące limity ilości żądań przesyłanych do API.
    /// </summary>
    public class EffectiveApiRateLimits
    {
        /// <summary>
        /// Limity otwierania sesji interaktywnych.
        /// </summary>
        public EffectiveApiRateLimitValues OnlineSession { get; set; }

        /// <summary>
        /// Limity zamykania sesji interaktywnych.
        /// </summary>
        public EffectiveApiRateLimitValues OnlineSessionClose { get; set; }

        /// <summary>
        /// Limity otwierania sesji wsadowych.
        /// </summary>
        public EffectiveApiRateLimitValues BatchSession { get; set; }

        /// <summary>
        /// Limity zamykania sesji wsadowych.
        /// </summary>
        public EffectiveApiRateLimitValues BatchSessionClose { get; set; }

        /// <summary>
        /// Limity wysyłki faktur.
        /// </summary>
        public EffectiveApiRateLimitValues InvoiceSend { get; set; }

        /// <summary>
        /// Limity pobierania statusu faktury z sesji.
        /// </summary>
        public EffectiveApiRateLimitValues InvoiceStatus { get; set; }

        /// <summary>
        /// Limity pobierania listy sesji.
        /// </summary>
        public EffectiveApiRateLimitValues SessionList { get; set; }

        /// <summary>
        /// Limity pobierania listy faktur w sesji.
        /// </summary>
        public EffectiveApiRateLimitValues SessionInvoiceList { get; set; }

        /// <summary>
        /// Limity pozostałych operacji w ramach sesji.
        /// </summary>
        public EffectiveApiRateLimitValues SessionMisc { get; set; }

        /// <summary>
        /// Limity pobierania metadanych faktur.
        /// </summary>
        public EffectiveApiRateLimitValues InvoiceMetadata { get; set; }

        /// <summary>
        /// Limity eksportu paczki faktur.
        /// </summary>
        public EffectiveApiRateLimitValues InvoiceExport { get; set; }

        /// <summary>
        /// Limity eksportu paczki faktur.
        /// </summary>
        public EffectiveApiRateLimitValues InvoiceExportStatus { get; set; }

        /// <summary>
        /// Limity pobierania faktur po numerze KSeF.
        /// </summary>
        public EffectiveApiRateLimitValues InvoiceDownload { get; set; }

        /// <summary>
        /// Limity pozostałych operacji API.
        /// </summary>
        public EffectiveApiRateLimitValues Other { get; set; }

        /// <summary>
        /// Limity generowania identyfikatorów zbiorczych.
        /// </summary>
        public EffectiveApiRateLimitValues CollectiveIdentifier { get; set; }

        /// <summary>
        /// Limity anonimowych (nieuwierzytelnionych) operacji API.
        /// </summary>
        public EffectiveApiRateLimitValues Anonymous { get; set; }

        /// <summary>
        /// Limity globalne API naliczane per adres IP. Mechanizm może być wyłączony (wartości -1).
        /// </summary>
        public EffectiveApiRateLimitValues Global { get; set; }
    }
}
