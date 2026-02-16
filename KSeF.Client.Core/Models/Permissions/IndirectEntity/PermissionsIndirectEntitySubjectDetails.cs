namespace KSeF.Client.Core.Models.Permissions.IndirectEntity
{
    public class PermissionsIndirectEntitySubjectDetails
    {
        public PermissionsIndirectEntitySubjectDetailsType SubjectDetailsType { get; set; }

        public PermissionsIndirectEntityPersonByIdentifier PersonById { get; set; }
        public PermissionsIndirectEntityPersonByFingerprintWithIdentifier PersonByFpWithId { get; set; }
        public PermissionsIndirectEntityPersonByFingerprintWithoutIdentifier PersonByFpNoId { get; set; }
    }
}
