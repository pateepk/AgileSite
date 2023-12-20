using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Converter that converts a JSON to a string.
    /// </summary>
    /// <remarks>
    /// This converter allows to deserialize a JSON string containing a complex object to a string. 
    /// Without this converter Newtonsoft deserializer will throw an exception when a JSON string containing a complex object is deserialized to string.
    /// </remarks>
    /// <seealso cref="JsonConverter" />
    internal sealed class JsonConverterObjectToString : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(JTokenType));
        }


        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken token = JToken.Load(reader);

            return token.ToString();
        }


        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="T:Newtonsoft.Json.JsonWriter" /> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <exception cref="NotImplementedException"></exception>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
