using System;

namespace KSeF.Client.Core.Models.CollectiveIdentifiers
{
    public class CollectiveIdentifiersByKsefNumberQueryResponseItem
    {
        public string CollectiveIdentifierNumber { get; set; }
        public bool CreatedInCurrentContext { get; set; }
        public DateTimeOffset DateCreated { get; set; }
    }
}
