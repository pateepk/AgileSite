using System;
using System.Linq;

using Kentico.Builder.Web.Mvc;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kentico.PageBuilder.Web.Mvc.PageTemplates
{
    /// <summary>
    /// Handles serialization and deserialization of page template configuration to/from JSON format.
    /// </summary>
    internal sealed class PageTemplateConfigurationSerializer : IPageTemplateConfigurationSerializer
    {
        /// <summary>
        /// Serializes editable areas configuration to JSON string.
        /// </summary>
        /// <param name="configuration">Editable areas configuration.</param>
        public string Serialize(PageTemplateConfiguration configuration)
        {
            return JsonConvert.SerializeObject(configuration, Formatting.None, GetSettings());
        }


        /// <summary>
        /// Deserializes JSON string to page template configuration.
        /// </summary>
        /// <param name="json">JSON string.</param>
        /// <param name="pageTemplateDefinitionProvider">Provider to retrieve page template definitions.</param>
        /// <exception cref="InvalidOperationException"><paramref name="json"/> is in incorrect format.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="pageTemplateDefinitionProvider"/> is <c>null</c>.</exception>
        public PageTemplateConfiguration Deserialize(string json, IComponentDefinitionProvider<PageTemplateDefinition> pageTemplateDefinitionProvider)
        {
            if (pageTemplateDefinitionProvider == null)
            {
                throw new ArgumentNullException(nameof(pageTemplateDefinitionProvider));
            }

            try
            {
                var jConfiguration = JObject.Parse(json);
                var config = (dynamic)jConfiguration;

                var pageTemplate = new PageTemplateConfiguration
                {
                    Identifier = config.identifier
                };

                if (config.configurationIdentifier != null)
                {
                    pageTemplate.ConfigurationIdentifier = config.configurationIdentifier;
                }

                var pageTemplateDefinition = GetPageTemplateDefinition(pageTemplateDefinitionProvider, pageTemplate);

                pageTemplate.Properties = GetProperties(jConfiguration as JObject, pageTemplateDefinition);

                return pageTemplate;
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Incorrect format of deserialized string.", e);
            }
        }


        private static PageTemplateDefinition GetPageTemplateDefinition(IComponentDefinitionProvider<PageTemplateDefinition> pageTemplateDefinitionProvider, PageTemplateConfiguration pageTemplate)
        {
            return pageTemplateDefinitionProvider
                .GetAll()
                .FirstOrDefault(def => def.Identifier.Equals(pageTemplate.Identifier, StringComparison.InvariantCultureIgnoreCase));
        }


        private IPageTemplateProperties GetProperties(JObject jObject, IPropertiesComponentDefinition componentDefinition)
        {
            var propertiesType = componentDefinition?.PropertiesType;
            if (propertiesType == null)
            {
                return null;
            }

            if (!jObject.TryGetValue("properties", StringComparison.InvariantCultureIgnoreCase, out var jProperties) || jProperties.Type == JTokenType.Null)
            {
                return new ComponentDefaultPropertiesProvider().Get<IPageTemplateProperties>(propertiesType);
            }

            return new ComponentPropertiesSerializer().DeserializeInternal<IPageTemplateProperties>(jProperties, componentDefinition.PropertiesType);
        }


        private static JsonSerializerSettings GetSettings()
        {
            return new JsonSerializerSettings
            {
                ContractResolver = SerializerHelper.GetDefaultContractResolver()
            };
        }
    }
}