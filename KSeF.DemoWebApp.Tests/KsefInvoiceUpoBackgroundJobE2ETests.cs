using KSeF.Client.ClientFactory.DI;
using KSeF.Client.Extensions;
using KSeF.Client.Tests.Utils.Upo;
using KSeF.DemoWebApp.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ClientFactoryEnvironment = KSeF.Client.ClientFactory.Environment;

namespace KSeF.DemoWebApp.Tests;

/// <summary>
/// E2E joba faktura+UPO uruchamianego przez BackgroundService.
/// </summary>
public sealed class KsefInvoiceUpoBackgroundJobE2ETests
{
    /// <summary>
    /// Weryfikuje, że KsefInvoiceUpoBackgroundJob w tle wysyła fakturę i odbiera UPO.
    /// </summary>
    /// <remarks>
    /// Kroki testu:
    /// 1. Zarejestruj tylko job faktura+UPO + BackgroundService
    /// 2. StartAsync
    /// 3. Poczekaj na IKsefBackgroundWorkResultStore
    /// 4. Zweryfikuj numer KSeF i treść UPO
    /// </remarks>
    [Fact]
    public async Task BackgroundService_InvoiceUpoJob_Succeeds()
    {
        // Arrange
        CryptographyConfigInitializer.EnsureInitialized();

        ServiceCollection services = new();
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.Information));
        services.AddSingleton<IHostEnvironment>(new TestHostEnvironment());
        services.Configure<BackgroundKsefOptions>(options =>
        {
            options.Enabled = true;
            options.IntervalSeconds = 5;
            options.Environment = ClientFactoryEnvironment.Test;
            options.RunOnce = true;
            options.InvoiceTemplateRelativePath = Path.Combine("Templates", "invoice-template-fa-3.xml");
        });
        services.RegisterKSeFClientFactory();
        services.AddSingleton<IKsefBackgroundWorkResultStore, KsefBackgroundWorkResultStore>();
        services.AddSingleton<IKsefBackgroundJob, KsefInvoiceUpoBackgroundJob>();
        services.AddHostedService<KsefClientBackgroundService>();

        await using ServiceProvider provider = services.BuildServiceProvider();
        IKsefBackgroundWorkResultStore resultStore =
            provider.GetRequiredService<IKsefBackgroundWorkResultStore>();
        resultStore.Reset();

        KsefClientBackgroundService backgroundService = provider
            .GetServices<IHostedService>()
            .OfType<KsefClientBackgroundService>()
            .Single();

        // Act
        await backgroundService.StartAsync(CancellationToken.None);
        KsefBackgroundWorkResult result = await resultStore.WaitForSuccessAsync(TimeSpan.FromMinutes(3));
        await backgroundService.StopAsync(CancellationToken.None);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(result.KsefNumber));
        Assert.False(string.IsNullOrWhiteSpace(result.SessionReferenceNumber));
        Assert.False(string.IsNullOrWhiteSpace(result.InvoiceReferenceNumber));
        Assert.False(string.IsNullOrWhiteSpace(result.InvoiceUpoXml));
        Assert.False(string.IsNullOrWhiteSpace(result.SessionUpoReferenceNumber));

        InvoiceUpoV4_3 invoiceUpo = UpoUtils.UpoParse<InvoiceUpoV4_3>(result.InvoiceUpoXml);
        Assert.Equal(result.KsefNumber, invoiceUpo.Document.KSeFDocumentNumber);
        Assert.NotNull(invoiceUpo.Signature);
    }

    private sealed class TestHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;

        public string ApplicationName { get; set; } = "KSeF.DemoWebApp.Tests";

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
