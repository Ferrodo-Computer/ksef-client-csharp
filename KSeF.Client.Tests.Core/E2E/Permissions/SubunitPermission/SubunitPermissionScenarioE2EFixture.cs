using KSeF.Client.Core.Models;
using KSeF.Client.Core.Models.Permissions;
using KSeF.Client.Core.Models.Permissions.Identifiers;
using KSeF.Client.Tests.Utils;

namespace KSeF.Client.Tests.Core.E2E.Permissions.SubunitPermission;

public class SubunitPermissionsScenarioE2EFixture
{
    public SubunitPermissionsScenarioE2EFixture()
    {
        UnitNipInternal = $"{Unit.Value}-00001";        
    }

    public SubunitContextIdentifier Unit { get; } = new SubunitContextIdentifier
    {
        Type = SubunitContextIdentifierType.Nip,
        Value = MiscellaneousUtils.GetRandomNip(),        
    };

    public string UnitNipInternal { get; set; }

    public SubunitContextIdentifier Subunit { get; } = new SubunitContextIdentifier
    {
        Type = SubunitContextIdentifierType.Nip,
        Value = MiscellaneousUtils.GetRandomNip()
    };

    public SubunitSubjectIdentifier SubjectIdentifier { get; } = new SubunitSubjectIdentifier
    {
        Type = SubUnitSubjectIdentifierType.Nip,
        Value = MiscellaneousUtils.GetRandomNip()
    };

    public OperationResponse GrantResponse { get; set; }
    public List<PermissionsOperationStatusResponse> RevokeStatusResults { get; set; } = [];
    public PagedPermissionsResponse<Client.Core.Models.Permissions.SubunitPermission> SearchResponse { get; internal set; }
}
