using KSeF.Client.Core.Interfaces.Rest;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace KSeF.Client.Core.Infrastructure.Rest
{
    public sealed class RestRequest : IRestRequest
    {
        public string Path { get; private set; }
        public HttpMethod Method { get; private set; }
        public string AccessToken { get; private set; }
        public string ContentType { get; private set; } 
        public string Accept { get; private set; }
        public IDictionary<string, string> Headers { get; private set; }
        public IDictionary<string, string> Query { get; private set; }        
        public TimeSpan? Timeout { get; private set; }
        public string ApiVersion { get; private set; }

        public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(100);

        public static RestRequest New(string path, HttpMethod method) => new RestRequest(path, method);

        private RestRequest(string path, HttpMethod method)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Method = method ?? throw new ArgumentNullException(nameof(method));
            ContentType = RestContentType.Json.ToMime();
            Accept = null;
            AccessToken = null;
            Headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            Query = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            Timeout = DefaultTimeout;
            ApiVersion = null;
        }

        // --- Fluent API (mutujące – C# 7.3) ---
        public RestRequest AddAccessToken(string accessToken) { AccessToken = accessToken; return this; }
        public RestRequest AddHeader(string name, string value)
        {
            Dictionary<string, string> dictionaryHeaders = 
                new Dictionary<string, string>(Headers, StringComparer.OrdinalIgnoreCase)
            {
                [name] = value
            };
            Headers = dictionaryHeaders;
            return this;
        }
        public RestRequest AddQueryParameter(string name, string value)
        {
            Dictionary<string, string> dictionaryParameters = 
                new Dictionary<string, string>(Query, StringComparer.OrdinalIgnoreCase)
            {
                [name] = value
            };
            Query = dictionaryParameters; 
            return this;
        }
        public RestRequest WithAccept(string accept) { Accept = accept; return this; }
        public RestRequest WithTimeout(TimeSpan timeout) { Timeout = timeout; return this; }
        public RestRequest WithContentType(string contentType) { ContentType = contentType; return this; }
        public RestRequest WithApiVersion(string apiVersion) { ApiVersion = apiVersion; return this; }

        public RestRequest<TBody> WithBody<TBody>(TBody body, RestContentType contentType = RestContentType.Json)
        {
            if (Equals(body, null))
            {
                throw new ArgumentNullException(nameof(body));
            }

            RestRequest<TBody> requestBuilder = RestRequestBuilder.New(Path, Method, body, contentType);

            if (!string.IsNullOrEmpty(AccessToken))
            {
                requestBuilder.AddAccessToken(AccessToken);
            }

            if (!string.IsNullOrEmpty(Accept))
            {
                requestBuilder.WithAccept(Accept);
            }

            if (!string.IsNullOrEmpty(ApiVersion))
            {
                requestBuilder.WithApiVersion(ApiVersion);
            }

            if (Timeout.HasValue)
            {
                requestBuilder.WithTimeout(Timeout.Value);
            }

            foreach (KeyValuePair<string, string> header in Headers)
            {
                requestBuilder.AddHeader(header.Key, header.Value);
            }

            foreach (KeyValuePair<string, string> queryParameter in Query)
            {
                requestBuilder.AddQueryParameter(queryParameter.Key, queryParameter.Value);
            }

            return requestBuilder;
        }
    }
}
