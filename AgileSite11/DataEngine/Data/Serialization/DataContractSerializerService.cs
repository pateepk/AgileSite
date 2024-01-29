using System;
using System.Runtime.Serialization;
using System.Xml;

using CMS.IO;

namespace CMS.DataEngine
{
    /// <summary>
    /// Service for serializing / deserializing objects from / to XML using <see cref="DataContractSerializer" />.
    /// </summary>
    /// <typeparam name="T">Type of object to serialize / deserialize.</typeparam>
    internal sealed class DataContractSerializerService<T> : IDataContractSerializerService<T> where T : class
    {
        /// <summary>
        /// Serializes object instance to XML string representation.
        /// </summary>
        /// <param name="obj">Input object for serialization.</param>
        /// <returns>Serialized object <paramref name="obj" /> to XML string representation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="obj"/> is <c>null</c>.</exception>
        public string Serialize(T obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj), "Input object to serialize is null.");
            }

            using (var stringWriter = new StringWriter())
            using (var xmlWriter = XmlWriter.Create(stringWriter))
            {
                var serializer = new DataContractSerializer(typeof(T));
                serializer.WriteObject(xmlWriter, obj);

                xmlWriter.Flush();
                return stringWriter.ToString();
            }
        }


        /// <summary>
        /// Deserializes XML string to object instance.
        /// </summary>
        /// <param name="xml">Input XML text for deserialization.</param>
        /// <returns>Deserialized object instance from <paramref name="xml" /> string representation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="xml"/> is <c>null</c>.</exception>
        /// <exception cref="SerializationException">Thrown when deserialization from input <paramref name="xml"/> failed.</exception>
        public T Deserialize(string xml)
        {
            if (string.IsNullOrEmpty(xml))
            {
                throw new ArgumentNullException(nameof(xml), "Input XML is null or empty.");
            }

            using (var stringReader = StringReader.New(xml))
            using (var xmlReader = XmlReader.Create(stringReader))
            {
                var deserializer = new DataContractSerializer(typeof(T));
                return (T)deserializer.ReadObject(xmlReader);
            }
        }
    }
}