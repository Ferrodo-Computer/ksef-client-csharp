using System;

namespace KSeF.Client.Core.Models.Permissions.IndirectEntity
{
    public class PermissionsIndirectEntityPersonByFingerprintWithoutIdentifier
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTimeOffset BirthDate { get; set; }
        public PermissionsIndirectEntityIdentityDocument IdDocument { get; set; }
    }
}
