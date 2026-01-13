namespace KSeF.Client.Core.Models.Permissions.EUEntity
{
    public class EuEntityRepresentativePersonIdentifier
    {
        public PermissionsEuEntityPersonIdentifierType Type {get; set; }
        public string Value { get; set; }

    }

    public enum PermissionsEuEntityPersonIdentifierType
    {
        Pesel,
        Nip
    }
}
