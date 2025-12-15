using KSeF.Client.Core.Models.Permissions.Identifiers;
using System.Collections.Generic;

namespace KSeF.Client.Core.Models.Permissions.IndirectEntity
{
    public class GrantPermissionsIndirectEntityRequest
    {
        public IndirectEntitySubjectIdentifier SubjectIdentifier { get; set; }
        public IndirectEntityTargetIdentifier TargetIdentifier { get; set; }
        public ICollection<IndirectEntityStandardPermissionType> Permissions { get; set; }
        public string Description { get; set; }
        public PermissionsIndirectEntitySubjectDetails SubjectDetails { get; set; }
    }
}
