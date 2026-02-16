using System.Net.Http.Headers;
using System.Text;
using KSeF.Client.Core.Infrastructure.Rest;

namespace KSeF.Client.Http.Helpers;

internal static class RestRequestExtensions
{
    public static HttpRequestMessage ToHttpRequestMessage(
        this RestRequest request,
        HttpClient httpClient)
    {
        string url = request.Path.WithQuery(request.Query, httpClient.BaseAddress);
        HttpRequestMessage httpRequest = new(request.Method, url);

        ApplyHeadersAuthAccept(request.AccessToken, request.Accept, request.Headers, httpRequest);
        return httpRequest;
    }

    public static HttpRequestMessage ToHttpRequestMessage<TBody>(
        this RestRequest<TBody> request,
        HttpClient httpClient,
        string defaultContentType)
    {
        string url = request.Path.WithQuery(request.Query, httpClient.BaseAddress);
        HttpRequestMessage httpRequest = new(request.Method, url);

        if (request.Body is not null && request.Method != HttpMethod.Get)
        {
            string content = request.ContentType == defaultContentType
                ? JsonUtil.Serialize(request.Body)
                : request.Body.ToString();

            httpRequest.Content = new StringContent(content!, Encoding.UTF8, request.ContentType);
        }

        ApplyHeadersAuthAccept(request.AccessToken, request.Accept, request.Headers, httpRequest);
        return httpRequest;
    }

    private static void ApplyHeadersAuthAccept(
        string accessToken,
        string accept,
        IDictionary<string, string> headers,
        HttpRequestMessage httpRequest)
    {
        if (!string.IsNullOrWhiteSpace(accessToken))
        {
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        if (!string.IsNullOrWhiteSpace(accept))
        {
            httpRequest.Headers.Accept.Clear();
            httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(accept));
        }

        if (headers != null && headers.Count > 0)
        {
            foreach (KeyValuePair<string, string> header in headers)
            {
                httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }
    }
}
