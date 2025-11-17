using KSeF.Client.Api.Builders.EntityPermissions;
using KSeF.Client.Core.Models.Permissions.Entity;
using Microsoft.AspNetCore.Mvc;
using KSeF.Client.Core.Interfaces.Clients;
using KSeF.Client.Core.Models;
using KSeF.Client.Core.Models.Permissions.Identifiers;

namespace KSeF.DemoWebApp.Controllers;

[ApiController]
[Route("[controller]")]
public class EntityPermissionsController(IKSeFClient ksefClient) : ControllerBase
{
    [HttpPost("grant-permissions-for-Entity")]
    public async Task<ActionResult<OperationResponse>> GrantPermissionsEntity(string accessToken, GrantPermissionsEntitySubjectIdentifier subjectIdentifier, CancellationToken cancellationToken)
    {
        GrantPermissionsEntityRequest request = GrantEntityPermissionsRequestBuilder
            .Create()
            .WithSubject(subjectIdentifier)
            .WithPermissions(
                EntityPermission.New(EntityStandardPermissionType.InvoiceRead, true),
                EntityPermission.New(EntityStandardPermissionType.InvoiceRead, false)
                )
            .WithDescription("Access for quarterly review")
            .Build();

        return await ksefClient.GrantsPermissionEntityAsync(request, accessToken, cancellationToken);
    }

    [HttpPost("revoke-permissions-for-Entity")]
    public async Task<ActionResult<OperationResponse>> RevokePermissionsEntity(
    string accessToken,
    string permissionId,
    CancellationToken cancellationToken)
    {
 
        return await ksefClient.RevokeCommonPermissionAsync(permissionId, accessToken, cancellationToken);
    }
}
