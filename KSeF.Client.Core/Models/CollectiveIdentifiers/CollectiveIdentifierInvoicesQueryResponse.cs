using System.Collections.Generic;

namespace KSeF.Client.Core.Models.CollectiveIdentifiers
{
    public class CollectiveIdentifierInvoicesQueryResponse
    {
        public string ContinuationToken { get; set; }
        public ICollection<CollectiveIdentifierInvoicesQueryResponseItem> Invoices { get; set; }
    }
}
