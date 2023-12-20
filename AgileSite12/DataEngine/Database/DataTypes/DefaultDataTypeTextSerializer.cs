using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Class responsible for serialization and deserialization of <see cref="DataType"/> values to text.
    /// Serializer is used as a fallback when no serializer is set for given data type.
    /// </summary>
    /// <seealso cref="DataTypeManager.ConvertToSystemType(TypeEnum, string, object, System.Globalization.CultureInfo, bool)"/>
    public class DefaultDataTypeTextSerializer : IDataTypeTextSerializer
    {
        private readonly string fieldType;


        /// <summary>
        /// Creates an instance of <see cref="DefaultDataTypeTextSerializer"/>.
        /// </summary>
        /// <param name="fieldType">Data type of the values the serializer will be used for.</param>
        /// <seealso cref="DataType.FieldType"/>
        public DefaultDataTypeTextSerializer(string fieldType)
        {
            if (String.IsNullOrEmpty(fieldType))
            {
                throw new ArgumentException(nameof(fieldType));
            }

            this.fieldType = fieldType;
        }
        

        /// <summary>
        /// Deserialize <paramref name="text"/> into desired object.
        /// </summary>
        /// <param name="text">Text to be deserialized.</param>
        public object Deserialize(string text)
        {
            return DataTypeManager.ConvertToSystemType(TypeEnum.Field, fieldType, text);
        }


        /// <summary>
        /// Serializes <paramref name="value"/> into text.
        /// </summary>
        /// <param name="value">Value to be serialized.</param>
        public string Serialize(object value)
        {
            return DataTypeManager.ConvertToSystemType(TypeEnum.Field, FieldDataType.Text, value, nullIfDefault: value == null) as string;
        }
    }
}
