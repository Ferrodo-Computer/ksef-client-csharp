using System.Text;
using KSeF.Client.Api.Builders.Online;
using KSeF.Client.ClientFactory;
using KSeF.Client.Core.Interfaces.Clients;
using KSeF.Client.Core.Interfaces.Services;
using KSeF.Client.Core.Models.ApiResponses;
using KSeF.Client.Core.Models.Invoices;
using KSeF.Client.Core.Models.Sessions;
using KSeF.Client.Core.Models.Sessions.OnlineSession;
using Microsoft.Extensions.Options;
using ClientFactoryEnvironment = KSeF.Client.ClientFactory.Environment;

namespace KSeF.DemoWebApp.Services;

/// <summary>
/// Przykładowy job: auth, wysyłka faktury w sesji online i odbiór UPO.
/// </summary>
public sealed class KsefInvoiceUpoBackgroundJob(
    IKSeFClientFactory clientFactory,
    IKSeFFactoryCryptographyServices cryptographyFactory,
    IKsefBackgroundWorkResultStore resultStore,
    IOptionsMonitor<BackgroundKsefOptions> optionsMonitor,
    IHostEnvironment hostEnvironment,
    ILogger<KsefInvoiceUpoBackgroundJob> logger) : IKsefBackgroundJob
{
    private const int PollSleepMs = 2000;
    private const int MaxStatusPollAttempts = 60;

    private int _completed;

    public string Name => nameof(KsefInvoiceUpoBackgroundJob);

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
            ICryptographyService cryptographyService =
                await cryptographyFactory.CryprographyService(environment).ConfigureAwait(false);

            string nip = "1111111101"; //NIP na potrzeby prezentacji
            logger.LogInformation("{Job}: start faktura+UPO dla NIP {Nip}.", Name, nip);

            string accessToken = await KsefBackgroundJobSupport
                .AuthenticateAsync(client, nip, cancellationToken)
                .ConfigureAwait(false);

            EncryptionData encryptionData = cryptographyService.GetEncryptionData();

            OpenOnlineSessionRequest openRequest = OpenOnlineSessionRequestBuilder
                .Create()
                .WithFormCode(
                    SystemCodeHelper.GetSystemCode(SystemCode.FA3),
                    SystemCodeHelper.GetSchemaVersion(SystemCode.FA3),
                    SystemCodeHelper.GetValue(SystemCode.FA3))
                .WithEncryption(
                    encryptionData.EncryptionInfo.EncryptedSymmetricKey,
                    encryptionData.EncryptionInfo.InitializationVector,
                    encryptionData.EncryptionInfo.PublicKeyId)
                .Build();

            OpenOnlineSessionResponse openSession = await client
                .OpenOnlineSessionAsync(openRequest, accessToken, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            await Task.Delay(PollSleepMs, cancellationToken).ConfigureAwait(false);

            string templatePath = ResolveTemplatePath(options.InvoiceTemplateRelativePath);
            SendInvoiceResponse sendResponse = await SendEncryptedInvoiceAsync(
                client,
                cryptographyService,
                encryptionData,
                openSession.ReferenceNumber,
                accessToken,
                nip,
                templatePath,
                cancellationToken).ConfigureAwait(false);
            await Task.Delay(PollSleepMs, cancellationToken).ConfigureAwait(false);

            SessionStatusResponse statusAfterSend = await PollSessionStatusAsync(
                client,
                openSession.ReferenceNumber,
                accessToken,
                status => status.SuccessfulInvoiceCount is not null,
                cancellationToken).ConfigureAwait(false);

            if (statusAfterSend.SuccessfulInvoiceCount != 1 || statusAfterSend.FailedInvoiceCount is not null)
            {
                throw new InvalidOperationException(
                    $"Faktura nie została przetworzona poprawnie. Success={statusAfterSend.SuccessfulInvoiceCount}, Failed={statusAfterSend.FailedInvoiceCount}.");
            }

            await client.CloseOnlineSessionAsync(openSession.ReferenceNumber, accessToken, cancellationToken)
                .ConfigureAwait(false);
            await Task.Delay(PollSleepMs, cancellationToken).ConfigureAwait(false);

            SessionInvoicesResponse invoices = await client
                .GetSessionInvoicesAsync(openSession.ReferenceNumber, accessToken, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            if (invoices.Invoices is null || invoices.Invoices.Count != 1)
            {
                throw new InvalidOperationException("Oczekiwano dokładnie jednej faktury w sesji.");
            }

            string ksefNumber = invoices.Invoices.First().KsefNumber;

            SessionStatusResponse statusAfterClose = await PollSessionStatusAsync(
                client,
                openSession.ReferenceNumber,
                accessToken,
                status => status.Status?.Code == OnlineSessionCodeResponse.ProcessedSuccessfully
                          && status.Upo?.Pages is { Count: > 0 },
                cancellationToken).ConfigureAwait(false);

            string sessionUpoReference = statusAfterClose.Upo.Pages.First().ReferenceNumber;
            string upoXml = await client
                .GetUpoAsync(invoices.Invoices.First().UpoDownloadUrl, cancellationToken)
                .ConfigureAwait(false);

            KsefBackgroundWorkResult result = new()
            {
                Nip = nip,
                SessionReferenceNumber = openSession.ReferenceNumber,
                InvoiceReferenceNumber = sendResponse.ReferenceNumber,
                KsefNumber = ksefNumber,
                InvoiceUpoXml = upoXml,
                SessionUpoReferenceNumber = sessionUpoReference,
            };

            resultStore.SetSuccess(result);
            Interlocked.Exchange(ref _completed, 1);

            logger.LogInformation(
                "{Job}: zakończono. KSeFNumber={KsefNumber}, Session={Session}.",
                Name,
                result.KsefNumber,
                result.SessionReferenceNumber);
        }
        catch (Exception ex)
        {
            resultStore.SetError(ex);
            throw;
        }
    }

    private static async Task<SendInvoiceResponse> SendEncryptedInvoiceAsync(
        IKSeFClient client,
        ICryptographyService cryptographyService,
        EncryptionData encryptionData,
        string sessionReferenceNumber,
        string accessToken,
        string nip,
        string templatePath,
        CancellationToken cancellationToken)
    {
        string xml = await File.ReadAllTextAsync(templatePath, Encoding.UTF8, cancellationToken).ConfigureAwait(false);
        xml = xml.Replace("#nip#", nip, StringComparison.Ordinal);
        xml = xml.Replace("#invoice_number#", Guid.NewGuid().ToString("N"), StringComparison.Ordinal);

        byte[] invoice = Encoding.UTF8.GetBytes(xml);
        byte[] encryptedInvoice = cryptographyService.EncryptBytesWithAES256(
            invoice,
            encryptionData.CipherKey,
            encryptionData.CipherIv);

        FileMetadata invoiceMetadata = cryptographyService.GetMetaData(invoice);
        FileMetadata encryptedMetadata = cryptographyService.GetMetaData(encryptedInvoice);

        SendInvoiceRequest request = SendInvoiceOnlineSessionRequestBuilder
            .Create()
            .WithInvoiceHash(invoiceMetadata.HashSHA, invoiceMetadata.FileSize)
            .WithEncryptedDocumentHash(encryptedMetadata.HashSHA, encryptedMetadata.FileSize)
            .WithEncryptedDocumentContent(Convert.ToBase64String(encryptedInvoice))
            .Build();

        return await client
            .SendOnlineSessionInvoiceAsync(request, sessionReferenceNumber, accessToken, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

    private static async Task<SessionStatusResponse> PollSessionStatusAsync(
        IKSeFClient client,
        string sessionReferenceNumber,
        string accessToken,
        Func<SessionStatusResponse, bool> condition,
        CancellationToken cancellationToken)
    {
        SessionStatusResponse? last = null;
        for (int attempt = 0; attempt < MaxStatusPollAttempts; attempt++)
        {
            last = await client
                .GetSessionStatusAsync(sessionReferenceNumber, accessToken, cancellationToken)
                .ConfigureAwait(false);

            if (condition(last))
            {
                return last;
            }

            await Task.Delay(PollSleepMs, cancellationToken).ConfigureAwait(false);
        }

        throw new TimeoutException(
            $"Timeout oczekiwania na status sesji. Ostatni kod={last?.Status?.Code}.");
    }

    private string ResolveTemplatePath(string relativePath)
    {
        string combined = Path.Combine(hostEnvironment.ContentRootPath, relativePath);
        if (File.Exists(combined))
        {
            return combined;
        }

        string fromBase = Path.Combine(AppContext.BaseDirectory, relativePath);
        if (File.Exists(fromBase))
        {
            return fromBase;
        }

        throw new FileNotFoundException($"Nie znaleziono szablonu faktury: {relativePath}");
    }
}