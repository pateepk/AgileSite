using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Represents the section configuration within an <see cref="EditableAreaConfiguration"/> instance.
    /// </summary>
    public sealed class SectionConfiguration
    {
        /// <summary>
        /// Identifier of the section used on the client.
        /// </summary>
        [DataMember]
        [JsonProperty("identifier")]
        public Guid Identifier { get; set; }


        /// <summary>
        /// Identifier of the section definition.
        /// </summary>
        [DataMember]
        [JsonProperty("type")]
        public string TypeIdentifier { get; set; } = SectionMetadata.DEFAULT_SECTION_TYPE_IDENTIFIER;


        /// <summary>
        /// Zones within the section.
        /// </summary>
        [DataMember]
        [JsonProperty("zones")]
        public List<ZoneConfiguration> Zones { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="SectionConfiguration"/> class.
        /// </summary>
        public SectionConfiguration()
        {
            Zones = new List<ZoneConfiguration>();
        }
    }
}
