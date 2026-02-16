namespace KSeF.Client.Core.Models.Permissions.Identifiers
{
    public class PersonPermissionsTargetIdentifier
    {
        public PersonPermissionsTargetIdentifierType Type { get; set; }
        public string Value { get; set; }
    }
    public enum PersonPermissionsTargetIdentifierType
    {
        Nip,
        AllPartners,
        InternalId
    }
}