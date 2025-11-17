namespace KSeF.Client.Core.Models.Permissions.Identifiers
{
    public class PersonalPermissionTargetIdentifier
    {
        public PersonalPermissionTargetIdentifierType Type { get; set; }
        public string Value { get; set; }
    }
    public enum PersonalPermissionTargetIdentifierType
    {
        Nip,
        AllPartners,
        InternalId
    }
}
