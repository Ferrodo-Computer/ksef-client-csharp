namespace KSeF.Client.Core.Models.Permissions.Identifiers
{
    public class GrantPermissionsPersonSubjectIdentifier
    {
        public GrantPermissionsPersonSubjectIdentifierType Type { get; set; }
        public string Value { get; set; }
    }
    public enum GrantPermissionsPersonSubjectIdentifierType
    {
        Nip,
        Pesel,
        Fingerprint
    }
}
