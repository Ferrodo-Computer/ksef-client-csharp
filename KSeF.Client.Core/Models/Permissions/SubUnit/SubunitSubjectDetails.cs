namespace KSeF.Client.Core.Models.Permissions.SubUnit
{
    public class SubunitSubjectDetails
    {
        public PermissionsSubunitSubjectDetailsType SubjectDetailsType { get; set; }

        public PermissionsSubunitPersonByIdentifier PersonById { get; set; }
        public PermissionsSubunitPersonByFingerprintWithIdentifier PersonByFpWithId { get; set; }
        public PermissionsSubunitPersonByFingerprintWithoutIdentifier PersonByFpNoId { get; set; }
    }

}
