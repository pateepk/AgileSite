using System;
using System.Collections.Generic;

using CMS.Core;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Encapsulates <see cref="JsonConverter"/> for visibility condition configuration.
    /// </summary>
    public class VisibilityConditionConfigurationJsonConverter : JsonConverter
    {
        /// <summary>
        /// Represents visibility condition definition provider.
        /// </summary>
        private readonly IVisibilityConditionDefinitionProvider visibilityConditionDefinitionProvider;


        /// <summary>
        /// Initializes the new instance of <see cref="VisibilityConditionConfigurationJsonConverter"/>.
        /// </summary>
        public VisibilityConditionConfigurationJsonConverter() : this(Service.Resolve<IVisibilityConditionDefinitionProvider>())
        {
        }


        /// <summary>
        /// Initializes the new instance of <see cref="VisibilityConditionConfigurationJsonConverter"/> with given <paramref name="visibilityConditionDefinitionProvider"/>.
        /// </summary>
        public VisibilityConditionConfigurationJsonConverter(IVisibilityConditionDefinitionProvider visibilityConditionDefinitionProvider)
        {
            this.visibilityConditionDefinitionProvider = visibilityConditionDefinitionProvider;
        }


        /// <summary>
        /// Writes the JSON representation of the object for visibility condition.
        /// </summary>
        /// <param name="writer">Writer that provided a way for generating JSON data.</param>
        /// <param name="value">Configuration to be serialized.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (!(value is VisibilityConditionConfiguration configuration))
            {
                serializer.Serialize(writer, null);
                return;
            }

            var dictionary = new Dictionary<string, object>()
            {
                { nameof(VisibilityConditionConfiguration.Identifier), configuration.Identifier },
                { nameof(VisibilityConditionConfiguration.VisibilityCondition), configuration.VisibilityCondition },
            };

            serializer.Serialize(writer, dictionary);
        }


        /// <summary>
        /// Reads the JSON representation of the object for visibility condition.
        /// </summary>
        /// <param name="reader">Reader that provides access to serialized JSON data.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of the object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            var jObject = JObject.Load(reader);
            if (jObject == null)
            {
                return null;
            }

            var identifier = jObject.GetValue(nameof(VisibilityConditionConfiguration.Identifier), StringComparison.OrdinalIgnoreCase);
            var visibilityCondition = jObject.GetValue(nameof(VisibilityConditionConfiguration.VisibilityCondition), StringComparison.OrdinalIgnoreCase);

            var definition = visibilityConditionDefinitionProvider.Get(identifier.Value<string>());
            var visibilityConditionInstance = visibilityCondition.ToObject(definition.VisibilityConditionType, serializer) as VisibilityCondition;

            return new VisibilityConditionConfiguration
            {
                Identifier = definition.Identifier,
                VisibilityCondition = visibilityConditionInstance,
            };
        }


        /// <summary>
        /// Returns <c>true</c> if <paramref name="objectType"/> is <see cref="VisibilityConditionConfiguration"/>.
        /// </summary>
        /// <param name="objectType">Type to be checked.</param>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(VisibilityConditionConfiguration);
        }
    }
}
