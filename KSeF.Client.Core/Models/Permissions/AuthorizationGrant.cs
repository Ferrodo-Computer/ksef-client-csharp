using KSeF.Client.Core.Models.Permissions.Authorizations;
using KSeF.Client.Core.Models.Permissions.Entity;
using KSeF.Client.Core.Models.Permissions.Identifiers;
using System;

namespace KSeF.Client.Core.Models.Permissions
{
    public class AuthorizationGrant
    {
        public string Id { get; set; }
        public AuthorIdentifier AuthorIdentifier { get; set; }
        public AuthorizedEntityIdentifier AuthorizedEntityIdentifier { get; set; }
        public AuthorizingEntityIdentifier AuthorizingEntityIdentifier { get; set; }
        public AuthorizationPermissionType AuthorizationScope { get; set; }
        public string Description { get; set; }
        public EntityPermissionSubjectEntityDetails SubjectEntityDetails { get; set; }
        public DateTime StartDate { get; set; }
    }
}
