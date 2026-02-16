namespace KSeF.Client.Core.Models.Permissions.Identifiers
{
    public class PersonalPermissionsTargetIdentifier
    {
        public PersonalPermissionsTargetIdentifierType Type { get; set; }
        public string Value { get; set; }

        public enum PersonalPermissionsTargetIdentifierType
        {
            Nip,
            AllPartners,
            InternalId
        }
    }
}
