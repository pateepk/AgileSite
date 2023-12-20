using System;
using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace CMS.DocumentEngine.Internal
{
    /// <summary>
    /// Page template configuration for a MVC page.
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
        /// Properties of the page template.
        /// </summary>
        [DataMember]
        [JsonProperty("properties")]
        public object Properties { get; set; }
    }
}
