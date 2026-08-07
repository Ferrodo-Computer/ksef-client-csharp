#nullable enable
using KSeF.Client.Core.Interfaces.Services;
using KSeF.Client.Core.Models.Invoices;
using KSeF.Client.Core.Models.Sessions;
using KSeF.Client.Core.Models.Sessions.BatchSession;
using KSeF.Client.Tests.Utils;
using KSeF.Client.Tests.Utils.Upo;

namespace KSeF.Client.Tests.Core.E2E.BatchSession;

public partial class BatchSessionE2ETests
{
    /// <summary>
    /// Wykonuje wspólny przebieg sesji wsadowej po otwarciu: wysyłka części, zamknięcie,
    /// weryfikacja statusu oraz pobranie UPO faktury i UPO zbiorczego sesji.
    /// </summary>
    private async Task<BatchSessionFlowResult> ExecuteBatchSessionFullIntegrationFlowAsync(
        OpenBatchSessionResult openResult,
        int expectedInvoiceCount,
        string accessToken,
        string sellerNip)
    {
        // Asercje kroku 1
        Assert.NotNull(openResult);
        Assert.False(string.IsNullOrWhiteSpace(openResult.ReferenceNumber));
        Assert.NotNull(openResult.OpenBatchSessionResponse);
        Assert.False(string.IsNullOrWhiteSpace(openResult.OpenBatchSessionResponse.ReferenceNumber));
        Assert.NotNull(openResult.OpenBatchSessionResponse.PartUploadRequests);

        foreach (PackagePartSignatureInitResponseType? part in openResult.OpenBatchSessionResponse.PartUploadRequests)
        {
            Assert.True(!string.IsNullOrWhiteSpace(part.Method));
            Assert.NotNull(part.Url);
            Assert.True(!string.IsNullOrWhiteSpace(part.Method));
            Assert.NotNull(part.Headers);
        }

        Assert.NotNull(openResult.EncryptedParts);
        Assert.NotEmpty(openResult.EncryptedParts);

        string batchSessionReferenceNumber = openResult.ReferenceNumber;
        OpenBatchSessionResponse openBatchSessionResponse = openResult.OpenBatchSessionResponse;
        List<BatchPartSendingInfo> encryptedParts = openResult.EncryptedParts;

        // 2. Wysłanie wszystkich części
        await KsefClient.SendBatchPartsAsync(openBatchSessionResponse, encryptedParts).ConfigureAwait(false);

        // 3. Zamknięcie sesji – zamiast stałego opóźnienia użyjemy pollingu aż zamknięcie powiedzie się
        Assert.False(string.IsNullOrWhiteSpace(batchSessionReferenceNumber));
        await AsyncPollingUtils.PollAsync(
            action: async () =>
            {
                await BatchUtils.CloseBatchAsync(KsefClient, batchSessionReferenceNumber, accessToken).ConfigureAwait(false);
                return true; // jeśli dotarliśmy tutaj, zamknięcie się powiodło
            },
            condition: closed => closed,
            delay: TimeSpan.FromSeconds(1),
            maxAttempts: 30,
            shouldRetryOnException: _ => true, // ponawiaj przy dowolnym wyjątku
            cancellationToken: CancellationToken
        ).ConfigureAwait(false);

        // 4. Status sesji
        SessionStatusResponse statusResponse = await AsyncPollingUtils.PollWithBackoffAsync(
                                action: () => KsefClient.GetSessionStatusAsync(batchSessionReferenceNumber, accessToken),
                                condition: s => s.Status.Code is ExpectedSessionStatusCode,
                                initialDelay: TimeSpan.FromSeconds(1),
                                maxDelay: TimeSpan.FromSeconds(5),
                                maxAttempts: 30,
                                cancellationToken: CancellationToken).ConfigureAwait(false);

        Assert.NotNull(statusResponse);
        Assert.True(statusResponse.SuccessfulInvoiceCount == expectedInvoiceCount);
        Assert.Equal(ExpectedFailedInvoiceCount, statusResponse.FailedInvoiceCount);
        Assert.NotNull(statusResponse.Upo);
        Assert.NotNull(statusResponse.Upo.Pages);
        // Porównanie z DateTime.UtcNow — data wygaśnięcia URL UPO z API jest w UTC.
        // DateTime.Now zwraca czas lokalny maszyny, co może dawać fałszywe wyniki
        // (np. +2h w CEST -> asercja przepuszcza większy zakres niż zamierzony).
        Assert.True(statusResponse.Upo.Pages.First().DownloadUrlExpirationDate < DateTime.UtcNow.AddDays(4));
        Assert.NotNull(statusResponse.Upo.Pages.First().DownloadUrl);
        Assert.False(string.IsNullOrWhiteSpace(statusResponse.Upo.Pages.First().ReferenceNumber));
        Assert.NotNull(statusResponse.ValidUntil);
        Assert.Equal(ExpectedSessionStatusCode, statusResponse.Status.Code);

        string upoReferenceNumber = statusResponse.Upo.Pages.First().ReferenceNumber;

        // 5. Dokumenty sesji
        SessionInvoicesResponse documents = await BatchUtils.GetSessionInvoicesAsync(KsefClient, batchSessionReferenceNumber, accessToken, expectedInvoiceCount).ConfigureAwait(false);

        Assert.NotNull(documents);
        Assert.Null(documents.ContinuationToken);
        Assert.NotEmpty(documents.Invoices);
        Assert.Equal(expectedInvoiceCount, documents.Invoices.Count);

        string ksefNumber = documents.Invoices.First().KsefNumber;

        // 6. Pobranie UPO faktury z URL zawartego w metadanych faktury
        Uri upoDownloadUrl = documents.Invoices.First().UpoDownloadUrl;
        string invoiceUpoXml = await UpoUtils.GetUpoAsync(KsefClient, upoDownloadUrl).ConfigureAwait(false);
        Assert.False(string.IsNullOrWhiteSpace(invoiceUpoXml));
        InvoiceUpoV4_3 invoiceUpo = UpoUtils.UpoParse<InvoiceUpoV4_3>(invoiceUpoXml);
        Assert.Equal(invoiceUpo.Document.KSeFDocumentNumber, ksefNumber);
        Assert.True(!string.IsNullOrWhiteSpace(invoiceUpo.ReceivingEntityName));
        Assert.True(!string.IsNullOrWhiteSpace(invoiceUpo.SessionReferenceNumber));
        Assert.NotNull(invoiceUpo.Authentication);
        Assert.True(!string.IsNullOrWhiteSpace(invoiceUpo.LogicalStructureName));
        Assert.True(!string.IsNullOrWhiteSpace(invoiceUpo.FormCode));
        Assert.NotNull(invoiceUpo.Signature);
        Assert.Equal(invoiceUpo.Document.SellerNip, sellerNip);

        // 7. Pobranie UPO zbiorczego sesji
        string sessionUpo = await KsefClient.GetSessionUpoAsync(
            batchSessionReferenceNumber,
            upoReferenceNumber,
            accessToken,
            CancellationToken
        ).ConfigureAwait(false);
        Assert.False(string.IsNullOrWhiteSpace(sessionUpo));

        return new BatchSessionFlowResult(
            batchSessionReferenceNumber,
            ksefNumber,
            upoReferenceNumber);
    }

