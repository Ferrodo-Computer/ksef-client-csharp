namespace KSeF.Client.Core.Models.CollectiveIdentifiers
{
    public class CollectiveIdentifierInvoice
    {
        public string KsefNumber { get; set; }
        public CollectiveIdentifierInvoicePayment Payment { get; set; }
        public string Description { get; set; }
    }
}
