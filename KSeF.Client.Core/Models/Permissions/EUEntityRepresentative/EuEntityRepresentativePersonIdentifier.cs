namespace KSeF.Client.Core.Models.Permissions.EuEntityRepresentative
{
    public class EuEntityRepresentativePersonIdentifier
	{
        public EuEntityRepresentativePersonIdentifierType Type {get; set; }
        public string Value { get; set; }

    }

    public enum EuEntityRepresentativePersonIdentifierType
	{
        Pesel,
        Nip
    }
}
