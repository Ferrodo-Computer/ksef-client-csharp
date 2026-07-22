using System.IO;
using System.Text;

namespace KSeF.Client.XmlSerialization
{
    internal sealed class Utf8StringWriter : StringWriter
    {
        private readonly Encoding encoding;

        public Utf8StringWriter(Encoding encoding)
        {
            this.encoding = encoding;
        }

        public override Encoding Encoding => encoding;
    }
}
