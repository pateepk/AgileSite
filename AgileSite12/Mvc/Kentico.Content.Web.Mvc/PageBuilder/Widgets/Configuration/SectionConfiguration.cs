using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Represents configuration of section within the <see cref="EditableAreaConfiguration"/> instance.
    /// </summary>
    [DataContract(Namespace = "", Name = "Section")]
    public sealed class SectionConfiguration
    {
        /// <summary>
        /// Identifier of the section.
        /// </summary>
        [DataMember]
        [JsonProperty("identifier")]
        public Guid Identifier { get; set; }


        /// <summary>
        /// Type section identifier.
        /// </summary>
        [DataMember]
        [JsonProperty("type")]
        public string TypeIdentifier { get; set; }


        /// <summary>
        /// Section properties.
        /// </summary>
        [DataMember]
        [JsonProperty("properties")]
        public ISectionProperties Properties { get; set; }


        /// <summary>
        /// Zones within the section.
        /// </summary>
        [DataMember]
        [JsonProperty("zones")]
        public List<ZoneConfiguration> Zones { get; private set; }


        /// <summary>
        /// Creates an instance of <see cref="EditableAreasConfiguration"/> class.
        /// </summary>
        public SectionConfiguration()
        {
            Zones = new List<ZoneConfiguration>();
        }
    }
}
