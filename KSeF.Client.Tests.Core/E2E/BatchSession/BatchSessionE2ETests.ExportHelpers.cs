#nullable enable
using KSeF.Client.Core.Models;
using KSeF.Client.Core.Models.ApiResponses;
using KSeF.Client.Core.Models.Invoices;
using KSeF.Client.Core.Models.Sessions;
using KSeF.Client.Core.Models.Sessions.BatchSession;
using KSeF.Client.Http;
using KSeF.Client.Tests.Utils;

namespace KSeF.Client.Tests.Core.E2E.BatchSession;

public partial class BatchSessionE2ETests
{
    private const int ExportMaxAttempts = 45;
    private const int ExportMetadataMaxAttempts = 60;
    private const string MetadataEntryName = "_metadata.json";
    private const string XmlFileExtension = ".xml";
    private static readonly TimeSpan ExportPollingDelay = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan ExportMetadataPollingDelay = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan ExportOperationTimeout = TimeSpan.FromMinutes(3);

    /// <summary>
    /// Eksportuje fakturę i sprawdza zawartość paczki eksportu.
    /// Widoczność faktury w metadanych jest sprawdzana osobno, bo zakończenie sesji wsadowej
    /// nie zawsze oznacza natychmiastową dostępność faktury dla query/export.
    /// </summary>
    private async Task VerifyInvoiceExportPackageAsync(
        EncryptionData encryptionData,
        string ksefNumber,
        string sellerNip,
        string accessToken,
        CompressionType? exportCompressionType = CompressionType.TarGz)
    {
        using CancellationTokenSource timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken);
        timeoutCts.CancelAfter(ExportOperationTimeout);
        CancellationToken exportCancellationToken = timeoutCts.Token;

        InvoiceQueryFilters query = new()
        {
            DateRange = new DateRange
            {
                From = DateTime.UtcNow.AddDays(-1),
                To = DateTime.UtcNow.AddDays(1),
                DateType = DateType.Invoicing
            },
            SubjectType = InvoiceSubjectType.Subject1,
            KsefNumber = ksefNumber
        };

        await WaitForInvoiceVisibleForExportAsync(
            query,
            ksefNumber,
            accessToken,
            exportCancellationToken).ConfigureAwait(false);

        InvoiceExportRequest invoiceExportRequest = new()
        {
            Encryption = encryptionData.EncryptionInfo,
            Filters = query
        };

        if (exportCompressionType.HasValue)
        {
            invoiceExportRequest.CompressionType = exportCompressionType.Value;
        }

        OperationResponse exportResponse = await KsefClient.ExportInvoicesAsync(
            invoiceExportRequest,
            accessToken,
            cancellationToken: exportCancellationToken).ConfigureAwait(false);
        Assert.NotNull(exportResponse?.ReferenceNumber);

        InvoiceExportStatusResponse exportStatus = await WaitForInvoiceExportFinishedAsync(
            exportResponse.ReferenceNumber,
            accessToken,
            exportCancellationToken).ConfigureAwait(false);

        Assert.Equal(
            InvoiceExportStatusCodeResponse.ExportSuccess,
            exportStatus.Status.Code);
        Assert.NotNull(exportStatus.Package);
        Assert.True(exportStatus.Package.InvoiceCount > 0, $"Eksport faktur powinien zawierać fakturę {ksefNumber}.");
        Assert.NotEmpty(exportStatus.Package.Parts);

        Assert.False(
            exportStatus.Package.IsTruncated,
            $"Paczka eksportu nie powinna być obcięta (IsTruncated=true) dla {exportStatus.Package.InvoiceCount} faktur. " +
            $"LastPermanentStorageDate={exportStatus.Package.LastPermanentStorageDate}, " +
            $"PermanentStorageHwmDate={exportStatus.Package.PermanentStorageHwmDate}");

        Assert.Null(exportStatus.Package.LastPermanentStorageDate);
        Assert.Null(exportStatus.Package.PermanentStorageHwmDate);

        using MemoryStream decryptedStream = await BatchUtils.DownloadAndDecryptPackagePartsAsync(
            exportStatus.Package.Parts,
            encryptionData,
            CryptographyService,
            cancellationToken: exportCancellationToken).ConfigureAwait(false);

        CompressionType actualCompressionType = await DetectPackageCompressionTypeAsync(decryptedStream, exportCancellationToken).ConfigureAwait(false);
        // Gdy CompressionType nie jest podany, API zwraca paczkę TarGz (zachowanie wstecznie kompatybilne).
        CompressionType expectedCompressionType = exportCompressionType ?? CompressionType.TarGz;
        Assert.Equal(expectedCompressionType, actualCompressionType);

