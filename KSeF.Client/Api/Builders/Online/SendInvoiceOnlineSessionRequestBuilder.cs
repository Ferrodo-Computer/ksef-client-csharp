using KSeF.Client.Core.Models.Sessions;

namespace KSeF.Client.Api.Builders.Online
{
    /// <summary>
    /// Buduje żądanie wysłania faktury w ramach sesji online w KSeF.
    /// </summary>
    public interface ISendInvoiceOnlineSessionRequestBuilder
    {
        /// <summary>
        /// Ustawia hash i rozmiar oryginalnego dokumentu faktury.
        /// </summary>
        /// <param name="documentHash">Skrót kryptograficzny dokumentu faktury (np. SHA-256).</param>
        /// <param name="documentSize">Rozmiar dokumentu faktury w bajtach. Nie może być ujemny.</param>
        /// <returns>
        /// Interfejs pozwalający ustawić hash zaszyfrowanej faktury.
        /// </returns>
        ISendInvoiceOnlineSessionRequestBuilderWithInvoiceHash WithInvoiceHash(string documentHash, long documentSize);
    }

    /// <summary>
    /// Etap budowy żądania po ustawieniu hash'a faktury.
    /// </summary>
    public interface ISendInvoiceOnlineSessionRequestBuilderWithInvoiceHash
    {
        /// <summary>
        /// Ustawia hash i rozmiar zaszyfrowanego dokumentu faktury.
        /// </summary>
        /// <param name="encryptedDocumentHash">Skrót kryptograficzny zaszyfrowanej faktury.</param>
        /// <param name="encryptedDocumentSize">Rozmiar zaszyfrowanego dokumentu w bajtach. Nie może być ujemny.</param>
        /// <returns>
        /// Interfejs pozwalający ustawić zaszyfrowaną treść faktury.
        /// </returns>
        ISendInvoiceOnlineSessionRequestBuilderWithEncryptedDocumentHash WithEncryptedDocumentHash(string encryptedDocumentHash, long encryptedDocumentSize);
    }

    /// <summary>
    /// Etap budowy żądania po ustawieniu skrótu (hasha) zaszyfrowanego dokumentu.
    /// </summary>
    public interface ISendInvoiceOnlineSessionRequestBuilderWithEncryptedDocumentHash
    {
        /// <summary>
        /// Ustawia zaszyfrowaną treść dokumentu faktury.
        /// </summary>
        /// <param name="encryptedDocumentContent">
        /// Zaszyfrowana zawartość faktury w formacie wymaganym przez KSeF.
        /// </param>
        /// <returns>
        /// Interfejs pozwalający ustawić dodatkowe opcje i zbudować żądanie.
        /// </returns>
        ISendInvoiceOnlineSessionRequestBuilderBuild WithEncryptedDocumentContent(string encryptedDocumentContent);
    }

    /// <summary>
    /// Ostatni etap budowy żądania wysłania faktury online.
    /// </summary>
    public interface ISendInvoiceOnlineSessionRequestBuilderBuild
    {
        /// <summary>
        /// Ustawia hash faktury korygowanej, jeśli wysyłany dokument jest korektą.
        /// </summary>
        /// <param name="hashOfCorrectedInvoice">
        /// Hash faktury korygowanej. Nie może być pusty, jeżeli jest ustawiany.
        /// </param>
        /// <returns>Ten sam interfejs, umożliwiający dalsze ustawienia lub zbudowanie żądania.</returns>
        ISendInvoiceOnlineSessionRequestBuilderBuild WithHashOfCorrectedInvoice(string hashOfCorrectedInvoice);

        /// <summary>
        /// Włącza lub wyłącza tryb offline przy wysyłce faktury.
        /// </summary>
        /// <param name="offlineMode">
        /// Wartość true włącza tryb offline, false pozostawia tryb domyślny.
        /// </param>
        /// <returns>Ten sam interfejs, umożliwiający dalsze ustawienia lub zbudowanie żądania.</returns>
        ISendInvoiceOnlineSessionRequestBuilderBuild WithOfflineMode(bool offlineMode);

        /// <summary>
        /// Tworzy finalne żądanie wysłania faktury w ramach sesji online.
        /// </summary>
        /// <returns>
        /// Obiekt <see cref="SendInvoiceRequest"/> gotowy do wysłania do KSeF.
        /// </returns>
        SendInvoiceRequest Build();
    }

