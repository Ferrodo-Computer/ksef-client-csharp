using KSeF.Client.Core.Models;
using KSeF.Client.Core.Models.Permissions;
using KSeF.Client.Core.Models.Permissions.Identifiers;
using KSeF.Client.Tests.Utils;

namespace KSeF.Client.Tests.Core.E2E.Permissions.AuthorizationPermissions;

public class AuthorizationPermissionsScenarioE2EFixture
{
    public AuthorizationSubjectIdentifier SubjectIdentifier { get; } =
        new AuthorizationSubjectIdentifier
        {
            Type = AuthorizationSubjectIdentifierType.Nip,
            Value = MiscellaneousUtils.GetRandomNip()
        };

    public OperationResponse GrantResponse { get; set; }
    public List<PermissionsOperationStatusResponse> RevokeStatusResults { get; set; } = new();
    public required PagedAuthorizationsResponse<AuthorizationGrant> SearchResponse { get; set; }
}
