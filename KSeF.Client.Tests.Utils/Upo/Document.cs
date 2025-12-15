using System.Xml.Serialization;

namespace KSeF.Client.Tests.Utils.Upo;

[XmlRoot("Dokument")]
public class Document
{
    [XmlElement("NipSprzedawcy")]
    public required string SellerNip { get; set; }

    [XmlElement("NumerKSeFDokumentu")]
    public required string KSeFDocumentNumber { get; set; }

    [XmlElement("NumerFaktury")]
    public required string InvoiceNumber { get; set; }

    // date (YYYY-MM-DD)
    [XmlElement("DataWystawieniaFaktury", DataType = "date")]
    public required DateTime InvoiceIssueDate { get; set; }

    // dateTime with offset
    [XmlElement("DataPrzeslaniaDokumentu")]
    public required DateTime DocumentSubmissionDate { get; set; }

    // dateTime with offset
    [XmlElement("DataNadaniaNumeruKSeF")]
    public required DateTime KSeFNumberAssignmentDate { get; set; }

    [XmlElement("SkrotDokumentu")]
    public required string DocumentHash { get; set; }

	[XmlElement("TrybWysylki")]
	public string InvoicingMode { get; set; }
}