namespace KSeF.Client.Core.Models.Permissions.Identifiers
{
    public class PersonPermissionAuthorizedIdentifier
    {
        public PersonPermissionAuthorizedIdentifierType Type { get; set; }
        public string Value { get; set; }
    }
    public enum PersonPermissionAuthorizedIdentifierType
    {
        Nip,
        Pesel,
        Fingerprint
    }
}
