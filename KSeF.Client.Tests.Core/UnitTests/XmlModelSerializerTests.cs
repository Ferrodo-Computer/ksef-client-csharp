using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using KSeF.Client.Core.Models;
using KSeF.Client.Core.Models.Invoices;
using KSeF.Client.Core.Models.Permissions.Entity;
using KSeF.Client.Core.Models.Permissions.Identifiers;
using KSeF.Client.Core.Models.Sessions;
using KSeF.Client.Core.Models.Sessions.BatchSession;
using KSeF.Client.XmlSerialization;

namespace KSeF.Client.Tests.Core.UnitTests;

public class XmlModelSerializerTests
{
    [Fact]
    public void Serialize_SessionInvoice_WritesUriCollectionsAndDictionary()
    {
        XmlModelSerializer serializer = new();
        SessionInvoice invoice = CreateSessionInvoice();

        string xml = serializer.Serialize(invoice);

        Assert.Contains("<UpoDownloadUrl>https://example.test/upo/123</UpoDownloadUrl>", xml);
        Assert.Contains("<Details>", xml);
        Assert.Contains("Duplikat faktury", xml);
        Assert.Contains("<Extensions>", xml);
        Assert.Contains("<Key>originalSessionReferenceNumber</Key>", xml);
        Assert.Contains("<Value>20260707-SO-2A4C4B6000-DC79898348-43</Value>", xml);
    }

    [Fact]
    public void Deserialize_SessionInvoice_ReadsUriCollectionsAndDictionary()
    {
        XmlModelSerializer serializer = new();
        SessionInvoice expected = CreateSessionInvoice();
        string xml = serializer.Serialize(expected);

        SessionInvoice actual = serializer.Deserialize<SessionInvoice>(xml);

        Assert.Equal(expected.UpoDownloadUrl, actual.UpoDownloadUrl);
        Assert.IsType<List<string>>(actual.Status.Details);
        Assert.Contains("Duplikat faktury. Faktura o numerze KSeF: 3400864125-20260707-64B07F400000-AE została już prawidłowo przesłana do systemu w sesji: 20260707-SO-2A4C4B6000-DC79898348-43", actual.Status.Details);
        Assert.IsType<Dictionary<string, string>>(actual.Status.Extensions);
        Assert.Equal("3400864125-20260707-64B07F400000-AE", actual.Status.Extensions["originalKsefNumber"]);
    }

    [Fact]
    public void Deserialize_SessionInvoicesResponse_CreatesListForICollectionProperty()
    {
        XmlModelSerializer serializer = new();
        SessionInvoicesResponse response = new()
        {
            ContinuationToken = "next",
            Invoices = new List<SessionInvoice> { CreateSessionInvoice() }
        };

        SessionInvoicesResponse actual = serializer.Deserialize<SessionInvoicesResponse>(serializer.Serialize(response));

        Assert.IsType<List<SessionInvoice>>(actual.Invoices);
        Assert.Single(actual.Invoices);
    }

    [Fact]
    public void SerializeAndDeserialize_PackagePartSignatureInitResponseType_HandlesHeadersDictionary()
    {
        XmlModelSerializer serializer = new();
        PackagePartSignatureInitResponseType response = new()
        {
            Method = "PUT",
            OrdinalNumber = 1,
            Url = new Uri("https://upload.example.test/part/1"),
            Headers = new Dictionary<string, string>
            {
                ["x-ms-blob-type"] = "BlockBlob",
                ["x-special"] = "a<b&c"
            }
        };

        PackagePartSignatureInitResponseType actual = serializer.Deserialize<PackagePartSignatureInitResponseType>(serializer.Serialize(response));

        Assert.Equal(response.Url, actual.Url);
        Assert.Equal("BlockBlob", actual.Headers["x-ms-blob-type"]);
        Assert.Equal("a<b&c", actual.Headers["x-special"]);
    }

    [Fact]
    public void SerializeAndDeserialize_UpoResponse_HandlesNestedCollectionAndUri()
    {
        XmlModelSerializer serializer = new();
        UpoResponse response = new()
        {
            Pages = new List<UpoPageResponse>
            {
                new()
                {
                    ReferenceNumber = "20260707-SO-2F6678D000-97123437E7-5A",
                    DownloadUrl = new Uri("https://example.test/upo/page/1"),
                    DownloadUrlExpirationDate = new DateTimeOffset(2026, 7, 7, 12, 0, 0, TimeSpan.FromHours(2))
                }
            }
        };

        UpoResponse actual = serializer.Deserialize<UpoResponse>(serializer.Serialize(response));

        Assert.Single(actual.Pages);
        Assert.Equal(new Uri("https://example.test/upo/page/1"), actual.Pages.First().DownloadUrl);
    }

    [Fact]
    public void Serialize_NullValues_OmitsByDefault()
    {
        XmlModelSerializer serializer = new();
        SessionInvoice invoice = CreateSessionInvoice();
        invoice.InvoiceNumber = null;
        invoice.Status.Extensions = null;

        string xml = serializer.Serialize(invoice);

        Assert.DoesNotContain("<InvoiceNumber", xml);
        Assert.DoesNotContain("<Extensions", xml);
    }

