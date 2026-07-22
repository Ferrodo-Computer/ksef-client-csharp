using System.Collections.Generic;

namespace KSeF.Client.Core.Models.CollectiveIdentifiers
{
    public class CollectiveIdentifiersByKsefNumberQueryResponse
    {
        public string ContinuationToken { get; set; }
        public ICollection<CollectiveIdentifiersByKsefNumberQueryResponseItem> CollectiveIdentifiers { get; set; }
    }
}
