namespace KSeF.Client.Core.Models.Permissions.Identifiers
{
    public class EuEntitySubjectIdentifier
    {
        public EuEntitySubjectIdentifierType Type { get; set; }
        public string Value { get; set; }
    }

    public enum EuEntitySubjectIdentifierType
    {
        Fingerprint
    }
}
