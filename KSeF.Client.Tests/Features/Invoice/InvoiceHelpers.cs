using System.Globalization;
using System.Text;
using System.Xml.Linq;

namespace KSeF.Client.Tests.Features;

public partial class InvoiceTests
{
    /// <summary>
    /// Pomocnicza klasa do pracy z szablonami faktur w testach.
    /// Umożliwia odczyt szablonu, podmianę wartości elementów oraz formatowanie dat.
    /// </summary>
    private sealed class InvoiceHelpers
    {
        /// <summary>
        /// Wczytuje treść pliku szablonu z folderu "Templates", a następnie
        /// podmienia w nim placeholdery <c>#nip#</c> oraz <c>#invoice_number#</c>.
        /// </summary>
        /// <param name="templatePath">Nazwa/ścieżka pliku szablonu względem folderu "Templates".</param>
        /// <param name="nip">Wartość NIP do wstawienia w miejsce <c>#nip#</c>.</param>
        /// <returns>Treść szablonu po podmianie placeholderów.</returns>
        /// <exception cref="DirectoryNotFoundException">Gdy plik szablonu nie zostanie znaleziony pod zbudowaną ścieżką.</exception>
        public static string GetTemplateText(string templatePath, string nip)
        {
            string path = Path.Combine(AppContext.BaseDirectory, "Templates", templatePath);
            if (!File.Exists(path))
            {
                throw new DirectoryNotFoundException($"Templates nie znaleziono pod: {path}");
            }

            return File.ReadAllText(path, Encoding.UTF8)
                .Replace("#nip#", nip)
                .Replace("#invoice_number#", Guid.NewGuid().ToString());
        }

        /// <summary>
        /// Ustawia wartość pierwszego elementu o wskazanej lokalnej nazwie w podanym XML.
        /// Zachowuje oryginalne białe znaki i zwraca XML bez dodatkowego formatowania.
        /// </summary>
        /// <param name="xml">Wejściowy dokument XML jako tekst.</param>
        /// <param name="localName">Lokalna nazwa elementu, którego wartość ma zostać zmieniona.</param>
        /// <param name="value">Nowa wartość elementu.</param>
        /// <returns>Tekst XML po modyfikacji.</returns>
        /// <exception cref="InvalidOperationException">Gdy element o podanej nazwie nie zostanie znaleziony.</exception>
        /// <exception cref="System.Xml.XmlException">Gdy wejściowy tekst nie jest poprawnym XML-em.</exception>
        public static string SetElementValue(string xml, string localName, string value)
        {
            XDocument doc = XDocument.Parse(xml, LoadOptions.PreserveWhitespace);

            XElement el = doc.Descendants().FirstOrDefault(e => e.Name.LocalName == localName)
                     ?? throw new InvalidOperationException($"Element '{localName}' nie znaleziono w Templates.");
            el.Value = value;

            return doc.ToString(SaveOptions.DisableFormatting);
        }

        /// <summary>
        /// Formatuje datę odpowiednio do typu elementu XML:
        /// - <c>DataWytworzeniaFa</c>: pełna data/czas UTC w ISO 8601 z sufiksem 'Z',
        /// - <c>P_1</c> i inne: data w formacie <c>yyyy-MM-dd</c>.
        /// </summary>
        /// <param name="elementName">Nazwa elementu decydująca o formacie daty.</param>
        /// <param name="date">Data do sformatowania.</param>
        /// <returns>Sformatowana wartość daty jako tekst.</returns>
        public static string SetDateForElement(string elementName, DateTime date)
        {
            return elementName switch
            {
                "DataWytworzeniaFa" => date.ToUniversalTime()
                    .ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture),
                "P_1" => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                _ => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            };
        }
    }
}