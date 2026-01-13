using KSeF.Client.Core.Models.Permissions.EUEntity;
using Microsoft.AspNetCore.Mvc;
using KSeF.Client.Core.Interfaces.Clients;
using KSeF.Client.Core.Models;

namespace KSeF.DemoWebApp.Controllers;

[ApiController]
[Route("[controller]")]
public class EUEntityPermissionsController(IKSeFClient ksefClient) : ControllerBase
{
    [HttpPost("grant-eu-entity-permissions")]
    public async Task<ActionResult<OperationResponse>> GrantPermissionsEntity(string accessToken, GrantPermissionsEuEntityRequest grantPermissionsRequest, CancellationToken cancellationToken)
    {
        return await ksefClient.GrantsPermissionEUEntityAsync(grantPermissionsRequest, accessToken, cancellationToken).ConfigureAwait(false);
    }

    [HttpPost("revoke-eu-entity-permissions")]
    public async Task<ActionResult<OperationResponse>> RevokePermissionsEntity(string permissionId, string accessToken, CancellationToken cancellationToken)
    {
      
        return await ksefClient.RevokeCommonPermissionAsync(permissionId, accessToken, cancellationToken).ConfigureAwait(false);
    }

}
