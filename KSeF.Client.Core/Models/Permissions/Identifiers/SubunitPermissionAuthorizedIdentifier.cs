namespace KSeF.Client.Core.Models.Permissions.Identifiers
{
    public class SubunitPermissionAuthorizedIdentifier
    {
        public SubunitPermissionAuthorizedIdentifierType Type { get; set; }
        public string Value { get; set; }
    }
    public enum SubunitPermissionAuthorizedIdentifierType
    {
        Nip,
        Pesel,
        Fingerprint
    }
}
