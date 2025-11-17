using KSeF.Client.Api.Builders.EUEntityRepresentativePermissions;
using KSeF.Client.Core.Models.Permissions.EuEntityRepresentative;
using Microsoft.AspNetCore.Mvc;
using KSeF.Client.Core.Interfaces.Clients;
using KSeF.Client.Core.Models;
using KSeF.Client.Core.Models.Permissions.Identifiers;

namespace KSeF.DemoWebApp.Controllers;

[ApiController]
[Route("[controller]")]
public class EUEntityRepresentativePermissionsController(IKSeFClient ksefClient) : ControllerBase
{
    [HttpPost("grant-eu-entity-representative-permissions")]
    public async Task<ActionResult<OperationResponse>> GrantPermissionsEntity(string accessToken, EuEntityRepresentativeSubjectIdentifier subjectIdentifier, CancellationToken cancellationToken)
    {
        GrantPermissionsEuEntityRepresentativeRequest request = GrantEUEntityRepresentativePermissionsRequestBuilder
            .Create()
            .WithSubject(subjectIdentifier)
            .WithPermissions(EuEntityRepresentativeStandardPermissionType.InvoiceRead, EuEntityRepresentativeStandardPermissionType.InvoiceWrite)
            .WithDescription("Representative access")
            .Build();

        return await ksefClient.GrantsPermissionEUEntityRepresentativeAsync(request, accessToken, cancellationToken);
    }

    [HttpPost("revoke-eu-entity-representative-permissions")]
    public async Task<ActionResult<OperationResponse>> RevokePermissionsEntity(string accessToken, string permissionId, CancellationToken cancellationToken)
    {
  
        return await ksefClient.RevokeAuthorizationsPermissionAsync(permissionId, accessToken, cancellationToken);
    }
}
