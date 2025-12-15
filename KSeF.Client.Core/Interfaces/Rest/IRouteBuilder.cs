using KSeF.Client.Core.Infrastructure.Rest;
using System;

namespace KSeF.Client.Core.Interfaces.Rest
{
    /// <summary>
    /// Udostępnia metody do tworzenia adresów endpointów API.
    /// </summary>
    public interface IRouteBuilder
    {
        /// <summary>
        /// Tworzy ścieżkę względną w formacie "/{apiPrefix}/{version}/{endpoint}".
        /// </summary>
        /// <param name="endpoint">Względny endpoint (bez poprzedzającego '/').</param>
        /// <param name="apiVersion">
        /// Opcjonalna wersja API; gdy null lub pusta, używana jest wersja domyślna.
        /// </param>
        /// <returns>Zbudowana ścieżka względna.</returns>
        /// <exception cref="ArgumentException">
        /// Gdy <paramref name="endpoint"/> jest pusty lub składa się wyłącznie z białych znaków.
        /// </exception>
        string Build(string endpoint, string apiVersion = null);

        /// <summary>
        /// Tworzy ścieżkę względną dla żądania REST, korzystając z wersji API ustawionej w żądaniu (jeśli podana).
        /// </summary>
        /// <param name="request">Opis żądania HTTP.</param>
        /// <param name="relativeEndpoint">Względny endpoint (bez poprzedzającego '/').</param>
        /// <returns>Zbudowana ścieżka względna.</returns>
        /// <exception cref="ArgumentException">
        /// Gdy <paramref name="relativeEndpoint"/> jest pusty lub składa się wyłącznie z białych znaków.
        /// </exception>
        string Resolve(RestRequest request, string relativeEndpoint);
    }
}