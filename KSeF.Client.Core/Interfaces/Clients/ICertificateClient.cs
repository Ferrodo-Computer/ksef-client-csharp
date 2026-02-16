using KSeF.Client.Core.Exceptions;
using KSeF.Client.Core.Models.Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace KSeF.Client.Core.Interfaces.Clients
{
    /// <summary>
    /// Klient służący do zarządzania certyfikatami.
    /// </summary>
    public interface ICertificateClient
    {

        /// <summary>
        /// Pobranie danych o limitach certyfikatów.
        /// Zwraca informacje o limitach certyfikatów oraz informacje czy użytkownik może zawnioskować o certyfikat.
        /// </summary>
        /// <param name="accessToken">Access token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns><see cref="CertificateLimitResponse"/></returns>
        /// <exception cref="KsefApiException">Nieprawidłowe żądanie. (400 Bad request)</exception>
        /// <exception cref="KsefApiException">Brak autoryzacji. (401 Unauthorized)</exception>
        Task<CertificateLimitResponse> GetCertificateLimitsAsync(string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Zwraca dane wymagane do przygotowania wniosku certyfikacyjnego.
        /// </summary>
        /// <param name="accessToken">Access token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns><see cref="CertificateEnrollmentsInfoResponse"/></returns>
        /// <exception cref="KsefApiException">Nieprawidłowe żądanie. (400 Bad request)</exception>
        /// <exception cref="KsefApiException">Brak autoryzacji. (401 Unauthorized)</exception>
        Task<CertificateEnrollmentsInfoResponse> GetCertificateEnrollmentDataAsync(string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Przyjmuje wniosek certyfikacyjny i rozpoczyna jego przetwarzanie.
        /// </summary>
        /// <param name="requestPayload"><see cref="SendCertificateEnrollmentRequest"/></param>
        /// <param name="accessToken">Access token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns><see cref="CertificateEnrollmentResponse"/></returns>
        /// <exception cref="KsefApiException">Nieprawidłowe żądanie. (400 Bad request)</exception>
        /// <exception cref="KsefApiException">Brak autoryzacji. (401 Unauthorized)</exception>
        Task<CertificateEnrollmentResponse> SendCertificateEnrollmentAsync(SendCertificateEnrollmentRequest requestPayload, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Zwraca informacje o statusie wniosku certyfikacyjnego.
        /// </summary>
        /// <param name="certificateRequestReferenceNumber">Numer referencyjny wniosku certyfikacyjnego.</param>
        /// <param name="accessToken">Access token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns><see cref="CertificateEnrollmentStatusResponse"/></returns>
        /// <exception cref="KsefApiException">Nieprawidłowe żądanie. (400 Bad request)</exception>
        /// <exception cref="KsefApiException">Brak autoryzacji. (401 Unauthorized)</exception>
        Task<CertificateEnrollmentStatusResponse> GetCertificateEnrollmentStatusAsync(string certificateRequestReferenceNumber, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Zwraca certyfikaty o podanych numerach seryjnych w formacie DER zakodowanym w Base64.
        /// </summary>
        /// <param name="requestPayload"><see cref="CertificateListRequest"/></param>
        /// <param name="accessToken">Access token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns><see cref="CertificateListResponse"/></returns>
        /// <exception cref="KsefApiException">Nieprawidłowe żądanie. (400 Bad request)</exception>
        /// <exception cref="KsefApiException">Brak autoryzacji. (401 Unauthorized)</exception>
        Task<CertificateListResponse> GetCertificateListAsync(CertificateListRequest requestPayload, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Unieważnia certyfikat o podanym numerze seryjnym.
        /// </summary>
        /// <param name="requestPayload"><see cref="CertificateRevokeRequest"/></param>
        /// <param name="serialNumber">Numer seryjny certyfikatu</param>
        /// <param name="accessToken">Access token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="KsefApiException">Nieprawidłowe żądanie. (400 Bad request)</exception>
        /// <exception cref="KsefApiException">Brak autoryzacji. (401 Unauthorized)</exception>
        Task RevokeCertificateAsync(CertificateRevokeRequest requestPayload, string serialNumber, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// Zwraca listę certyfikatów spełniających podane kryteria wyszukiwania. W przypadku braku podania kryteriów wyszukiwania zwrócona zostanie nieprzefiltrowana lista.
        /// </summary>
        /// <param name="accessToken">Access token</param>
        /// <param name="requestPayload"><see cref="CertificateMetadataListRequest"/></param>
        /// <param name="pageSize">Ilość elementów na stronie (domyślnie 10)</param>
        /// <param name="pageOffset">Index strony wyników (domyślnie 0)</param>
        /// <param name="cancellationToken"></param>
        /// <returns><see cref="CertificateMetadataListResponse"/></returns>
        /// <exception cref="KsefApiException">Nieprawidłowe żądanie. (400 Bad request)</exception>
        /// <exception cref="KsefApiException">Brak autoryzacji. (401 Unauthorized)</exception>
        Task<CertificateMetadataListResponse> GetCertificateMetadataListAsync(string accessToken, CertificateMetadataListRequest requestPayload = null, int? pageSize = null, int? pageOffset = null, CancellationToken cancellationToken = default);

    }
}
