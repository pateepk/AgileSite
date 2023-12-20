using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Defines contract for serialization and deserialization of <see cref="DataType"/> values to text.
    /// </summary>
    /// <remarks>Used for saving values in BizForm definitions.</remarks>
    public interface IDataTypeTextSerializer
    {
        /// <summary>
        /// Serializes <paramref name="value"/> into text.
        /// </summary>
        /// <param name="value">Value to be serialized.</param>
        /// <exception cref="ArgumentException">Thrown when value is not of expected type.</exception>
        string Serialize(object value);


        /// <summary>
        /// Deserialize <paramref name="text"/> into desired object.
        /// </summary>
        /// <param name="text">Text to be deserialized.</param>
        object Deserialize(string text);
    }
}
