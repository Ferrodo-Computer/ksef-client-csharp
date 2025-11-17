using System.Runtime.Serialization;

namespace KSeF.Client.Core.Models.Authorization
{
    public class AuthenticationKsefTokenSubjectIdentifier
    {
        public AuthenticationKsefTokenSubjectIdentifierType Type { get; set; }
        public string Value { get; set; }
    }

    public enum AuthenticationKsefTokenSubjectIdentifierType
    {
        [EnumMember(Value = "nip")]
        Nip,
        [EnumMember(Value = "pesel")]
        Pesel,
        [EnumMember(Value = "fingerprint")]
        Fingerprint
    }
}