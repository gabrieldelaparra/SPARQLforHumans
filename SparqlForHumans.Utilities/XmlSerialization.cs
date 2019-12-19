using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace SparqlForHumans.Utilities
{
    public static class XmlSerialization
    {
        public static void SerializeXml(this object objectToSerialize, string outputXmlFilename)
        {
            if (!string.IsNullOrWhiteSpace(Path.GetDirectoryName(outputXmlFilename)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(outputXmlFilename));
            }
            var xmlWriterSettings = new XmlWriterSettings
            {
                Indent = true,
                OmitXmlDeclaration = false,
                Encoding = Encoding.UTF8
            };

            using (var stream = new FileStream(outputXmlFilename, FileMode.Create, FileAccess.Write))
            {
                using (var xmlWriter = XmlWriter.Create(stream, xmlWriterSettings))
                {
                    var serializer = new XmlSerializer(objectToSerialize.GetType());
                    serializer.Serialize(xmlWriter, objectToSerialize);
                }
            }
        }

        public static T DeserializeXml<T>(string inputXmlFilename)
        {
            using (var stream = new FileStream(inputXmlFilename, FileMode.Open, FileAccess.Read))
            {
                if (stream.Length > 0)
                {
                    var s = new XmlSerializer(typeof(T));
                    return (T)s.Deserialize(stream);
                }
            }
            return default;
        }
    }
}
