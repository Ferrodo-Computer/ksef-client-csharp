using KSeF.Client.Core.Models;
using KSeF.Client.Core.Models.Permissions;
using KSeF.Client.Core.Models.Permissions.Identifiers;
using KSeF.Client.Tests.Utils;

namespace KSeF.Client.Tests.Core.E2E.Permissions.EuEntityPermissions;

public class EuEntityPermissionScenarioE2EFixture
{
    public string AccessToken { get; set; }
    public EuEntitySubjectIdentifier EuEntity { get; } = new EuEntitySubjectIdentifier
    {
        Type = EuEntitySubjectIdentifierType.Fingerprint,
        Value = MiscellaneousUtils.GetRandomNip()
    };
    public OperationResponse GrantResponse { get; set; }
    public List<PermissionsOperationStatusResponse> RevokeStatusResults { get; set; } = new List<PermissionsOperationStatusResponse>();
    public PagedPermissionsResponse<EuEntityPermission> SearchResponse { get; set; }
    public string NipVatUe { get; set; }
}
