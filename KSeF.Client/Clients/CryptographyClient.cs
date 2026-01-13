using KSeF.Client.Core.Interfaces.Clients;
using KSeF.Client.Core.Interfaces.Rest;
using KSeF.Client.Core.Models.Certificates;
using KSeF.Client.Http;

namespace KSeF.Client.Clients;

/// <inheritdoc />
public class CryptographyClient(IRestClient restClient) : ICryptographyClient
{
    private readonly IRestClient _restClient = restClient;

    /// <inheritdoc />
    public async Task<ICollection<PemCertificateInfo>> GetPublicCertificatesAsync(CancellationToken cancellationToken = default)
    {
        return await _restClient.SendAsync<ICollection<PemCertificateInfo>, string>(HttpMethod.Get,
                                                                      "/v2/security/public-key-certificates",
                                                                      default,
                                                                      default,
                                                                      RestClient.DefaultContentType,
                                                                      cancellationToken).ConfigureAwait(false);
    }
}

