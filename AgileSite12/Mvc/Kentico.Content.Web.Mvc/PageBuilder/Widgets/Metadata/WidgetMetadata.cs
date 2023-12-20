using Newtonsoft.Json;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Metadata of a widget definition.
    /// </summary>
    public sealed class WidgetMetadata
    {
        /// <summary>
        /// Type widget identifier.
        /// </summary>
        [JsonProperty("typeIdentifier")]
        public string TypeIdentifier { get; internal set; }


        /// <summary>
        /// URL of the widget definition to retrieve the markup.
        /// </summary>
        [JsonProperty("markupUrl")]
        public string MarkupUrl { get; internal set; }


        /// <summary>
        /// URL of the widget definition to retrieve the default properties.
        /// </summary>
        [JsonProperty("defaultPropertiesUrl")]
        public string DefaultPropertiesUrl { get; internal set; }


        /// <summary>
        /// URL of the widget definition to retrieve the properties form markup.
        /// </summary>
        [JsonProperty("propertiesFormMarkupUrl")]
        public string PropertiesFormMarkupUrl { get; internal set; }


        /// <summary>
        /// Name of the registered widget.
        /// </summary> 
        [JsonProperty("name")]
        public string Name { get; internal set; }


        /// <summary>
        /// Description of the registered widget.
        /// </summary> 
        [JsonProperty("description")]
        public string Description { get; internal set; }


        /// <summary>
        /// Icon CSS class of the registered widget.
        /// </summary> 
        [JsonProperty("iconClass")]
        public string IconClass { get; internal set; }


        /// <summary>
        /// Indicates that registered widget has properties.
        /// </summary> 
        [JsonProperty("hasProperties")]
        public bool HasProperties { get; internal set; }


        /// <summary>
        /// Indicates whether the widget has at least one property which is decorated with <see cref="Kentico.Forms.Web.Mvc.EditingComponentAttribute"/>
        /// </summary>
        [JsonProperty("hasEditableProperties")]
        public bool HasEditableProperties { get; set; }
    }
}
