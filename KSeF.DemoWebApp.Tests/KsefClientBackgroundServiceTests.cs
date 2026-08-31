using KSeF.Client.ClientFactory.DI;
using KSeF.DemoWebApp.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using ClientFactoryEnvironment = KSeF.Client.ClientFactory.Environment;

namespace KSeF.DemoWebApp.Tests;

/// <summary>
/// Testy jednostkowe KsefClientBackgroundService (orchestracja jobów).
/// </summary>
public sealed class KsefClientBackgroundServiceTests
{
    /// <summary>
    /// Weryfikuje, że rejestracja BackgroundService z jobami przechodzi walidację DI.
    /// </summary>
    [Fact]
    public void RegisterHostedService_WithJobs_PassesDiValidation()
    {
        // Arrange
        ServiceCollection services = new();
        services.AddLogging();
        services.AddSingleton<IHostEnvironment>(new TestHostEnvironment());
        services.AddSingleton<IKsefAuthJobResultStore, KsefAuthJobResultStore>();
        services.AddSingleton<IKsefBackgroundWorkResultStore, KsefBackgroundWorkResultStore>();
        services.Configure<BackgroundKsefOptions>(options =>
        {
            options.Enabled = false;
            options.IntervalSeconds = 5;
            options.Environment = ClientFactoryEnvironment.Test;
        });
        services.RegisterKSeFClientFactory();
        services.AddSingleton<IKsefBackgroundJob, KsefAuthenticationBackgroundJob>();
        services.AddSingleton<IKsefBackgroundJob, KsefInvoiceUpoBackgroundJob>();
        services.AddHostedService<KsefClientBackgroundService>();

        // Act
        using ServiceProvider provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true,
        });

        // Assert
        Assert.Equal(2, provider.GetServices<IKsefBackgroundJob>().Count());
        IHostedService hostedService = Assert.Single(
            provider.GetServices<IHostedService>(),
            service => service is KsefClientBackgroundService);
        Assert.NotNull(hostedService);
    }

    /// <summary>
    /// Weryfikuje, że przy Enabled=false joby nie są uruchamiane.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WhenDisabled_DoesNotRunJobs()
    {
        // Arrange
        CountingBackgroundJob job = new();
        KsefClientBackgroundService service = new(
            [job],
            new TestOptionsMonitor<BackgroundKsefOptions>(new BackgroundKsefOptions
            {
                Enabled = false,
                IntervalSeconds = 5,
            }),
            NullLogger<KsefClientBackgroundService>.Instance);

        // Act
        await service.StartAsync(CancellationToken.None);
        await Task.Delay(200);
        await service.StopAsync(CancellationToken.None);

        // Assert
        Assert.Equal(0, job.ExecuteCount);
    }

    /// <summary>
    /// Weryfikuje, że przy Enabled=true BackgroundService uruchamia zarejestrowane joby.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WhenEnabled_RunsRegisteredJobs()
    {
        // Arrange
        CountingBackgroundJob job = new();
        KsefClientBackgroundService service = new(
            [job],
            new TestOptionsMonitor<BackgroundKsefOptions>(new BackgroundKsefOptions
            {
                Enabled = true,
                IntervalSeconds = 5,
            }),
            NullLogger<KsefClientBackgroundService>.Instance);

        // Act
        await service.StartAsync(CancellationToken.None);
        await Task.Delay(200);
        await service.StopAsync(CancellationToken.None);

        // Assert
        Assert.True(job.ExecuteCount >= 1);
    }

    private sealed class CountingBackgroundJob : IKsefBackgroundJob
    {
        public string Name => nameof(CountingBackgroundJob);

        public int ExecuteCount { get; private set; }

        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            ExecuteCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;

        public string ApplicationName { get; set; } = "KSeF.DemoWebApp.Tests";

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
