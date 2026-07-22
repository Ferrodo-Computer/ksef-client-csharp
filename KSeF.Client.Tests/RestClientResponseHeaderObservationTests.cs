using KSeF.Client.Core.Infrastructure.Rest;
using KSeF.Client.Http;
using System.Net;

namespace KSeF.Client.Tests;

/// <summary>
/// Testy jednostkowe obserwacji skonfigurowanych nagłówków odpowiedzi przez
/// <see cref="RestClient.ObserveResponseHeaders"/>.
/// </summary>
/// <remarks>
/// Subskrypcja jest statyczna (wspólna dla wszystkich instancji RestClient w procesie), dlatego każdy
/// test zwalnia swoją subskrypcję przez <c>using</c>, żeby nie wpływać na inne testy.
/// </remarks>
public class RestClientResponseHeaderObservationTests
{
    private const string SystemWarningHeader = "X-System-Warning";

    [Fact]
    public async Task SendAsync_WhenObservationDisabled_DoesNotRaiseEvent()
    {
        RestClient client = CreateClient(
            new ResponseHeaderObservationOptions { Enabled = false },
            responseHeaders: new() { [SystemWarningHeader] = "Uwaga: przerwa techniczna" });

        ResponseHeaderObservedEventArgs observed = null;
        using (RestClient.ObserveResponseHeaders((_, e) => observed = e))
        {
            await client.SendAsync<object, object>(HttpMethod.Get, "https://localhost/test", null, null, "application/json", additionalHeaders: null, CancellationToken.None);
        }

        Assert.Null(observed);
    }

    [Fact]
    public async Task SendAsync_WhenObservationEnabledAndHeaderPresent_RaisesEventWithValues()
    {
        ResponseHeaderObservationOptions options = new() { Enabled = true };
        RestClient client = CreateClient(
            options,
            responseHeaders: new() { [SystemWarningHeader] = "Uwaga: przerwa techniczna" });

        ResponseHeaderObservedEventArgs observed = null;
        using (RestClient.ObserveResponseHeaders((_, e) => observed = e))
        {
            await client.SendAsync<object, object>(HttpMethod.Get, "https://localhost/test", null, null, "application/json", additionalHeaders: null, CancellationToken.None);
        }

        Assert.NotNull(observed);
        Assert.Equal(SystemWarningHeader, observed.HeaderName, ignoreCase: true);
        Assert.Contains("Uwaga: przerwa techniczna", observed.Values);
        Assert.Equal(HttpMethod.Get, observed.RequestMethod);
    }

    [Fact]
    public async Task SendAsync_WhenObservationEnabledAndHeaderAbsent_DoesNotRaiseEvent()
    {
        ResponseHeaderObservationOptions options = new() { Enabled = true };
        RestClient client = CreateClient(options, responseHeaders: null);

        ResponseHeaderObservedEventArgs observed = null;
        using (RestClient.ObserveResponseHeaders((_, e) => observed = e))
        {
            await client.SendAsync<object, object>(HttpMethod.Get, "https://localhost/test", null, null, "application/json", additionalHeaders: null, CancellationToken.None);
        }

        Assert.Null(observed);
    }

    [Fact]
    public async Task SendAsync_HeaderNameMatchIsCaseInsensitive()
    {
        ResponseHeaderObservationOptions options = new() { Enabled = true };
        RestClient client = CreateClient(
            options,
            responseHeaders: new() { ["x-system-warning"] = "lower-case header" });

        ResponseHeaderObservedEventArgs observed = null;
        using (RestClient.ObserveResponseHeaders((_, e) => observed = e))
        {
            await client.SendAsync<object, object>(HttpMethod.Get, "https://localhost/test", null, null, "application/json", additionalHeaders: null, CancellationToken.None);
        }

        Assert.NotNull(observed);
        Assert.Contains("lower-case header", observed.Values);
    }

