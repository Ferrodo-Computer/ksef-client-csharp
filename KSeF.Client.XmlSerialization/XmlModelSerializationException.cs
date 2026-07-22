using System;

namespace KSeF.Client.XmlSerialization
{
    public class XmlModelSerializationException : Exception
    {
        public XmlModelSerializationException(string message)
            : base(message)
        {
        }

        public XmlModelSerializationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
