using KSeF.Client.Core.Interfaces.Rest;
using System;

namespace KSeF.Client.Core.Infrastructure.Rest
{
    ///<inheritdoc/>
    public sealed class RouteBuilder : IRouteBuilder
    {
        private readonly string _apiPrefix;
        private readonly string _defaultVersion;

        public RouteBuilder(string apiPrefix, string defaultVersion)
        {
            _apiPrefix = string.IsNullOrWhiteSpace(apiPrefix) ? "api" : apiPrefix.TrimEnd('/');
            _defaultVersion = string.IsNullOrWhiteSpace(defaultVersion) ? "v2" : defaultVersion;
        }

        ///<inheritdoc/>
        public string Build(string endpoint, string apiVersion = null)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new ArgumentException("Endpoint cannot be empty", nameof(endpoint));
            }

            string version = string.IsNullOrWhiteSpace(apiVersion) ? _defaultVersion : apiVersion;
            string clean = endpoint.TrimStart('/');
            return $"/{_apiPrefix}/{version}/{clean}";
        }

        ///<inheritdoc/>
        public string Resolve(RestRequest request, string relativeEndpoint) =>
            Build(relativeEndpoint, request?.ApiVersion);
    }
}