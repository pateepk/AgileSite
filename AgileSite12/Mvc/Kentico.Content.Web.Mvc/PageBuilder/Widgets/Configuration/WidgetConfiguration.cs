using System.Runtime.Serialization;
using System;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Represents the configuration of a widget within the <see cref="ZoneConfiguration.Widgets"/> list.
    /// </summary>
    [DataContract(Namespace = "", Name = "Widget")]
    public sealed class WidgetConfiguration
    {
        /// <summary>
        /// Identifier of the widget instance.
        /// </summary>
        [DataMember]
        [JsonProperty("identifier")]
        public Guid Identifier { get; set; }


        /// <summary>
        /// Type widget identifier.
        /// </summary>
        [DataMember]
        [JsonProperty("type")]
        public string TypeIdentifier { get; set; }


        /// <summary>
        /// Personalization condition type identifier.
        /// </summary>
        [DataMember]
        [JsonProperty("conditionType", NullValueHandling = NullValueHandling.Ignore)]
        public string PersonalizationConditionTypeIdentifier { get; set; }


        /// <summary>
        /// List of widget variants.
        /// </summary>
        [DataMember]
        [JsonProperty("variants")]
        public List<WidgetVariantConfiguration> Variants { get; set; }

        
        /// <summary>
        /// Creates an instance of <see cref="WidgetConfiguration"/> class.
        /// </summary>
        public WidgetConfiguration()
        {
            Variants = new List<WidgetVariantConfiguration>();
        }
    }
}
