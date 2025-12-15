using System;

namespace KSeF.Client.Core.Models.Permissions.SubUnit
{
    public class PermissionsSubunitPersonByFingerprintWithoutIdentifier
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTimeOffset BirthDate { get; set; }
        public PermissionsSubunitIdentityDocument IdDocument { get; set; }
    }

}
