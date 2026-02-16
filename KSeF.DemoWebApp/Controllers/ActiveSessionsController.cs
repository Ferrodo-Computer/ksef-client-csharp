using KSeF.Client.Core.Models.Sessions.ActiveSessions;
using Microsoft.AspNetCore.Mvc;
using KSeF.Client.Core.Interfaces.Clients;

namespace KSeF.DemoWebApp.Controllers;
[Route("active-sessions")]
[ApiController]
public class ActiveSessionsController(IActiveSessionsClient activeSessionsClient) : ControllerBase
{ 
    /// <summary>
    /// Pobranie listy aktywnych sesji.
    /// </summary>
    [HttpGet("list")]
    public async Task<ActionResult<ICollection<AuthenticationListItem>>> GetSessionsAsync([FromQuery] string accessToken, CancellationToken cancellationToken)
    {
        const int pageSize = 20;
        string continuationToken = string.Empty;
        List<AuthenticationListItem> activeSessions = [];
        do
        {
            AuthenticationListResponse response = await activeSessionsClient.GetActiveSessions(accessToken, pageSize, continuationToken, cancellationToken).ConfigureAwait(false);
            continuationToken = response.ContinuationToken;
            activeSessions.AddRange(response.Items);
        }
        while (!string.IsNullOrWhiteSpace(continuationToken));

        return Ok(activeSessions);
    }

    /// <summary>
    /// Unieważnia sesję powiązaną z tokenem użytym do wywołania tej operacji.
    /// </summary>
    /// <param name="token">Access token lub Refresh token.</param>
    /// <param name="cancellationToken"></param>
    [HttpDelete("revoke-current-session")]
    public async Task<ActionResult> RevokeCurrentSessionAsync([FromQuery] string token, CancellationToken cancellationToken)
    {
        await activeSessionsClient.RevokeCurrentSessionAsync(token, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }

    /// <summary>
    /// Unieważnia sesję o podanym numerze referencyjnym.
    /// </summary>
    [HttpDelete("revoke-session")]
    public async Task<ActionResult> RevokeSessionAsync([FromQuery] string sessionReferenceNumber, [FromQuery] string accessToken, CancellationToken cancellationToken)
    {
        await activeSessionsClient.RevokeSessionAsync(sessionReferenceNumber, accessToken, cancellationToken).ConfigureAwait(false);
        return NoContent();
    }
}