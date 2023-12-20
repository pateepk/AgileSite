using System;

using CMS.DataEngine;

namespace CMS.OnlineForms
{
    /// <summary>
    /// Class responsible for serialization and deserialization of <see cref="BizFormUploadFile"/> instances.
    /// </summary>
    public class BizFormUploadFileSerializer : IDataTypeTextSerializer
    {
        /// <summary>
        /// Deserializes <paramref name="text"/> into a <see cref="BizFormUploadFile"/> instance.
        /// </summary>
        /// <param name="text">Text to be deserialized.</param>
        public object Deserialize(string text)
        {
            return BizFormUploadFile.ConvertToBizFormUploadFile(text, null, null);
        }

        /// <summary>
        /// Serializes a <see cref="BizFormUploadFile"/> object into string.
        /// </summary>
        /// <param name="value"><see cref="BizFormUploadFile"/> instance to be serialized.</param>
        /// <exception cref="ArgumentException">Thrown when value is not of <see cref="BizFormUploadFile"/> type.</exception>
        public string Serialize(object value)
        {
            if (value == null)
            {
                return null;
            }
            
            if (value.GetType() != typeof(BizFormUploadFile))
            {
                throw new ArgumentException($"Argument '{nameof(value)}' is not of '{nameof(BizFormUploadFile)}' type.");
            }

            return BizFormUploadFile.ConvertToDatabaseValue(value as BizFormUploadFile, null, null) as string;
        }
    }
}
