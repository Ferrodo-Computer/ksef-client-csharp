using System;

namespace KSeF.Client.Core.Models.Permissions.Person
{
    public class PersonPermissionSubjectPersonDetails
    {
		public PersonPermissionSubjectDetailsType SubjectDetailsType { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public PersonPermissionPersonIdentifier PersonIdentifier { get; set; }
		public DateTime? BirthDate { get; set; }
		public PersonPermissionIdentityDocument IdDocument { get; set; }
	}
}
