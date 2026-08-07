using KSeF.Client.Helpers;
using KSeF.Client.Tests.Utils;
using System.Text;
using System.Xml.Linq;
namespace KSeF.Client.Tests.Core.UnitTests;

public class ValidationHelperTests
{
    private static string GetXmlInvoice(string nip1, string nip2, string nip3_1, string idwew1, string nip3_2, string idwew2)
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Templates", "invoice-template-fa-3-with-multiple-Subject3.xml");
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Template not found at: {path}");
        }

        string xml = File.ReadAllText(path, Encoding.UTF8);
        xml = xml.Replace("#nip_podmiot1#", nip1);
        xml = xml.Replace("#nip_podmiot2#", nip2);
        xml = xml.Replace("#nip1_podmiot3#", nip3_1);
        xml = xml.Replace("#idwew1_podmiot3#", idwew1);
        xml = xml.Replace("#nip2_podmiot3#", nip3_2);
        xml = xml.Replace("#idwew2_podmiot3#", idwew2);
        return xml;
    }

    [Fact]
    public void ValidateInvoiceBeforeSending_AllValidData_ReturnsSuccess()
    {
        // Arrange
        string validNip1 = MiscellaneousUtils.GetRandomNip();
        string validNip2 = MiscellaneousUtils.GetRandomNip();
        string validNip3_1 = MiscellaneousUtils.GetRandomNip();
        string validIdwew1 = MiscellaneousUtils.GenerateInternalIdentifier();
        string validNip3_2 = MiscellaneousUtils.GetRandomNip();
        string validIdwew2 = MiscellaneousUtils.GenerateInternalIdentifier();
        string xml = GetXmlInvoice(validNip1, validNip2, validNip3_1, validIdwew1, validNip3_2, validIdwew2);

        // Act
        InvoiceValidationResult result = ValidationHelper.ValidateInvoiceBeforeSending(xml);

        // Assert
        Assert.True(result.XmlValidationResult.IsValid);
        Assert.True(result.SellerNipValidationResult.IsValid);
        Assert.True(result.BuyerNipValidationResult.IsValid);
        Assert.True(result.ThirdSubjectsNipValidationResult.All(r => r.IsValid));
        Assert.True(result.ThirdSubjectsInternalIdValidationResult.All(r => r.IsValid));
    }

    [Fact]
    public void ValidateInvoiceBeforeSending_InvalidXml_ReturnsXmlError()
    {
        // Arrange
        string invalidXml = "<invalid>xml</invalid_>";

        // Act
        InvoiceValidationResult result = ValidationHelper.ValidateInvoiceBeforeSending(invalidXml);

        // Assert
        Assert.False(result.XmlValidationResult.IsValid);
    }

    [Fact]
    public void ValidateInvoiceBeforeSending_InvalidSellerNip_ReturnsSellerError()
    {
        // Arrange
        string invalidNip = "123";
        string xml = GetXmlInvoice(invalidNip, MiscellaneousUtils.GetRandomNip(), MiscellaneousUtils.GetRandomNip(),
            MiscellaneousUtils.GenerateInternalIdentifier(), MiscellaneousUtils.GetRandomNip(), MiscellaneousUtils.GenerateInternalIdentifier());

        // Act
        InvoiceValidationResult result = ValidationHelper.ValidateInvoiceBeforeSending(xml);

        // Assert
        Assert.False(result.SellerNipValidationResult.IsValid);
    }

    [Fact]
    public void ValidateInvoiceBeforeSending_MissingBuyerNip_ReturnsSuccess()
    {
        // Arrange - generuj pełny valid XML
        string validNip1 = MiscellaneousUtils.GetRandomNip();
        string validNip3_1 = MiscellaneousUtils.GetRandomNip();
        string validIdwew1 = MiscellaneousUtils.GenerateInternalIdentifier();
        string validNip3_2 = MiscellaneousUtils.GetRandomNip();
        string validIdwew2 = MiscellaneousUtils.GenerateInternalIdentifier();
        string fullXml = GetXmlInvoice(validNip1, "dummy", validNip3_1, validIdwew1, validNip3_2, validIdwew2);

        // Usuń <NIP> z Podmiot2
        XDocument doc = XDocument.Parse(fullXml);
        XNamespace ns = doc.Root.GetDefaultNamespace();
        doc.Root.Element(ns + "Podmiot2")?
             .Element(ns + "DaneIdentyfikacyjne")?
             .Element(ns + "NIP")?
             .Remove();
        string xmlNoBuyerNip = doc.ToString();

        // Act
        InvoiceValidationResult result = ValidationHelper.ValidateInvoiceBeforeSending(xmlNoBuyerNip);

        // Assert
        Assert.True(result.XmlValidationResult.IsValid);
        Assert.True(result.BuyerNipValidationResult.IsValid);  // dozwolone bez NIP
        Assert.True(result.SellerNipValidationResult.IsValid);
        Assert.True(result.ThirdSubjectsNipValidationResult.All(r => r.IsValid));
        Assert.True(result.ThirdSubjectsInternalIdValidationResult.All(r => r.IsValid));
    }


    [Fact]
    public void ValidateInvoiceBeforeSending_InvalidThirdNip_ReturnsThirdNipErrors()
    {
        // Arrange
        string invalidNip = "invalid";
        string xml = GetXmlInvoice(MiscellaneousUtils.GetRandomNip(), MiscellaneousUtils.GetRandomNip(), invalidNip,
            MiscellaneousUtils.GenerateInternalIdentifier(), invalidNip, MiscellaneousUtils.GenerateInternalIdentifier());

        // Act
        InvoiceValidationResult result = ValidationHelper.ValidateInvoiceBeforeSending(xml);

        // Assert
        Assert.Contains(result.ThirdSubjectsNipValidationResult, r => !r.IsValid);
        Assert.Equal(2, result.ThirdSubjectsNipValidationResult.Count(r => !r.IsValid));
    }

    [Fact]
    public void ValidateInvoiceBeforeSending_InvalidThirdInternalId_ReturnsInternalIdErrors()
    {
        // Arrange
        string invalidIdwew = "invalid";
        string xml = GetXmlInvoice(MiscellaneousUtils.GetRandomNip(), MiscellaneousUtils.GetRandomNip(), MiscellaneousUtils.GetRandomNip(),
            invalidIdwew, MiscellaneousUtils.GetRandomNip(), invalidIdwew);

        // Act
        InvoiceValidationResult result = ValidationHelper.ValidateInvoiceBeforeSending(xml);

        // Assert
        Assert.Contains(result.ThirdSubjectsInternalIdValidationResult, r => !r.IsValid);
        Assert.Equal(2, result.ThirdSubjectsInternalIdValidationResult.Count(r => !r.IsValid));
    }

    [Fact]
    public void ValidateInvoiceBeforeSending_EmptyInvoice_ReturnsXmlError()
    {
        // Act & Assert
        InvoiceValidationResult result = ValidationHelper.ValidateInvoiceBeforeSending("");
        Assert.False(result.XmlValidationResult.IsValid);
    }

    [Fact]
    public void ValidateInvoiceBeforeSending_DisallowedUnicodeCharacter_ReturnsXmlError()
    {
        // Arrange - znak kontrolny U+007F wstrzyknięty do nazwy sprzedawcy
        string xml = GetXmlInvoice(MiscellaneousUtils.GetRandomNip(), MiscellaneousUtils.GetRandomNip(), MiscellaneousUtils.GetRandomNip(),
            MiscellaneousUtils.GenerateInternalIdentifier(), MiscellaneousUtils.GetRandomNip(), MiscellaneousUtils.GenerateInternalIdentifier());
        xml = xml.Replace("</Naglowek>", "\u007F</Naglowek>");

        // Act
        InvoiceValidationResult result = ValidationHelper.ValidateInvoiceBeforeSending(xml);

        // Assert
        Assert.False(result.XmlValidationResult.IsValid);
        Assert.Contains("U+007F", result.XmlValidationResult.Message);
    }

    [Fact]
    public void ValidateInvoiceBeforeSending_MojibakeWithHiddenControlCharacters_ReturnsXmlError()
    {
        // Arrange - faktura z błędnie zdekodowanym tekstem (mojibake), zawierającym ukryte znaki kontrolne C1
        string path = Path.Combine(AppContext.BaseDirectory, "Templates", "invoice-template-fa-3-with-disallowed-unicode-characters.xml");
        string xml = File.ReadAllText(path, Encoding.UTF8);

        // Act
        InvoiceValidationResult result = ValidationHelper.ValidateInvoiceBeforeSending(xml);

        // Assert
        Assert.False(result.XmlValidationResult.IsValid);
        Assert.Contains("U+009B", result.XmlValidationResult.Message);
    }

    [Fact]
    public void ValidateInvoiceBeforeSending_XmlWithBom_ReturnsXmlError()
    {
        // Arrange - znak BOM (U+FEFF) na początku treści faktury
        string xml = "\uFEFF<Faktura></Faktura>";

        // Act
        InvoiceValidationResult result = ValidationHelper.ValidateInvoiceBeforeSending(xml);

        // Assert
        Assert.False(result.XmlValidationResult.IsValid);
    }

    [Fact]
    public void ValidateInvoiceBeforeSending_RawBytesWithBom_ReturnsXmlError()
    {
        // Arrange - surowe bajty UTF-8 z BOM (0xEF 0xBB 0xBF) na początku, tak jak faktycznie występuje w pliku
        byte[] bom = [0xEF, 0xBB, 0xBF];
        byte[] content = Encoding.UTF8.GetBytes("<Faktura></Faktura>");
        byte[] bytes = [.. bom, .. content];

        // Act
        InvoiceValidationResult result = ValidationHelper.ValidateInvoiceBeforeSending(bytes);

        // Assert
        Assert.False(result.XmlValidationResult.IsValid);
    }

    [Fact]
    public void ValidateInvoiceBeforeSending_RawBytesWithoutBom_DoesNotReturnBomError()
    {
        // Arrange - te same bajty, ale bez BOM
        byte[] bytes = Encoding.UTF8.GetBytes("<Faktura></Faktura>");

        // Act
        InvoiceValidationResult result = ValidationHelper.ValidateInvoiceBeforeSending(bytes);

        // Assert
        Assert.True(result.XmlValidationResult.IsValid);
    }

    [Fact]
    public void ValidateInvoiceBeforeSending_PrologDeclaresNonUtf8Encoding_ReturnsXmlError()
    {
        // Arrange - prolog XML wskazujący kodowanie inne niż UTF-8
        string xml = "<?xml version=\"1.0\" encoding=\"ISO-8859-2\"?><Faktura></Faktura>";

        // Act
        InvoiceValidationResult result = ValidationHelper.ValidateInvoiceBeforeSending(xml);

        // Assert
        Assert.False(result.XmlValidationResult.IsValid);
        Assert.Contains("ISO-8859-2", result.XmlValidationResult.Message);
    }

    [Fact]
    public void ValidateInvoiceBeforeSending_PrologDeclaresUtf8Encoding_DoesNotReturnEncodingError()
    {
        // Arrange - prolog XML jawnie wskazujący UTF-8 (dozwolone)
        string xml = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Faktura></Faktura>";

        // Act
        InvoiceValidationResult result = ValidationHelper.ValidateInvoiceBeforeSending(xml);

        // Assert
        Assert.True(result.XmlValidationResult.IsValid);
    }

    [Fact]
    public void ValidateInvoiceBeforeSending_ContainsProcessingInstruction_ReturnsXmlError()
    {
        // Arrange - instrukcja przetwarzania XML inna niż deklaracja <?xml ... ?>
        string xml = "<?xml version=\"1.0\"?><?custom-instruction data?><Faktura></Faktura>";

        // Act
        InvoiceValidationResult result = ValidationHelper.ValidateInvoiceBeforeSending(xml);

        // Assert
        Assert.False(result.XmlValidationResult.IsValid);
        Assert.Contains("custom-instruction", result.XmlValidationResult.Message);
    }
}
