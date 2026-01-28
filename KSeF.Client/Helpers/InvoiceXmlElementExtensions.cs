namespace KSeF.Client.Helpers;

/// <summary>
/// Rozszerzenia dla enuma InvoiceXmlElement.
/// </summary>
public static class InvoiceXmlElementExtensions
{
    /// <summary>
    /// Konwertuje element enuma na nazwę elementu XML.
    /// </summary>
    public static string ToXmlName(this InvoiceXmlElement element) => element.ToString();
}
