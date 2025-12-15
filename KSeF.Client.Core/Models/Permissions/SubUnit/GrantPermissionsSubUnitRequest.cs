using KSeF.Client.Core.Models.Permissions.Identifiers;

namespace KSeF.Client.Core.Models.Permissions.SubUnit
{
    public class GrantPermissionsSubunitRequest
    {
        public SubunitSubjectIdentifier SubjectIdentifier { get; set; }
        public SubunitContextIdentifier ContextIdentifier { get; set; }
        public string Description { get; set; }
        public string SubunitName { get; set; }
        public SubunitSubjectDetails SubjectDetails { get; set; }
    }

}
