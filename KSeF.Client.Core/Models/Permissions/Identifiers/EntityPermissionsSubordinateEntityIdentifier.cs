namespace KSeF.Client.Core.Models.Permissions.Identifiers
{
    public class EntityPermissionsSubordinateEntityIdentifier
    {
        public EntityPermissionsSubordinateEntityIdentifierType Type { get; set; }
        public string Value { get; set; }
    }

    public enum EntityPermissionsSubordinateEntityIdentifierType
    {
        Nip
    }
}
