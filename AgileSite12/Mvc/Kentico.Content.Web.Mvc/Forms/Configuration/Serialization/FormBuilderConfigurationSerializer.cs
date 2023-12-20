using System;
using System.Linq;

using Kentico.Builder.Web.Mvc;
using Kentico.Forms.Web.Mvc.Internal;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Serializer of <see cref="FormBuilderConfiguration"/>.
    /// </summary>
    internal class FormBuilderConfigurationSerializer : IFormBuilderConfigurationSerializer
    {
        /// <summary>
        /// Serializes Form builder configuration to JSON.
        /// </summary>
        /// <param name="formBuilderConfiguration">Configuration to be serialized to JSON.</param>
        /// <param name="clearFormComponents">If <c>true</c> serialized 'formComponent' tokens will contain only 'identifier' property</param>
        /// <returns>Returns serialized configuration.</returns>
        public string Serialize(FormBuilderConfiguration formBuilderConfiguration, bool clearFormComponents = false)
        {
            if (formBuilderConfiguration == null)
            {
                throw new ArgumentNullException(nameof(formBuilderConfiguration));
            }

            var jConfig = JObject.FromObject(formBuilderConfiguration, JsonSerializer.Create(GetSettings()));

            if (clearFormComponents)
            {
                // Clear configuration to keep just form layout
                var componentTokens = jConfig.SelectTokens("editableAreas[*].sections[*].zones[*].formComponents[*]");
                foreach (var jToken in componentTokens)
                {
                    // Remove all properties of 'formComponent' token except 'identifier' property
                    jToken.Children<JProperty>().Where(p => !p.Name.Equals("identifier", StringComparison.OrdinalIgnoreCase)).ToList().ForEach(p => p.Remove());
                }
            }

            return JsonConvert.SerializeObject(jConfig, Formatting.Indented, GetSettings());
        }


        /// <summary>
        /// Deserializes Form builder configuration from JSON.
        /// </summary>
        /// <param name="configurationJson">Configuration in JSON to be deserialized.</param>
        /// <returns>Returns deserialized configuration.</returns>
        public FormBuilderConfiguration Deserialize(string configurationJson)
        {
            if (configurationJson == null)
            {
                throw new ArgumentNullException(nameof(configurationJson));
            }

            try
            {
                return JsonConvert.DeserializeObject<FormBuilderConfiguration>(configurationJson, GetSettings());
            }
            catch(Exception ex)
            {
                throw new InvalidOperationException($"Deserialization of Form builder configuration failed. See the inner exception for details. The original JSON string follows.{Environment.NewLine}{Environment.NewLine}{configurationJson}", ex);
            }
        }


        /// <summary>
        /// Deserializes form component configuration from JSON.
        /// </summary>
        /// <param name="formComponentJson">Configuration in JSON to be deserialized.</param>
        /// <returns>Returns deserialized configuration.</returns>
        public FormComponentConfiguration DeserializeFormComponentConfiguration(string formComponentJson)
        {
            if (formComponentJson == null)
            {
                throw new ArgumentNullException(nameof(formComponentJson));
            }

            try
            {
                return JsonConvert.DeserializeObject<FormComponentConfiguration>(formComponentJson, GetSettings());
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Deserialization of form component configuration failed. See the inner exception for details. The original JSON string follows.{Environment.NewLine}{Environment.NewLine}{formComponentJson}", ex);
            }
        }


        /// <summary>
        /// Gets serializer settings used to de/serialize FormBuilder objects.
        /// </summary>
        public static JsonSerializerSettings GetSettings()
        {
            return new JsonSerializerSettings
            {
                ContractResolver = SerializerHelper.GetDefaultContractResolver(),
                TypeNameHandling = TypeNameHandling.Auto,
                SerializationBinder = new FormBuilderTypesBinder(),
                StringEscapeHandling = StringEscapeHandling.EscapeHtml
            };
        }
    }
}
