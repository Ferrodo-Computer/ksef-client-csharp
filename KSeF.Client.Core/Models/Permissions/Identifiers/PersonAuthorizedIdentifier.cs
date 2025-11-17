namespace KSeF.Client.Core.Models.Permissions.Identifiers
{
    public class PersonAuthorizedIdentifier
    {
        public PersonAuthorizedIdentifierType Type { get; set; }
        public string Value { get; set; }
    }

    public enum PersonAuthorizedIdentifierType
    {
        Nip,
        Pesel,
        Fingerprint
    }
}
