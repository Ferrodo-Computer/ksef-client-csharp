namespace KSeF.Client.Core.Models.Invoices
{
    public class BuyerIdentifier
    {
        public BuyerIdentifierType Type { get; set; }
        public string Value { get; set; }
    }
    public enum BuyerIdentifierType
    {
        None,
        Other,
        Nip,
        VatUe,
    }
}
