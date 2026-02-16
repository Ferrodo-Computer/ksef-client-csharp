namespace KSeF.Client.Core.Models.Permissions.Identifiers
{
    public  class SubunitContextIdentifier
    {
        public SubunitContextIdentifierType Type { get; set; }
        public string Value { get; set; }
    }

    public enum SubunitContextIdentifierType
    {
        Nip,
        InternalId,
    }
}
