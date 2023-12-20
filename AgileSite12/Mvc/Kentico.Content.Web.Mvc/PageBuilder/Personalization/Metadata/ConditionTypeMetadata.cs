using Newtonsoft.Json;

namespace Kentico.PageBuilder.Web.Mvc.Personalization
{
    /// <summary>
    /// Metadata of a personalization condition type definition.
    /// </summary>
    public sealed class ConditionTypeMetadata
    {
        /// <summary>
        /// Identifier of condition type.
        /// </summary>
        [JsonProperty("typeIdentifier")]
        public string TypeIdentifier { get; internal set; }


        /// <summary>
        /// URL of the condition type to retrieve the configuration form markup.
        /// </summary>
        [JsonProperty("markupUrl")]
        public string MarkupUrl { get; internal set; }


        /// <summary>
        /// Name of the registered condition.
        /// </summary> 
        [JsonProperty("name")]
        public string Name { get; internal set; }


        /// <summary>
        /// Description of the registered condition.
        /// </summary> 
        [JsonProperty("description")]
        public string Description { get; internal set; }


        /// <summary>
        /// Icon CSS class of the registered condition.
        /// </summary> 
        [JsonProperty("iconClass")]
        public string IconClass { get; internal set; }


        /// <summary>
        /// Hint displayed above configuration form.
        /// </summary>
        [JsonProperty("hint")]
        public string Hint { get; internal set; }
    }
}
