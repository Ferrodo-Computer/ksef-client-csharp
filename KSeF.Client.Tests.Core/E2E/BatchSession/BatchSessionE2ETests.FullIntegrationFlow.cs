#nullable enable
using KSeF.Client.Core.Models.Invoices;

namespace KSeF.Client.Tests.Core.E2E.BatchSession;

public partial class BatchSessionE2ETests
{
    /// <summary>
    /// End-to-end test weryfikujący pełny, poprawny przebieg przetwarzania sesji wsadowej w KSeF
    /// dla paczki ZIP.
    /// Generuje 20 faktur z szablonu, szyfruje i dzieli paczkę na części, otwiera sesję,
    /// wysyła wszystkie części, zamyka sesję, sprawdza status przetwarzania oraz pobiera UPO
    /// pojedynczej faktury i UPO zbiorcze sesji.
    /// </summary>
    /// <remarks>
    /// Kroki:
    /// 1. Przygotowanie paczki ZIP, szyfrowanie, podział na części i otwarcie sesji.
    /// 2. Wysłanie wszystkich zaszyfrowanych części.
    /// 3. Zamknięcie sesji i oczekiwanie na zakończenie przetwarzania faktur.
    /// 4. Weryfikacja statusu sesji: SuccessfulInvoiceCount == 20, FailedInvoiceCount == 0, Status.Code == 200; pobranie numeru referencyjnego UPO.
    /// 5. Pobranie dokumentów sesji i zapis pierwszego numeru KSeF.
    /// 6. Pobranie UPO faktury po numerze KSeF.
    /// 7. Pobranie UPO zbiorczego sesji.
    /// </remarks>
    [Theory]
    [InlineData(SystemCode.FA2, "invoice-template-fa-2.xml")]
    [InlineData(SystemCode.FA3, "invoice-template-fa-3.xml")]
    public async Task BatchSessionFullIntegrationFlowReturnsUpo(SystemCode systemCode, string invoiceTemplatePath)
    {
        OpenBatchSessionResult openResult = await PrepareAndOpenBatchSessionAsync(
            CryptographyService,
            TotalInvoices,
            PartQuantity,
            sellerNip,
            systemCode,
            invoiceTemplatePath,
            accessToken
        );

        await ExecuteBatchSessionFullIntegrationFlowAsync(
            openResult,
            TotalInvoices,
            accessToken,
            sellerNip);
    }
}
