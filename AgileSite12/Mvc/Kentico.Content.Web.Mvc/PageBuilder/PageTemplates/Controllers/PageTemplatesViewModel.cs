using System.Collections.Generic;

using Newtonsoft.Json;

namespace Kentico.PageBuilder.Web.Mvc.PageTemplates.Internal
{
    /// <summary>
    /// Encapsulates default and custom page templates view models.
    /// </summary>
    public class PageTemplatesViewModel
    {
        /// <summary>
        /// Default page templates.
        /// </summary>
        [JsonProperty("defaultPageTemplates")]
        public IEnumerable<DefaultPageTemplateViewModel> DefaultPageTemplates { get; set; }


        /// <summary>
        /// Custom page templates.
        /// </summary>
        [JsonProperty("customPageTemplates")]
        public IEnumerable<CustomPageTemplateViewModel> CustomPageTemplates { get; set; }
    }
}
