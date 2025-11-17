namespace KSeF.Client.Core.Models.TestData
{
    public class TestDataSessionLimitsBase
    {
        public int MaxInvoiceSizeInMB { get; set; } = 0;
        public int MaxInvoiceWithAttachmentSizeInMB { get; set; } = 0;
        public int MaxInvoices { get; set; } = 0;
    }
}
