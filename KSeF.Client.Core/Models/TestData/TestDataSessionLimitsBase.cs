using System;

namespace KSeF.Client.Core.Models.TestData
{
    [Obsolete("Klasa zostanie usunięta. Używaj klasy SessionLimits.")]
    public class TestDataSessionLimitsBase
    {
        public int MaxInvoiceSizeInMB { get; set; }
        public int MaxInvoiceWithAttachmentSizeInMB { get; set; }
        public int MaxInvoices { get; set; }
    }
}
