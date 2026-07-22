using System.IO;

namespace KSeF.Client.XmlSerialization
{
    public interface IXmlModelSerializer
    {
        string Serialize<T>(T value);

        void Serialize<T>(Stream stream, T value);

        T Deserialize<T>(string xml);

        T Deserialize<T>(Stream stream);
    }
}
