namespace KSeF.Client.Api.Builders.Batch
{
    using KSeF.Client.Core.Models.Sessions;
    using KSeF.Client.Core.Models.Sessions.BatchSession;
    using KSeF.Client.Validation;

    /// <summary>
    /// Umożliwia zbudowanie żądania otwarcia sesji wsadowej w KSeF.
    /// </summary>
    public interface IOpenBatchSessionRequestBuilder
    {
        /// <summary>
        /// Ustawia kod formularza używany w sesji wsadowej.
        /// </summary>
        /// <param name="systemCode">Kod systemowy formularza zgodny ze specyfikacją KSeF.</param>
        /// <param name="schemaVersion">Wersja schematu formularza.</param>
        /// <param name="value">Wartość identyfikująca formularz.</param>
        /// <returns>Interfejs pozwalający kontynuować budowę żądania.</returns>
        IOpenBatchSessionRequestBuilderWithFormCode WithFormCode(string systemCode, string schemaVersion, string value);
    }

    /// <summary>
    /// Etap budowy żądania po ustawieniu kodu formularza.
    /// </summary>
    public interface IOpenBatchSessionRequestBuilderWithFormCode
    {
        /// <summary>
        /// Ustawia podstawowe informacje o pliku wsadowym.
        /// </summary>
        /// <param name="fileSize">Rozmiar pliku wsadowego w bajtach. Schemat OpenAPI <c>BatchFileInfo</c> dopuszcza zakres 1–5 000 000 000.</param>
        /// <param name="fileHash">Skrót kryptograficzny całego pliku wsadowego zgodny ze schematem OpenAPI <c>Sha256HashBase64</c>.</param>
        /// <returns>Interfejs do dodawania części pliku wsadowego.</returns>
        IOpenBatchSessionRequestBuilderBatchFile WithBatchFile(long fileSize, string fileHash);

        /// <summary>
        /// Ustawia podstawowe informacje o pliku wsadowym wraz z typem kompresji.
        /// </summary>
        /// <param name="fileSize">Rozmiar pliku wsadowego w bajtach. Schemat OpenAPI <c>BatchFileInfo</c> dopuszcza zakres 1–5 000 000 000.</param>
        /// <param name="fileHash">Skrót kryptograficzny całego pliku wsadowego zgodny ze schematem OpenAPI <c>Sha256HashBase64</c>.</param>
        /// <param name="compressionType">Typ kompresji pliku wsadowego (np. Zip lub TarGz). Domyślnie przyjmuje typ TarGz</param>
        /// <returns>Interfejs do dodawania części pliku wsadowego.</returns>
        IOpenBatchSessionRequestBuilderBatchFile WithBatchFile(long fileSize, string fileHash, CompressionType compressionType = CompressionType.TarGz);

        /// <summary>
        /// Włącza lub wyłącza tryb offline sesji wsadowej.
        /// </summary>
        /// <param name="offlineMode">Wartość true włącza tryb offline, false pozostawia tryb domyślny.</param>
        /// <returns>Ten sam interfejs, umożliwiający dalsze ustawienia.</returns>
        IOpenBatchSessionRequestBuilderWithFormCode WithOfflineMode(bool offlineMode = false);
    }

    /// <summary>
    /// Etap budowy żądania, w którym dodawane są części pliku wsadowego.
    /// </summary>
    public interface IOpenBatchSessionRequestBuilderBatchFile
    {
        /// <summary>
        /// Dodaje wiele części pliku wsadowego.
        /// </summary>
        /// <param name="parts">
        /// Części pliku wsadowego zawierające nazwę pliku, numer porządkowy,
        /// rozmiar części w bajtach oraz skrót kryptograficzny.
        /// Schemat OpenAPI <c>BatchFileInfo</c> dopuszcza od 1 do 50 części.
        /// </param>
        /// <returns>Interfejs pozwalający dodać kolejne części lub zakończyć opis pliku.</returns>
        [Obsolete("Użyj AddBatchFileParts(IEnumerable<(int ordinalNumber, long fileSize, string fileHash)>). Parametr fileName nie jest używany i zostanie usunięty w przyszłej wersji.")]
        IOpenBatchSessionRequestBuilderBatchFile AddBatchFileParts(IEnumerable<(string fileName, int ordinalNumber, long fileSize, string fileHash)> parts);

