using KSeF.Client.Clients;
using KSeF.Client.Core.Interfaces.Clients;
using KSeF.Client.Core.Interfaces.Rest;
using KSeF.Client.DI;
using Microsoft.Extensions.DependencyInjection;

namespace KSeF.Client.Tests.WarningTestApp;

/// <summary>
/// Rejestruje i konfiguruje klienta KSeF na potrzeby demo aplikacji - analogicznie do
/// <c>TestBase</c> używanego w testach E2E: cała konfiguracja DI siedzi w jednym miejscu
/// (konstruktorze), a reszta programu korzysta wyłącznie z gotowych właściwości.
/// </summary>
public sealed class DemoAppContext : IDisposable
{
    private const string TestSystemWarningHeader = "X-Test-System-Warning";

    private readonly ServiceProvider _serviceProvider;
    private readonly IServiceScope _scope;

    public IAuthorizationClient AuthorizationClient => _scope.ServiceProvider.GetRequiredService<IAuthorizationClient>();

    public ICryptographyClient CryptographyClient => _scope.ServiceProvider.GetRequiredService<ICryptographyClient>();

    /// <param name="forcedWarningMessage">
    /// Treść ostrzeżenia wymuszanego na środowisku TEST przez nagłówek "X-Test-System-Warning"
    /// </param>
    public DemoAppContext(string forcedWarningMessage)
    {
        ServiceCollection services = new();

        // Cała konfiguracja klienta KSeF w jednym miejscu - tak jak w TestBase.
        services.AddKSeFClient(options =>
        {
            options.BaseUrl = KsefEnvironmentsUris.TEST;
            options.CustomHeaders = new Dictionary<string, string>
            {
                [TestSystemWarningHeader] = forcedWarningMessage,
            };
            // Włączenie obserwacji nagłówków - bez tego RestClient.ObserveResponseHeaders
            // nigdy się nie odpali, niezależnie od tego, co API faktycznie zwróci.
            options.ResponseHeaderObservation.Enabled = true;
        });

        services.AddScoped<ICryptographyClient, CryptographyClient>();

        _serviceProvider = services.BuildServiceProvider();
        _scope = _serviceProvider.CreateScope();
    }

    public void Dispose()
    {
        _scope.Dispose();
        _serviceProvider.Dispose();
        GC.SuppressFinalize(this);
    }
}
