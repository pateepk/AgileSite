using Newtonsoft.Json;

namespace Kentico.PageBuilder.Web.Mvc.PageTemplates.Internal
{
    /// <summary>
    /// Default page template view model.
    /// </summary>
    public class DefaultPageTemplateViewModel
    {
        /// <summary>
        /// Page template name.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }


        /// <summary>
        /// Page template identifier.
        /// </summary>
        [JsonProperty("identifier")]
        public string Identifier { get; set; }


        /// <summary>
        /// Page template description.
        /// </summary>
        [JsonProperty("description")]
        public string Description { get; set; }


        /// <summary>
        /// Icon CSS class of the registered page template.
        /// </summary>
        [JsonProperty("iconClass")]
        public string IconClass { get; set; }
    }
}
