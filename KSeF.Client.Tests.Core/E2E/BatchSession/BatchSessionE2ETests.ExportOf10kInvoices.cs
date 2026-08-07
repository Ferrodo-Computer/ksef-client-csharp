#nullable enable
using KSeF.Client.Core.Models;
using KSeF.Client.Core.Models.ApiResponses;
using KSeF.Client.Core.Models.Invoices;
using KSeF.Client.Core.Models.Sessions;
using KSeF.Client.Core.Models.Sessions.BatchSession;
using KSeF.Client.Tests.Utils;

namespace KSeF.Client.Tests.Core.E2E.BatchSession;

public partial class BatchSessionE2ETests
{
    /// <summary>
    /// Eksport paczki dokładnie 10 000 faktur nie powinien zwracać IsTruncated=true.
    /// </summary>
    /// <remarks>
    /// Kroki:
    /// 1. Wysyła 10 000 faktur w paczce przez sesję wsadową.
    /// 2. Czeka na zakończenie przetwarzania sesji.
    /// 3. Eksportuje faktury z żądaniem konkretnego formatu.
    /// 4. Weryfikuje, że IsTruncated=false, a LastPermanentStorageDate i PermanentStorageHwmDate są null.
    /// </remarks>
    [Theory]
    [InlineData(SystemCode.FA3, "invoice-template-fa-3.xml", CompressionType.TarGz, CompressionType.TarGz)]
    [InlineData(SystemCode.FA3, "invoice-template-fa-3.xml", CompressionType.Zip, CompressionType.Zip)]
    public async Task BatchSession_ExportOf10kInvoices_ShouldNotBeTruncated(
        SystemCode systemCode,
        string invoiceTemplatePath,
        CompressionType inputCompressionType,
        CompressionType exportCompressionType)
    {
        // 1. Wysyła 10 000 faktur w paczce przez sesję wsadową.
        EncryptionData encryptionData = CryptographyService.GetEncryptionData();

        List<(string FileName, byte[] Content)> invoices = BatchUtils.GenerateInvoicesInMemory(
            count: TotalInvoices10k,
            nip: sellerNip,
            templatePath: invoiceTemplatePath);

        (byte[] packageBytes, FileMetadata packageMetadata) = inputCompressionType == CompressionType.TarGz
            ? BatchUtils.BuildTarGz(invoices, CryptographyService)
            : BatchUtils.BuildZip(invoices, CryptographyService);

        List<BatchPartSendingInfo> encryptedParts =
            BatchUtils.EncryptAndSplit(packageBytes, encryptionData, CryptographyService);

        OpenBatchSessionRequest openBatchRequest = BatchUtils.BuildOpenBatchRequest(
            packageMetadata,
            encryptionData,
            encryptedParts,
            systemCode,
            SystemCodeHelper.GetSchemaVersion(systemCode),
            SystemCodeHelper.GetValue(systemCode),
            inputCompressionType);

        OpenBatchSessionResponse openBatchSessionResponse =
            await BatchUtils.OpenBatchAsync(KsefClient, openBatchRequest, accessToken);

        string batchSessionReferenceNumber = openBatchSessionResponse.ReferenceNumber;

        await KsefClient.SendBatchPartsAsync(openBatchSessionResponse, encryptedParts);

        await AsyncPollingUtils.PollAsync(
            action: async () =>
            {
                await BatchUtils.CloseBatchAsync(KsefClient, batchSessionReferenceNumber, accessToken).ConfigureAwait(false);
                return true;
            },
            condition: closed => closed,
            delay: TimeSpan.FromSeconds(2),
            maxAttempts: 30,
            shouldRetryOnException: _ => true,
            cancellationToken: CancellationToken);

        // 2. Czeka na zakończenie przetwarzania sesji.
        using CancellationTokenSource sessionCts = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken);
        sessionCts.CancelAfter(SessionTimeout10k);

