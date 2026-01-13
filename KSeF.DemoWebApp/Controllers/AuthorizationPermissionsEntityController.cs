using Microsoft.AspNetCore.Mvc;
using KSeF.Client.Core.Interfaces.Clients;
using KSeF.Client.Core.Models.Permissions.Authorizations;
using KSeF.Client.Core.Models;

namespace KSeF.DemoWebApp.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthorizationPermissionsEntityController(IKSeFClient ksefClient) : ControllerBase
{
    [HttpPost("grant-authorization-permissions-for-entity")]
    public async Task<ActionResult<OperationResponse>> GrantPermissionsEntity(string accessToken, GrantPermissionsAuthorizationRequest request, CancellationToken cancellationToken)
    {
        return await ksefClient.GrantsAuthorizationPermissionAsync(request, accessToken, cancellationToken).ConfigureAwait(false);
    }

    [HttpPost("revoke-authorization-permissions-for-entity")]
    public async Task<ActionResult<OperationResponse>> RevokePermissionsEntity(
    string accessToken,
    string permissionId,
    CancellationToken cancellationToken)
    {

        return await ksefClient.RevokeAuthorizationsPermissionAsync(permissionId, accessToken, cancellationToken).ConfigureAwait(false);
    }
}