        /// <summary>
        /// Dodaje wiele części pliku wsadowego.
        /// </summary>
        /// <param name="parts">
        /// Części pliku wsadowego zawierające numer porządkowy,
        /// rozmiar części w bajtach oraz skrót kryptograficzny.
        /// Schemat OpenAPI <c>BatchFileInfo</c> dopuszcza od 1 do 50 części.
        /// </param>
        /// <returns>Interfejs pozwalający dodać kolejne części lub zakończyć opis pliku.</returns>
        IOpenBatchSessionRequestBuilderBatchFile AddBatchFileParts(IEnumerable<(int ordinalNumber, long fileSize, string fileHash)> parts);

        /// <summary>
        /// Dodaje pojedynczą część pliku wsadowego.
        /// </summary>
        /// <param name="fileName">Nieużywany. Parametr zostanie usunięty w przyszłej wersji.</param>
        /// <param name="ordinalNumber">Numer porządkowy części w pliku wsadowym. Minimum według OpenAPI <c>BatchFilePartInfo</c>: 1.</param>
        /// <param name="fileSize">Rozmiar części pliku w bajtach. Minimum według OpenAPI <c>BatchFilePartInfo</c>: 1.</param>
        /// <param name="fileHash">Skrót kryptograficzny części pliku zgodny ze schematem OpenAPI <c>Sha256HashBase64</c>.</param>
        /// <returns>Interfejs pozwalający dodać kolejne części lub zakończyć opis pliku.</returns>
        [Obsolete("Użyj AddBatchFilePart(int ordinalNumber, long fileSize, string fileHash). Parametr fileName nie jest używany i zostanie usunięty w przyszłej wersji.")]
        IOpenBatchSessionRequestBuilderBatchFile AddBatchFilePart(string fileName, int ordinalNumber, long fileSize, string fileHash);

        /// <summary>
        /// Dodaje pojedynczą część pliku wsadowego.
        /// </summary>
        /// <param name="ordinalNumber">Numer porządkowy części w pliku wsadowym. Minimum według OpenAPI <c>BatchFilePartInfo</c>: 1.</param>
        /// <param name="fileSize">Rozmiar części pliku w bajtach. Minimum według OpenAPI <c>BatchFilePartInfo</c>: 1.</param>
        /// <param name="fileHash">Skrót kryptograficzny części pliku zgodny ze schematem OpenAPI <c>Sha256HashBase64</c>.</param>
        /// <returns>Interfejs pozwalający dodać kolejne części lub zakończyć opis pliku.</returns>
        IOpenBatchSessionRequestBuilderBatchFile AddBatchFilePart(int ordinalNumber, long fileSize, string fileHash);

        /// <summary>
        /// Kończy opis pliku wsadowego i przechodzi do ustawienia danych szyfrowania.
        /// Wymaga co najmniej jednej części zgodnie ze schematem OpenAPI <c>BatchFileInfo</c>.
        /// </summary>
        /// <returns>Interfejs do ustawienia danych szyfrowania.</returns>
        IOpenBatchSessionRequestBuilderEncryption EndBatchFile();
    }

    /// <summary>
    /// Etap budowy żądania, w którym ustawiane są dane szyfrowania pliku wsadowego.
    /// </summary>
    public interface IOpenBatchSessionRequestBuilderEncryption
    {
        /// <summary>
        /// Ustawia dane szyfrowania używane dla pliku wsadowego.
        /// </summary>
        /// <param name="encryptedSymmetricKey">Zaszyfrowany klucz symetryczny użyty do zaszyfrowania pliku.</param>
        /// <param name="initializationVector">Wektor inicjalizujący użyty przy szyfrowaniu.</param>
        /// <param name="publicKeyId">Identyfikator klucza publicznego użytego do zaszyfrowania klucza symetrycznego.</param>
        /// <returns>Interfejs pozwalający utworzyć końcowe żądanie.</returns>
#nullable enable
        IOpenBatchSessionRequestBuilderBuild WithEncryption(string encryptedSymmetricKey, string initializationVector, string? publicKeyId = null);
#nullable disable
    }

    /// <summary>
    /// Ostatni etap budowy żądania otwarcia sesji wsadowej.
    /// </summary>
    public interface IOpenBatchSessionRequestBuilderBuild
    {
        /// <summary>
        /// Tworzy obiekt żądania otwarcia sesji wsadowej w KSeF na podstawie ustawionych parametrów.
        /// </summary>
        /// <returns>Gotowy obiekt żądania otwarcia sesji wsadowej.</returns>
        OpenBatchSessionRequest Build();
    }

