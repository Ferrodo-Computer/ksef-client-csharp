using System.Runtime.Serialization;

namespace KSeF.Client.Core.Models.TestData
{
    public class TestDataSubjectIdentifier
    {
        public TestDataSubjectIdentifierType Type { get; set; }
        public string Value { get; set; }
    }

    public enum TestDataSubjectIdentifierType
    {
        None,
        [EnumMember(Value = "nip")]
        Nip,
        [EnumMember(Value = "pesel")]
        Pesel,
        [EnumMember(Value = "fingerprint")]
        Fingerprint,
        [EnumMember(Value = "token")]
        Token
    }
}
