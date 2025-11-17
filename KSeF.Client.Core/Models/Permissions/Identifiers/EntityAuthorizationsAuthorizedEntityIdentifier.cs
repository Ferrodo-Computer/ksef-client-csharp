namespace KSeF.Client.Core.Models.Permissions.Identifiers
{
    public class EntityAuthorizationsAuthorizedEntityIdentifier
    {
        public EntityAuthorizationsAuthorizedEntityIdentifierType Type { get; set; }
        public string Value { get; set; }
    }

    public enum EntityAuthorizationsAuthorizedEntityIdentifierType
    {
        Nip,
        PeppolId,
    }
}
