using System;

namespace KSeF.Client.Core.Models.Permissions.EUEntity
{
    public class PermissionsEuEntityPersonByFpNoId
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTimeOffset BirthDate { get; set; }
        public PermissionsEuEntityIdentityDocument IdDocument { get; set; }
    }
}
