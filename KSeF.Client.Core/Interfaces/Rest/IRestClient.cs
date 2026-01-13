using KSeF.Client.Core.Infrastructure.Rest;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace KSeF.Client.Core.Interfaces.Rest
{
    /// <summary>
    /// Klient REST obsługujący żądania GET, POST i DELETE z opcjonalną autoryzacją,
    /// serializacją/deserializacją treści oraz strukturalną obsługą błędów.
    /// </summary>
    public interface IRestClient
    {
        /// <summary>
        /// Wysyła żądanie HTTP i zwraca odpowiedź w postaci obiektu typu TResponse.
        /// </summary>
        /// <typeparam name="TResponse">Typ obiektu odpowiedzi.</typeparam>
        /// <typeparam name="TRequest">Typ obiektu żądania.</typeparam>
        /// <param name="method">Metoda HTTP (np. GET, POST).</param>
        /// <param name="url">Adres URL żądania.</param>
        /// <param name="requestBody">Treść żądania (opcjonalne).</param>
        /// <param name="token">Token uwierzytelniający accessToken/refreshToken (opcjonalne).</param>
        /// <param name="contentType">Typ treści żądania (domyślnie "application/json").</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <param name="additionalHeaders">Dodatkowe nagłówki HTTP (opcjonalne).</param>
        /// <returns>Obiekt typu TResponse</returns>
        Task<TResponse> SendAsync<TResponse, TRequest>(HttpMethod method, string url, TRequest requestBody = default, string token = null, string contentType = "application/json", Dictionary<string, string> additionalHeaders = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Wysyła żądanie HTTP z podanym HttpContent, bez serializacji obiektów.
        /// Przeznaczone do ręcznego przesyłania danych binarnych (np. plików, strumieni).
        /// Nie modyfikuje zawartości ani nie zmienia nagłówków.
        /// </summary>
        /// <param name="method">Metoda HTTP (np. PUT, POST).</param>
        /// <param name="url">Adres URL żądania.</param>
        /// <param name="content">Zawartość HTTP do wysłania (np. ByteArrayContent, StreamContent).</param>
        /// <param name="additionalHeaders">Dodatkowe nagłówki HTTP (opcjonalne).</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task SendAsync(
            HttpMethod method,
            string url,
            HttpContent content,
            IDictionary<string, string> additionalHeaders = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Wysyła żądanie HTTP z podanym HttpContent, bez serializacji obiektów.
        /// Przeznaczone do ręcznego przesyłania danych binarnych (np. plików, strumieni).
        /// Nie modyfikuje zawartości ani nie zmienia nagłówków.
        /// </summary>
        /// <param name="method">Metoda HTTP (np. PUT, POST).</param>
        /// <param name="url">Adres URL żądania.</param>
        /// <param name="requestBody"></param>
        /// <param name="token"></param>
        /// <param name="contentType"></param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task<TResponse> SendAsync<TResponse, TRequest>(HttpMethod method, string url, TRequest requestBody = default, string token = null, string contentType = "application/json", CancellationToken cancellationToken = default);

        /// <summary>
        /// Wysyła żądanie HTTP bez oczekiwania na odpowiedź w postaci obiektu.
        /// </summary>
        /// <typeparam name="TRequest">Typ obiektu żądania.</typeparam>
        /// <param name="method">Metoda HTTP (np. GET, POST).</param>
        /// <param name="url">Adres URL żądania.</param>
        /// <param name="requestBody">Treść żądania (opcjonalne).</param>
        /// <param name="token">Token uwierzytelniający accessToken/refreshToken (opcjonalne).</param>
        /// <param name="contentType">Typ treści żądania (domyślnie "application/json").</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task SendAsync<TRequest>(HttpMethod method, string url, TRequest requestBody = default, string token = null, string contentType = "application/json", CancellationToken cancellationToken = default);

        /// <summary>
        /// Wysyła żądanie HTTP bez treści żądania i bez oczekiwania na odpowiedź w postaci obiektu.
        /// </summary>
        /// <param name="method">Metoda HTTP (np. GET, POST).</param>
        /// <param name="url">Adres URL żądania.</param>
        /// <param name="token">Token uwierzytelniający accessToken/refreshToken (opcjonalne).</param>
        /// <param name="contentType">Typ treści żądania (domyślnie "application/json").</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task SendAsync(HttpMethod method, string url, string token = null, string contentType = "application/json", CancellationToken cancellationToken = default);

        /// <summary>
        /// Wysyła żądanie opisane przez <see cref="RestRequest"/> i zwraca zdeserializowaną odpowiedź.
        /// Zalecane, gdy potrzebujesz query params, niestandardowych nagłówków, Accept, per-request timeout.
        /// </summary>
        Task<TResponse> SendAsync<TResponse>(
            RestRequest request, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Wysyła żądanie opisane przez <see cref="RestRequest"/> bez oczekiwania na wynik w postaci obiektu.
        /// </summary>
        Task SendAsync(
            RestRequest request,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Alias nazewniczy.
        /// </summary>
        Task<TResponse> ExecuteAsync<TResponse>(
            RestRequest request, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Alias nazewniczy.
        /// </summary>
        Task ExecuteAsync(RestRequest request, CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        Task<TResponse> ExecuteAsync<TResponse, TRequest>(
            RestRequest<TRequest> request, 
            CancellationToken 
            cancellationToken = default);

        /// <summary>
        /// Wysyła żądanie HTTP i zwraca odpowiedź wraz z nagłówkami.
        /// </summary>
        Task<RestResponse<TResponse>> SendWithHeadersAsync<TResponse, TRequest>(
            HttpMethod method,
            string url,
            TRequest requestBody = default,
            string token = null,
            string contentType = "application/json",
            Dictionary<string, string> additionalHeaders = null,
            CancellationToken cancellationToken = default);
    }
}