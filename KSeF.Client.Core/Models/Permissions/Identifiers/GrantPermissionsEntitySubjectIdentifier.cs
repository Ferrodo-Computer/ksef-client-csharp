namespace KSeF.Client.Core.Models.Permissions.Identifiers
{
    public class GrantPermissionsEntitySubjectIdentifier
    {
        public GrantPermissionsEntitySubjectIdentifierType Type { get; set; }
        public string Value { get; set; }
    }
    public enum GrantPermissionsEntitySubjectIdentifierType
    {
        Nip,
        PeppolId
    }
}