    /// <summary>
    /// Generuje faktury z szablonu (Templates/invoice-template-fa-{x}.xml), buduje paczkę, szyfruje i dzieli na części.
    /// Zwraca numer referencyjny sesji, odpowiedź otwarcia sesji i listę zaszyfrowanych części.
    /// </summary>
    private async Task<OpenBatchSessionResult> PrepareAndOpenBatchSessionAsync(
        ICryptographyService cryptographyService,
        int invoiceCount,
        int partQuantity,
        string sellerNip,
        SystemCode systemCode,
        string invoiceTemplatePath,
        string accessToken,
        CompressionType? compressionType = null)
    {
        EncryptionData encryptionData = cryptographyService.GetEncryptionData();

        List<(string FileName, byte[] Content)> invoices = BatchUtils.GenerateInvoicesInMemory(
            count: invoiceCount,
            nip: sellerNip,
            templatePath: invoiceTemplatePath);

        compressionType = compressionType ?? CompressionType.TarGz;

        (byte[] packageBytes, FileMetadata packageMetadata) = compressionType == CompressionType.TarGz
            ? BatchUtils.BuildTarGz(invoices, cryptographyService)
            : BatchUtils.BuildZip(invoices, cryptographyService);

        List<BatchPartSendingInfo> encryptedParts =
            BatchUtils.EncryptAndSplit(packageBytes, encryptionData, cryptographyService, partQuantity);

        OpenBatchSessionRequest openBatchRequest = compressionType.HasValue
            ? BatchUtils.BuildOpenBatchRequest(
                packageMetadata,
                encryptionData,
                encryptedParts,
                systemCode,
                SystemCodeHelper.GetSchemaVersion(systemCode),
                SystemCodeHelper.GetValue(systemCode),
                compressionType.Value)
            : BatchUtils.BuildOpenBatchRequest(packageMetadata, encryptionData, encryptedParts, systemCode);

        OpenBatchSessionResponse openBatchSessionResponse =
            await BatchUtils.OpenBatchAsync(KsefClient, openBatchRequest, accessToken).ConfigureAwait(false);

        return new OpenBatchSessionResult(
            openBatchSessionResponse.ReferenceNumber,
            openBatchRequest,
            openBatchSessionResponse,
            encryptedParts
        );
    }

    /// <summary>
    /// Przygotowuje i otwiera sesję wsadową z paczką w formacie TAR.GZ.
    /// </summary>
    private async Task<OpenBatchSessionResult> PrepareAndOpenBatchSessionWithTarGzAsync(
        ICryptographyService cryptographyService,
        int invoiceCount,
        int partQuantity,
        string sellerNip,
        SystemCode systemCode,
        string invoiceTemplatePath,
        string accessToken)
    {
        return await PrepareAndOpenBatchSessionAsync(
            cryptographyService,
            invoiceCount,
            partQuantity,
            sellerNip,
            systemCode,
            invoiceTemplatePath,
            accessToken,
            CompressionType.TarGz).ConfigureAwait(false);
    }
}
