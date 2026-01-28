using System.Collections.Generic;

namespace KSeF.Client.Core.Models
{
    public class InvoiceStatusInfo
    {
        public int Code { get; set; }
        public string Description { get; set; }
        public ICollection<string> Details { get; set; }
        public IDictionary<string, string> Extensions { get; set; }
    }
}