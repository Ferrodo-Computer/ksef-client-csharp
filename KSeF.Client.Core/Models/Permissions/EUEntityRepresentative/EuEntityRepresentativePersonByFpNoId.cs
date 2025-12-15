using System;

namespace KSeF.Client.Core.Models.Permissions.EuEntityRepresentative
{
    public class EuEntityRepresentativePersonByFpNoId
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTimeOffset BirthDate { get; set; }
        public EuEntityRepresentativeIdentityDocument IdDocument { get; set; }
    }
}
