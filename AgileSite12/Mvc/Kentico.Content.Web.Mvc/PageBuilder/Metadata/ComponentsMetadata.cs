using System.Collections.Generic;

using Kentico.PageBuilder.Web.Mvc.PageTemplates;
using Kentico.PageBuilder.Web.Mvc.Personalization;

using Newtonsoft.Json;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Metadata of component definitions used within Page builder feature.
    /// </summary>
    public sealed class ComponentsMetadata
    {
        /// <summary>
        /// List of widgets metadata.
        /// </summary>
        [JsonProperty("widgets")]
        public IList<WidgetMetadata> Widgets { get; internal set; }


        /// <summary>
        /// List of sections metadata.
        /// </summary>
        [JsonProperty("sections")]
        public IList<SectionMetadata> Sections { get; internal set; }


        /// <summary>
        /// List of page templates metadata.
        /// </summary>
        [JsonProperty("pageTemplates")]
        public IList<PageTemplateMetadata> PageTemplates { get; internal set; }


        /// <summary>
        /// List of personalization condition types metadata.
        /// </summary>
        [JsonProperty("personalizationConditionTypes")]
        public IList<ConditionTypeMetadata> PersonalizationConditionTypes { get; internal set; }


        /// <summary>
        /// Creates an instance of <see cref="ComponentsMetadata"/> class.
        /// </summary>
        public ComponentsMetadata()
        {
            Widgets = new List<WidgetMetadata>();
            Sections = new List<SectionMetadata>();
            PageTemplates = new List<PageTemplateMetadata>();
            PersonalizationConditionTypes = new List<ConditionTypeMetadata>();
        }
    }
}
