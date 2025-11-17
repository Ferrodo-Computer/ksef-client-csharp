using System.Runtime.Serialization;

namespace KSeF.Client.Core.Models.Invoices
{
    public enum SystemCode
    {
        [EnumMember(Value = "FA (2)")] FA2,
        [EnumMember(Value = "FA (3)")] FA3,
        [EnumMember(Value = "PEF (3)")] PEF,
        [EnumMember(Value = "PEF_KOR (3)")] PEFKOR
    }

    public static class SystemCodeHelper
    {
        public static string GetSystemCode(SystemCode code)
        {
            switch (code)
            {
                case SystemCode.FA2:
                    return "FA (2)";
                case SystemCode.FA3:
                    return "FA (3)";
                case SystemCode.PEF:
                    return "PEF (3)";
                case SystemCode.PEFKOR:
                    return "PEF_KOR (3)";
                default:
                    return code.ToString();
            }
        }

        public static string GetValue(SystemCode code)
        {
            switch (code)
            {
                case SystemCode.FA2:
                    return "FA";
                case SystemCode.FA3:
                    return "FA";
                case SystemCode.PEF:
                    return "PEF";
                case SystemCode.PEFKOR:
                    return "PEF";
                default:
                    return code.ToString();
            }
        }

        public static string GetSchemaVersion(SystemCode code)
        {
            switch (code)
            {
                case SystemCode.FA2:
                    return "1-0E";
                case SystemCode.FA3:
                    return "1-0E";
                case SystemCode.PEF:
                    return "2-1";
                case SystemCode.PEFKOR:
                    return "2-1";
                default:
                    return code.ToString();
            }
        }
    }
}
