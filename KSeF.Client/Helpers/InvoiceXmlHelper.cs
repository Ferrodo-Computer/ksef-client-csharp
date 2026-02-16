using System.Xml.Linq;

namespace KSeF.Client.Helpers;

/// <summary>
/// Klasa pomocnicza do wyciągania danych z dokumentów XML faktur.
/// </summary>
public static class InvoiceXmlHelper
{

    /// <summary>
    /// Pobiera NIP sprzedawcy (Podmiot1) z faktury.
    /// </summary>
    /// <param name="invoiceXDocument">Dokument XML faktury.</param>
    /// <returns>NIP sprzedawcy lub null jeśli nie został znaleziony.</returns>
    public static string GetSellerNip(XDocument invoiceXDocument)
    {
        return GetElementValue(invoiceXDocument, 
            InvoiceXmlElement.Podmiot1, 
            InvoiceXmlElement.DaneIdentyfikacyjne, 
            InvoiceXmlElement.NIP);
    }

    /// <summary>
    /// Pobiera NIP nabywcy (Podmiot2) z faktury.
    /// </summary>
    /// <param name="invoiceXDocument">Dokument XML faktury.</param>
    /// <returns>NIP nabywcy lub null jeśli nie został znaleziony.</returns>
    public static string GetBuyerNip(XDocument invoiceXDocument)
    {
        return GetElementValue(invoiceXDocument, 
            InvoiceXmlElement.Podmiot2, 
            InvoiceXmlElement.DaneIdentyfikacyjne, 
            InvoiceXmlElement.NIP);
    }

    /// <summary>
    /// Pobiera listę NIP-ów podmiotów trzecich (Podmiot3) z faktury.
    /// </summary>
    /// <param name="invoiceXDocument">Dokument XML faktury.</param>
    /// <returns>Lista NIP-ów podmiotów trzecich.</returns>
    public static List<string> GetThirdPartiesNips(XDocument invoiceXDocument)
    {
        return GetDescendantValues(invoiceXDocument, InvoiceXmlElement.Podmiot3, InvoiceXmlElement.NIP);
    }

    /// <summary>
    /// Pobiera listę identyfikatorów wewnętrznych (IDWew) podmiotów trzecich (Podmiot3) z faktury.
    /// </summary>
    /// <param name="invoiceXDocument">Dokument XML faktury.</param>
    /// <returns>Lista identyfikatorów wewnętrznych podmiotów trzecich.</returns>
    public static List<string> GetThirdPartiesInternalIds(XDocument invoiceXDocument)
    {
        return GetDescendantValues(invoiceXDocument, InvoiceXmlElement.Podmiot3, InvoiceXmlElement.IDWew);
    }

    /// <summary>
    /// Pobiera wartość elementu na podstawie ścieżki elementów.
    /// </summary>
    /// <param name="document">Dokument XML.</param>
    /// <param name="elementPath">Ścieżka elementów (kolejne elementy enuma).</param>
    /// <returns>Wartość elementu lub null jeśli element nie został znaleziony.</returns>
    public static string GetElementValue(XDocument document, params InvoiceXmlElement[] elementPath)
    {
        XNamespace ns = document.Root?.GetDefaultNamespace() ?? "";
        XElement current = document.Root;

        foreach (InvoiceXmlElement element in elementPath)
        {
            current = current?.Element(ns + element.ToXmlName());
            if (current is null)
            {
                return null;
            }
        }

        return current?.Value;
    }

    /// <summary>
    /// Pobiera wartości wszystkich elementów potomnych o określonej nazwie, znajdujących się w elementach nadrzędnych o określonej nazwie.
    /// </summary>
    /// <param name="document">Dokument XML.</param>
    /// <param name="parentElement">Element nadrzędny.</param>
    /// <param name="targetElement">Element docelowy.</param>
    /// <returns>Lista wartości znalezionych elementów.</returns>
    public static List<string> GetDescendantValues(XDocument document, InvoiceXmlElement parentElement, InvoiceXmlElement targetElement)
    {
        XNamespace ns = document.Root?.GetDefaultNamespace() ?? "";

        return document.Root?
            .Descendants(ns + parentElement.ToXmlName())
            .SelectMany(p => p.Descendants(ns + targetElement.ToXmlName()))
            .Select(e => e.Value ?? "")
            .ToList() ?? [];
    }
}
