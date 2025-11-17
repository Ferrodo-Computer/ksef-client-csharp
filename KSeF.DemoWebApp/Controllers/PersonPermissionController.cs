using KSeF.Client.Api.Builders.PersonPermissions;
using KSeF.Client.Core.Models.Permissions.Person;
using Microsoft.AspNetCore.Mvc;
using KSeF.Client.Core.Interfaces.Clients;
using KSeF.Client.Core.Models;
using KSeF.Client.Core.Models.Permissions.Identifiers;

namespace WebApplication.Controllers;

[ApiController]
[Route("[controller]")]
public class PersonPermissionController(IKSeFClient ksefClient) : ControllerBase
{
    [HttpPost("grant-permissions-for-person")]
    public async Task<ActionResult<OperationResponse>> GrantPermissionsPerson(string accessToken, GrantPermissionsPersonSubjectIdentifier subjectIdentifier, CancellationToken cancellationToken)
    {
        GrantPermissionsPersonRequest request = GrantPersonPermissionsRequestBuilder
            .Create()
            .WithSubject(subjectIdentifier)
            .WithPermissions(PersonPermissionType.InvoiceRead, PersonPermissionType.InvoiceWrite)
            .WithDescription("Access for quarterly review")
            .Build();

        return await ksefClient.GrantsPermissionPersonAsync(request,  accessToken, cancellationToken);
    }

    [HttpPost("revoke-permissions-for-person")]
    public async Task<ActionResult<OperationResponse>> RevokePermissionsPerson(
    string accessToken,
    string permissionId,
    CancellationToken cancellationToken)
    {
        return await ksefClient.RevokeCommonPermissionAsync(permissionId, accessToken, cancellationToken);
    }
}
