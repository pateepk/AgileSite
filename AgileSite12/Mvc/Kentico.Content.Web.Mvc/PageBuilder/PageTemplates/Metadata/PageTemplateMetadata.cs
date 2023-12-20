using Newtonsoft.Json;

namespace Kentico.PageBuilder.Web.Mvc.PageTemplates
{
    /// <summary>
    /// Metadata of a page template definition.
    /// </summary>
    public sealed class PageTemplateMetadata
    {
        /// <summary>
        /// Type page template identifier.
        /// </summary>
        [JsonProperty("typeIdentifier")]
        public string TypeIdentifier { get; internal set; }


        /// <summary>
        /// URL of the page template definition to retrieve the default properties.
        /// </summary>
        [JsonProperty("defaultPropertiesUrl")]
        public string DefaultPropertiesUrl { get; internal set; }


        /// <summary>
        /// URL of the page template definition to retrieve the properties form markup.
        /// </summary>
        [JsonProperty("propertiesFormMarkupUrl")]
        public string PropertiesFormMarkupUrl { get; internal set; }


        /// <summary>
        /// Name of the registered page template.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; internal set; }


        /// <summary>
        /// Description of the registered page template.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; internal set; }


        /// <summary>
        /// Icon CSS class of the registered page template.
        /// </summary> 
        [JsonProperty("iconClass")]
        public string IconClass { get; internal set; }
        

        /// <summary>
        /// Indicates whether the page template has at least one property which is decorated with <see cref="Kentico.Forms.Web.Mvc.EditingComponentAttribute"/>
        /// </summary> 
        [JsonProperty("hasProperties")]
        public bool HasProperties { get; internal set; }
    }
}
