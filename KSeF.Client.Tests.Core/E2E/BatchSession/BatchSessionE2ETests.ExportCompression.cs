#nullable enable
using KSeF.Client.Core.Models.Invoices;
using KSeF.Client.Core.Models.Sessions.BatchSession;

namespace KSeF.Client.Tests.Core.E2E.BatchSession;

public partial class BatchSessionE2ETests
{
    /// <summary>
    /// Sprawdza, czy format paczki użyty przy wysyłce wsadowej nie ogranicza formatu późniejszego eksportu.
    /// Eksport paczki faktur również obsługuje wskazanie typu kompresji przez InvoiceExportRequest.CompressionType.
    /// Dla paczek TAR.GZ ustawiamy CompressionType.TarGz, dla ZIP można jawnie wskazać CompressionType.Zip.
    /// Brak wartości zachowuje domyślną kompatybilność API (TAR.GZ).
    /// </summary>
    /// <remarks>
    /// Kroki:
    /// 1. Wysyła faktury w paczce ZIP albo TAR.GZ, jawnie lub przez domyślny format API.
    /// 2. Czeka, aż faktura będzie dostępna dla eksportu.
    /// 3. Eksportuje ją jako ZIP albo TAR.GZ, jawnie lub przez domyślny format API.
    /// 4. Sprawdza format pobranej paczki po nagłówku pliku.
    /// 5. Sprawdza, czy paczka zawiera _metadata.json i XML faktury z bieżącej sesji.
    /// </remarks>
    [Theory]
    [InlineData(SystemCode.FA3, "invoice-template-fa-3.xml", CompressionType.Zip, CompressionType.Zip)]
    [InlineData(SystemCode.FA3, "invoice-template-fa-3.xml", CompressionType.Zip, CompressionType.TarGz)]
    [InlineData(SystemCode.FA3, "invoice-template-fa-3.xml", CompressionType.Zip, null)]
    [InlineData(SystemCode.FA3, "invoice-template-fa-3.xml", CompressionType.TarGz, CompressionType.Zip)]
    [InlineData(SystemCode.FA3, "invoice-template-fa-3.xml", CompressionType.TarGz, CompressionType.TarGz)]
    [InlineData(SystemCode.FA3, "invoice-template-fa-3.xml", CompressionType.TarGz, null)]
    [InlineData(SystemCode.FA3, "invoice-template-fa-3.xml", null, CompressionType.Zip)]
    [InlineData(SystemCode.FA3, "invoice-template-fa-3.xml", null, CompressionType.TarGz)]
    [InlineData(SystemCode.FA3, "invoice-template-fa-3.xml", null, null)]
    public async Task BatchSession_ShouldExportInvoicePackageWithRequestedOrDefaultCompression(
        SystemCode systemCode,
        string invoiceTemplatePath,
        CompressionType? inputCompressionType = CompressionType.TarGz,
        CompressionType? exportCompressionType = CompressionType.TarGz)
    {
        OpenBatchSessionResult openResult = await PrepareAndOpenBatchSessionAsync(
            CryptographyService,
            TotalInvoices,
            PartQuantity,
            sellerNip,
            systemCode,
            invoiceTemplatePath,
            accessToken,
            inputCompressionType);

        Assert.Equal(inputCompressionType ?? CompressionType.TarGz, openResult.OpenBatchSessionRequest.BatchFile.CompressionType);

        BatchSessionFlowResult flowResult = await ExecuteBatchSessionFullIntegrationFlowAsync(
            openResult,
            TotalInvoices,
            accessToken,
            sellerNip);

        await VerifyInvoiceExportPackageAsync(
            CryptographyService.GetEncryptionData(),
            flowResult.KsefNumber,
            sellerNip,
            accessToken,
            exportCompressionType);
    }
}
