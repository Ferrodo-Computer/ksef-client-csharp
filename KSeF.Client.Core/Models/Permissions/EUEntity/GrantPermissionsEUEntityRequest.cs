using KSeF.Client.Core.Models.Permissions.Identifiers;

namespace KSeF.Client.Core.Models.Permissions.EUEntity
{
    public class GrantPermissionsEuEntityRequest
    {
        public EuEntitySubjectIdentifier SubjectIdentifier { get; set; }
        public EuEntityContextIdentifier ContextIdentifier { get; set; }
        public string Description { get; set; }
        public string EuEntityName { get; set; }
    }
}
