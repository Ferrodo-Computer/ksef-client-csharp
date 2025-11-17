using KSeF.Client.Api.Builders.AuthorizationPermissions;
using Microsoft.AspNetCore.Mvc;
using KSeF.Client.Core.Interfaces.Clients;
using KSeF.Client.Core.Models.Permissions.Authorizations;
using KSeF.Client.Core.Models;
using KSeF.Client.Core.Models.Permissions.Identifiers;

namespace KSeF.DemoWebApp.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthorizationPermissionsEntityController(IKSeFClient ksefClient) : ControllerBase
{
    [HttpPost("grant-authorization-permissions-for-entity")]
    public async Task<ActionResult<OperationResponse>> GrantPermissionsEntity(string accessToken, AuthorizationSubjectIdentifier subjectIdentifier, CancellationToken cancellationToken)
    {
        GrantPermissionsAuthorizationRequest request = GrantAuthorizationPermissionsRequestBuilder
            .Create()
            .WithSubject(subjectIdentifier)
            .WithPermission(AuthorizationPermissionType.TaxRepresentative)
            .WithDescription("Access for quarterly review")
            .Build();

        return await ksefClient.GrantsAuthorizationPermissionAsync(request, accessToken, cancellationToken);
    }

    [HttpPost("revoke-authorization-permissions-for-entity")]
    public async Task<ActionResult<OperationResponse>> RevokePermissionsEntity(
    string accessToken,
    string permissionId,
    CancellationToken cancellationToken)
    {

        return await ksefClient.RevokeAuthorizationsPermissionAsync(permissionId, accessToken, cancellationToken);
    }
}
