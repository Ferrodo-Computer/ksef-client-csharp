
using System.Runtime.Serialization;

namespace KSeF.Client.Core.Models.Certificates
{
    public class CertificateSubjectIdentifier
    {
        public CertificateSubjectIdentifierType Type { get; set; }
        public string Value { get; set; }
    }

    public enum CertificateSubjectIdentifierType
    {
        None,
        [EnumMember(Value = "nip")]
        Nip,
        [EnumMember(Value = "pesel")]
        Pesel,
        [EnumMember(Value = "fingerprint")]
        Fingerprint
    }
}