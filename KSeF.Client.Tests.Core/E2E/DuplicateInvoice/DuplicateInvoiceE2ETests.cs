using KSeF.Client.Core.Models.ApiResponses;
using KSeF.Client.Core.Models.Authorization;
using KSeF.Client.Core.Models.Invoices;
using KSeF.Client.Core.Models.Sessions;
using KSeF.Client.Core.Models.Sessions.BatchSession;
using KSeF.Client.Core.Models.Sessions.OnlineSession;
using KSeF.Client.Tests.Utils;
using System.Text;

namespace KSeF.Client.Tests.Core.E2E.DuplicateInvoice;

[Collection("DuplicateInvoiceScenario")]
public class DuplicateInvoiceE2ETests : TestBase
{
    // Stałe domenowe i konfiguracyjne testu
    private const int BatchTotalInvoices = 1;
    private const int PartQuantity = 1;

    private const int ExpectedFailedInvoiceCount = 0;

    private const int OnlineSessionStatusPollDelaySeconds = 1;
    private const int OnlineSessionStatusMaxAttempts = 60;
    // Dodatkowe parametry dla pollingu listy nieudanych faktur (może pojawiać się z opóźnieniem względem statusu sesji)
    private const int FailedInvoicesPollDelaySeconds = 2;
    private const int FailedInvoicesMaxAttempts = 120;

    private const int MaxDelay = 10;

    private string accessToken = string.Empty;
    private string sellerNip = string.Empty;

    public DuplicateInvoiceE2ETests()
    {
        string nip = MiscellaneousUtils.GetRandomNip();
        AuthenticationOperationStatusResponse authInfo = AuthenticationUtils
            .AuthenticateAsync(AuthorizationClient, nip)
            .GetAwaiter().GetResult();

        accessToken = authInfo.AccessToken.Token;
        sellerNip = nip;
    }

    /// <summary>
    /// Scenariusz duplikatu faktury (bez polegania na wyjątku HTTP):
    /// 1. Wysyła fakturę w sesji wsadowej (batch) – przetworzona jako poprawna.
    /// 2. Otwiera sesję interaktywną (online).
    /// 3. Wysyła tę samą fakturę (ten sam numer / ta sama treść logiczna).
    /// 4. Zamyka sesję online.
    /// 5. Pobiera listę nieudanych faktur: GET /sessions/{ref}/invoices/failed.
    /// 6. Weryfikuje, że status zawiera informację o duplikacie (kod 440).
    /// </summary>
    [Theory]
    [InlineData(SystemCode.FA2, "invoice-template-fa-2.xml")]
    [InlineData(SystemCode.FA3, "invoice-template-fa-3.xml")]
    public async Task DuplicateInvoice_EndToEnd_FailedListContainsDuplicate(SystemCode systemCode, string invoiceTemplatePath)
    {
        EncryptionData encryptionData = CryptographyService.GetEncryptionData();
        string sharedInvoiceNumber = Guid.NewGuid().ToString();

        // 1. Batch: wysłanie i przetworzenie faktury
        string batchSessionRef = await SendInvoiceInBatchSessionAsync(systemCode, invoiceTemplatePath, sharedInvoiceNumber, encryptionData).ConfigureAwait(true);
        SessionStatusResponse batchStatus = await BatchUtils.WaitForBatchStatusAsync(
            KsefClient,
            batchSessionRef,
            accessToken).ConfigureAwait(true);

        Assert.NotNull(batchStatus);
        Assert.Equal(InvoiceInSessionStatusCodeResponse.Success, batchStatus.Status.Code);
        Assert.Equal(BatchTotalInvoices, batchStatus.SuccessfulInvoiceCount);
        Assert.Equal(ExpectedFailedInvoiceCount, batchStatus.FailedInvoiceCount ?? 0);

        // 2-4. Online: przetworzenie duplikatu
        (string onlineRef, SessionStatusResponse onlineFinalStatus) = await ProcessOnlineSessionAsync(systemCode, invoiceTemplatePath, sharedInvoiceNumber, encryptionData).ConfigureAwait(false);
        Assert.NotNull(onlineFinalStatus);

        // 5. Pobranie nieudanych faktur /invoices/failed (duplikat)
        SessionInvoicesResponse failedInvoices = await GetFailedInvoicesAsync(onlineRef).ConfigureAwait(true);

        Assert.NotNull(failedInvoices);
        Assert.NotNull(failedInvoices.Invoices);
        Assert.NotEmpty(failedInvoices.Invoices);

        SessionInvoice failed = failedInvoices.Invoices.First();
        Assert.NotNull(failed.Status);
        Assert.Equal(InvoiceInSessionStatusCodeResponse.DuplicateInvoice, failed.Status.Code);
    }

