using KSeF.Client.Api.Builders.Batch;
using KSeF.Client.Core.Models.Invoices;
using KSeF.Client.Core.Models.Sessions.BatchSession;
using KSeF.Client.Http;

namespace KSeF.Client.Tests.Core.UnitTests;

/// <summary>
/// Zestaw testów jednostkowych buildera żądania otwarcia sesji wsadowej.
/// Pokrywa walidację zgodną ze schematem OpenAPI BatchFileInfo i BatchFilePartInfo.
/// </summary>
public class OpenBatchSessionRequestBuilderTests
{
    private const long OpenApiMinimumFileSizeInBytes = 1;
    private const long OpenApiMaximumBatchFileSizeInBytes = 5_000_000_000;
    private const int OpenApiMinimumBatchFilePartOrdinalNumber = 1;
    private const int OpenApiMaximumBatchFileParts = 50;
    private const string ValidSha256Base64Hash = "47DEQpj8HBSa+/TImW+5JCeuQeRkm5NMpJWZG3hSuFU=";

    private static IOpenBatchSessionRequestBuilderBatchFile CreateBatchFileBuilder()
    {
        return OpenBatchSessionRequestBuilder
            .Create()
            .WithFormCode(
                SystemCodeHelper.GetSystemCode(SystemCode.FA3),
                SystemCodeHelper.GetSchemaVersion(SystemCode.FA3),
                SystemCodeHelper.GetValue(SystemCode.FA3))
            .WithBatchFile(OpenApiMinimumFileSizeInBytes, ValidSha256Base64Hash);
    }

