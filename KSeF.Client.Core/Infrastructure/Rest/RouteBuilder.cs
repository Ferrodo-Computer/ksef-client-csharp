using KSeF.Client.Core.Interfaces.Rest;
using System;

namespace KSeF.Client.Core.Infrastructure.Rest
{

    public sealed class RouteBuilder : IRouteBuilder
    {
        private readonly string _apiPrefix;
        private readonly string _defaultVersion;

        public RouteBuilder(string apiPrefix, string defaultVersion)
        {
            _apiPrefix = string.IsNullOrWhiteSpace(apiPrefix) ? "api" : apiPrefix.TrimEnd('/');
            _defaultVersion = string.IsNullOrWhiteSpace(defaultVersion) ? "v2" : defaultVersion;
        }

        /// <summary>
        /// Buduje ścieżkę względną w formacie "/{apiPrefix}/{version}/{endpoint}".
        /// </summary>
        /// <param name="endpoint">Względny endpoint (bez poprzedzającego '/').</param>
        /// <param name="apiVersion">Opcjonalna wersja API; gdy null używana jest wersja domyślna.</param>
        /// <returns>Zbudowana ścieżka względna.</returns>
        /// <exception cref="ArgumentException">Gdy endpoint jest pusty lub składa się wyłącznie z białych znaków.</exception>
        public string Build(string endpoint, string apiVersion = null)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
                throw new ArgumentException("Endpoint cannot be empty", nameof(endpoint));

            string version = string.IsNullOrWhiteSpace(apiVersion) ? _defaultVersion : apiVersion;
            string clean = endpoint.TrimStart('/');
            return $"/{_apiPrefix}/{version}/{clean}";
        }

        /// <summary>
        /// Wyznacza pełną ścieżkę dla żądania REST, wykorzystując jego wersję API (jeśli podana).
        /// </summary>
        /// <param name="request">Opis żądania HTTP.</param>
        /// <param name="relativeEndpoint">Względny endpoint.</param>
        /// <returns>Zbudowany adres ścieżki.</returns>
        public string Resolve(RestRequest request, string relativeEndpoint) =>
            Build(relativeEndpoint, request?.ApiVersion);
    }
}