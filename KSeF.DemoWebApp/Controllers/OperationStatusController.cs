using KSeF.Client.Core.Models.Permissions;
using Microsoft.AspNetCore.Mvc;
using KSeF.Client.Core.Interfaces.Clients;

namespace KSeF.DemoWebApp.Controllers;

[ApiController]
[Route("[controller]")]
public class OperationStatusController(IKSeFClient ksefClient) : ControllerBase
{
    [HttpGet("{operationReferenceNumber}/status")]
    public async Task<ActionResult<PermissionsOperationStatusResponse>> GetOperationStatusAsync([FromRoute] string operationReferenceNumber, string accessToken, CancellationToken cancellationToken)
    {
        PermissionsOperationStatusResponse status = await ksefClient.OperationsStatusAsync(operationReferenceNumber, accessToken, cancellationToken).ConfigureAwait(false); 
        return Ok(status); 
    }
}
