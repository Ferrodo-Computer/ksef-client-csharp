using KSeF.Client.Core.Models.Permissions.Identifiers;
using System.Collections.Generic;

namespace KSeF.Client.Core.Models.Permissions.EuEntityRepresentative
{
    public class GrantPermissionsEuEntityRepresentativeRequest
    {
        public EuEntityRepresentativeSubjectIdentifier SubjectIdentifier { get; set; }
        public ICollection<EuEntityRepresentativeStandardPermissionType> Permissions { get; set; }
        public string Description { get; set; }
        public EuEntityRepresentativeSubjectDetails SubjectDetails { get; set; }
    }
}
