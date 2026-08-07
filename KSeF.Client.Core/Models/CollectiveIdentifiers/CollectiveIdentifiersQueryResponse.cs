using System.Collections.Generic;

namespace KSeF.Client.Core.Models.CollectiveIdentifiers
{
    public class CollectiveIdentifiersQueryResponse
    {
        public string ContinuationToken { get; set; }
        public ICollection<CollectiveIdentifiersQueryResponseItem> CollectiveIdentifiers { get; set; }
    }
}
