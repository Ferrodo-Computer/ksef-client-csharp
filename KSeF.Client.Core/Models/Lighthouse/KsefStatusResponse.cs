using System.Collections.Generic;

namespace KSeF.Client.Core.Models.Lighthouse
{
    /// <summary>
    /// Odpowiedź statusu systemu KSeF zwracana przez Latarnię.
    /// </summary>
    public sealed class KsefStatusResponse
    {
        /// <summary>
        /// Status systemu KSeF.
        /// Możliwe wartości: AVAILABLE, MAINTENANCE, FAILURE, TOTAL_FAILURE
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Wiadomości dotyczące statusu systemu KSeF.
        /// </summary>
        public List<Message> Messages { get; set; }
    }
}
