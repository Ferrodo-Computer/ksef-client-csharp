using KSeF.Client.Core.Infrastructure.Rest;
using KSeF.Client.Core.Interfaces.Rest;
using KSeF.Client.Core.Models.Authorization;
using KSeF.Client.Http;

namespace KSeF.Client.Tests.Core.E2E.Infrastructure;

/// <summary>
/// Test E2E weryfikujący, że nagłówek <c>X-System-Warning</c> (wymuszony na środowisku TEST przez
/// <c>X-Test-System-Warning</c>) da się odczytać przez subskrypcję
/// <see cref="RestClient.ObserveResponseHeaders"/> - korzystając wyłącznie z
/// <see cref="TestBase"/> (bez budowania własnego kontenera DI).
/// </summary>
public class ResponseHeaderObservationE2ETests : TestBase
{
    private const string TestSystemWarningHeader = "X-Test-System-Warning";
    private const string SystemWarningHeader = "X-System-Warning";
    private const string ForcedWarningMessage = "E2E-test-forced-system-warning";

    public ResponseHeaderObservationE2ETests()
    {
        Get<ResponseHeaderObservationOptions>().HeaderNames.Add(SystemWarningHeader);
    }

    [Fact]
    public async Task SendAsync_WhenTestSystemWarningHeaderForced_SubscriptionObservesWarning()
    {
        ResponseHeaderObservedEventArgs observed = null;

        // Act - wymuszamy X-System-Warning przez X-Test-System-Warning na tym jednym żądaniu,
        // korzystając z RestRequest/IRouteBuilder już dostępnych przez TestBase.
        using (KSeF.Client.Http.RestClient.ObserveResponseHeaders((_, e) =>
        {
            if (e.HeaderName.Equals(SystemWarningHeader, StringComparison.OrdinalIgnoreCase))
            {
                observed = e;
            }
        }))
        {
            IRouteBuilder routeBuilder = Get<IRouteBuilder>();
            RestRequest request = RestRequest
                .New(routeBuilder.Build(Routes.Authorization.Challenge), HttpMethod.Post)
                .AddHeader(TestSystemWarningHeader, ForcedWarningMessage);

            AuthenticationChallengeResponse challengeResponse = await RestClient.SendAsync<AuthenticationChallengeResponse>(request);

            // Assert - zwraca body wysłanego requesta.
            Assert.NotNull(challengeResponse);
            Assert.False(string.IsNullOrWhiteSpace(challengeResponse.Challenge));
        }

		// Asset - ostrzeżenie zostało przechwycone przez subskrypcję.
		Assert.NotNull(observed);
        Assert.Contains(observed.Values, v => v.IndexOf(ForcedWarningMessage, StringComparison.OrdinalIgnoreCase) >= 0);
    }
}
