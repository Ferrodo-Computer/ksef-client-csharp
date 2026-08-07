using KSeF.Client.Core.Exceptions;
using KSeF.Client.Core.Infrastructure.Rest;
using KSeF.Client.Core.Interfaces.Rest;
using KSeF.Client.Http.Helpers;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
namespace KSeF.Client.Http;

/// <inheritdoc />
public sealed partial class RestClient(
    HttpClient httpClient,
    ResponseHeaderObservationOptions responseHeaderObservationOptions = null) : IRestClient
{
    private readonly HttpClient httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    private readonly ResponseHeaderObservationOptions responseHeaderObservationOptions =
        responseHeaderObservationOptions ?? new ResponseHeaderObservationOptions();

    private static event EventHandler<ResponseHeaderObservedEventArgs> responseHeaderObserved;

    /// <summary>
    /// Subskrybuje zgłoszenia zdarzeń, gdy odpowiedź HTTP zawiera jeden z nagłówków skonfigurowanych w
    /// <see cref="ResponseHeaderObservationOptions.HeaderNames"/> (obserwacja musi być włączona
    /// przez <see cref="ResponseHeaderObservationOptions.Enabled"/>). Subskrypcja jest statyczna -
    /// wspólna dla wszystkich instancji <see cref="RestClient"/> w procesie, niezależnie od tego,
    /// jak <c>IRestClient</c> jest zarejestrowany w kontenerze DI (Transient/Scoped/Singleton).
    /// </summary>
    /// <remarks>
    /// Zwrócony <see cref="IDisposable"/> należy zwolnić (najlepiej przez <c>using</c>), gdy subskrybent
    /// przestaje być potrzebny - dopóki nie zostanie zwolniony, statyczna subskrypcja trzyma silną
    /// referencję do handlera przez cały czas życia procesu.
    /// </remarks>
    /// <example>
    /// <code>
    /// using IDisposable subscription = RestClient.ObserveResponseHeaders((sender, e) =>
    /// {
    ///     if (e.HeaderName.Equals("X-System-Warning", StringComparison.OrdinalIgnoreCase))
    ///         logger.LogWarning("KSeF: {Warning}", string.Join("; ", e.Values));
    /// });
    /// </code>
    /// </example>
    public static IDisposable ObserveResponseHeaders(EventHandler<ResponseHeaderObservedEventArgs> handler)
    {
        if (handler is null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        responseHeaderObserved += handler;
        return new ResponseHeaderObservationSubscription(handler);
    }

    private sealed class ResponseHeaderObservationSubscription : IDisposable
    {
        private EventHandler<ResponseHeaderObservedEventArgs> handler;

        public ResponseHeaderObservationSubscription(EventHandler<ResponseHeaderObservedEventArgs> handler)
        {
            this.handler = handler;
        }

        public void Dispose()
        {
            if (handler is null)
            {
                return;
            }

            responseHeaderObserved -= handler;
            handler = null;
        }
    }

    /// <summary>
    /// Domyślny typ treści żądania REST.
    /// </summary>
    public const string DefaultContentType = "application/json";

    /// <summary>
    /// Typ treści XML.
    /// </summary>
    public const string XmlContentType = "application/xml";

    private const string UnauthorizedText = "Unauthorized";
    private const string ForbiddenText = "Forbidden";
    private const string GoneText = "Gone";
    private const string UnknownText = "Unknown";
    private const string ProblemDetailsText = "ProblemDetails";
    private const string ServiceNameText = "KSeF API";
    private const string NotFoundText = "Not found";
    private const string RateLimitText = "Przekroczono limit ilości zapytań do API (HTTP 429)";
    private const string BearerScheme = "Bearer";
    private const string UnknownMediaTypeText = "nieznany";

    /// <inheritdoc />
    public async Task<TResponse> SendAsync<TResponse, TRequest>(
        HttpMethod method,
        string url,
        TRequest requestBody = default,
        string token = null,
        string contentType = "application/json",
        CancellationToken cancellationToken = default)
    {
        return await SendAsync<TResponse, TRequest>(method, url, requestBody, token, contentType, additionalHeaders: null, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<TResponse> SendAsync<TResponse, TRequest>(
        HttpMethod method,
        string url,
        TRequest requestBody = default,
        string token = null,
        string contentType = RestContentTypeExtensions.DefaultContentType,
        Dictionary<string, string> additionalHeaders = null,
        CancellationToken cancellationToken = default)
    {
        RestResponse<TResponse> response = await SendWithHeadersAsync<TResponse, TRequest>(
            method,
            url,
            requestBody,
            token,
            contentType,
            additionalHeaders,
            cancellationToken).ConfigureAwait(false);

        return response.Body;
    }

    /// <inheritdoc />
    public async Task<RestResponse<TResponse>> SendWithHeadersAsync<TResponse, TRequest>(
        HttpMethod method,
        string url,
        TRequest requestBody = default,
        string token = null,
        string contentType = RestContentTypeExtensions.DefaultContentType,
        Dictionary<string, string> additionalHeaders = null,
        CancellationToken cancellationToken = default)
    {
        Guard.ThrowIfNull(method);
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("Adres URL nie może być pusty.", nameof(url));
        }

        using HttpRequestMessage httpRequestMessage = new(method, httpClient.BaseAddress.Combine(url));

        bool shouldSendBody = method != HttpMethod.Get &&
                              !EqualityComparer<TRequest>.Default.Equals(requestBody, default);

        if (shouldSendBody)
        {
            string requestContent = RestContentTypeExtensions.IsDefaultType(contentType)
                ? JsonUtil.Serialize(requestBody)
                : requestBody?.ToString();

            if (!string.IsNullOrEmpty(requestContent))
            {
                httpRequestMessage.Content = new StringContent(requestContent, Encoding.UTF8, contentType);
            }
        }

        if (!string.IsNullOrWhiteSpace(token))
        {
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue(BearerScheme, token);
        }

        if (additionalHeaders is not null)
        {
            foreach (KeyValuePair<string, string> header in additionalHeaders)
            {
                httpRequestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return await SendCoreWithHeadersAsync<TResponse>(httpRequestMessage, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task SendAsync(
        HttpMethod method,
        string url,
        HttpContent content,
        IDictionary<string, string> additionalHeaders = null,
        CancellationToken cancellationToken = default)
    {
        Guard.ThrowIfNull(method);
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("Adres URL nie może być pusty.", nameof(url));
        }

        Guard.ThrowIfNull(content);

        using HttpRequestMessage httpRequestMessage = new(method, httpClient.BaseAddress.Combine(url))
        {
            Content = content
        };

        if (additionalHeaders is not null)
        {
            foreach (KeyValuePair<string, string> header in additionalHeaders)
            {
                httpRequestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        await SendCoreAsync<object>(httpRequestMessage, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task SendAsync<TRequest>(
        HttpMethod method,
        string url,
        TRequest requestBody = default,
        string token = null,
        string contentType = RestContentTypeExtensions.DefaultContentType,
        CancellationToken cancellationToken = default)
    {
        await SendAsync<object, TRequest>(method, url, requestBody, token, contentType, additionalHeaders: null, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task SendAsync(
        HttpMethod method,
        string url,
        string token = null,
        string contentType = RestContentTypeExtensions.DefaultContentType,
        CancellationToken cancellationToken = default)
    {
        Guard.ThrowIfNull(method);
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new ArgumentException("Adres URL nie może być pusty.", nameof(url));
        }

        using HttpRequestMessage httpRequestMessage = new(method, httpClient.BaseAddress.Combine(url));

        if (!string.IsNullOrWhiteSpace(token))
        {
            httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue(BearerScheme, token);
        }

        await SendCoreAsync<string>(httpRequestMessage, cancellationToken).ConfigureAwait(false);
    }

    // ================== RestRequest overloads ==================
    /// <inheritdoc />
    public async Task<TResponse> SendAsync<TResponse>(RestRequest request, CancellationToken cancellationToken = default)
    {
        Guard.ThrowIfNull(request);

        using HttpRequestMessage httpRequestMessage = request.ToHttpRequestMessage(httpClient);
        using CancellationTokenSource cancellationTokenSource = CreateTimeoutCancellationTokenSource(request.Timeout, cancellationToken);

        return await SendCoreAsync<TResponse>(httpRequestMessage, cancellationTokenSource.Token).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task SendAsync(RestRequest request, CancellationToken cancellationToken = default)
        => SendAsync<object>(request, cancellationToken);

    /// <inheritdoc />
    public Task<TResponse> ExecuteAsync<TResponse>(RestRequest request, CancellationToken cancellationToken = default)
        => SendAsync<TResponse>(request, cancellationToken);

    /// <inheritdoc />
    public Task ExecuteAsync(RestRequest request, CancellationToken cancellationToken = default)
        => SendAsync(request, cancellationToken);

    /// <inheritdoc />
    public Task<TResponse> ExecuteAsync<TResponse, TRequest>(RestRequest<TRequest> request, CancellationToken cancellationToken = default)
        => SendAsync<TResponse, TRequest>(request, cancellationToken);

    /// <inheritdoc />
    public async Task<TResponse> SendAsync<TResponse, TRequest>(RestRequest<TRequest> request, CancellationToken cancellationToken = default)
    {
        Guard.ThrowIfNull(request);

        using HttpRequestMessage httpRequestMessage = request.ToHttpRequestMessage(httpClient, DefaultContentType);
        using CancellationTokenSource cancellationTokenSource = CreateTimeoutCancellationTokenSource(request.Timeout, cancellationToken);

        return await SendCoreAsync<TResponse>(httpRequestMessage, cancellationTokenSource.Token).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task SendAsync<TRequest>(RestRequest<TRequest> request, CancellationToken cancellationToken = default)
        => SendAsync<object, TRequest>(request, cancellationToken);

    // ================== Core ==================
    private async Task<T> SendCoreAsync<T>(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken)
    {
        using HttpResponseMessage httpResponseMessage = await httpClient
            .SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);

        ObserveConfiguredHeaders(httpRequestMessage, httpResponseMessage);

        bool hasContent = httpResponseMessage.HasBody(httpRequestMessage.Method);

        if (httpResponseMessage.IsSuccessStatusCode)
        {
            if (!hasContent || typeof(T) == typeof(object))
            {
                return default!;
            }

            if (typeof(T) == typeof(string))
            {
                string responseText = await ReadContentAsync(httpResponseMessage, cancellationToken).ConfigureAwait(false);
                return (T)(object)(responseText ?? string.Empty);
            }

            MediaTypeHeaderValue contentTypeHeader = httpResponseMessage.Content?.Headers?.ContentType;
            string mediaType = contentTypeHeader?.MediaType;

            if (!IsJsonMediaType(mediaType))
            {
                throw new KsefApiException($"Nieoczekiwany typ treści '{mediaType ?? UnknownMediaTypeText}' dla {typeof(T).Name}.", httpResponseMessage.StatusCode);
            }

#if NETSTANDARD2_0
            using Stream responseStream = await httpResponseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);
#else
            using Stream responseStream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
#endif
            return await JsonUtil.DeserializeAsync<T>(responseStream).ConfigureAwait(false);
        }

        await HandleInvalidStatusCode(httpResponseMessage, cancellationToken).ConfigureAwait(false);
        throw new InvalidOperationException("HandleInvalidStatusCode musi zgłosić wyjątek.");
    }

    private async Task<RestResponse<T>> SendCoreWithHeadersAsync<T>(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken)
    {
        using HttpResponseMessage httpResponseMessage = await httpClient
            .SendAsync(httpRequestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);

        ObserveConfiguredHeaders(httpRequestMessage, httpResponseMessage);

        bool hasContent = httpResponseMessage.HasBody(httpRequestMessage.Method);

        Dictionary<string, IEnumerable<string>> headers = new(StringComparer.OrdinalIgnoreCase);

        foreach (KeyValuePair<string, IEnumerable<string>> header in httpResponseMessage.Headers)
        {
            headers[header.Key] = header.Value;
        }

        if (httpResponseMessage.Content is not null)
        {
            foreach (KeyValuePair<string, IEnumerable<string>> header in httpResponseMessage.Content.Headers)
            {
                headers[header.Key] = header.Value;
            }
        }

        if (httpResponseMessage.IsSuccessStatusCode)
        {
            if (!hasContent || typeof(T) == typeof(object))
            {
                return new RestResponse<T>(default!, headers);
            }

            if (typeof(T) == typeof(string))
            {
                string responseText = await ReadContentAsync(httpResponseMessage, cancellationToken).ConfigureAwait(false);
                return new RestResponse<T>((T)(object)(responseText ?? string.Empty), headers);
            }

            MediaTypeHeaderValue contentTypeHeader = httpResponseMessage.Content?.Headers?.ContentType;
            string mediaType = contentTypeHeader?.MediaType;

            if (!IsJsonMediaType(mediaType))
            {
                throw new KsefApiException($"Nieoczekiwany typ treści '{mediaType ?? UnknownMediaTypeText}' dla {typeof(T).Name}.", httpResponseMessage.StatusCode);
            }

#if NETSTANDARD2_0
            using Stream responseStream = await httpResponseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);
#else
            using Stream responseStream = await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
#endif
            T body = await JsonUtil.DeserializeAsync<T>(responseStream).ConfigureAwait(false);
            return new RestResponse<T>(body, headers);
        }

        await HandleInvalidStatusCode(httpResponseMessage, cancellationToken).ConfigureAwait(false);
        throw new InvalidOperationException("HandleInvalidStatusCode musi zgłosić wyjątek.");
    }


    /// <summary>
    /// Zgłasza zdarzenie subskrybowane przez <see cref="ObserveResponseHeaders"/> dla
    /// nagłówków skonfigurowanych w <see cref="ResponseHeaderObservationOptions"/>, jeśli obserwacja
    /// jest włączona.
    /// </summary>
    private void ObserveConfiguredHeaders(HttpRequestMessage httpRequestMessage, HttpResponseMessage httpResponseMessage)
    {
        if (!responseHeaderObservationOptions.Enabled || responseHeaderObserved is null)
        {
            return;
        }

        foreach (string headerName in responseHeaderObservationOptions.HeaderNames)
        {
            IEnumerable<string> values = null;

            if (httpResponseMessage.Headers.TryGetValues(headerName, out IEnumerable<string> responseHeaderValues))
            {
                values = responseHeaderValues;
            }
            else if (httpResponseMessage.Content is not null &&
                     httpResponseMessage.Content.Headers.TryGetValues(headerName, out IEnumerable<string> contentHeaderValues))
            {
                values = contentHeaderValues;
            }

            if (values is null)
            {
                continue;
            }

            RaiseResponseHeaderObserved(
                new ResponseHeaderObservedEventArgs(headerName, values.ToList(), httpRequestMessage.Method, httpRequestMessage.RequestUri));
        }
    }

    /// <summary>
    /// Wywołuje każdego subskrybenta <see cref="responseHeaderObserved"/> osobno, izolując wyjątki -
    /// błąd w jednym handlerze (np. w logowaniu) nie może przerwać ani pozostałych subskrybentów,
    /// ani prawdziwego wywołania API, do którego ta obserwacja jest tylko efektem ubocznym.
    /// </summary>
    private void RaiseResponseHeaderObserved(ResponseHeaderObservedEventArgs args)
    {
        Delegate[] handlers = responseHeaderObserved?.GetInvocationList();
        if (handlers is null)
        {
            return;
        }

        foreach (Delegate handler in handlers)
        {
            try
            {
                ((EventHandler<ResponseHeaderObservedEventArgs>)handler).Invoke(this, args);
            }
            catch (Exception)
            {
				// Celowo ignorowane.
				// Mechanizm obserwacji ma charakter diagnostyczny i nie może wpływać
				// na przebieg właściwego wywołania API.
			}
		}
    }

    /// <summary>
    /// Mapuje nie-2xx odpowiedzi na wyjątki.
    /// </summary>
    private static async Task HandleInvalidStatusCode(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (StatusCodeHandlers.TryGetValue(response.StatusCode, out Func<HttpResponseMessage, CancellationToken, Task> handler))
        {
            await handler(response, cancellationToken).ConfigureAwait(false);
            return;
        }

        await HandleOtherErrorsAsync(response, cancellationToken).ConfigureAwait(false);
    }

    private static bool IsJsonMediaType(string mediaType)
    {
        return !string.IsNullOrEmpty(mediaType) &&
               mediaType.Contains("json", StringComparison.OrdinalIgnoreCase);
    }

    private static CancellationTokenSource CreateTimeoutCancellationTokenSource(TimeSpan? perRequestTimeout, CancellationToken cancellationToken)
    {
        if (perRequestTimeout is null)
        {
            return CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        }

        CancellationTokenSource cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cancellationTokenSource.CancelAfter(perRequestTimeout.Value);
        return cancellationTokenSource;
    }

    private static async Task<string> ReadContentAsync(HttpResponseMessage resp, CancellationToken ct)
    {
        if (resp?.Content is null)
        {
            return null;
        }

#if NETSTANDARD2_0
        return await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
#else
        return await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
#endif
    }
}