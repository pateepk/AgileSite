using Newtonsoft.Json;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Metadata of a section definition.
    /// </summary>
    public sealed class SectionMetadata
    {
        /// <summary>
        /// Type identifier for the default Form builder's section.
        /// </summary>
        public const string DEFAULT_SECTION_TYPE_IDENTIFIER = "Kentico.DefaultSection";


        /// <summary>
        /// Type section identifier.
        /// </summary>
        [JsonProperty("typeIdentifier")]
        public string TypeIdentifier { get; internal set; } = DEFAULT_SECTION_TYPE_IDENTIFIER;


        /// <summary>
        /// URL of the section definition to retrieve the markup.
        /// </summary>
        [JsonProperty("markupUrl")]
        public string MarkupUrl { get; internal set; }


        /// <summary>
        /// Name of the registered section.
        /// </summary> 
        [JsonProperty("name")]
        public string Name { get; internal set; }


        /// <summary>
        /// Description of the registered section.
        /// </summary> 
        [JsonProperty("description")]
        public string Description { get; internal set; }


        /// <summary>
        /// Icon CSS class of the registered section.
        /// </summary> 
        [JsonProperty("iconClass")]
        public string IconClass { get; internal set; }
    }
}
