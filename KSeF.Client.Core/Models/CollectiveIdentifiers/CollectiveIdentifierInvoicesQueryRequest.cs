using System.Collections.Generic;

namespace KSeF.Client.Core.Models.CollectiveIdentifiers
{
    public class CollectiveIdentifierInvoicesQueryRequest
    {
        /// <summary>
        /// Numery identyfikatorów zbiorczych. Maksymalna liczba to 10.
        /// </summary>
        public ICollection<string> CollectiveIdentifierNumbers { get; set; }
    }
}
