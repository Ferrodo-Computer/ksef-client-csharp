using KSeF.Client.Core.Models.Permissions.Identifiers;
using System.Collections.Generic;

namespace KSeF.Client.Core.Models.Permissions.Person
{
    public class GrantPermissionsPersonRequest
    {
        public GrantPermissionsPersonSubjectIdentifier SubjectIdentifier { get; set; }
        public ICollection<PersonPermissionType> Permissions { get; set; }
        public string Description { get; set; }
        public PersonPermissionSubjectDetails SubjectDetails { get;set; }
    }
}
