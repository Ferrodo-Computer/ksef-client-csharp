namespace KSeF.Client.Core.Models.Permissions.Person
{
    public class PersonPermissionSubjectDetails
    {
        public PersonPermissionSubjectDetailsType SubjectDetailsType { get; set; }

        public PersonPermissionPersonById PersonById { get; set; }

        public PersonPermissionPersonByFingerprintWithId PersonByFpWithId { get; set; }

        public PersonPermissionPersonByFingerprintNoId PersonByFpNoId { get; set; }
    }
}
