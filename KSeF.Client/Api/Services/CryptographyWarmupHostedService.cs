using KSeF.Client.Core.Interfaces.Services;
using Microsoft.Extensions.Hosting;

namespace KSeF.Client.Api.Services;

public sealed partial class CryptographyWarmupHostedService : IHostedService
{
    private readonly ICryptographyService _cryptographyService;
    private readonly CryptographyServiceWarmupMode _warmupMode;

    public CryptographyWarmupHostedService(
        ICryptographyService cryptographyService,
        CryptographyServiceWarmupMode warmupMode = CryptographyServiceWarmupMode.Blocking)
    {
        _cryptographyService = cryptographyService;
        _warmupMode = warmupMode;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        switch (_warmupMode)
        {
            case CryptographyServiceWarmupMode.Disabled:
                return Task.CompletedTask;
            case CryptographyServiceWarmupMode.NonBlocking:
                _ = Task.Run(() => SafeWarmup(cancellationToken), CancellationToken.None);
                return Task.CompletedTask;
            case CryptographyServiceWarmupMode.Blocking:
                return SafeWarmup(cancellationToken);
            default:
                return Task.CompletedTask;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task SafeWarmup(CancellationToken cancellationToken)
    {
        try
        {
            await _cryptographyService.WarmupAsync(cancellationToken);
        }
        catch (Exception)
        {
            if (_warmupMode == CryptographyServiceWarmupMode.Blocking)
            {
                throw;
            }
        }
    }
}
