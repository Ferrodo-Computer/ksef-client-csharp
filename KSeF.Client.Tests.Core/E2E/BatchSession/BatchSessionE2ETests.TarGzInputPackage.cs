#nullable enable
using KSeF.Client.Core.Models.Invoices;
using KSeF.Client.Core.Models.Sessions.BatchSession;

namespace KSeF.Client.Tests.Core.E2E.BatchSession;

public partial class BatchSessionE2ETests
{
    /// <summary>
    /// End-to-end test weryfikujący pełny, poprawny przebieg przetwarzania sesji wsadowej w KSeF
    /// dla paczki TAR.GZ.
    /// Generuje 20 faktur z szablonu, szyfruje i dzieli paczkę na części, otwiera sesję
    /// z CompressionType.TarGz, wysyła wszystkie części, zamyka sesję, sprawdza status przetwarzania
    /// oraz pobiera UPO pojedynczej faktury i UPO zbiorcze sesji.
    /// </summary>
    /// <remarks>
    /// Kroki:
    /// 1. Przygotowanie paczki TAR.GZ, szyfrowanie, podział na części i otwarcie sesji.
    /// 2. Sprawdzenie, że żądanie otwarcia sesji wskazuje CompressionType.TarGz.
    /// 3. Wysłanie wszystkich zaszyfrowanych części.
    /// 4. Zamknięcie sesji i oczekiwanie na zakończenie przetwarzania faktur.
    /// 5. Weryfikacja statusu sesji: SuccessfulInvoiceCount == 20, FailedInvoiceCount == 0, Status.Code == 200; pobranie numeru referencyjnego UPO.
    /// 6. Pobranie dokumentów sesji i zapis pierwszego numeru KSeF.
    /// 7. Pobranie UPO faktury po numerze KSeF.
    /// 8. Pobranie UPO zbiorczego sesji.
    /// </remarks>
    [Theory]
    [InlineData(SystemCode.FA3, "invoice-template-fa-3.xml")]
    public async Task BatchSession_WithTarGzInputPackage_ShouldProcessInvoicesAndReturnUpo(
        SystemCode systemCode,
        string invoiceTemplatePath)
    {
        OpenBatchSessionResult openResult = await PrepareAndOpenBatchSessionWithTarGzAsync(
            CryptographyService,
            TotalInvoices,
            PartQuantity,
            sellerNip,
            systemCode,
            invoiceTemplatePath,
            accessToken
        );

        Assert.Equal(CompressionType.TarGz, openResult.OpenBatchSessionRequest.BatchFile.CompressionType);

        await ExecuteBatchSessionFullIntegrationFlowAsync(
            openResult,
            TotalInvoices,
            accessToken,
            sellerNip);
    }
}
