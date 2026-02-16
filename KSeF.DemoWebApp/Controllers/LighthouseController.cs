using KSeF.Client.Core.Interfaces.Clients;
using KSeF.Client.Core.Models.Lighthouse;
using Microsoft.AspNetCore.Mvc;

namespace KSeF.DemoWebApp.Controllers;

/// <summary>
/// Kontroler demonstracyjny prezentujący działanie klienta Latarni (status systemu KSeF i komunikaty).
/// </summary>
[ApiController]
[Route("[controller]")]
public sealed class LighthouseController(ILighthouseClient lighthouseClient) : ControllerBase
{
    private readonly ILighthouseClient lighthouseClient = lighthouseClient ?? throw new ArgumentNullException(nameof(lighthouseClient));

    /// <summary>
    /// Zwraca bieżący status systemu KSeF wg Latarni.
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(typeof(KsefStatusResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<KsefStatusResponse>> GetStatus(CancellationToken cancellationToken)
    {
        KsefStatusResponse result = await lighthouseClient.GetStatusAsync(cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }

    /// <summary>
    /// Zwraca bieżące komunikaty Latarni.
    /// </summary>
    [HttpGet("messages")]
    [ProducesResponseType(typeof(KsefMessagesResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<KsefMessagesResponse>> GetMessages(CancellationToken cancellationToken)
    {
        KsefMessagesResponse result = await lighthouseClient.GetMessagesAsync(cancellationToken).ConfigureAwait(false);
        return Ok(result);
    }
}
