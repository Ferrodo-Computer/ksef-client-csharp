using KSeF.Client.Validation;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace KSeF.Client.Helpers;

/// <summary>
/// Klasa pomocnicza zawierająca metody do walidacji faktur i ich komponentów.
/// </summary>
public static class ValidationHelper
{
    // Komunikaty błędów walidacji - XML
    private const string ErrorInvoiceContentEmpty = "Treść faktury nie może być pusta.";
    private const string ErrorXmlEmptyOrNoRoot = "XML faktury jest pusty lub bez elementu głównego.";
    private const string ErrorXmlFormatPrefix = "Błąd XML: ";

    // Komunikaty błędów walidacji - Sprzedawca (Podmiot1)
    private const string ErrorSellerNipNotFound = "NIP sprzedawcy (Podmiot1) nie został znaleziony w fakturze.";
    private const string ErrorSellerNipInvalidFormat = "NIP sprzedawcy (Podmiot1) {0} ma nieprawidłowy format.";
    private const string ErrorSellerNipInvalidChecksum = "NIP sprzedawcy (Podmiot1) {0} ma nieprawidłową sumę kontrolną.";

    // Komunikaty walidacji - Nabywca (Podmiot2)
    private const string InfoBuyerNipNotFound = "NIP nabywcy (Podmiot2) nie został znaleziony w fakturze.";
    private const string ErrorBuyerNipInvalidFormat = "NIP nabywcy (Podmiot2) {0} ma nieprawidłowy format.";
    private const string ErrorBuyerNipInvalidChecksum = "NIP nabywcy (Podmiot2) {0} ma nieprawidłową sumę kontrolną.";

    // Komunikaty błędów walidacji - Podmioty trzecie (Podmiot3)
    private const string ErrorThirdPartyNipInvalidFormat = "Nieprawidłowy format NIP Podmiot3: {0}";
    private const string ErrorThirdPartyNipInvalidChecksum = "Błędna suma kontrolna NIP Podmiot3: {0}";
    private const string ErrorThirdPartyIdWewInvalidFormat = "Nieprawidłowy format IDWew (internalId) Podmiot3: {0}";
    private const string ErrorThirdPartyIdWewInvalidChecksum = "Błędna suma kontrolna IDWew (internalId) Podmiot3: {0}";

    // CompositeFormat dla optymalizacji wydajności formatowania
    private static readonly CompositeFormat ErrorSellerNipInvalidFormatComposite = CompositeFormat.Parse(ErrorSellerNipInvalidFormat);
    private static readonly CompositeFormat ErrorSellerNipInvalidChecksumComposite = CompositeFormat.Parse(ErrorSellerNipInvalidChecksum);
    private static readonly CompositeFormat ErrorBuyerNipInvalidFormatComposite = CompositeFormat.Parse(ErrorBuyerNipInvalidFormat);
    private static readonly CompositeFormat ErrorBuyerNipInvalidChecksumComposite = CompositeFormat.Parse(ErrorBuyerNipInvalidChecksum);
    private static readonly CompositeFormat ErrorThirdPartyNipInvalidFormatComposite = CompositeFormat.Parse(ErrorThirdPartyNipInvalidFormat);
    private static readonly CompositeFormat ErrorThirdPartyNipInvalidChecksumComposite = CompositeFormat.Parse(ErrorThirdPartyNipInvalidChecksum);
    private static readonly CompositeFormat ErrorThirdPartyIdWewInvalidFormatComposite = CompositeFormat.Parse(ErrorThirdPartyIdWewInvalidFormat);
    private static readonly CompositeFormat ErrorThirdPartyIdWewInvalidChecksumComposite = CompositeFormat.Parse(ErrorThirdPartyIdWewInvalidChecksum);

    /// <summary>
    /// Waliduje format XML faktury.
    /// </summary>
    /// <param name="invoiceXml">Treść faktury w formacie XML.</param>
    /// <returns>Obiekt <see cref="XmlValidationResult"/> zawierający wyniki walidacji.</returns>
    public static XmlValidationResult ValidateInvoiceXmlFormat(string invoiceXml)
    {
        if (string.IsNullOrWhiteSpace(invoiceXml))
        {
            return new XmlValidationResult(false, ErrorInvoiceContentEmpty, null);
        }

        try
        {
            XDocument document = XDocument.Parse(invoiceXml);
            return document.Root == null
                ? new XmlValidationResult(false, ErrorXmlEmptyOrNoRoot, document)
                : new XmlValidationResult(true, null, document);
        }
        catch (XmlException xmlEx)
        {
            return new XmlValidationResult(false, $"{ErrorXmlFormatPrefix}{xmlEx.Message}", null);
        }
    }

    /// <summary>
    /// Waliduje fakturę przed wysłaniem, sprawdzając format XML i wszystkie NIP-y oraz identyfikatory wewnętrzne.
    /// </summary>
    /// <param name="invoiceXml">Treść faktury w formacie XML.</param>
    /// <returns>Obiekt <see cref="InvoiceValidationResult"/> zawierający wyniki walidacji.</returns>
    public static InvoiceValidationResult ValidateInvoiceBeforeSending(string invoiceXml)
    {
        XmlValidationResult xmlValidationResult = ValidateInvoiceXmlFormat(invoiceXml);

        if (!xmlValidationResult.IsValid)
        {
            return new InvoiceValidationResult { XmlValidationResult = xmlValidationResult };
        }

        XDocument document = xmlValidationResult.InvoiceXDocument!;

        return new InvoiceValidationResult
        {
            XmlValidationResult = xmlValidationResult,
            SellerNipValidationResult = ValidateSellerNipInInvoice(document),
            BuyerNipValidationResult = ValidateBuyerNipInInvoice(document),
            ThirdSubjectsNipValidationResult = ValidateThirdSubjectsNipInInvoice(document),
            ThirdSubjectsInternalIdValidationResult = ValidateThirdSubjectsInternalIdsInInvoice(document)
        };
    }

