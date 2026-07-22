using KSeF.Client.Clients;
using KSeF.Client.Core.Interfaces.Clients;
using KSeF.Client.Core.Models.CollectiveIdentifiers;
using KSeF.Client.Http;
using System.Net;
using System.Text;

namespace KSeF.Client.Tests.Core.UnitTests;

public class KSeFClientCollectiveIdentifiersTests
{
    [Fact]
    public async Task GenerateCollectiveIdentifierAsync_UsesExpectedEndpoint()
    {
        RecordingHttpMessageHandler handler = new();
        IKSeFClient client = CreateClient(handler);

        await client.GenerateCollectiveIdentifierAsync(
            new GenerateCollectiveIdentifierRequest
            {
                Invoices = new[] { new CollectiveIdentifierInvoice { KsefNumber = "ksef-number" } }
            },
            "access-token");

        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.Equal("/v2/collective-identifiers", handler.RequestUri.AbsolutePath);
        Assert.Contains("ksef-number", handler.RequestBody);
        Assert.Equal("Bearer", handler.AuthorizationScheme);
        Assert.Equal("access-token", handler.AuthorizationParameter);
    }

    [Fact]
    public async Task QueryCollectiveIdentifiersAsync_AddsPaginationAndContinuationToken()
    {
        RecordingHttpMessageHandler handler = new();
        IKSeFClient client = CreateClient(handler);

        await client.QueryCollectiveIdentifiersAsync(
            new CollectiveIdentifiersQueryRequest(),
            "access-token",
            pageSize: 25,
            continuationToken: "continuation-token");

        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.Equal("/v2/collective-identifiers/query?pageSize=25", handler.RequestUri.PathAndQuery);
        Assert.Equal("continuation-token", handler.ContinuationToken);
    }

    [Fact]
    public async Task GetCollectiveIdentifiersByKsefNumberAsync_UsesExpectedEndpoint()
    {
        RecordingHttpMessageHandler handler = new();
        IKSeFClient client = CreateClient(handler);

        await client.GetCollectiveIdentifiersByKsefNumberAsync(
            "ksef/number",
            "access-token",
            pageSize: 10);

        Assert.Equal(HttpMethod.Get, handler.Method);
        Assert.Equal("/v2/collective-identifiers/ksef/ksef%2Fnumber?pageSize=10", handler.RequestUri.PathAndQuery);
    }

    [Fact]
    public async Task GetCollectiveIdentifierInvoicesAsync_UsesExpectedEndpoint()
    {
        RecordingHttpMessageHandler handler = new();
        IKSeFClient client = CreateClient(handler);

        await client.GetCollectiveIdentifierInvoicesAsync(
            "collective/identifier",
            "access-token",
            continuationToken: "continuation-token",
            pageSize: 50);

        Assert.Equal(HttpMethod.Get, handler.Method);
        Assert.Equal("/v2/collective-identifiers/collective%2Fidentifier/invoices?pageSize=50", handler.RequestUri.PathAndQuery);
        Assert.Equal("continuation-token", handler.ContinuationToken);
    }

    private static IKSeFClient CreateClient(RecordingHttpMessageHandler handler)
    {
        HttpClient httpClient = new(handler)
        {
            BaseAddress = new Uri("https://localhost")
        };

        return new KSeFClient(new RestClient(httpClient));
    }

    private sealed class RecordingHttpMessageHandler : HttpMessageHandler
    {
        public HttpMethod Method { get; private set; } = HttpMethod.Get;
        public Uri RequestUri { get; private set; } = new("https://localhost");
        public string RequestBody { get; private set; } = string.Empty;
        public string AuthorizationScheme { get; private set; } = string.Empty;
        public string AuthorizationParameter { get; private set; } = string.Empty;
        public string ContinuationToken { get; private set; } = string.Empty;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Method = request.Method;
            RequestUri = request.RequestUri ?? throw new InvalidOperationException("Request URI is required.");
            RequestBody = string.Empty;
            if (request.Content is not null)
            {
#if NET5_0_OR_GREATER
                RequestBody = await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#else
                RequestBody = await request.Content.ReadAsStringAsync().ConfigureAwait(false);
#endif
            }
            AuthorizationScheme = request.Headers.Authorization?.Scheme ?? string.Empty;
            AuthorizationParameter = request.Headers.Authorization?.Parameter ?? string.Empty;
            ContinuationToken = request.Headers.Contains("x-continuation-token")
                ? request.Headers.GetValues("x-continuation-token").Single()
                : string.Empty;

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            };
        }
    }
}
