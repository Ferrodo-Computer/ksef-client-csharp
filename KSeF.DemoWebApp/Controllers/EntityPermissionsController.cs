using KSeF.Client.Core.Models.Permissions.Entity;
using Microsoft.AspNetCore.Mvc;
using KSeF.Client.Core.Interfaces.Clients;
using KSeF.Client.Core.Models;

namespace KSeF.DemoWebApp.Controllers;

[ApiController]
[Route("[controller]")]
public class EntityPermissionsController(IKSeFClient ksefClient) : ControllerBase
{
    [HttpPost("grant-permissions-for-Entity")]
    public async Task<ActionResult<OperationResponse>> GrantPermissionsEntity(string accessToken, GrantPermissionsEntityRequest request, CancellationToken cancellationToken)
    {
        return await ksefClient.GrantsPermissionEntityAsync(request, accessToken, cancellationToken).ConfigureAwait(false);
    }

    [HttpPost("revoke-permissions-for-Entity")]
    public async Task<ActionResult<OperationResponse>> RevokePermissionsEntity(
    string accessToken,
    string permissionId,
    CancellationToken cancellationToken)
    {
 
        return await ksefClient.RevokeCommonPermissionAsync(permissionId, accessToken, cancellationToken).ConfigureAwait(false);
    }
}