    /// <summary>
    /// Waliduje NIP sprzedawcy (Podmiot1) w fakturze.
    /// </summary>
    /// <param name="invoiceXDocument">Dokument XML faktury.</param>
    /// <returns>Obiekt <see cref="ValidationResult"/> zawierający wyniki walidacji.</returns>
    public static ValidationResult ValidateSellerNipInInvoice(XDocument invoiceXDocument)
    {
        string nip = InvoiceXmlHelper.GetSellerNip(invoiceXDocument);

        if (string.IsNullOrWhiteSpace(nip))
        {
            return new ValidationResult(false, ErrorSellerNipNotFound);
        }

        return ValidateNip(nip, ErrorSellerNipInvalidFormatComposite, ErrorSellerNipInvalidChecksumComposite);
    }

    /// <summary>
    /// Waliduje NIP nabywcy (Podmiot2) w fakturze.
    /// </summary>
    /// <param name="invoiceXDocument">Dokument XML faktury.</param>
    /// <returns>Obiekt <see cref="ValidationResult"/> zawierający wyniki walidacji.</returns>
    public static ValidationResult ValidateBuyerNipInInvoice(XDocument invoiceXDocument)
    {
        string nip = InvoiceXmlHelper.GetBuyerNip(invoiceXDocument);

        if (string.IsNullOrWhiteSpace(nip))
        {
            // Podmiot2 może być określony bez NIP (np. NrVatUE)
            return new ValidationResult(true, InfoBuyerNipNotFound);
        }

        return ValidateNip(nip, ErrorBuyerNipInvalidFormatComposite, ErrorBuyerNipInvalidChecksumComposite);
    }

    /// <summary>
    /// Waliduje NIP podmiotów trzecich (Podmiot3) w fakturze.
    /// </summary>
    /// <param name="invoiceXDocument">Dokument XML faktury.</param>
    /// <returns>Lista obiektów <see cref="ValidationResult"/> zawierających wyniki walidacji dla każdego podmiotu trzeciego.</returns>
    public static List<ValidationResult> ValidateThirdSubjectsNipInInvoice(XDocument invoiceXDocument)
    {
        List<string> nips = InvoiceXmlHelper.GetThirdPartiesNips(invoiceXDocument);

        return nips
            .Select(nip => ValidateIdentifier(
                nip,
                RegexPatterns.Nip,
                IdentifierValidators.IsValidNip,
                string.Format(System.Globalization.CultureInfo.InvariantCulture, ErrorThirdPartyNipInvalidFormatComposite, nip),
                string.Format(System.Globalization.CultureInfo.InvariantCulture, ErrorThirdPartyNipInvalidChecksumComposite, nip)))
            .ToList();
    }

    /// <summary>
    /// Waliduje identyfikatory wewnętrzne (IDWew) podmiotów trzecich (Podmiot3) w fakturze.
    /// </summary>
    /// <param name="invoiceXDocument">Dokument XML faktury.</param>
    /// <returns>Lista obiektów <see cref="ValidationResult"/> zawierających wyniki walidacji dla każdego identyfikatora wewnętrznego.</returns>
    public static List<ValidationResult> ValidateThirdSubjectsInternalIdsInInvoice(XDocument invoiceXDocument)
    {
        List<string> internalIds = InvoiceXmlHelper.GetThirdPartiesInternalIds(invoiceXDocument);

        return internalIds
            .Select(id => ValidateIdentifier(
                id,
                RegexPatterns.InternalId,
                IdentifierValidators.IsValidInternalId,
                string.Format(System.Globalization.CultureInfo.InvariantCulture, ErrorThirdPartyIdWewInvalidFormatComposite, id),
                string.Format(System.Globalization.CultureInfo.InvariantCulture, ErrorThirdPartyIdWewInvalidChecksumComposite, id)))
            .ToList();
    }

    private static ValidationResult ValidateNip(string nip, CompositeFormat formatErrorMessage, CompositeFormat checksumErrorMessage)
    {
        if (!RegexPatterns.Nip.IsMatch(nip))
        {
            return new ValidationResult(false, string.Format(System.Globalization.CultureInfo.InvariantCulture, formatErrorMessage, nip));
        }

        if (!IdentifierValidators.IsValidNip(nip))
        {
            return new ValidationResult(false, string.Format(System.Globalization.CultureInfo.InvariantCulture, checksumErrorMessage, nip));
        }

        return new ValidationResult(true, null);
    }

    private static ValidationResult ValidateIdentifier(
        string value,
        Regex pattern,
        Func<string, bool> checksumValidator,
        string formatErrorMessage,
        string checksumErrorMessage)
    {
        if (!pattern.IsMatch(value))
        {
            return new ValidationResult(false, formatErrorMessage);
        }

        if (!checksumValidator(value))
        {
            return new ValidationResult(false, checksumErrorMessage);
        }

        return new ValidationResult(true, null);
    }
}