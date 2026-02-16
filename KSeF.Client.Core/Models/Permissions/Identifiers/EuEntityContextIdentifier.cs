namespace KSeF.Client.Core.Models.Permissions.Identifiers
{
    public class EuEntityContextIdentifier
    {
        public EuEntityContextIdentifierType Type { get; set; }
        public string Value { get; set; }
    }

    public enum EuEntityContextIdentifierType
    {
        NipVatUe
    }
}
