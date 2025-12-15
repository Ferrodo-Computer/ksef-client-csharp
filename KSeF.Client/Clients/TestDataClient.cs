using KSeF.Client.Core.Infrastructure.Rest;
using KSeF.Client.Core.Interfaces.Clients;
using KSeF.Client.Core.Interfaces.Rest;
using KSeF.Client.Core.Models.RateLimits;
using KSeF.Client.Core.Models.TestData;

namespace KSeF.Client.Clients
{
    /// <inheritdoc />
    public sealed class TestDataClient(IRestClient rest, IRouteBuilder routeBuilder) : ClientBase(rest, routeBuilder), ITestDataClient
    {

        /// <inheritdoc />
        public Task CreateSubjectAsync(SubjectCreateRequest request, CancellationToken cancellationToken = default) =>
            ExecuteAsync(Routes.TestData.CreateSubject, request, cancellationToken);

        /// <inheritdoc />
        public Task RemoveSubjectAsync(SubjectRemoveRequest request, CancellationToken cancellationToken = default) =>
            ExecuteAsync(Routes.TestData.RemoveSubject, request, cancellationToken);

        /// <inheritdoc />
        public Task CreatePersonAsync(PersonCreateRequest request, CancellationToken cancellationToken = default) =>
            ExecuteAsync(Routes.TestData.CreatePerson, request, cancellationToken);

        /// <inheritdoc />
        public Task RemovePersonAsync(PersonRemoveRequest request, CancellationToken cancellationToken = default) =>
            ExecuteAsync(Routes.TestData.RemovePerson, request, cancellationToken);

        /// <inheritdoc />
        public Task GrantPermissionsAsync(TestDataPermissionsGrantRequest request, CancellationToken cancellationToken = default) =>
            ExecuteAsync(Routes.TestData.GrantPerms, request, cancellationToken);

        /// <inheritdoc />
        public Task RevokePermissionsAsync(TestDataPermissionsRevokeRequest request, CancellationToken cancellationToken = default) =>
            ExecuteAsync(Routes.TestData.RevokePerms, request, cancellationToken);

        /// <inheritdoc />
        public Task EnableAttachmentAsync(AttachmentPermissionGrantRequest request, CancellationToken cancellationToken = default) =>
            ExecuteAsync(Routes.TestData.EnableAttach, request, cancellationToken);

        /// <inheritdoc />
        public Task DisableAttachmentAsync(AttachmentPermissionRevokeRequest request, CancellationToken cancellationToken = default) =>
            ExecuteAsync(Routes.TestData.DisableAttach, request, cancellationToken);

        /// <inheritdoc />
        public Task ChangeSessionLimitsInCurrentContextAsync(ChangeSessionLimitsInCurrentContextRequest request, string accessToken, CancellationToken cancellationToken = default) =>
            ExecuteAsync(Routes.TestData.ChangeSessionLimitsInCurrentContext, request, accessToken, cancellationToken);

        /// <inheritdoc />
        public Task RestoreDefaultSessionLimitsInCurrentContextAsync(string accessToken, CancellationToken cancellationToken = default) =>
            ExecuteAsync(Routes.TestData.RestoreDefaultSessionLimitsInCurrentContext, HttpMethod.Delete, accessToken, cancellationToken);

        /// <inheritdoc />
        public Task ChangeCertificatesLimitInCurrentSubjectAsync(ChangeCertificatesLimitInCurrentSubjectRequest request, string accessToken, CancellationToken cancellationToken = default) =>
            ExecuteAsync(Routes.TestData.ChangeCertificatesLimitInCurrentSubject, request, accessToken, cancellationToken);

        /// <inheritdoc />
        public Task RestoreDefaultCertificatesLimitInCurrentSubjectAsync(string accessToken, CancellationToken cancellationToken = default) =>
            ExecuteAsync(Routes.TestData.RestoreDefaultCertificatesLimitInCurrentSubject, HttpMethod.Delete, accessToken, cancellationToken);

        /// <inheritdoc />
        public Task RestoreRateLimitsAsync(string accessToken, CancellationToken cancellationToken = default) =>
            ExecuteAsync(Routes.TestData.RateLimits, HttpMethod.Delete, accessToken, cancellationToken);

        /// <inheritdoc />
        public Task SetRateLimitsAsync(EffectiveApiRateLimitsRequest requestPayload, string accessToken, CancellationToken cancellationToken = default) =>
            ExecuteAsync(Routes.TestData.RateLimits, requestPayload, accessToken, cancellationToken);

        public Task RestoreProductionRateLimitsAsync(string accessToken, CancellationToken cancellationToken = default) =>
            ExecuteAsync(Routes.TestData.RateLimits, HttpMethod.Delete, accessToken, cancellationToken);
    }
}
