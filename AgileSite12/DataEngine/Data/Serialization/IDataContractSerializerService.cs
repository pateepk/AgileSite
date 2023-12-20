using System;
using System.Runtime.Serialization;

namespace CMS.DataEngine
{
    /// <summary>
    /// Interface for service serializing / deserializing objects from / to XML using <see cref="DataContractSerializer" />.
    /// </summary>
    /// <typeparam name="T">Type of object to serialize / deserialize.</typeparam>
    public interface IDataContractSerializerService<T>
    {
        /// <summary>
        /// Serializes object instance to XML string representation.
        /// </summary>
        /// <param name="obj">Input object for serialization.</param>
        /// <returns>Serialized object <paramref name="obj" /> to XML string representation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="obj"/> is <c>null</c>.</exception>
        string Serialize(T obj);


        /// <summary>
        /// Deserializes XML string to object instance.
        /// </summary>
        /// <param name="xml">Input XML text for deserialization.</param>
        /// <returns>Deserialized object instance from <paramref name="xml" /> string representation.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="xml"/> is <c>null</c>.</exception>
        /// <exception cref="SerializationException">Thrown when deserialization from input <paramref name="xml"/> failed.</exception>
        T Deserialize(string xml);
    }
}