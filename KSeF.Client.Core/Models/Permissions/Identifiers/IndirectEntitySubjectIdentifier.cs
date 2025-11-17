namespace KSeF.Client.Core.Models.Permissions.Identifiers
{
    public class IndirectEntitySubjectIdentifier
    {
        public IndirectEntitySubjectIdentifierType Type { get; set; }
        public string Value { get; set; }
    }

    public enum IndirectEntitySubjectIdentifierType
    {
        Nip,
        Pesel,
        Fingerprint
    }
}
