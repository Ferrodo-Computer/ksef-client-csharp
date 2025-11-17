namespace KSeF.Client.Core.Models.Permissions.Identifiers
{
    public class PersonPermissionsAuthorizedIdentifier
    {
        public PersonAuthorizedIdentifierType Type { get; set; }
        public string Value { get; set; }
    }
    public enum PersonPermissionsAuthorizedIdentifierType
    {
        Nip,
        Pesel,
        Fingerprint
    }
}
