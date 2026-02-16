namespace KSeF.Client.Core.Models.Permissions.Identifiers
{
    public class AuthorIdentifier
    {
        public AuthorIdentifierType Type { get; set; }
        public string Value { get; set; }
    }

    public enum AuthorIdentifierType
    {
        Nip,
        Pesel,
        Fingerprint,
        System
    }
}
