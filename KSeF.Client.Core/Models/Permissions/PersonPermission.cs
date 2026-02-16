using KSeF.Client.Core.Models.Permissions.Entity;
using KSeF.Client.Core.Models.Permissions.Identifiers;
using KSeF.Client.Core.Models.Permissions.Person;
using System;

namespace KSeF.Client.Core.Models.Permissions
{
    public class PersonPermission
    {
        public string Id { get; set; }
        public PersonPermissionAuthorizedIdentifier AuthorizedIdentifier { get; set; }
        public PersonPermissionContextIdentifier ContextIdentifier { get; set; }
        public PersonPermissionTargetIdentifier TargetIdentifier { get; set; }
        public AuthorIdentifier AuthorIdentifier { get; set; }
        public PersonPermissionType PermissionScope { get; set; }
        public string Description { get; set; }
        public PersonPermissionSubjectPersonDetails SubjectPersonDetails { get; set; }
		public EntityPermissionSubjectEntityDetails SubjectEntityDetails { get; set; }
		public PersonPermissionState PermissionState { get; set; }
        public DateTime StartDate { get; set; }
        public bool CanDelegate { get; set; }
    }
}