using Newtonsoft.Json;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Metadata describing a form component for the client.
    /// </summary>
    public sealed class FormComponentMetadata
    {
        /// <summary>
        /// Form component identifier.
        /// </summary>
        [JsonProperty("identifier")]
        public string Identifier { get; internal set; }


        /// <summary>
        /// URL of an endpoint for retrieving markup of a rendered form component.
        /// </summary>
        [JsonProperty("markupUrl")]
        public string MarkupUrl { get; internal set; }


        /// <summary>
        /// URL of an endpoint for retrieving default properties of a form component.
        /// </summary>
        [JsonProperty("defaultPropertiesUrl")]
        public string DefaultPropertiesUrl { get; internal set; }


        /// <summary>
        /// Form component name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; internal set; }


        /// <summary>
        /// Form component description.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; internal set; }


        /// <summary>
        /// Form component icon CSS class.
        /// </summary>
        [JsonProperty("iconClass")]
        public string IconClass { get; internal set; }


        /// <summary>
        /// Type of the resulting component value, used to find appropriate validation rules.
        /// </summary>
        /// <remarks>
        /// Full name of type normalized to lower case is expected.
        /// </remarks>
        [JsonProperty("valueType")]
        public string ValueType { get; internal set; }
    }
}
