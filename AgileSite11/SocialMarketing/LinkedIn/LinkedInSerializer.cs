using System.Text;
using System.Xml.Serialization;

using SystemIO = System.IO;

namespace CMS.SocialMarketing
{
    /// <summary>
    /// Helper methods for LinkedIn xml serialization.
    /// </summary>
    internal static class LinkedInXmlSerializer
    {
        public static T Deserialize<T>(string xml)
        {
            if (string.IsNullOrEmpty(xml))
            {
                return default(T);
            }

            var serializer = new XmlSerializer(typeof(T));
            using (var stream = new SystemIO.MemoryStream(Encoding.UTF8.GetBytes(xml)))
            {
                return (T)serializer.Deserialize(stream);
            }
        }


        public static string Serialize<T>(T instance)
        {
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            var serializer = new XmlSerializer(typeof(T));
            using (var stream = new SystemIO.MemoryStream())
            {
                serializer.Serialize(stream, instance, namespaces);
                return Encoding.UTF8.GetString(stream.GetBuffer(), 0, (int)stream.Length);
            }
        }
    }
}
