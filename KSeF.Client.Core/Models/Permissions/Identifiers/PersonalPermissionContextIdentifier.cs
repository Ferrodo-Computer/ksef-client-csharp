namespace KSeF.Client.Core.Models.Permissions
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