    /// <summary>
    /// Odwrócony scenariusz duplikatu:
    /// 1. Wysyła fakturę w sesji online – przetworzona jako poprawna.
    /// 2. Wysyła identyczną fakturę w sesji wsadowej.
    /// 3. Pobiera listę nieudanych faktur wsadu i oczekuje kodu duplikatu (440).
    /// </summary>
    [Theory]
    [InlineData(SystemCode.FA2, "invoice-template-fa-2.xml")]
    [InlineData(SystemCode.FA3, "invoice-template-fa-3.xml")]
    public async Task DuplicateInvoice_OnlineFirst_BatchFailedListContainsDuplicate(SystemCode systemCode, string invoiceTemplatePath)
    {
        EncryptionData encryptionData = CryptographyService.GetEncryptionData();
        string sharedInvoiceNumber = Guid.NewGuid().ToString();

        // 1. Online: pierwsze wysłanie (powinno przejść poprawnie)
        (string onlineRef, SessionStatusResponse onlineStatus) = await ProcessOnlineSessionAsync(systemCode, invoiceTemplatePath, sharedInvoiceNumber, encryptionData).ConfigureAwait(false);
        Assert.NotNull(onlineStatus);
        // Sesja online może zakończyć się kodem 200 
        Assert.True(onlineStatus.Status.Code == InvoiceInSessionStatusCodeResponse.Success,
            $"Oczekiwano kodu {InvoiceInSessionStatusCodeResponse.Success}, otrzymano {onlineStatus.Status.Code}");
        Assert.Equal(1, onlineStatus.SuccessfulInvoiceCount);
        Assert.Equal(0, onlineStatus.FailedInvoiceCount ?? 0);

        // 2. Batch: wysłanie duplikatu
        string batchSessionRef = await SendInvoiceInBatchSessionAsync(systemCode, invoiceTemplatePath, sharedInvoiceNumber, encryptionData).ConfigureAwait(true);
        SessionStatusResponse batchStatus = await BatchUtils.WaitForBatchStatusAsync(
            KsefClient,
            batchSessionRef,
            accessToken).ConfigureAwait(true);

        Assert.NotNull(batchStatus);

        // 3. Pobranie nieudanych faktur sesji wsadowej (polling aż dostępne)
        SessionInvoicesResponse failedBatchInvoices = await GetFailedInvoicesAsync(batchSessionRef).ConfigureAwait(true);
        Assert.NotNull(failedBatchInvoices);
        Assert.NotEmpty(failedBatchInvoices.Invoices);
        SessionInvoice failed = failedBatchInvoices.Invoices.First();
        Assert.NotNull(failed.Status);
        Assert.NotNull(failed.Status.Description);
        Assert.NotNull(failed.Status.Details);
        Assert.Equal(InvoiceInSessionStatusCodeResponse.DuplicateInvoice, failed.Status.Code);
    }

    private async Task<string> SendInvoiceInBatchSessionAsync(
        SystemCode systemCode,
        string invoiceTemplatePath,
        string invoiceNumber,
        EncryptionData encryptionData)
    {
        List<(string FileName, byte[] Content)> invoices = BatchUtils.GenerateInvoicesInMemory(
            count: BatchTotalInvoices,
            nip: sellerNip,
            templatePath: invoiceTemplatePath,
            invoiceNumberFactory: () => invoiceNumber);

        (byte[] zipBytes, FileMetadata zipMeta) = BatchUtils.BuildZip(invoices, CryptographyService);
        List<BatchPartSendingInfo> encryptedParts = BatchUtils.EncryptAndSplit(zipBytes, encryptionData, CryptographyService, PartQuantity);
        OpenBatchSessionRequest openBatchRequest = BatchUtils.BuildOpenBatchRequest(zipMeta, encryptionData, encryptedParts, systemCode);
        OpenBatchSessionResponse openBatchResponse = await BatchUtils.OpenBatchAsync(KsefClient, openBatchRequest, accessToken).ConfigureAwait(false);
        await KsefClient.SendBatchPartsAsync(openBatchResponse, encryptedParts).ConfigureAwait(false);

        await BatchUtils.CloseBatchAsync(KsefClient, openBatchResponse.ReferenceNumber, accessToken).ConfigureAwait(false);
        return openBatchResponse.ReferenceNumber;
    }

