namespace KSeF.Client.Core.Models.Permissions.Identifiers
{
    public class SubunitPermissionsSubunitIdentifier
    {
        public SubunitIQuerydentifierType Type { get; set; }
        public string Value { get; set; }
    }
    public enum SubunitIQuerydentifierType
    {
        InternalId,
        Nip
    }
}
