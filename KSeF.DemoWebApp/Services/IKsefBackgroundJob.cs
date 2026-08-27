namespace KSeF.DemoWebApp.Services;

/// <summary>
/// Przykładowy job uruchamiany przez KsefClientBackgroundService.
/// </summary>
public interface IKsefBackgroundJob
{
    /// <summary>
    /// Nazwa joba (do logów).
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Wykonuje jednostkę pracy joba.
    /// </summary>
    Task ExecuteAsync(CancellationToken cancellationToken);
}
