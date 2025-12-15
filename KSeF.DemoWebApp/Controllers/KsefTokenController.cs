using Microsoft.AspNetCore.Mvc;
using KSeF.Client.Core.Models.Authorization;
using KSeF.Client.Core.Interfaces.Clients;

namespace KSeF.DemoWebApp.Controllers;

[Route("[controller]")]
[ApiController]
public class KsefTokenController(IKSeFClient ksefClient) : ControllerBase
{
    private readonly IKSeFClient ksefClient = ksefClient;

    [HttpGet("get-new-token")]
    public async Task<ActionResult<KsefTokenResponse>> GetNewTokenAsync(string accessToken, CancellationToken cancellationToken)
    {
        KsefTokenRequest tokenRequest = new()
        {
            Permissions = [
                KsefTokenPermissionType.InvoiceRead,
                KsefTokenPermissionType.InvoiceWrite
                ],
            Description = "Demo token",
        };
        KsefTokenResponse ksefToken = await ksefClient.GenerateKsefTokenAsync(tokenRequest, accessToken, cancellationToken).ConfigureAwait(false);
        return Ok(ksefToken);
    }

    [HttpGet("query-tokens")]
    public async Task<ActionResult<AuthenticationKsefToken>> QueryTokensAsync(string accessToken, CancellationToken cancellationToken)
    {
        List<AuthenticationKsefToken> result = [];
        const int pageSize = 20;
        AuthenticationKsefTokenStatus status = AuthenticationKsefTokenStatus.Active;
        string continuationToken = string.Empty;

        do
        {
            QueryKsefTokensResponse tokens = await ksefClient.QueryKsefTokensAsync(accessToken, [status], continuationToken, pageSize : pageSize, cancellationToken: cancellationToken).ConfigureAwait(false);
            result.AddRange(tokens.Tokens);
            continuationToken = tokens.ContinuationToken;
        } while (!string.IsNullOrEmpty(continuationToken));

        return Ok(result);
    }
    [HttpGet("get-token")]
    public async Task<ActionResult<AuthenticationKsefToken>> GetTokenAsync(string tokenReferenceNumber, string accessToken, CancellationToken cancellationToken)
    {
        AuthenticationKsefToken ksefToken = await ksefClient.GetKsefTokenAsync(tokenReferenceNumber, accessToken, cancellationToken).ConfigureAwait(false);
        return Ok(ksefToken);
    }

    [HttpDelete]
    public async Task<ActionResult> RevokeAsync(string tokenReferenceNumber, string accessToken, CancellationToken cancellationToken)
    {
        await ksefClient.RevokeKsefTokenAsync(tokenReferenceNumber, accessToken, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }
}
