
namespace KSeF.Client.Core.Models.Lighthouse
{
    /// <summary>
    /// Kategorie trwających zdarzeń raportowanych przez Latarnię.
    /// </summary>
    public static class MessageCategory
    {
        /// <summary>
        /// Awaria.
        /// </summary>
        public const string Failure = "FAILURE";

        /// <summary>
        /// Całkowita awaria.
        /// </summary>
        public const string TotalFailure = "TOTAL_FAILURE";

        /// <summary>
        /// Planowana niedostępność.
        /// </summary>
        public const string Maintenance = "MAINTENANCE";
    }
}
