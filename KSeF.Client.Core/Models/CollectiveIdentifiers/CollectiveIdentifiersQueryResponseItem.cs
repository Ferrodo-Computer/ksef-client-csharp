using System;

namespace KSeF.Client.Core.Models.CollectiveIdentifiers
{
    public class CollectiveIdentifiersQueryResponseItem
    {
        public string CollectiveIdentifierNumber { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public int InvoiceCount { get; set; }
        public bool CreatedInCurrentContext { get; set; }
    }
}
