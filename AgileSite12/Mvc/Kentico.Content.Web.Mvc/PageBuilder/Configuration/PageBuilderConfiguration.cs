using System.Runtime.Serialization;

using Kentico.PageBuilder.Web.Mvc.PageTemplates;

using Newtonsoft.Json;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Represents configuration of Page builder.
    /// </summary>
    [DataContract(Namespace = "", Name = "Configuration")]
    internal sealed class PageBuilderConfiguration
    {
        /// <summary>
        /// Editable areas within the page.
        /// </summary>
        [DataMember]
        [JsonProperty("page")]
        public EditableAreasConfiguration Page { get; set; }


        /// <summary>
        /// Page template configuration for the page.
        /// </summary>
        [DataMember]
        [JsonProperty("pageTemplate")]
        public PageTemplateConfiguration PageTemplate { get; set; }


        /// <summary>
        /// Creates an instance of <see cref="PageBuilderConfiguration"/> class.
        /// </summary>
        public PageBuilderConfiguration()
        {
            Page = new EditableAreasConfiguration();
        }
    }
}
