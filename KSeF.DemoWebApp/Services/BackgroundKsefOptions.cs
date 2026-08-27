using ClientFactoryEnvironment = KSeF.Client.ClientFactory.Environment;

namespace KSeF.DemoWebApp.Services;

/// <summary>
/// Opcje demonstracyjnego BackgroundService korzystającego z fabryki klientów KSeF.
/// </summary>
public sealed class BackgroundKsefOptions
{
    public const string SectionName = "BackgroundKsef";

    /// <summary>
    /// Włącza pracę BackgroundService (uruchamianie zarejestrowanych jobów).
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Interwał między kolejnymi tickami w sekundach.
    /// </summary>
    public int IntervalSeconds { get; set; } = 60;

    /// <summary>
    /// Środowisko KSeF dla fabryki: Test, Demo lub Prod.
    /// </summary>
    public ClientFactoryEnvironment Environment { get; set; } = ClientFactoryEnvironment.Test;

    /// <summary>
    /// Ścieżka względna do szablonu faktury (względem katalogu aplikacji).
    /// </summary>
    public string InvoiceTemplateRelativePath { get; set; } = Path.Combine("Templates", "invoice-template-fa-3.xml");

    /// <summary>
    /// Po udanym wykonaniu joba nie powtarzaj go do restartu procesu.
    /// </summary>
    public bool RunOnce { get; set; } = true;
}