    [Fact]
    public void CompressionTypeContract_ShouldMatchOpenApi()
    {
        Assert.Equal(2, typeof(CompressionType).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).Length);
        Assert.Equal("Zip", CompressionType.Zip.ToString());
        Assert.Equal("TarGz", CompressionType.TarGz.ToString());
        Assert.Equal(typeof(CompressionType?), typeof(BatchFileInfo).GetProperty(nameof(BatchFileInfo.CompressionType))?.PropertyType);
    }

    [Fact]
    public void Build_WithCompressionTypeTarGz_SetsBatchFileCompressionType()
    {
        // Arrange
        OpenBatchSessionRequest request = OpenBatchSessionRequestBuilder
            .Create()
            .WithFormCode(
                SystemCodeHelper.GetSystemCode(SystemCode.FA3),
                SystemCodeHelper.GetSchemaVersion(SystemCode.FA3),
                SystemCodeHelper.GetValue(SystemCode.FA3))
            .WithBatchFile(fileSize: 1024, fileHash: ValidSha256Base64Hash, compressionType: CompressionType.TarGz)
            .AddBatchFilePart(ordinalNumber: 1, fileSize: 1024, fileHash: ValidSha256Base64Hash)
            .EndBatchFile()
            .WithEncryption("encrypted-key", "iv")
            .Build();

        // Assert
        Assert.NotNull(request.BatchFile);
        Assert.Equal(CompressionType.TarGz, request.BatchFile.CompressionType);
    }

    [Fact]
    public void Build_WithoutCompressionType_KeepsBackwardCompatibility()
    {
        // Arrange
        OpenBatchSessionRequest request = OpenBatchSessionRequestBuilder
            .Create()
            .WithFormCode(
                SystemCodeHelper.GetSystemCode(SystemCode.FA3),
                SystemCodeHelper.GetSchemaVersion(SystemCode.FA3),
                SystemCodeHelper.GetValue(SystemCode.FA3))
            .WithBatchFile(fileSize: 1024, fileHash: ValidSha256Base64Hash)
            .AddBatchFilePart(ordinalNumber: 1, fileSize: 1024, fileHash: ValidSha256Base64Hash)
            .EndBatchFile()
            .WithEncryption("encrypted-key", "iv")
            .Build();

        // Assert
        Assert.NotNull(request.BatchFile);
        Assert.Null(request.BatchFile.CompressionType);
    }

    [Fact]
    public void Serialize_WithCompressionTypeTarGz_WritesCompressionTypeInPayload()
    {
        // Arrange
        OpenBatchSessionRequest request = OpenBatchSessionRequestBuilder
            .Create()
            .WithFormCode(
                SystemCodeHelper.GetSystemCode(SystemCode.FA3),
                SystemCodeHelper.GetSchemaVersion(SystemCode.FA3),
                SystemCodeHelper.GetValue(SystemCode.FA3))
            .WithBatchFile(fileSize: 1024, fileHash: ValidSha256Base64Hash, compressionType: CompressionType.TarGz)
            .AddBatchFilePart(ordinalNumber: 1, fileSize: 1024, fileHash: ValidSha256Base64Hash)
            .EndBatchFile()
            .WithEncryption("encrypted-key", "iv")
            .Build();

        // Act
        string json = JsonUtil.Serialize(request);

        // Assert
        Assert.Contains("\"TarGz\"", json);
        Assert.Contains("compressionType", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Serialize_WithCompressionTypeZip_WritesCompressionTypeInPayload()
    {
        // Arrange
        OpenBatchSessionRequest request = OpenBatchSessionRequestBuilder
            .Create()
            .WithFormCode(
                SystemCodeHelper.GetSystemCode(SystemCode.FA3),
                SystemCodeHelper.GetSchemaVersion(SystemCode.FA3),
                SystemCodeHelper.GetValue(SystemCode.FA3))
            .WithBatchFile(fileSize: 1024, fileHash: ValidSha256Base64Hash, compressionType: CompressionType.Zip)
            .AddBatchFilePart(ordinalNumber: 1, fileSize: 1024, fileHash: ValidSha256Base64Hash)
            .EndBatchFile()
            .WithEncryption("encrypted-key", "iv")
            .Build();

        // Act
        string json = JsonUtil.Serialize(request);

        // Assert
        Assert.Contains("\"Zip\"", json);
        Assert.Contains("compressionType", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Serialize_WithoutCompressionType_KeepsBackwardCompatibility()
    {
        // Arrange
        OpenBatchSessionRequest request = OpenBatchSessionRequestBuilder
            .Create()
            .WithFormCode(
                SystemCodeHelper.GetSystemCode(SystemCode.FA3),
                SystemCodeHelper.GetSchemaVersion(SystemCode.FA3),
                SystemCodeHelper.GetValue(SystemCode.FA3))
            .WithBatchFile(fileSize: 1024, fileHash: ValidSha256Base64Hash)
            .AddBatchFilePart(ordinalNumber: 1, fileSize: 1024, fileHash: ValidSha256Base64Hash)
            .EndBatchFile()
            .WithEncryption("encrypted-key", "iv")
            .Build();

        // Act
        string json = JsonUtil.Serialize(request);

        // Assert
        Assert.DoesNotContain("compressionType", json);
        Assert.DoesNotContain("CompressionType", json);
    }

    [Theory]
    [InlineData(OpenApiMinimumFileSizeInBytes - 1)]
    public void WithBatchFile_WhenFileSizeIsBelowOpenApiMinimum_ThrowsArgumentException(long fileSize)
    {
        IOpenBatchSessionRequestBuilderWithFormCode builder = OpenBatchSessionRequestBuilder
            .Create()
            .WithFormCode(
                SystemCodeHelper.GetSystemCode(SystemCode.FA3),
                SystemCodeHelper.GetSchemaVersion(SystemCode.FA3),
                SystemCodeHelper.GetValue(SystemCode.FA3));

        Assert.Throws<ArgumentException>(() => builder.WithBatchFile(fileSize, ValidSha256Base64Hash));
        Assert.Throws<ArgumentException>(() => builder.WithBatchFile(fileSize, ValidSha256Base64Hash, CompressionType.TarGz));
    }

    /// <summary>
    /// Weryfikuje odrzucenie paczki przekraczającej maksymalny rozmiar 5 GB.
    /// </summary>
    /// <remarks>
    /// Kroki testu:
    /// 1. Przygotowanie buildera z kodem formularza
    /// 2. Wywołanie WithBatchFile z fileSize większym niż 5_000_000_000 bajtów
    /// 3. Oczekiwanie ArgumentException dla obu przeciążeń WithBatchFile
    /// </remarks>
    [Fact]
    public void WithBatchFile_WhenTotalPackageSizeExceedsOpenApiMaximum_ThrowsArgumentException()
    {
        // Arrange - przygotowanie buildera
        long exceededTotalPackageSizeInBytes = OpenApiMaximumBatchFileSizeInBytes + 1;
        IOpenBatchSessionRequestBuilderWithFormCode builder = OpenBatchSessionRequestBuilder
            .Create()
            .WithFormCode(
                SystemCodeHelper.GetSystemCode(SystemCode.FA3),
                SystemCodeHelper.GetSchemaVersion(SystemCode.FA3),
                SystemCodeHelper.GetValue(SystemCode.FA3));

        // Act - próba ustawienia fileSize powyżej limitu
        // Assert - weryfikacja wyjątku
        ArgumentException exceptionWithoutCompression = Assert.Throws<ArgumentException>(
            () => builder.WithBatchFile(exceededTotalPackageSizeInBytes, ValidSha256Base64Hash));
        ArgumentException exceptionWithCompression = Assert.Throws<ArgumentException>(
            () => builder.WithBatchFile(exceededTotalPackageSizeInBytes, ValidSha256Base64Hash, CompressionType.TarGz));

        Assert.Contains("fileSize", exceptionWithoutCompression.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("fileSize", exceptionWithCompression.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(OpenApiMinimumFileSizeInBytes)]
    [InlineData(OpenApiMaximumBatchFileSizeInBytes)]
    public void WithBatchFile_WhenFileSizeMatchesOpenApiBoundary_AcceptsValue(long fileSize)
    {
        IOpenBatchSessionRequestBuilderBatchFile builder = OpenBatchSessionRequestBuilder
            .Create()
            .WithFormCode(
                SystemCodeHelper.GetSystemCode(SystemCode.FA3),
                SystemCodeHelper.GetSchemaVersion(SystemCode.FA3),
                SystemCodeHelper.GetValue(SystemCode.FA3))
            .WithBatchFile(fileSize, ValidSha256Base64Hash);

        Assert.NotNull(builder);
    }

    [Theory]
    [InlineData(OpenApiMinimumBatchFilePartOrdinalNumber - 1, OpenApiMinimumFileSizeInBytes)]
    [InlineData(OpenApiMinimumBatchFilePartOrdinalNumber, OpenApiMinimumFileSizeInBytes - 1)]
    public void AddBatchFilePart_WhenValueIsBelowOpenApiMinimum_ThrowsArgumentException(int ordinalNumber, long fileSize)
    {
        IOpenBatchSessionRequestBuilderBatchFile builder = OpenBatchSessionRequestBuilder
            .Create()
            .WithFormCode(
                SystemCodeHelper.GetSystemCode(SystemCode.FA3),
                SystemCodeHelper.GetSchemaVersion(SystemCode.FA3),
                SystemCodeHelper.GetValue(SystemCode.FA3))
            .WithBatchFile(OpenApiMinimumFileSizeInBytes, ValidSha256Base64Hash);

        Assert.Throws<ArgumentException>(() => builder.AddBatchFilePart(ordinalNumber, fileSize, ValidSha256Base64Hash));
    }

    [Fact]
    public void EndBatchFile_WithoutParts_ThrowsInvalidOperationException()
    {
        IOpenBatchSessionRequestBuilderBatchFile builder = OpenBatchSessionRequestBuilder
            .Create()
            .WithFormCode(
                SystemCodeHelper.GetSystemCode(SystemCode.FA3),
                SystemCodeHelper.GetSchemaVersion(SystemCode.FA3),
                SystemCodeHelper.GetValue(SystemCode.FA3))
            .WithBatchFile(OpenApiMinimumFileSizeInBytes, ValidSha256Base64Hash);

        Assert.Throws<InvalidOperationException>(() => builder.EndBatchFile());
    }

    /// <summary>
    /// Weryfikuje, że builder akceptuje maksymalną liczbę części pliku zgodną ze schematem OpenAPI (50).
    /// </summary>
    /// <remarks>
    /// Kroki testu:
    /// 1. Przygotowanie buildera z plikiem wsadowym
    /// 2. Dodanie dokładnie 50 części pliku
    /// 3. Zbudowanie żądania i weryfikacja liczby części
    /// </remarks>
    [Fact]
    public void AddBatchFilePart_WhenPartCountMatchesOpenApiMaximum_AcceptsValue()
    {
        // Arrange - przygotowanie buildera
        IOpenBatchSessionRequestBuilderBatchFile builder = CreateBatchFileBuilder();

        // Act - dodanie maksymalnej dozwolonej liczby części
        for (int ordinalNumber = OpenApiMinimumBatchFilePartOrdinalNumber;
             ordinalNumber <= OpenApiMaximumBatchFileParts;
             ordinalNumber++)
        {
            builder.AddBatchFilePart(ordinalNumber, OpenApiMinimumFileSizeInBytes, ValidSha256Base64Hash);
        }

        OpenBatchSessionRequest request = builder
            .EndBatchFile()
            .WithEncryption("encrypted-key", "iv")
            .Build();

        // Assert - weryfikacja, że 50 części zostało zaakceptowanych
        Assert.NotNull(request.BatchFile);
        Assert.NotNull(request.BatchFile.FileParts);
        Assert.Equal(OpenApiMaximumBatchFileParts, request.BatchFile.FileParts.Count);
    }

    /// <summary>
    /// Weryfikuje, że przekroczenie limitu 50 części pliku skutkuje InvalidOperationException.
    /// </summary>
    /// <remarks>
    /// Kroki testu:
    /// 1. Przygotowanie buildera z plikiem wsadowym
    /// 2. Dodanie 50 poprawnych części pliku
    /// 3. Próba dodania 51. części i oczekiwanie wyjątku
    /// </remarks>
    [Fact]
    public void AddBatchFilePart_WhenOpenApiMaximumIsExceeded_ThrowsInvalidOperationException()
    {
        // Arrange - przygotowanie buildera i wypełnienie limitu 50 części
        IOpenBatchSessionRequestBuilderBatchFile builder = CreateBatchFileBuilder();

        for (int ordinalNumber = OpenApiMinimumBatchFilePartOrdinalNumber;
             ordinalNumber <= OpenApiMaximumBatchFileParts;
             ordinalNumber++)
        {
            builder.AddBatchFilePart(ordinalNumber, OpenApiMinimumFileSizeInBytes, ValidSha256Base64Hash);
        }

        // Act - próba dodania części ponad limit OpenAPI
        // Assert - weryfikacja wyjątku
        Assert.Throws<InvalidOperationException>(() => builder.AddBatchFilePart(
            OpenApiMaximumBatchFileParts + 1,
            OpenApiMinimumFileSizeInBytes,
            ValidSha256Base64Hash));
    }

    /// <summary>
    /// Weryfikuje, że AddBatchFileParts odrzuca kolekcję przekraczającą limit 50 części pliku.
    /// </summary>
    /// <remarks>
    /// Kroki testu:
    /// 1. Przygotowanie buildera z plikiem wsadowym
    /// 2. Przygotowanie kolekcji 51 części
    /// 3. Wywołanie AddBatchFileParts i oczekiwanie wyjątku
    /// </remarks>
    [Fact]
    public void AddBatchFileParts_WhenOpenApiMaximumIsExceeded_ThrowsInvalidOperationException()
    {
        // Arrange - przygotowanie buildera i kolekcji przekraczającej limit
        IOpenBatchSessionRequestBuilderBatchFile builder = CreateBatchFileBuilder();
        (int ordinalNumber, long fileSize, string fileHash)[] parts =
            new (int ordinalNumber, long fileSize, string fileHash)[OpenApiMaximumBatchFileParts + 1];

        for (int index = 0; index < parts.Length; index++)
        {
            parts[index] = (
                OpenApiMinimumBatchFilePartOrdinalNumber + index,
                OpenApiMinimumFileSizeInBytes,
                ValidSha256Base64Hash);
        }

        // Act - dodanie kolekcji części ponad limit OpenAPI
        // Assert - weryfikacja wyjątku
        Assert.Throws<InvalidOperationException>(() => builder.AddBatchFileParts(parts));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-sha256-hash")]
    public void WithBatchFile_WhenHashDoesNotMatchOpenApiSha256HashBase64_ThrowsArgumentException(string fileHash)
    {
        IOpenBatchSessionRequestBuilderWithFormCode builder = OpenBatchSessionRequestBuilder
            .Create()
            .WithFormCode(
                SystemCodeHelper.GetSystemCode(SystemCode.FA3),
                SystemCodeHelper.GetSchemaVersion(SystemCode.FA3),
                SystemCodeHelper.GetValue(SystemCode.FA3));

        Assert.Throws<ArgumentException>(() => builder.WithBatchFile(OpenApiMinimumFileSizeInBytes, fileHash));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-sha256-hash")]
    public void AddBatchFilePart_WhenHashDoesNotMatchOpenApiSha256HashBase64_ThrowsArgumentException(string fileHash)
    {
        IOpenBatchSessionRequestBuilderBatchFile builder = OpenBatchSessionRequestBuilder
            .Create()
            .WithFormCode(
                SystemCodeHelper.GetSystemCode(SystemCode.FA3),
                SystemCodeHelper.GetSchemaVersion(SystemCode.FA3),
                SystemCodeHelper.GetValue(SystemCode.FA3))
            .WithBatchFile(OpenApiMinimumFileSizeInBytes, ValidSha256Base64Hash);

        Assert.Throws<ArgumentException>(() => builder.AddBatchFilePart(
            OpenApiMinimumBatchFilePartOrdinalNumber,
            OpenApiMinimumFileSizeInBytes,
            fileHash));
    }

}
