using Microsoft.Extensions.Options;

namespace KSeF.DemoWebApp.Services;

/// <summary>
/// Demonstracyjny BackgroundService: uruchamia zarejestrowane joby KSeF
/// przez osobną ścieżkę DI (IKSeFClientFactory).
/// </summary>
public sealed class KsefClientBackgroundService(
    IEnumerable<IKsefBackgroundJob> jobs,
    IOptionsMonitor<BackgroundKsefOptions> optionsMonitor,
    ILogger<KsefClientBackgroundService> logger) : BackgroundService
{
    private const int MinIntervalSeconds = 5;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        IKsefBackgroundJob[] jobList = jobs.ToArray();
        logger.LogInformation(
            "Uruchomiono {Service} z {JobCount} jobami. Ustaw BackgroundKsef:Enabled=true, aby je uruchomić.",
            nameof(KsefClientBackgroundService),
            jobList.Length);

        while (!stoppingToken.IsCancellationRequested)
        {
            BackgroundKsefOptions options = optionsMonitor.CurrentValue;
            int intervalSeconds = Math.Max(options.IntervalSeconds, MinIntervalSeconds);

            try
            {
                if (!options.Enabled)
                {
                    logger.LogDebug("BackgroundKsef jest wyłączony (Enabled=false).");
                }
                else
                {
                    foreach (IKsefBackgroundJob job in jobList)
                    {
                        stoppingToken.ThrowIfCancellationRequested();
                        logger.LogDebug("Uruchamianie joba {Job}.", job.Name);
                        await job.ExecuteAsync(stoppingToken).ConfigureAwait(false);
                    }
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Błąd podczas pracy BackgroundService KSeF.");
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(intervalSeconds), stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
}
