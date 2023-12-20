using System;
using System.Runtime.Serialization;

using CMS.DocumentEngine;

using Newtonsoft.Json;

namespace Kentico.PageBuilder.Web.Mvc.PageTemplates
{
    /// <summary>
    /// Page template configuration for the <see cref="TreeNode"/> instance.
    /// </summary>
    [DataContract(Namespace = "", Name = "PageTemplate")]
    public class PageTemplateConfiguration
    {
        /// <summary>
        /// Identifier of the page template.
        /// </summary>
        [DataMember]
        [JsonProperty("identifier")]
        public string Identifier { get; set; }


        /// <summary>
        /// Identifier of the page template configuration based on which the page was created.
        /// </summary>
        [DataMember]
        [JsonProperty("configurationIdentifier", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public Guid ConfigurationIdentifier { get; set; }


        /// <summary>
        /// Page template properties.
        /// </summary>
        [DataMember]
        [JsonProperty("properties")]
        public IPageTemplateProperties Properties { get; set; }
    }
}
