using KSeF.Client.ClientFactory.DI;
using KSeF.Client.Extensions;
using KSeF.DemoWebApp.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ClientFactoryEnvironment = KSeF.Client.ClientFactory.Environment;

namespace KSeF.DemoWebApp.Tests;

/// <summary>
/// E2E joba uwierzytelnienia uruchamianego przez BackgroundService.
/// </summary>
public sealed class KsefAuthenticationBackgroundJobE2ETests
{
    /// <summary>
    /// Weryfikuje, że KsefAuthenticationBackgroundJob w tle loguje się i zwraca access token.
    /// </summary>
    /// <remarks>
    /// Kroki testu:
    /// 1. Zarejestruj tylko job auth + BackgroundService
    /// 2. StartAsync
    /// 3. Poczekaj na IKsefAuthJobResultStore
    /// 4. Zweryfikuj NIP, token i listę aktywnych sesji
    /// </remarks>
    [Fact]
    public async Task BackgroundService_AuthenticationJob_Succeeds()
    {
        // Arrange
        CryptographyConfigInitializer.EnsureInitialized();

        ServiceCollection services = new();
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Information));
        services.Configure<BackgroundKsefOptions>(options =>
        {
            options.Enabled = true;
            options.IntervalSeconds = 5;
            options.Environment = ClientFactoryEnvironment.Test;
            options.RunOnce = true;
        });
        services.RegisterKSeFClientFactory();
        services.AddSingleton<IKsefAuthJobResultStore, KsefAuthJobResultStore>();
        services.AddSingleton<IKsefBackgroundJob, KsefAuthenticationBackgroundJob>();
        services.AddHostedService<KsefClientBackgroundService>();

        await using ServiceProvider provider = services.BuildServiceProvider();
        IKsefAuthJobResultStore resultStore = provider.GetRequiredService<IKsefAuthJobResultStore>();
        resultStore.Reset();

        KsefClientBackgroundService backgroundService = provider
            .GetServices<IHostedService>()
            .OfType<KsefClientBackgroundService>()
            .Single();

        // Act
        await backgroundService.StartAsync(CancellationToken.None);
        KsefAuthJobResult result = await resultStore.WaitForSuccessAsync(TimeSpan.FromMinutes(2));
        await backgroundService.StopAsync(CancellationToken.None);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(result.Nip));
        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
        Assert.True(result.ActiveSessionsCount >= 0);
    }
}
