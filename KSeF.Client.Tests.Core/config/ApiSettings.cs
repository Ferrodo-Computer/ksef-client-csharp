using KSeF.Client.Core.Infrastructure.Rest;

namespace KSeF.Client.Tests.Core.Config;

public sealed class ApiSettings
{
    public string BaseUrl { get; init; }
    public Dictionary<string, string> CustomHeaders { get; set; }
        = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    public ResponseHeaderObservationOptions ResponseHeaderObservation { get; set; }
        = new ResponseHeaderObservationOptions();
}
