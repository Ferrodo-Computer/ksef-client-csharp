namespace KSeF.Client.Core.Models.Lighthouse
{
    /// <summary>
    /// Typy komunikatów raportowanych przez Latarnię.
    /// </summary>
    public static class MessageType
    {
        /// <summary>
        /// Rozpoczęcie awarii.
        /// </summary>
        public const string FailureStart = "FAILURE_START";

        /// <summary>
        /// Zakończenie awarii.
        /// </summary>
        public const string FailureEnd = "FAILURE_END";

        /// <summary>
        /// Planowana niedostępność (ogłoszenie).
        /// </summary>
        public const string MaintenanceAnnouncement = "MAINTENANCE_ANNOUNCEMENT";
    }
}
