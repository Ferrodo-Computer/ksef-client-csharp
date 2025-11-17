namespace KSeF.Client.Core.Models.Permissions.Identifiers
{
    public class SubunitSubjectIdentifier 
    {
        public SubUnitSubjectIdentifierType Type { get; set; }
        public string Value { get; set; }
    }

    public enum SubUnitSubjectIdentifierType
    {
        Nip,
        Pesel,
        Fingerprint
    }
}
