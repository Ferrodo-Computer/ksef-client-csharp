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
    private const string ErrorDisallowedUnicodeCharacter = "Niedozwolony znak Unicode w fakturze: {0}";
    private const string ErrorBomDetected = "Faktura zawiera znak BOM na początku treści. Wymagane jest kodowanie UTF-8 bez BOM.";
    private const string ErrorPrologEncodingNotUtf8 = "Prolog XML wskazuje kodowanie inne niż UTF-8: {0}.";
    private const string ErrorProcessingInstructionFound = "Faktura nie może zawierać instrukcji przetwarzania XML (processing instructions): {0}.";

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
    private static readonly CompositeFormat ErrorDisallowedUnicodeCharacterComposite = CompositeFormat.Parse(ErrorDisallowedUnicodeCharacter);
    private static readonly CompositeFormat ErrorPrologEncodingNotUtf8Composite = CompositeFormat.Parse(ErrorPrologEncodingNotUtf8);
    private static readonly CompositeFormat ErrorProcessingInstructionFoundComposite = CompositeFormat.Parse(ErrorProcessingInstructionFound);

	/// <summary>
	/// Waliduje format XML faktury na podstawie surowych bajtów, zanim zostaną zdekodowane do stringa.
	/// Pozwala to wykryć sekwencję bajtów BOM przed dekodowaniem, który typowe metody dekodowania (np. <see cref="System.IO.File.ReadAllText(string)"/>) zdejmują automatycznie.
	/// </summary>
	/// <param name="invoiceXmlBytes">Surowa zawartość faktury w formacie XML.</param>
	/// <returns>Obiekt <see cref="XmlValidationResult"/> zawierający wyniki walidacji.</returns>
	public static XmlValidationResult ValidateInvoiceXmlFormat(byte[] invoiceXmlBytes)
    {
        if (invoiceXmlBytes == null || invoiceXmlBytes.Length == 0)
        {
            return new XmlValidationResult(false, ErrorInvoiceContentEmpty, null);
        }

        if (invoiceXmlBytes.Length >= 3
            && invoiceXmlBytes[0] == 0xEF
            && invoiceXmlBytes[1] == 0xBB
            && invoiceXmlBytes[2] == 0xBF)
        {
            return new XmlValidationResult(false, ErrorBomDetected, null);
        }

        return ValidateInvoiceXmlFormat(Encoding.UTF8.GetString(invoiceXmlBytes));
    }

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

        if (invoiceXml[0] == '\uFEFF')
        {
            return new XmlValidationResult(false, ErrorBomDetected, null);
        }

        string disallowedCharacter = XmlUnicodeValidator.FindDisallowedUnicodeCharacter(invoiceXml);
        if (disallowedCharacter != null)
        {
            return new XmlValidationResult(false, string.Format(System.Globalization.CultureInfo.InvariantCulture, ErrorDisallowedUnicodeCharacterComposite, disallowedCharacter), null);
        }

        XDocument document;
        try
        {
            document = XDocument.Parse(invoiceXml);
        }
        catch (XmlException xmlEx)
        {
            return new XmlValidationResult(false, $"{ErrorXmlFormatPrefix}{xmlEx.Message}", null);
        }

        if (document.Root == null)
        {
            return new XmlValidationResult(false, ErrorXmlEmptyOrNoRoot, document);
        }

        string declaredEncoding = document.Declaration?.Encoding;
        if (declaredEncoding != null && !string.Equals(declaredEncoding, "UTF-8", StringComparison.OrdinalIgnoreCase))
        {
            return new XmlValidationResult(false, string.Format(System.Globalization.CultureInfo.InvariantCulture, ErrorPrologEncodingNotUtf8Composite, declaredEncoding), null);
        }

        XProcessingInstruction processingInstruction = document.DescendantNodes().OfType<XProcessingInstruction>().FirstOrDefault();
        if (processingInstruction != null)
        {
            return new XmlValidationResult(false, string.Format(System.Globalization.CultureInfo.InvariantCulture, ErrorProcessingInstructionFoundComposite, processingInstruction.Target), null);
        }

        return new XmlValidationResult(true, null, document);
    }

    /// <summary>
    /// Waliduje fakturę przed wysłaniem na podstawie surowych bajtów, sprawdzając format XML (w tym BOM) i wszystkie NIP-y oraz identyfikatory wewnętrzne.
    /// </summary>
    /// <param name="invoiceXmlBytes">Surowa zawartość faktury w formacie XML.</param>
    /// <returns>Obiekt <see cref="InvoiceValidationResult"/> zawierający wyniki walidacji.</returns>
    public static InvoiceValidationResult ValidateInvoiceBeforeSending(byte[] invoiceXmlBytes)
    {
        return BuildInvoiceValidationResult(ValidateInvoiceXmlFormat(invoiceXmlBytes));
    }

    /// <summary>
    /// Waliduje fakturę przed wysłaniem, sprawdzając format XML i wszystkie NIP-y oraz identyfikatory wewnętrzne.
    /// </summary>
    /// <param name="invoiceXml">Treść faktury w formacie XML.</param>
    /// <returns>Obiekt <see cref="InvoiceValidationResult"/> zawierający wyniki walidacji.</returns>
    public static InvoiceValidationResult ValidateInvoiceBeforeSending(string invoiceXml)
    {
        return BuildInvoiceValidationResult(ValidateInvoiceXmlFormat(invoiceXml));
    }

    private static InvoiceValidationResult BuildInvoiceValidationResult(XmlValidationResult xmlValidationResult)
    {
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