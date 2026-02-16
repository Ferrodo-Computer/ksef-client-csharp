using KSeF.Client.Core.Models.Permissions.Identifiers;
using System.Collections.Generic;

namespace KSeF.Client.Core.Models.Permissions.Person
{
    public class PersonalPermissionsQueryRequest
    {
        public PersonalPermissionsContextIdentifier ContextIdentifier { get; set; }
        public PersonalPermissionsTargetIdentifier TargetIdentifier { get; set; }
        public List<PersonPermissionType> PermissionTypes { get; set; }
        public PersonPermissionState? PermissionState { get; set; }
    }
}