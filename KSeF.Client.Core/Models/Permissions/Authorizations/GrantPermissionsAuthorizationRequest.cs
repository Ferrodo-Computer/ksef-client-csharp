using KSeF.Client.Core.Models.Permissions.Identifiers;

namespace KSeF.Client.Core.Models.Permissions.Authorizations
{
    public class GrantPermissionsAuthorizationRequest
    {
        public AuthorizationSubjectIdentifier SubjectIdentifier { get; set; }
        public AuthorizationPermissionType Permission { get; set; }
        public string Description { get; set; }
    }

    public enum AuthorizationPermissionType
    {
        SelfInvoicing,
        RRInvoicing,
        TaxRepresentative,
        PefInvoicing
    }

    
}
