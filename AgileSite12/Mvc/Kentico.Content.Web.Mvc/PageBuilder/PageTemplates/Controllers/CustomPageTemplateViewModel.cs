
using Newtonsoft.Json;

namespace Kentico.PageBuilder.Web.Mvc.PageTemplates.Internal
{
    /// <summary>
    /// Custom page templates view model.
    /// </summary>
    public class CustomPageTemplateViewModel
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
        /// Path to the thumbnail image.
        /// </summary>
        [JsonProperty("imagePath")]
        public string ImagePath { get; set; }
    }
}
