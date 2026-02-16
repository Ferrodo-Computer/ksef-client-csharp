namespace KSeF.Client.Core.Models.Permissions.Identifiers
{
    public class AuthorizationSubjectIdentifier
    {
        public AuthorizationSubjectIdentifierType Type { get; set; }
        public string Value { get; set; }
    }
    public enum AuthorizationSubjectIdentifierType
    {
        Nip,
        PeppolId
    }
}