    [Fact]
    public void Serialize_NullValues_CanEmitNilElements()
    {
        XmlModelSerializer serializer = new(new XmlSerializationOptions { EmitNullValues = true });
        SessionInvoice invoice = CreateSessionInvoice();
        invoice.InvoiceNumber = null;

        string xml = serializer.Serialize(invoice);

        Assert.Contains("<InvoiceNumber", xml);
        Assert.Contains("nil=\"true\"", xml);
    }

    [Fact]
    public void Serialize_EmptyCollections_WritesContainerWithoutItems()
    {
        XmlModelSerializer serializer = new();
        InvoiceStatusInfo status = new()
        {
            Code = 427,
            Description = "Duplikat faktury",
            Details = new List<string>(),
            Extensions = new Dictionary<string, string>()
        };

        string xml = serializer.Serialize(status);

        Assert.Contains("<Details />", xml);
        Assert.Contains("<Extensions />", xml);
    }

    [Fact]
    public void Deserialize_InvalidUri_ThrowsXmlModelSerializationException()
    {
        XmlModelSerializer serializer = new();
        const string xml = "<UpoPageResponse><ReferenceNumber>ref</ReferenceNumber><DownloadUrl>http://[bad-uri</DownloadUrl></UpoPageResponse>";

        XmlModelSerializationException exception = Assert.Throws<XmlModelSerializationException>(() => serializer.Deserialize<UpoPageResponse>(xml));
        Assert.Contains("Invalid URI", exception.Message);
        Assert.Contains("DownloadUrl", exception.Message);
    }

    [Fact]
    public void Deserialize_DuplicateDictionaryKeys_ThrowsXmlModelSerializationException()
    {
        XmlModelSerializer serializer = new();
        const string xml = "<InvoiceStatusInfo><Code>427</Code><Description>Duplikat faktury</Description><Extensions><Item><Key>a</Key><Value>1</Value></Item><Item><Key>a</Key><Value>2</Value></Item></Extensions></InvoiceStatusInfo>";

        XmlModelSerializationException exception = Assert.Throws<XmlModelSerializationException>(() => serializer.Deserialize<InvoiceStatusInfo>(xml));
        Assert.Contains("Duplicate dictionary key", exception.Message);
    }

    [Fact]
    public void Serialize_UsesInvariantFormatting()
    {
        CultureInfo currentCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("pl-PL");
        try
        {
            XmlModelSerializer serializer = new();
            FormattingFixture fixture = new()
            {
                Amount = 1234.56m,
                Timestamp = new DateTimeOffset(2026, 7, 7, 14, 15, 16, TimeSpan.FromHours(2))
            };

            string xml = serializer.Serialize(fixture);

            Assert.Contains("<Amount>1234.56</Amount>", xml);
            Assert.Contains("<Timestamp>2026-07-07T14:15:16.0000000+02:00</Timestamp>", xml);
        }
        finally
        {
            CultureInfo.CurrentCulture = currentCulture;
        }
    }

    [Fact]
    public void StreamOverloads_RoundTripModel()
    {
        XmlModelSerializer serializer = new();
        SessionInvoice invoice = CreateSessionInvoice();
        using MemoryStream stream = new();

        serializer.Serialize(stream, invoice);
        stream.Position = 0;
        SessionInvoice actual = serializer.Deserialize<SessionInvoice>(stream);

        Assert.Equal(invoice.ReferenceNumber, actual.ReferenceNumber);
        Assert.Equal(invoice.UpoDownloadUrl, actual.UpoDownloadUrl);
    }

    [Fact]
    public void Options_CanOverrideRootAndDictionaryElementNames()
    {
        XmlModelSerializer serializer = new(new XmlSerializationOptions
        {
            RootElementName = "Status",
            DictionaryItemElementName = "Entry",
            DictionaryKeyElementName = "Name",
            DictionaryValueElementName = "Text"
        });
        InvoiceStatusInfo status = new()
        {
            Code = 427,
            Description = "Duplikat faktury",
            Extensions = new Dictionary<string, string>
            {
                ["originalSessionReferenceNumber"] = "20260707-SO-2A4C4B6000-DC79898348-43",
                ["originalKsefNumber"] = "3400864125-20260707-64B07F400000-AE"
            }
        };

        string xml = serializer.Serialize(status);
        InvoiceStatusInfo actual = serializer.Deserialize<InvoiceStatusInfo>(xml);

        Assert.Contains("<Status>", xml);
        Assert.Contains("<Entry>", xml);
        Assert.Contains("<Name>originalSessionReferenceNumber</Name>", xml);
        Assert.Contains("<Text>20260707-SO-2A4C4B6000-DC79898348-43</Text>", xml);
        Assert.Equal("3400864125-20260707-64B07F400000-AE", actual.Extensions["originalKsefNumber"]);
    }

