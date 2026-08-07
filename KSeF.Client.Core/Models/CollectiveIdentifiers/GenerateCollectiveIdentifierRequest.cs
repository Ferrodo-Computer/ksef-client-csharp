using System.Collections.Generic;

namespace KSeF.Client.Core.Models.CollectiveIdentifiers
{
    public class GenerateCollectiveIdentifierRequest
    {
        public ICollection<CollectiveIdentifierInvoice> Invoices { get; set; }
    }
}
