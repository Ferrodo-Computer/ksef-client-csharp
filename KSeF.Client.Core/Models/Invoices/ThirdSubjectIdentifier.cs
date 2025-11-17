namespace KSeF.Client.Core.Models.Invoices
{
    public class ThirdSubjectIdentifier
    {
        public ThirdSubjectIdentifierType Type { get; set; }
        public string Value { get; set; }
    }
    public enum ThirdSubjectIdentifierType
    {
        None,
        Other,
        Nip,
        VatUe,
        InternalId
    }
}
