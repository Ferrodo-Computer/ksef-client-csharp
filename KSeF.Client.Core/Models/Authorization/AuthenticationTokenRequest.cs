using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace KSeF.Client.Core.Models.Authorization
{
    [XmlRoot("AuthTokenRequest", Namespace = "http://ksef.mf.gov.pl/auth/token/2.0")]
    public class AuthenticationTokenRequest
    {
        public string Challenge { get; set; }

        public AuthenticationTokenContextIdentifier ContextIdentifier { get; set; }

        public AuthenticationTokenSubjectIdentifierTypeEnum SubjectIdentifierType { get; set; }

        public AuthenticationTokenAuthorizationPolicy AuthorizationPolicy { get; set; }    
    }

    public class AuthenticationTokenContextIdentifier : IXmlSerializable
    {
        public AuthenticationTokenContextIdentifierType Type { get; set; }
        public string Value { get; set; }

        public XmlSchema GetSchema() => null;

        public void ReadXml(XmlReader reader)
        {
            throw new NotImplementedException();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement(Type.ToString());
            if (Value != null)
            {
                writer.WriteString(Value);
            }

            writer.WriteEndElement();
        }
    }
    public enum AuthenticationTokenContextIdentifierType
    {
        [EnumMember(Value = "Nip")]
        [XmlEnum("Nip")]
        Nip,
        [EnumMember(Value = "InternalId")]
        [XmlEnum("InternalId")]
        InternalId,
        [EnumMember(Value = "NipVatUe")]
        [XmlEnum("NipVatUe")]
        NipVatUe,
        [EnumMember(Value = "PeppolId")]
        [XmlEnum("PeppolId")]
        PeppolId
    }

    public enum AuthenticationTokenSubjectIdentifierTypeEnum
    {
        [EnumMember(Value = "certificateSubject")]
        [XmlEnum("certificateSubject")]
        CertificateSubject,
        [EnumMember(Value = "certificateFingerprint")]
        [XmlEnum("certificateFingerprint")]
        CertificateFingerprint
    }

    public class AuthenticationTokenAuthorizationPolicy
    {
        public AuthenticationTokenAllowedIps AllowedIps { get; set; } = new AuthenticationTokenAllowedIps();
    }

    public class AuthenticationTokenAllowedIps
    {
        [XmlElement("Ip4Address")]
        public List<string> Ip4Addresses { get; set; } = new List<string>();

        [XmlElement("Ip4Range")]
        public List<string> Ip4Ranges { get; set; } = new List<string>();

        [XmlElement("Ip4Mask")]
        public List<string> Ip4Masks { get; set; } = new List<string>();
    }

    public class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }

    public static class AuthenticationTokenRequestSerializer
    {
        public static string SerializeToXmlString(this AuthenticationTokenRequest request)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(AuthenticationTokenRequest));

            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = false,
                Encoding = Encoding.UTF8
            };

            using (Utf8StringWriter sw = new Utf8StringWriter())
            {
                using (XmlWriter writer = XmlWriter.Create(sw, settings))
                {
                    serializer.Serialize(writer, request);
                }
                return sw.ToString();
            }
        }
    }
}
