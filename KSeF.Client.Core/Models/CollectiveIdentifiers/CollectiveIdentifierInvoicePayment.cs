using KSeF.Client.Core.Models.Invoices.Common;

namespace KSeF.Client.Core.Models.CollectiveIdentifiers
{
    public class CollectiveIdentifierInvoicePayment
    {
        public double Amount { get; set; }
        public CurrencyCode Currency { get; set; }
    }
}
