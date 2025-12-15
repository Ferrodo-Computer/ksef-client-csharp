namespace KSeF.Client.Core.Models.Permissions.Identifiers
{
    public class PersonPermissionTargetIdentifier
    {
        public PersonPermissionTargetIdentifierType Type { get; set; }
        public string Value { get; set; }
    }
    public enum PersonPermissionTargetIdentifierType
    {
        Nip,
        AllPartners,
        InternalId
    }
}
