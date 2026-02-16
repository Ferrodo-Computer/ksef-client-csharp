
using System;

namespace KSeF.Client.Core.Models.Sessions
{
    public class Session
    {
        public string ReferenceNumber { get; set; }
        public OperationStatusInfo Status { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public DateTimeOffset DateUpdated { get; set; }
        public DateTimeOffset ValidUntil { get; set; }
        public int TotalInvoiceCount { get; set; }
        public int SuccessfulInvoiceCount { get; set; }
        public int FailedInvoiceCount { get; set; }
    }
}