        Dictionary<string, string> packageFiles = await UnpackExportPackageAsync(
            decryptedStream,
            actualCompressionType,
            exportCancellationToken).ConfigureAwait(false);
        VerifyExportPackageContent(packageFiles, ksefNumber, sellerNip);
    }

    private async Task WaitForInvoiceVisibleForExportAsync(
        InvoiceQueryFilters query,
        string ksefNumber,
        string accessToken,
        CancellationToken cancellationToken,
        TimeSpan? pollingDelay = null,
        int? maxAttempts = null)
    {
        await AsyncPollingUtils.PollAsync(
            async () => await KsefClient.QueryInvoiceMetadataAsync(
                query,
                accessToken,
                pageSize: 10,
                cancellationToken: cancellationToken).ConfigureAwait(false),
            result => result?.Invoices is not null
                && result.Invoices.Any(invoice => string.Equals(invoice.KsefNumber, ksefNumber, StringComparison.OrdinalIgnoreCase)),
            description: $"Faktura {ksefNumber} powinna być widoczna w metadanych przed eksportem.",
            delay: pollingDelay ?? ExportMetadataPollingDelay,
            maxAttempts: maxAttempts ?? ExportMetadataMaxAttempts,
            cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Czeka na zakończenie eksportu. Polling kończy się na pierwszym statusie terminalnym,
    /// a osobna asercja sprawdza dopiero, czy był to sukces.
    /// </summary>
    private async Task<InvoiceExportStatusResponse> WaitForInvoiceExportFinishedAsync(
        string referenceNumber,
        string accessToken,
        CancellationToken cancellationToken)
    {
        return await AsyncPollingUtils.PollAsync(
            async () => await KsefClient.GetInvoiceExportStatusAsync(
                referenceNumber,
                accessToken,
                cancellationToken).ConfigureAwait(false),
            IsInvoiceExportFinished,
            description: $"Eksport faktur {referenceNumber} powinien zakończyć się statusem terminalnym.",
            delay: ExportPollingDelay,
            maxAttempts: ExportMaxAttempts,
            cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Dla testu status inny niż ExportInProgress traktujemy jako terminalny.
    /// Dzięki temu błąd eksportu kończy test od razu, zamiast dopiero po limicie czasu.
    /// </summary>
    private static bool IsInvoiceExportFinished(InvoiceExportStatusResponse? response)
    {
        return response?.Status?.Code is not null
            && response.Status.Code != InvoiceExportStatusCodeResponse.ExportInProgress;
    }

    /// <summary>
    /// Sprawdza zawartość odszyfrowanej i rozpakowanej paczki eksportu:
    /// metadane muszą wskazywać wskazaną fakturę, a paczka musi zawierać jej XML.
    /// </summary>
    private static void VerifyExportPackageContent(
        Dictionary<string, string> packageFiles,
        string ksefNumber,
        string sellerNip)
    {
        Assert.NotEmpty(packageFiles);

        Assert.True(
            packageFiles.TryGetValue(MetadataEntryName, out string? metadataJson),
            $"Paczka eksportu powinna zawierać {MetadataEntryName}.");
        Assert.False(string.IsNullOrWhiteSpace(metadataJson));

        InvoicePackageMetadata metadata = JsonUtil.Deserialize<InvoicePackageMetadata>(metadataJson);

        Assert.NotNull(metadata.Invoices);
        Assert.Contains(metadata.Invoices, invoice => string.Equals(invoice.KsefNumber, ksefNumber, StringComparison.OrdinalIgnoreCase));

        KeyValuePair<string, string>[] invoiceXmlFiles = packageFiles
            .Where(file => file.Key.EndsWith(XmlFileExtension, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        Assert.NotEmpty(invoiceXmlFiles);
        Assert.Contains(invoiceXmlFiles, file => !string.IsNullOrWhiteSpace(file.Value));
        Assert.Contains(invoiceXmlFiles, file => file.Value.Contains(sellerNip));
    }

    /// <summary>
    /// Wykrywa format odszyfrowanej paczki eksportu po nagłówku pliku.
    /// </summary>
    private static async Task<CompressionType> DetectPackageCompressionTypeAsync(
        Stream stream,
        CancellationToken cancellationToken)
    {
        byte[] magic = new byte[2];
#if NETFRAMEWORK
        int read = await stream.ReadAsync(magic, 0, 2, cancellationToken).ConfigureAwait(false);
#else
        int read = await stream.ReadAsync(magic.AsMemory(0, 2), cancellationToken).ConfigureAwait(false);
#endif
        stream.Position = 0;

        if (read == 2 && magic[0] == 0x1F && magic[1] == 0x8B)
        {
            return CompressionType.TarGz;
        }

        if (read == 2 && magic[0] == 0x50 && magic[1] == 0x4B)
        {
            return CompressionType.Zip;
        }

        throw new InvalidOperationException("Nieznany format paczki eksportu.");
    }

    /// <summary>
    /// Rozpakowuje odszyfrowaną paczkę eksportu jako TAR.GZ albo ZIP.
    /// Format eksportu sprawdzamy po nagłówku pliku, ponieważ nie musi być taki sam jak format wejściowej paczki faktur.
    /// </summary>
    private static async Task<Dictionary<string, string>> UnpackExportPackageAsync(
        Stream stream,
        CompressionType compressionType,
        CancellationToken cancellationToken)
    {
        stream.Position = 0;

        return compressionType == CompressionType.TarGz
            ? await BatchUtils.UnzipTarGzAsync(stream, cancellationToken).ConfigureAwait(false)
            : await BatchUtils.UnzipAsync(stream, cancellationToken).ConfigureAwait(false);
    }
}
