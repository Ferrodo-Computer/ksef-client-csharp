using KSeF.Client.Core.Models.Permissions.Entity;
using KSeF.Client.Core.Models.Permissions.EUEntity;
using KSeF.Client.Core.Models.Permissions.Identifiers;
using KSeF.Client.Core.Models.Permissions.Person;
using System;

namespace KSeF.Client.Core.Models.Permissions
{
    public class EuEntityPermission
    {
        public string Id { get; set; }
        public AuthorIdentifier AuthorIdentifier { get; set; }
        public string VatUeIdentifier { get; set; }
        public string EuEntityName { get; set; }
        public string AuthorizedFingerprintIdentifier { get; set; }
        public EuEntityPermissionType PermissionScope { get; set; }
        public string Description { get; set; }
        public PersonPermissionSubjectPersonDetails SubjectPersonDetails { get; set; }
        public EntityPermissionSubjectEntityDetails SubjectEntityDetails { get; set; }
        public PermissionsEuEntityDetails EuEntityDetails { get; set; }
        public DateTime StartDate { get; set; }
    }
}
