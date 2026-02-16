using KSeF.Client.Core.Infrastructure.Rest;
using KSeF.Client.Core.Interfaces.Clients;
using KSeF.Client.Core.Models.TestData;
using KSeF.Client.Core.Interfaces.Rest;
using KSeF.Client.Core.Models.RateLimits;

namespace KSeF.Client.Clients;

/// <inheritdoc />
public sealed class LimitsClient(IRestClient rest, IRouteBuilder routeBuilder) : ClientBase(rest, routeBuilder), ILimitsClient
{

    /// <inheritdoc />
    public Task<SessionLimitsInCurrentContextResponse> GetLimitsForCurrentContextAsync(string accessToken, CancellationToken cancellationToken = default) => 
        ExecuteAsync<SessionLimitsInCurrentContextResponse>(Routes.Limits.CurrentContext, HttpMethod.Get, accessToken, cancellationToken);

    /// <inheritdoc />
    public Task<CertificatesLimitInCurrentSubjectResponse> GetLimitsForCurrentSubjectAsync(string accessToken, CancellationToken cancellationToken = default) => 
        ExecuteAsync<CertificatesLimitInCurrentSubjectResponse>(Routes.Limits.CurrentSubject, HttpMethod.Get, accessToken, cancellationToken);

    /// <inheritdoc />
    public Task<EffectiveApiRateLimits> GetRateLimitsAsync(string accessToken, CancellationToken cancellationToken = default) =>
        ExecuteAsync<EffectiveApiRateLimits>(Routes.Limits.RateLimits, HttpMethod.Get, accessToken, cancellationToken);
}
