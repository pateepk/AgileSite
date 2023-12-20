using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CMS.DocumentEngine.Internal
{
    /// <summary>
    /// Handles serialization and deserialization of page template configuration to/from JSON format.
    /// </summary>
    public class PageTemplateConfigurationSerializer
    {
        /// <summary>
        /// Serializes page template configuration to JSON string.
        /// </summary>
        /// <param name="configuration">Page template configuration.</param>        
        public string Serialize(PageTemplateConfiguration configuration)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            return JsonConvert.SerializeObject(configuration, Formatting.None, settings);
        }


        /// <summary>
        /// Deserializes JSON string to page template configuration.
        /// </summary>
        /// <param name="json">JSON string.</param>
        public PageTemplateConfiguration Deserialize(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return new PageTemplateConfiguration();
            }

            return JsonConvert.DeserializeObject<PageTemplateConfiguration>(json);
        }
    }
}