        SessionStatusResponse statusResponse = await AsyncPollingUtils.PollAsync(
            async () => await KsefClient.GetSessionStatusAsync(batchSessionReferenceNumber, accessToken, sessionCts.Token).ConfigureAwait(false),
            s => s?.Status?.Code == ExpectedSessionStatusCode,
            delay: TimeSpan.FromSeconds(5),
            maxAttempts: SessionMaxAttempts10k,
            cancellationToken: sessionCts.Token);

        Assert.NotNull(statusResponse);
        Assert.Equal(TotalInvoices10k, statusResponse.SuccessfulInvoiceCount);
        Assert.Equal(ExpectedFailedInvoiceCount, statusResponse.FailedInvoiceCount);

        // 3. Eksportuje faktury z żądaniem konkretnego formatu.
        using CancellationTokenSource exportCts = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken);
        exportCts.CancelAfter(ExportTimeout10k);

        SessionInvoicesResponse documents = await BatchUtils.GetSessionInvoicesAsync(
            KsefClient, batchSessionReferenceNumber, accessToken, pageSize: 10);
        string ksefNumber = documents.Invoices.First().KsefNumber;

        DateRange exportDateRange = new()
        {
            From = DateTime.UtcNow.AddDays(-1),
            To = DateTime.UtcNow.AddDays(1),
            DateType = DateType.Invoicing
        };

        InvoiceQueryFilters visibilityQuery = new()
        {
            DateRange = exportDateRange,
            SubjectType = InvoiceSubjectType.Subject1,
            KsefNumber = ksefNumber
        };

        await WaitForInvoiceVisibleForExportAsync(
            visibilityQuery,
            ksefNumber,
            accessToken,
            exportCts.Token,
            pollingDelay: ExportMetadataPollingDelay10k,
            maxAttempts: ExportMaxAttempts10k);

        InvoiceQueryFilters exportQuery = new()
        {
            DateRange = exportDateRange,
            SubjectType = InvoiceSubjectType.Subject1
        };

        InvoiceExportRequest invoiceExportRequest = new()
        {
            Encryption = encryptionData.EncryptionInfo,
            CompressionType = exportCompressionType,
            Filters = exportQuery
        };

        OperationResponse exportResponse = await KsefClient.ExportInvoicesAsync(
            invoiceExportRequest,
            accessToken,
            cancellationToken: exportCts.Token);
        Assert.NotNull(exportResponse?.ReferenceNumber);

        InvoiceExportStatusResponse exportStatus = await AsyncPollingUtils.PollAsync(
            async () => await KsefClient.GetInvoiceExportStatusAsync(
                exportResponse.ReferenceNumber,
                accessToken,
                exportCts.Token).ConfigureAwait(false),
            IsInvoiceExportFinished,
            description: $"Eksport 10k faktur {exportResponse.ReferenceNumber} powinien zakończyć się statusem terminalnym.",
            delay: TimeSpan.FromSeconds(5),
            maxAttempts: ExportMaxAttempts10k,
            cancellationToken: exportCts.Token);

        // 4. Weryfikuje, że IsTruncated=false, a LastPermanentStorageDate i PermanentStorageHwmDate są null.
        Assert.Equal(InvoiceExportStatusCodeResponse.ExportSuccess, exportStatus.Status.Code);
        Assert.NotNull(exportStatus.Package);
        Assert.True(
            exportStatus.Package.InvoiceCount >= TotalInvoices10k,
            $"Eksport powinien zawierać co najmniej {TotalInvoices10k} faktur, zwrócił {exportStatus.Package.InvoiceCount}.");
        Assert.NotEmpty(exportStatus.Package.Parts);

        Assert.False(
            exportStatus.Package.IsTruncated,
            $"Paczka eksportu 10 000 faktur nie powinna być obcięta (IsTruncated=true). " +
            $"InvoiceCount={exportStatus.Package.InvoiceCount}, " +
            $"LastPermanentStorageDate={exportStatus.Package.LastPermanentStorageDate}, " +
            $"PermanentStorageHwmDate={exportStatus.Package.PermanentStorageHwmDate}");

        Assert.Null(exportStatus.Package.LastPermanentStorageDate);
        Assert.Null(exportStatus.Package.PermanentStorageHwmDate);
    }
}
