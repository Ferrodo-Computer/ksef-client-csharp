using System.Globalization;
using System.Text;

namespace KSeF.Client.XmlSerialization
{
    public sealed class XmlSerializationOptions
    {
        public string RootElementName { get; set; }

        public string Namespace { get; set; }

        public bool Indent { get; set; } = true;

        public Encoding Encoding { get; set; } = Encoding.UTF8;

        public bool EmitNullValues { get; set; }

        public string CollectionItemElementName { get; set; } = "Item";

        public string DictionaryItemElementName { get; set; } = "Item";

        public string DictionaryKeyElementName { get; set; } = "Key";

        public string DictionaryValueElementName { get; set; } = "Value";

        public CultureInfo FormatCulture { get; set; } = CultureInfo.InvariantCulture;
    }
}
