namespace KSeF.Client.Core.Models.Permissions.Identifiers
{
    public class PersonPermissionsContextIdentifier
    {
        public PersonPermissionsContextIdentifierType Type { get; set; }
        public string Value { get; set; }
    }

    public enum PersonPermissionsContextIdentifierType
    {
        Nip,
        InternalId
    }
}