    /// <inheritdoc />
    internal class OpenBatchSessionRequestBuilderImpl
        : IOpenBatchSessionRequestBuilder
        , IOpenBatchSessionRequestBuilderWithFormCode
        , IOpenBatchSessionRequestBuilderBatchFile
        , IOpenBatchSessionRequestBuilderEncryption
        , IOpenBatchSessionRequestBuilderBuild
    {
        // Ograniczenia wynikają bezpośrednio ze schematów OpenAPI BatchFileInfo i BatchFilePartInfo.
        private const long OpenApiMinimumFileSizeInBytes = 1;
        private const long OpenApiMaximumBatchFileSizeInBytes = 5_000_000_000;
        private const int OpenApiMinimumBatchFilePartOrdinalNumber = 1;
        private const int OpenApiMinimumBatchFileParts = 1;
        private const int OpenApiMaximumBatchFileParts = 50;

        private FormCode _formCode;
        private readonly List<BatchFilePartInfo> _parts = new();
        private long _batchFileSize;
        private string _batchFileHash = "";
        private CompressionType? _batchFileCompressionType;
        private bool _offlineMode;
        private readonly EncryptionInfo _encryption = new();

        /// <summary>
        /// Tworzy nową instancję buildera.
        /// Użyj metody <see cref="Create"/>, aby z niego skorzystać.
        /// </summary>
        private OpenBatchSessionRequestBuilderImpl() { }

        /// <summary>
        /// Tworzy nową instancję buildera sesji wsadowej.
        /// </summary>
        /// <returns>Builder gotowy do ustawienia kodu formularza.</returns>
        public static IOpenBatchSessionRequestBuilder Create() => new OpenBatchSessionRequestBuilderImpl();

        /// <inheritdoc />
        public IOpenBatchSessionRequestBuilderWithFormCode WithFormCode(string systemCode, string schemaVersion, string value)
        {
            if (string.IsNullOrWhiteSpace(systemCode) || string.IsNullOrWhiteSpace(schemaVersion) || string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Parametry FormCode nie mogą być puste ani null.");
            }

            _formCode = new FormCode
            {
                SystemCode = systemCode,
                SchemaVersion = schemaVersion,
                Value = value
            };
            return this;
        }

        /// <inheritdoc />
        public IOpenBatchSessionRequestBuilderBatchFile WithBatchFile(long fileSize, string fileHash)
        {
            ValidateBatchFile(fileSize, fileHash);

            _batchFileSize = fileSize;
            _batchFileHash = fileHash;
            _batchFileCompressionType = null;
            return this;
        }

        /// <inheritdoc />
        public IOpenBatchSessionRequestBuilderBatchFile WithBatchFile(long fileSize, string fileHash, CompressionType compressionType = CompressionType.TarGz)
        {
            ValidateBatchFile(fileSize, fileHash);

            _batchFileSize = fileSize;
            _batchFileHash = fileHash;
            _batchFileCompressionType = compressionType;
            return this;
        }

        /// <inheritdoc />
        [Obsolete("Użyj AddBatchFileParts(IEnumerable<(int ordinalNumber, long fileSize, string fileHash)>). Parametr fileName nie jest używany i zostanie usunięty w przyszłej wersji.")]
        public IOpenBatchSessionRequestBuilderBatchFile AddBatchFileParts(
            IEnumerable<(string fileName, int ordinalNumber, long fileSize, string fileHash)> parts)
        {
            foreach ((string fileName, int ordinalNumber, long fileSize, string fileHash) in parts)
            {
                AddBatchFilePart(ordinalNumber, fileSize, fileHash);
            }
            return this;
        }

        /// <inheritdoc />
        [Obsolete("Użyj AddBatchFilePart(int ordinalNumber, long fileSize, string fileHash). Parametr fileName nie jest używany i zostanie usunięty w przyszłej wersji.")]
        public IOpenBatchSessionRequestBuilderBatchFile AddBatchFilePart(string fileName, int ordinalNumber, long fileSize, string fileHash)
            => AddBatchFilePart(ordinalNumber, fileSize, fileHash);

        /// <inheritdoc />
        public IOpenBatchSessionRequestBuilderBatchFile AddBatchFileParts(
            IEnumerable<(int ordinalNumber, long fileSize, string fileHash)> parts)
        {
            foreach ((int ordinalNumber, long fileSize, string fileHash) in parts)
            {
                AddBatchFilePart(ordinalNumber, fileSize, fileHash);
            }
            return this;
        }

        /// <inheritdoc />
        public IOpenBatchSessionRequestBuilderBatchFile AddBatchFilePart(int ordinalNumber, long fileSize, string fileHash)
        {
            if (ordinalNumber < OpenApiMinimumBatchFilePartOrdinalNumber ||
                fileSize < OpenApiMinimumFileSizeInBytes)
            {
                throw new ArgumentException(
                    $"Parametry BatchFilePart są nieprawidłowe. Schemat OpenAPI BatchFilePartInfo wymaga " +
                    $"ordinalNumber >= {OpenApiMinimumBatchFilePartOrdinalNumber} i " +
                    $"fileSize >= {OpenApiMinimumFileSizeInBytes}.");
            }

            if (string.IsNullOrWhiteSpace(fileHash) || !RegexPatterns.Sha256Base64.IsMatch(fileHash))
            {
                throw new ArgumentException(
                    "Schemat OpenAPI BatchFilePartInfo wymaga, aby fileHash był zgodny z Sha256HashBase64.");
            }

            if (_parts.Count >= OpenApiMaximumBatchFileParts)
            {
                throw new InvalidOperationException(
                    $"Schemat OpenAPI BatchFileInfo dopuszcza maksymalnie {OpenApiMaximumBatchFileParts} części pliku.");
            }

            _parts.Add(new BatchFilePartInfo
            {
                OrdinalNumber = ordinalNumber,
                FileSize = fileSize,
                FileHash = fileHash,
            });
            return this;
        }

        /// <inheritdoc />
        public IOpenBatchSessionRequestBuilderEncryption EndBatchFile()
        {
            if (string.IsNullOrWhiteSpace(_batchFileHash))
            {
                throw new InvalidOperationException("Hash BatchFile musi być ustawiony.");
            }

            if (_parts.Count < OpenApiMinimumBatchFileParts)
            {
                throw new InvalidOperationException(
                    $"Schemat OpenAPI BatchFileInfo wymaga co najmniej {OpenApiMinimumBatchFileParts} części pliku.");
            }

            return this;
        }

        private static void ValidateBatchFile(long fileSize, string fileHash)
        {
            if (fileSize < OpenApiMinimumFileSizeInBytes ||
                fileSize > OpenApiMaximumBatchFileSizeInBytes)
            {
                throw new ArgumentException(
                    $"Parametry BatchFile są nieprawidłowe. Schemat OpenAPI BatchFileInfo wymaga " +
                    $"fileSize w zakresie {OpenApiMinimumFileSizeInBytes}..{OpenApiMaximumBatchFileSizeInBytes} bajtów.");
            }

            if (string.IsNullOrWhiteSpace(fileHash) || !RegexPatterns.Sha256Base64.IsMatch(fileHash))
            {
                throw new ArgumentException(
                    "Schemat OpenAPI BatchFileInfo wymaga, aby fileHash był zgodny z Sha256HashBase64.");
            }
        }

        /// <inheritdoc />
#nullable enable
        public IOpenBatchSessionRequestBuilderBuild WithEncryption(string encryptedSymmetricKey, string initializationVector, string? publicKeyId = null)
        {
            if (string.IsNullOrWhiteSpace(encryptedSymmetricKey) || string.IsNullOrWhiteSpace(initializationVector))
            {
                throw new ArgumentException("Parametry szyfrowania nie mogą być puste ani null.");
            }

            _encryption.EncryptedSymmetricKey = encryptedSymmetricKey;
            _encryption.InitializationVector = initializationVector;
            _encryption.PublicKeyId = publicKeyId;
            return this;
        }
#nullable disable
        /// <inheritdoc />
        public IOpenBatchSessionRequestBuilderWithFormCode WithOfflineMode(bool offlineMode = false)
        {
            _offlineMode = offlineMode;
            return this;
        }

        /// <inheritdoc />
        public OpenBatchSessionRequest Build()
        {
            if (_formCode == null)
            {
                throw new InvalidOperationException("FormCode jest wymagany.");
            }

            if (string.IsNullOrWhiteSpace(_encryption.EncryptedSymmetricKey) || string.IsNullOrWhiteSpace(_encryption.InitializationVector))
            {
                throw new InvalidOperationException("Konfiguracja szyfrowania jest niekompletna.");
            }

            return new OpenBatchSessionRequest
            {
                FormCode = _formCode,
                BatchFile = new BatchFileInfo
                {
                    FileSize = _batchFileSize,
                    FileHash = _batchFileHash,
                    CompressionType = _batchFileCompressionType,
                    FileParts = _parts
                },
                OfflineMode = _offlineMode,
                Encryption = _encryption
            };
        }
    }

    /// <summary>
    /// Udostępnia metodę pomocniczą do tworzenia buildera sesji wsadowej.
    /// </summary>
    public static class OpenBatchSessionRequestBuilder
    {
        /// <summary>
        /// Tworzy nowy builder żądania otwarcia sesji wsadowej.
        /// </summary>
        /// <returns>Interfejs pozwalający zbudować żądanie.</returns>
        public static IOpenBatchSessionRequestBuilder Create() =>
            OpenBatchSessionRequestBuilderImpl.Create();
    }
}

