namespace KSeF.Client.Core.Models.Lighthouse
{
    /// <summary>
    /// Statusy systemu KSeF zwracane przez Latarnię.
    /// </summary>
    public static class KsefStatus
    {
        /// <summary>
        /// Pełna dostępność.
        /// </summary>
        public const string Available = "AVAILABLE";

        /// <summary>
        /// Trwająca planowana niedostępność.
        /// </summary>
        public const string Maintenance = "MAINTENANCE";

        /// <summary>
        /// Trwająca awaria.
        /// </summary>
        public const string Failure = "FAILURE";

        /// <summary>
        /// Trwająca awaria całkowita.
        /// </summary>
        public const string TotalFailure = "TOTAL_FAILURE";
    }
}
