using System.Collections.Generic;

namespace KSeF.Client.Core.Models
{
    public class StatusInfo
    {
        public int Code { get; set; }
        public string Description { get; set; }
        public ICollection<string> Details { get; set; }
    }
}