using System.Xml.Serialization;
using System.Xml;
using System.Text;
using System.IO;

namespace SquintScript
{
    public class Serializer
    {
        public sealed class StringWriterWithEncoding : StringWriter
        {
            public override Encoding Encoding { get; }

            public StringWriterWithEncoding(Encoding encoding)
            {
                Encoding = encoding;
            }
        }

        public T Deserialize<T>(string input) where T : class
        {
            System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(typeof(T));

            using (StringReader sr = new StringReader(input))
            {
                return (T)ser.Deserialize(sr);
            }
        }

        public string Serialize<T>(T ObjectToSerialize, string path)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(ObjectToSerialize.GetType());
            XmlWriterSettings settings = new XmlWriterSettings()
            {
                Encoding = new UnicodeEncoding(false, false), // no BOM in a .NET string
                Indent = false,
                OmitXmlDeclaration = false
            };

            using (StringWriterWithEncoding textWriter = new StringWriterWithEncoding(Encoding.UTF8))
            {

                xmlSerializer.Serialize(textWriter, ObjectToSerialize);
                //SaveFileDialog sf = new SaveFileDialog();
                //sf.FileName = path;
                //sf.ShowDialog();
                //File.WriteAllText(sf.FileName, textWriter.ToString());
                File.WriteAllText(@path, textWriter.ToString());
                return textWriter.ToString();
            }
        }
    }
}
