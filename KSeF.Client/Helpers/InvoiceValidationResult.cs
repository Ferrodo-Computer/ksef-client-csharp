using System.Xml.Linq;

namespace KSeF.Client.Helpers;

/// <summary>
/// Zawiera wyniki walidacji faktury ze wszystkimi jej komponentami.
/// </summary>
public class InvoiceValidationResult
{
    /// <summary>
    /// Uogólniony wynik walidacji, łączy wszystkie wyniki cząstkowe.
    /// </summary>
    public ValidationResult ValidationResult => new(
        IsOverallValid(),
            string.Join(", ", GetAllErrorMessages()));
    /// <summary>
    /// Wynik walidacji formatu XML.
    /// </summary>
    public XmlValidationResult XmlValidationResult { get; set; }
    /// <summary>
    /// Wynik walidacji NIP sprzedawcy (Podmiot1).
    /// </summary>
    public ValidationResult SellerNipValidationResult { get; set; }
    /// <summary>
    /// Wynik walidacji NIP nabywcy (Podmiot2).
    /// </summary>
    public ValidationResult BuyerNipValidationResult { get; set; }
    /// <summary>
    /// Wynik walidacji NIP podmiotów trzecich (Podmiot3).
    /// </summary>
    public List<ValidationResult> ThirdSubjectsNipValidationResult { get; set; }
    /// <summary>
    /// Wynik walidacji identyfikatorów wewnętrznych (IDWew) podmiotów trzecich (Podmiot3).
    /// </summary>
    public List<ValidationResult> ThirdSubjectsInternalIdValidationResult { get; set; }
    
    private bool IsOverallValid()
    {
        return XmlValidationResult?.IsValid == true &&
               SellerNipValidationResult?.IsValid != false &&
               BuyerNipValidationResult?.IsValid != false &&
               ThirdSubjectsNipValidationResult?.All(r => r.IsValid) != false &&
               ThirdSubjectsInternalIdValidationResult?.All(r => r.IsValid) != false;
    }

    private IEnumerable<string> GetAllErrorMessages()
    {
        return GetType().GetProperties()
            .Where(p => p.Name != nameof(ValidationResult))
            .Where(p => typeof(ValidationResult).IsAssignableFrom(p.PropertyType))
            .Select(p => p.GetValue(this))
            .OfType<ValidationResult>()
            .Where(r => !r.IsValid)
            .Select(r => r.Message);
    }
}

/// <summary>
/// Wynik walidacji pojedynczego komponentu.
/// </summary>
/// <remarks>
/// Inicjalizuje nową instancję klasy <see cref="ValidationResult"/>.
/// </remarks>
/// <param name="isValid">Czy walidacja przebiegła pomyślnie.</param>
/// <param name="message">Komunikat walidacji (zwykle błędu).</param>
public class ValidationResult(bool isValid, string message)
{
    /// <summary>
    /// Wskazuje, czy walidacja przebiegła pomyślnie.
    /// </summary>
    public bool IsValid { get; } = isValid;
    /// <summary>
    /// Komunikat walidacji zawierający szczegóły będdu lub inne informacje.
    /// </summary>
    public string Message { get; } = message;
}

/// <summary>
/// Zawiera wynik walidacji formatu XML oraz dokument XML.
/// </summary>
/// <remarks>
/// Inicjalizuje nową instancję klasy <see cref="XmlValidationResult"/>.
/// </remarks>
/// <param name="isValid">Czy walidacja XML przebiegła pomyślnie.</param>
/// <param name="message">Komunikat walidacji.</param>
/// <param name="invoiceXDocument">Dokument XML faktury.</param>
public class XmlValidationResult(bool isValid, string message, XDocument invoiceXDocument) : ValidationResult(isValid, message)
{
    /// <summary>
    /// Dokument XML faktury.
    /// </summary>
    public XDocument InvoiceXDocument { get; } = invoiceXDocument;
}