    [Fact]
    public async Task SendAsync_CustomHeaderNameAddedToOptions_IsObserved()
    {
        ResponseHeaderObservationOptions options = new() { Enabled = true };
        options.HeaderNames.Add("X-Custom-Header");

        RestClient client = CreateClient(
            options,
            responseHeaders: new() { ["X-Custom-Header"] = "custom value" });

        ResponseHeaderObservedEventArgs observed = null;
        using (RestClient.ObserveResponseHeaders((_, e) => observed = e))
        {
            await client.SendAsync<object, object>(HttpMethod.Get, "https://localhost/test", null, null, "application/json", additionalHeaders: null, CancellationToken.None);
        }

        Assert.NotNull(observed);
        Assert.Equal("X-Custom-Header", observed.HeaderName, ignoreCase: true);
        Assert.Contains("custom value", observed.Values);
    }

    [Fact]
    public async Task SendWithHeadersAsync_WhenObservationEnabled_AlsoRaisesEvent()
    {
        ResponseHeaderObservationOptions options = new() { Enabled = true };
        RestClient client = CreateClient(
            options,
            responseHeaders: new() { [SystemWarningHeader] = "Uwaga: przerwa techniczna" });

        ResponseHeaderObservedEventArgs observed = null;
        using (RestClient.ObserveResponseHeaders((_, e) => observed = e))
        {
            await client.SendWithHeadersAsync<object, object>(HttpMethod.Get, "https://localhost/test");
        }

        Assert.NotNull(observed);
        Assert.Contains("Uwaga: przerwa techniczna", observed.Values);
    }

    [Fact]
    public async Task ObserveResponseHeaders_AfterDispose_NoLongerReceivesEvents()
    {
        ResponseHeaderObservationOptions options = new() { Enabled = true };
        RestClient client = CreateClient(
            options,
            responseHeaders: new() { [SystemWarningHeader] = "Uwaga: przerwa techniczna" });

        ResponseHeaderObservedEventArgs observed = null;
        IDisposable subscription = RestClient.ObserveResponseHeaders((_, e) => observed = e);
        subscription.Dispose();

        await client.SendAsync<object, object>(HttpMethod.Get, "https://localhost/test", null, null, "application/json", additionalHeaders: null, CancellationToken.None);

        Assert.Null(observed);
    }

    [Fact]
    public async Task SendAsync_WhenHandlerThrows_ExceptionIsSwallowedAndOtherHandlersStillRun()
    {
        ResponseHeaderObservationOptions options = new() { Enabled = true };
        RestClient client = CreateClient(
            options,
            responseHeaders: new() { [SystemWarningHeader] = "Uwaga: przerwa techniczna" });

        bool secondHandlerRan = false;

        using (RestClient.ObserveResponseHeaders((_, _) => throw new InvalidOperationException("Błąd w handlerze subskrybenta.")))
        using (RestClient.ObserveResponseHeaders((_, _) => secondHandlerRan = true))
        {
            // Act - wywołanie API nie może wybuchnąć, mimo że pierwszy subskrybent rzuca wyjątek.
            object body = await client.SendAsync<object, object>(HttpMethod.Get, "https://localhost/test", null, null, "application/json", additionalHeaders: null, CancellationToken.None);

            Assert.Null(body);
        }

        Assert.True(secondHandlerRan);
    }

    private static RestClient CreateClient(
        ResponseHeaderObservationOptions options,
        Dictionary<string, string> responseHeaders)
    {
        FakeHttpMessageHandler handler = new(responseHeaders);
        HttpClient http = new(handler) { BaseAddress = new Uri("https://localhost") };
        return new RestClient(http, options);
    }

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Dictionary<string, string> _responseHeaders;

        public FakeHttpMessageHandler(Dictionary<string, string> responseHeaders)
        {
            _responseHeaders = responseHeaders;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            HttpResponseMessage response = new(HttpStatusCode.NoContent);

            if (_responseHeaders is not null)
            {
                foreach (KeyValuePair<string, string> header in _responseHeaders)
                {
                    response.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            return Task.FromResult(response);
        }
    }
}
