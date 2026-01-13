using System.Runtime.Serialization;

namespace KSeF.Client.Core.Models.TestData
{
    public enum TestDataSubjectIdentifierType
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
