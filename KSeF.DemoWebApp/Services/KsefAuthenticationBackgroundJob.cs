using KSeF.Client.ClientFactory;
using KSeF.Client.Core.Interfaces.Clients;
using KSeF.Client.Core.Models.Sessions.ActiveSessions;
using Microsoft.Extensions.Options;
using ClientFactoryEnvironment = KSeF.Client.ClientFactory.Environment;

namespace KSeF.DemoWebApp.Services;

/// <summary>
/// Przykładowy job: utworzenie klienta z fabryki, uwierzytelnienie i odczyt aktywnych sesji.
/// </summary>
public sealed class KsefAuthenticationBackgroundJob(
    IKSeFClientFactory clientFactory,
    IKsefAuthJobResultStore resultStore,
    IOptionsMonitor<BackgroundKsefOptions> optionsMonitor,
    ILogger<KsefAuthenticationBackgroundJob> logger) : IKsefBackgroundJob
{
    private int _completed;

    public string Name => nameof(KsefAuthenticationBackgroundJob);

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        BackgroundKsefOptions options = optionsMonitor.CurrentValue;
        if (options.RunOnce && Interlocked.CompareExchange(ref _completed, 1, 1) == 1)
        {
            logger.LogDebug("{Job} już wykonany (RunOnce=true).", Name);
            return;
        }

        try
        {
            ClientFactoryEnvironment environment = options.Environment;
            IKSeFClient client = clientFactory.KSeFClient(environment);

            string nip = "1111111101"; //NIP na potrzeby prezentacji
            logger.LogInformation("{Job}: start uwierzytelnienia dla NIP {Nip}.", Name, nip);

            string accessToken = await KsefBackgroundJobSupport
                .AuthenticateAsync(client, nip, cancellationToken)
                .ConfigureAwait(false);

            AuthenticationListResponse sessions = await client
                .GetActiveSessions(accessToken, pageSize: 20, continuationToken: string.Empty, cancellationToken)
                .ConfigureAwait(false);

            KsefAuthJobResult result = new()
            {
                Nip = nip,
                AccessToken = accessToken,
                ActiveSessionsCount = sessions.Items?.Count ?? 0,
            };

            resultStore.SetSuccess(result);
            Interlocked.Exchange(ref _completed, 1);

            logger.LogInformation(
                "{Job}: zakończono. NIP={Nip}, ActiveSessions={Count}.",
                Name,
                result.Nip,
                result.ActiveSessionsCount);
        }
        catch (Exception ex)
        {
            resultStore.SetError(ex);
            throw;
        }
    }
}
