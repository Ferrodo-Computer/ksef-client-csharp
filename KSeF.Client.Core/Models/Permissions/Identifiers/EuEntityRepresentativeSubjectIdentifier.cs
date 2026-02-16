namespace KSeF.Client.Core.Models.Permissions.Identifiers
{
    public class EuEntityRepresentativeSubjectIdentifier
    {
        public EuEntityRepresentativeSubjectIdentifierType Type { get; set; }
        public string Value { get; set; }
    }
    public enum EuEntityRepresentativeSubjectIdentifierType
    {
        Fingerprint,
    }
}
