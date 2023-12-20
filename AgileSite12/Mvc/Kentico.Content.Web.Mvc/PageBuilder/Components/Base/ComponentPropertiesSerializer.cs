using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Class responsible for component properties serialization.
    /// </summary>
    internal sealed class ComponentPropertiesSerializer : IComponentPropertiesSerializer
    {
        /// <summary>
        /// Deserializes the JSON string to component properties object representation.
        /// </summary>
        /// <param name="json">Component properties JSON string.</param>
        /// <param name="propertiesType">Type of the registered component properties model.</param>
        /// <returns>Deserialized properties object. If <paramref name="json"/> is empty, returns an object of type <see cref="IComponentProperties"/> with default properties values.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="propertiesType"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException"><paramref name="json" /> is in incorrect format.</exception>
        public IComponentProperties Deserialize(string json, Type propertiesType)
        {
            if (propertiesType == null)
            {
                throw new ArgumentNullException(nameof(propertiesType));
            }

            try
            {
                if (string.IsNullOrEmpty(json))
                {
                    return new ComponentDefaultPropertiesProvider().Get<IComponentProperties>(propertiesType);
                }

                var jProperties = JObject.Parse(json);
                return DeserializeInternal<IComponentProperties>(jProperties, propertiesType);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Incorrect format of the deserialized string.", ex);
            }
        }


        internal TPropertiesInterface DeserializeInternal<TPropertiesInterface>(JToken jProperties, Type propertiesType)
            where TPropertiesInterface : IComponentProperties
        {
            return (TPropertiesInterface)jProperties.ToObject(propertiesType);
        }


        /// <summary>
        /// Deserializes JSON string to properties defined by the <typeparamref name="TPropertiesType" /> type.
        /// </summary>
        /// <typeparam name="TPropertiesType">Type of the component properties.</typeparam>
        /// <param name="json">JSON string.</param>
        /// <returns>Deserialized properties object. If <paramref name="json"/> is empty, returns an object of type <typeparamref name="TPropertiesType"/> with default properties values.</returns>
        /// <exception cref="InvalidOperationException"><paramref name="json"/> is in incorrect format.</exception>
        public TPropertiesType Deserialize<TPropertiesType>(string json)
            where TPropertiesType : class, IComponentProperties, new()
        {
            try
            {
                if (string.IsNullOrEmpty(json))
                {
                    return new ComponentDefaultPropertiesProvider().Get<TPropertiesType>();
                }

                return JsonConvert.DeserializeObject<TPropertiesType>(json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Incorrect format of deserialized string.", ex);
            }
        }
    }
}