    /// <summary>
    /// Wysyła fakturę w sesji online i zwraca referencję oraz finalny status po zamknięciu.
    /// </summary>
    private async Task<(string SessionRef, SessionStatusResponse FinalStatus)> ProcessOnlineSessionAsync(
        SystemCode systemCode,
        string invoiceTemplatePath,
        string invoiceNumber,
        EncryptionData encryptionData)
    {
        OpenOnlineSessionResponse onlineSession = await OnlineSessionUtils.OpenOnlineSessionAsync(
            KsefClient,
            encryptionData,
            accessToken,
            systemCode).ConfigureAwait(false);
        Assert.False(string.IsNullOrWhiteSpace(onlineSession.ReferenceNumber));

        string xml = GetInvoiceXml(invoiceTemplatePath, sellerNip, invoiceNumber);
        SendInvoiceResponse sendResp = await OnlineSessionUtils.SendInvoiceFromXmlAsync(
            KsefClient,
            onlineSession.ReferenceNumber,
            accessToken,
            xml,
            encryptionData,
            CryptographyService).ConfigureAwait(false);
        Assert.False(string.IsNullOrWhiteSpace(sendResp.ReferenceNumber));

        await KsefClient.CloseOnlineSessionAsync(onlineSession.ReferenceNumber, accessToken).ConfigureAwait(false);
        // Krótka pauza po zamknięciu sesji, żeby zredukować race condition między zamknięciem a materializacją wyników
        await Task.Delay(TimeSpan.FromSeconds(OnlineSessionStatusPollDelaySeconds), CancellationToken).ConfigureAwait(false);

        SessionStatusResponse finalStatus = await AsyncPollingUtils.PollWithBackoffAsync(
            action: () => KsefClient.GetSessionStatusAsync(onlineSession.ReferenceNumber, accessToken),
            condition: s => s.Status.Code != InvoiceInSessionStatusCodeResponse.Processing,
            initialDelay: TimeSpan.FromSeconds(OnlineSessionStatusPollDelaySeconds),
            maxDelay: TimeSpan.FromSeconds(MaxDelay),
            maxAttempts: OnlineSessionStatusMaxAttempts,
            jitter: true,
            description: "Polling statusu sesji online",
            cancellationToken: CancellationToken).ConfigureAwait(false);

        return (onlineSession.ReferenceNumber, finalStatus);
    }

	/// <summary>
	/// Pobiera listę nieudanych faktur podanej sesji (polling aż dostępne).
	/// </summary>
	private async Task<SessionInvoicesResponse> GetFailedInvoicesAsync(string sessionRef)
    {
        SessionInvoicesResponse failedInvoices = await AsyncPollingUtils.PollWithBackoffAsync(
            action: () => KsefClient.GetSessionFailedInvoicesAsync(
                sessionRef,
                accessToken,
                null,
                continuationToken: null,
                CancellationToken),
            condition: r => r.Invoices is not null && r.Invoices.Count > 0,
            initialDelay: TimeSpan.FromSeconds(FailedInvoicesPollDelaySeconds),
            maxDelay: TimeSpan.FromSeconds(MaxDelay),
            maxAttempts: FailedInvoicesMaxAttempts,
            jitter: true,
            description: "Polling listy nieudanych faktur",
            cancellationToken: CancellationToken).ConfigureAwait(false);
        return failedInvoices;
    }

    /// <summary>
    /// Buduje XML faktury na podstawie szablonu z podmianą NIP i numeru faktury.
    /// </summary>
    private static string GetInvoiceXml(string templatePath, string nip, string invoiceNumber)
    {
        string invoiceFilePath = Path.Combine(AppContext.BaseDirectory, "Templates", templatePath);
        if (!File.Exists(invoiceFilePath))
        {
            throw new FileNotFoundException($"Szablon nie został znaleziony: {invoiceFilePath}");
        }

        string xml = File.ReadAllText(invoiceFilePath, Encoding.UTF8)
            .Replace("#nip#", nip)
            .Replace("#invoice_number#", invoiceNumber);
        return xml;
    }
}