    /// <inheritdoc />
    internal class SendInvoiceOnlineSessionRequestBuilderImpl
        : ISendInvoiceOnlineSessionRequestBuilder
        , ISendInvoiceOnlineSessionRequestBuilderWithInvoiceHash
        , ISendInvoiceOnlineSessionRequestBuilderWithEncryptedDocumentHash
        , ISendInvoiceOnlineSessionRequestBuilderBuild
    {
        private string _documentHash;
        private long _documentSize;
        private string _encryptedDocumentHash;
        private long _encryptedDocumentSize;
        private string _encryptedDocumentContent;
        private string _hashOfCorrectedInvoice;
        private bool _offlineMode;

        private SendInvoiceOnlineSessionRequestBuilderImpl() { }

        /// <summary>
        /// Tworzy nową instancję buildera żądania wysłania faktury online.
        /// </summary>
        /// <returns>Interfejs startowy buildera.</returns>
        public static ISendInvoiceOnlineSessionRequestBuilder Create() => new SendInvoiceOnlineSessionRequestBuilderImpl();

        /// <inheritdoc />
        public ISendInvoiceOnlineSessionRequestBuilderWithInvoiceHash WithInvoiceHash(string documentHash, long documentSize)
        {
            if (string.IsNullOrWhiteSpace(documentHash) || documentSize < 0)
            {
                throw new ArgumentException("Parametry InvoiceHash są nieprawidłowe.");
            }

            _documentHash = documentHash;
            _documentSize = documentSize;
            return this;
        }

        /// <inheritdoc />
        public ISendInvoiceOnlineSessionRequestBuilderWithEncryptedDocumentHash WithEncryptedDocumentHash(string encryptedDocumentHash, long encryptedDocumentSize)
        {
            if (string.IsNullOrWhiteSpace(encryptedDocumentHash) || encryptedDocumentSize < 0)
            {
                throw new ArgumentException("Parametry EncryptedInvoiceHash są nieprawidłowe.");
            }

            _encryptedDocumentHash = encryptedDocumentHash;
            _encryptedDocumentSize = encryptedDocumentSize;
            return this;
        }

        /// <inheritdoc />
        public ISendInvoiceOnlineSessionRequestBuilderBuild WithEncryptedDocumentContent(string encryptedDocumentContent)
        {
            if (string.IsNullOrWhiteSpace(encryptedDocumentContent))
            {
                throw new ArgumentException("EncryptedInvoiceContent nie może być puste ani null.");
            }

            _encryptedDocumentContent = encryptedDocumentContent;
            return this;
        }

        /// <inheritdoc />
        public ISendInvoiceOnlineSessionRequestBuilderBuild WithHashOfCorrectedInvoice(string hashOfCorrectedInvoice)
        {
            if (string.IsNullOrWhiteSpace(hashOfCorrectedInvoice))
            {
                throw new ArgumentException("HashOfCorrectedInvoice nie może być puste ani null.");
            }

            _hashOfCorrectedInvoice = hashOfCorrectedInvoice;
            return this;
        }

        /// <inheritdoc />
        public ISendInvoiceOnlineSessionRequestBuilderBuild WithOfflineMode(bool offlineMode)
        {
            _offlineMode = offlineMode;
            return this;
        }

        /// <inheritdoc />
        public SendInvoiceRequest Build()
        {
            if (string.IsNullOrWhiteSpace(_documentHash))
            {
                throw new InvalidOperationException("InvoiceHash jest wymagany.");
            }

            if (string.IsNullOrWhiteSpace(_encryptedDocumentHash))
            {
                throw new InvalidOperationException("EncryptedInvoiceHash jest wymagany.");
            }

            if (string.IsNullOrWhiteSpace(_encryptedDocumentContent))
            {
                throw new InvalidOperationException("EncryptedInvoiceContent jest wymagany.");
            }

            return new SendInvoiceRequest
            {
                InvoiceHash = _documentHash,
                InvoiceSize = _documentSize,
                EncryptedInvoiceHash = _encryptedDocumentHash,
                EncryptedInvoiceSize = _encryptedDocumentSize,
                EncryptedInvoiceContent = _encryptedDocumentContent,
                HashOfCorrectedInvoice = _hashOfCorrectedInvoice,
                OfflineMode = _offlineMode
            };
        }
    }

    /// <summary>
    /// Udostępnia metodę pomocniczą do tworzenia buildera żądania wysłania faktury online.
    /// </summary>
    public static class SendInvoiceOnlineSessionRequestBuilder
    {
        /// <summary>
        /// Tworzy nowy builder żądania wysłania faktury w ramach sesji online.
        /// </summary>
        /// <returns>Interfejs startowy buildera.</returns>
        public static ISendInvoiceOnlineSessionRequestBuilder Create() =>
            SendInvoiceOnlineSessionRequestBuilderImpl.Create();
    }
}