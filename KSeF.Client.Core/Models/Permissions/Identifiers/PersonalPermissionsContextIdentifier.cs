namespace KSeF.Client.Core.Models.Permissions.Identifiers
{
    public class PersonalPermissionsContextIdentifier
    {
        public PersonalPermissionsContextIdentifierType Type { get; set; }
        public string Value { get; set; }
    }
    public enum PersonalPermissionsContextIdentifierType
    {
        Nip
    }
}