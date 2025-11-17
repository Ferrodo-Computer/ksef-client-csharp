namespace KSeF.Client.Core.Models.Permissions.Identifiers
{
    public class PersonPermissionsAuthorIdentifier
    {
        public PersonPermissionsAuthorIdentifierType Type { get; set; }
        public string Value { get; set; }
    }
    public enum PersonPermissionsAuthorIdentifierType
    {
        Nip,
        Pesel,
        Fingerprint,
        System
    }
}
