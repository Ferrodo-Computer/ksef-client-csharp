namespace KSeF.Client.Core.Models.Permissions.Identifiers
{
    public class EntityAuthorizationsAuthorizingEntityIdentifier
    {
        public EntityAuthorizationsAuthorizingEntityIdentifierType Type { get; set; }
        public string Value { get; set; }
    }
    public enum EntityAuthorizationsAuthorizingEntityIdentifierType
    {
        Nip
    }
}
