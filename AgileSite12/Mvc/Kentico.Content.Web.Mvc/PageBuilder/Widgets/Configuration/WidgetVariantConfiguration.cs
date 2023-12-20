using System.Runtime.Serialization;
using System;

using Kentico.PageBuilder.Web.Mvc.Personalization;

using Newtonsoft.Json;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Represents the configuration variant of a widget within the <see cref="WidgetConfiguration.Variants"/> list.
    /// </summary>
    [DataContract(Namespace = "", Name = "Variant")]
    public sealed class WidgetVariantConfiguration
    {
        /// <summary>
        /// Identifier of the variant instance.
        /// </summary>
        [DataMember]
        [JsonProperty("identifier")]
        public Guid Identifier { get; set; }


        /// <summary>
        /// Widget variant name.
        /// </summary>
        [DataMember]
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }


        /// <summary>
        /// Widget variant properties.
        /// </summary>
        [DataMember]
        [JsonProperty("properties")]
        public IWidgetProperties Properties { get; set; }


        /// <summary>
        /// Widget variant personalization condition type.
        /// </summary>
        /// <remarks>Only personalization condition type parameters are serialized to JSON.</remarks>
        [DataMember]
        [JsonProperty("conditionTypeParameters", NullValueHandling = NullValueHandling.Ignore)]
        public IConditionType PersonalizationConditionType { get; set; }
    }
}
