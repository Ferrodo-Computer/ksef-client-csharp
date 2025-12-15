namespace KSeF.Client.Core.Models.Permissions.Identifiers
{
    public class PersonalPermissionContextIdentifier
    {
        public PersonalPermissionContextIdentifierType Type { get; set; }
        public string Value { get; set; }
    }
    public enum PersonalPermissionContextIdentifierType
    {
        Nip
    }
}
