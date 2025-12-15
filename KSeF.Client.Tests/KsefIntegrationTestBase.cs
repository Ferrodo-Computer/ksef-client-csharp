using KSeF.Client.Api.Services;
using KSeF.Client.Api.Services.Internal;
using KSeF.Client.Clients;
using KSeF.Client.Core.Interfaces.Clients;
using KSeF.Client.Core.Interfaces.Services;
using KSeF.Client.DI;
using KSeF.Client.Extensions;
using KSeF.Client.Tests.Config;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace KSeF.Client.Tests;

[Collection("KsefIntegration")]
public abstract class KsefIntegrationTestBase : IDisposable
{
    internal const int SleepTime = 500;

    private readonly ServiceProvider _serviceProvider = default!;
    private readonly IServiceScope _scope = default!;

    protected IKSeFClient KsefClient => _scope.ServiceProvider.GetRequiredService<IKSeFClient>();
    protected IAuthorizationClient AuthorizationClient => _scope.ServiceProvider.GetRequiredService<IAuthorizationClient>();
    protected IPersonTokenService TokenService => _scope.ServiceProvider.GetRequiredService<IPersonTokenService>();
    protected ICryptographyService CryptographyService => _scope.ServiceProvider.GetRequiredService<ICryptographyService>();
    protected IVerificationLinkService VerificationLinkService => _scope.ServiceProvider.GetRequiredService<IVerificationLinkService>();

    public KsefIntegrationTestBase()
    {
        CryptographyConfigInitializer.EnsureInitialized();
        ServiceCollection services = new();

        ApiSettings apiSettings = TestConfig.GetApiSettings();

        string customHeadersFromSettings = TestConfig.Load()["ApiSettings:customHeaders"] ?? string.Empty;
        if (!string.IsNullOrEmpty(customHeadersFromSettings))
        {
            apiSettings.CustomHeaders = JsonSerializer.Deserialize<Dictionary<string, string>>(customHeadersFromSettings)
                ?? [];
        }

        services.AddKSeFClient(options =>
        {
            options.BaseUrl = apiSettings.BaseUrl!;
            options.CustomHeaders = apiSettings.CustomHeaders ?? [];
        });

        // UWAGA! w testach nie używamy AddCryptographyClient tylko rejestrujemy ręcznie, bo on uruchamia HostedService w tle
        services.AddSingleton<ICryptographyClient, CryptographyClient>();
        services.AddSingleton<ICertificateFetcher, DefaultCertificateFetcher>();
        services.AddSingleton<ICryptographyService, CryptographyService>();
        // Rejestracja usługi hostowanej (Hosted Service) jako singleton na potrzeby testów
        services.AddSingleton<CryptographyWarmupHostedService>();

        _serviceProvider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });

        _scope = _serviceProvider.CreateScope();

        // opcjonalne: inicjalizacja lub inne czynności startowe
        // Uruchomienie usługi hostowanej w trybie blokującym (domyślnym) na potrzeby testów
        _scope.ServiceProvider.GetRequiredService<CryptographyWarmupHostedService>()
                   .StartAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    public Task DisposeAsync() => Task.Run(() => Dispose());

    public void Dispose()
    {
        _scope.Dispose();
        _serviceProvider.Dispose();
        GC.SuppressFinalize(this);
    }
}
