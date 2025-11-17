using KSeF.Client.Core.Models.RateLimits;
using KSeF.Client.Core.Models.TestData;
using System.Threading;
using System.Threading.Tasks;

namespace KSeF.Client.Core.Interfaces.Clients
{
    /// <summary>
    /// Operacje sekcji „Dane testowe” + pomocnicze „Query Grants” (weryfikacja efektów).
    /// </summary>
    public interface ITestDataClient
    {
        /// <summary>
        /// POST /api/v2/testdata/subject — utwórz podmiot testowy.
        /// </summary>
        Task CreateSubjectAsync(SubjectCreateRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// POST /api/v2/testdata/subject/remove — usuń podmiot testowy.
        /// </summary>
        Task RemoveSubjectAsync(SubjectRemoveRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// POST /api/v2/testdata/person — utwórz osobę testową.
        /// </summary>
        Task CreatePersonAsync(PersonCreateRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// POST /api/v2/testdata/person/remove — usuń osobę testową.
        /// </summary>
        Task RemovePersonAsync(PersonRemoveRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// POST /api/v2/testdata/permissions — nadaj uprawnienia testowe.
        /// </summary>
        Task GrantPermissionsAsync(TestDataPermissionsGrantRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// POST /api/v2/testdata/permissions/revoke — cofnij uprawnienia testowe.
        /// </summary>
        Task RevokePermissionsAsync(TestDataPermissionsRevokeRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// POST /api/v2/testdata/attachment — włącz załączniki (test).
        /// </summary>
        Task EnableAttachmentAsync(AttachmentPermissionGrantRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// POST /api/v2/testdata/attachment/revoke — wyłącz załączniki (test).
        /// </summary>
        Task DisableAttachmentAsync(AttachmentPermissionRevokeRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// POST /api/v2/testdata/limits/context/session — zmiana limitów sesji dla bieżącego kontekstu (tylko na środowiskach testowych).
        /// </summary>
        Task ChangeSessionLimitsInCurrentContextAsync(ChangeSessionLimitsInCurrentContextRequest request, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// POST /api/v2/testdata/limits/context/session — zmiana limitów sesji dla bieżącego kontekstu (tylko na środowiskach testowych).
        /// </summary>
        Task RestoreDefaultSessionLimitsInCurrentContextAsync(string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// POST /api/v2/testdata/limits/context/session — zmiana limitów sesji dla bieżącego kontekstu (tylko na środowiskach testowych).
        /// </summary>
        Task ChangeCertificatesLimitInCurrentSubjectAsync(ChangeCertificatesLimitInCurrentSubjectRequest request, string accessToken, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// POST /api/v2/testdata/limits/context/session — zmiana limitów sesji dla bieżącego kontekstu (tylko na środowiskach testowych).
        /// </summary>
        Task RestoreDefaultCertificatesLimitInCurrentSubjectAsync(string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// POST /api/v2/testdata/rate-limits — zmienia wartości aktualnie obowiązujących limitów żądań przesyłanych do API dla bieżącego kontekstu. Tylko na środowisku testowym.
        /// </summary>
        Task SetRateLimitsAsync(EffectiveApiRateLimitsRequest requestPayload, string accessToken, CancellationToken cancellationToken = default);

        /// <summary>
        /// DELETE /api/v2/testdata/rate-limits — przywraca wartości aktualnie obowiązujących limitów żądań przesyłanych do API dla bieżącego kontekstu do wartości domyślnych. Tylko na środowiskach testowych.
        /// </summary>
        Task RestoreRateLimitsAsync(string accessToken, CancellationToken cancellationToken = default);
    }
}
