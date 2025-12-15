namespace KSeF.Client.Core.Models.Permissions.EUEntity
{
    public class PermissionsEuEntitySubjectDetails
    {
        public PermissionsEuEntitySubjectDetailsType SubjectDetailsType { get; set; }

        public PermissionsEuEntityPersonByFpNoId PersonByFpNoId { get; set; }

        public PermissionsEuEntityEntityByFp EntityByFp { get; set; }
    }
}
