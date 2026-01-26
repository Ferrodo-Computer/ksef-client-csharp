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
}
