using KSeF.Client.Core.Interfaces.Rest;
using System;

namespace KSeF.Client.Core.Infrastructure.Rest
{
    ///<inheritdoc/>
    public sealed class RouteBuilder : IRouteBuilder
    {
        private readonly string _defaultVersion;

        public RouteBuilder(string defaultVersion)
        {
            _defaultVersion = string.IsNullOrWhiteSpace(defaultVersion) ? "v2" : defaultVersion;
        }

        ///<inheritdoc/>
        public string Build(string endpoint, string apiVersion = null)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new ArgumentException("Adres nie może być pusty", nameof(endpoint));
            }

            string version = string.IsNullOrWhiteSpace(apiVersion) ? _defaultVersion : apiVersion;
            string clean = endpoint.TrimStart('/');
            return $"/{version}/{clean}";
        }

        ///<inheritdoc/>
        public string Resolve(RestRequest request, string relativeEndpoint) =>
            Build(relativeEndpoint, request?.ApiVersion);
    }
}