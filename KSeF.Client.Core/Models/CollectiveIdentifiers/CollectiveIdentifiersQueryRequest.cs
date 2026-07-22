using System;

namespace KSeF.Client.Core.Models.CollectiveIdentifiers
{
    public class CollectiveIdentifiersQueryRequest
    {
        public string CollectiveIdentifierNumber { get; set; }
        public DateTimeOffset DateCreatedFrom { get; set; }
        public DateTimeOffset DateCreatedTo { get; set; }
        public int? InvoiceCountFrom { get; set; }
        public int? InvoiceCountTo { get; set; }
        public bool? CreatedInCurrentContext { get; set; }
    }
}