    [Fact]
    public void SerializeAndDeserialize_ByteArrayProperties_UsesBase64()
    {
        XmlModelSerializer serializer = new();
        EncryptionData encryptionData = new()
        {
            CipherKey = new byte[] { 1, 2, 3, 4 },
            CipherIv = new byte[] { 5, 6, 7, 8 },
            EncryptionInfo = new EncryptionInfo
            {
                EncryptedSymmetricKey = "key",
                InitializationVector = "iv"
            }
        };

        string xml = serializer.Serialize(encryptionData);
        EncryptionData actual = serializer.Deserialize<EncryptionData>(xml);

        Assert.Contains("<CipherKey>AQIDBA==</CipherKey>", xml);
        Assert.Equal(encryptionData.CipherKey, actual.CipherKey);
        Assert.Equal(encryptionData.CipherIv, actual.CipherIv);
    }

    [Fact]
    public void SerializeAndDeserialize_BatchPartSendingInfo_HandlesPayloadBytes()
    {
        XmlModelSerializer serializer = new();
        BatchPartSendingInfo part = new()
        {
            Data = Encoding.UTF8.GetBytes("invoice-part"),
            Metadata = new FileMetadata
            {
                HashSHA = "hash",
                FileSize = 12
            },
            OrdinalNumber = 1
        };

        BatchPartSendingInfo actual = serializer.Deserialize<BatchPartSendingInfo>(serializer.Serialize(part));

        Assert.Equal(part.Data, actual.Data);
        Assert.Equal(part.Metadata.HashSHA, actual.Metadata.HashSHA);
    }

    [Fact]
    public void Deserialize_TypeWithoutParameterlessConstructor_CreatesDtoAndSetsProperties()
    {
        XmlModelSerializer serializer = new();
        GrantPermissionsEntityRequest request = new()
        {
            SubjectIdentifier = new GrantPermissionsEntitySubjectIdentifier
            {
                Type = GrantPermissionsEntitySubjectIdentifierType.Nip,
                Value = "1234567890"
            },
            Permissions = new List<EntityPermission>
            {
                EntityPermission.New(EntityStandardPermissionType.InvoiceRead, true)
            },
            Description = "test",
            SubjectDetails = new PermissionsEntitySubjectDetails
            {
                FullName = "Podmiot Testowy"
            }
        };

        GrantPermissionsEntityRequest actual = serializer.Deserialize<GrantPermissionsEntityRequest>(serializer.Serialize(request));

        Assert.Single(actual.Permissions);
        EntityPermission permission = actual.Permissions.First();
        Assert.Equal(EntityStandardPermissionType.InvoiceRead, permission.Type);
        Assert.True(permission.CanDelegate);
    }

    [Fact]
    public void Deserialize_EmptyNullableElement_ReturnsNull()
    {
        XmlModelSerializer serializer = new();
        const string xml = "<SessionInvoice><OrdinalNumber>1</OrdinalNumber><AcquisitionDate /></SessionInvoice>";

        SessionInvoice actual = serializer.Deserialize<SessionInvoice>(xml);

        Assert.Null(actual.AcquisitionDate);
    }

    private static SessionInvoice CreateSessionInvoice()
    {
        return new SessionInvoice
        {
            OrdinalNumber = 1,
            InvoiceNumber = "FV/1/2026",
            KsefNumber = "5265437635-20260707-0101111AF629-AF",
            ReferenceNumber = "20260707-SO-2F6678D000-97123437E7-5A",
            InvoiceHash = "mhmZZXQz7QqxwFn0SyUpVrDWWPtq/Egc9QmNyYE=",
            InvoiceFileName = "invoice.xml",
            AcquisitionDate = new DateTimeOffset(2026, 7, 7, 10, 0, 0, TimeSpan.FromHours(2)),
            InvoicingDate = new DateTimeOffset(2026, 7, 7, 9, 0, 0, TimeSpan.FromHours(2)),
            PermanentStorageDate = new DateTimeOffset(2026, 7, 7, 11, 0, 0, TimeSpan.FromHours(2)),
            UpoDownloadUrl = new Uri("https://example.test/upo/123"),
            Status = new InvoiceStatusInfo
            {
                Code = 427,
                Description = "Duplikat faktury",
                Details = new List<string> { "Duplikat faktury. Faktura o numerze KSeF: 3400864125-20260707-64B07F400000-AE została już prawidłowo przesłana do systemu w sesji: 20260707-SO-2A4C4B6000-DC79898348-43" },
                Extensions = new Dictionary<string, string> { ["originalSessionReferenceNumber"] = "20260707-SO-2A4C4B6000-DC79898348-43", ["originalKsefNumber"] = "3400864125-20260707-64B07F400000-AE" }
            },
            InvoicingMode = InvoicingMode.Online,
            UpoDownloadUrlExpirationDate = new DateTimeOffset(2026, 7, 7, 23, 59, 0, TimeSpan.FromHours(2))
        };
    }

    private sealed class FormattingFixture
    {
        public decimal Amount { get; set; }

        public DateTimeOffset Timestamp { get; set; }
    }
}
