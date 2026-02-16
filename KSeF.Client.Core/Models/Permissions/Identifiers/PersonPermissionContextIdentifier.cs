namespace KSeF.Client.Core.Models.Permissions.Identifiers
{
    public class PersonPermissionContextIdentifier
    {
        public PersonPermissionContextIdentifierType Type { get; set; }
        public string Value { get; set; }
    }

    public enum PersonPermissionContextIdentifierType
    {
        Nip,
        InternalId,
    }
}
