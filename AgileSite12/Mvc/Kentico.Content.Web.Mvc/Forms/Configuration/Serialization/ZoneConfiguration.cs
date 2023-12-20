using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace Kentico.Forms.Web.Mvc
{
    /// <summary>
    /// Represents the zone configuration within a <see cref="SectionConfiguration"/> instance.
    /// </summary>
    [DataContract(Namespace = "", Name = "Zone")]
    public sealed class ZoneConfiguration
    {
        /// <summary>
        /// Identifier of the zone.
        /// </summary>
        /// <remarks>
        /// Form builder contains only one default zone, this information is required by client only, therefore, <see cref="Guid.NewGuid"/> is always returned.
        /// </remarks>
        [DataMember]
        [JsonProperty("identifier")]
        public Guid Identifier { get; internal set; } = Guid.NewGuid();


        /// <summary>
        /// Form components within the zone.
        /// </summary>
        [DataMember]
        [JsonProperty("formComponents")]
        public List<FormComponentConfiguration> FormComponents { get; private set; }


        /// <summary>
        /// Initializes a new instance of the <see cref="ZoneConfiguration"/> class.
        /// </summary>
        public ZoneConfiguration()
        {
            FormComponents = new List<FormComponentConfiguration>();
        }
    }
}
