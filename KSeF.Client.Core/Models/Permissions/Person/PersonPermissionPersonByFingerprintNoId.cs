namespace KSeF.Client.Core.Models.Permissions.Person
{
    public class PersonPermissionPersonByFingerprintNoId
    {
        public string FirstName { get; set; }
        public string LastName { get; set; } 
        public string BirthDate { get; set; }
        public PersonPermissionIdentityDocument IdDocument { get; set; } 
    }
}
