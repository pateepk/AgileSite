using System;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kentico.PageBuilder.Web.Mvc.Personalization
{
    /// <summary>
    /// Class responsible for condition type parameters serialization.
    /// </summary>
    internal class ConditionTypeParametersSerializer : IConditionTypeParametersSerializer
    {
        /// <summary>
        /// Deserializes the JSON string to condition type parameters object representation.
        /// </summary>
        /// <param name="json">Condition type parameters JSON string.</param>
        /// <param name="conditionType">Type of the registered condition type.</param>
        /// <exception cref="ArgumentException"><paramref name="json"/> is <c>null</c> or empty.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="conditionType"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException"><paramref name="json" /> is in incorrect format.</exception>
        public IConditionType Deserialize(string json, Type conditionType)
        {
            if (String.IsNullOrEmpty(json))
            {
                throw new ArgumentException("Parameter cannot be null or empty", nameof(json));
            }

            if (conditionType == null)
            {
                throw new ArgumentNullException(nameof(conditionType));
            }

            try
            {
                var jProperties = JObject.Parse(json);
                return jProperties.ToObject(conditionType) as IConditionType;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Incorrect format of the deserialized string.", ex);
            }
        }


        /// <summary>
        /// Deserializes the JSON string to condition type parameters object representation.
        /// </summary>
        /// <param name="json">Condition type parameters JSON string.</param>
        /// <returns>Deserialized condition type object. If <paramref name="json"/> is empty, returns an object of type <typeparamref name="TConditionType"/> with default properties values.</returns>
        /// <exception cref="InvalidOperationException"><paramref name="json" /> is in incorrect format.</exception>
        public TConditionType Deserialize<TConditionType>(string json)
            where TConditionType : class, IConditionType, new()
        {
            try
            {
                if (string.IsNullOrEmpty(json))
                {
                    return new TConditionType();
                }

                return JsonConvert.DeserializeObject<TConditionType>(json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Incorrect format of deserialized string.", ex);
            }
        }
    }
}
