namespace KSeF.Client.Core.Models.Permissions.Identifiers
{
    public class IndirectEntityTargetIdentifier
    {
        public IndirectEntityTargetIdentifierType Type { get; set; }
        public string Value { get; set; }
    }
    public enum IndirectEntityTargetIdentifierType
    {
        Nip,
        AllPartners,
        InternalId
    }
}
