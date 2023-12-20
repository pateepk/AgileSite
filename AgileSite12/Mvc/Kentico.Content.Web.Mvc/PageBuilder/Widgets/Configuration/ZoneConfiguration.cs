using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

using Newtonsoft.Json;

namespace Kentico.PageBuilder.Web.Mvc
{
    /// <summary>
    /// Represents the zone within the <see cref="EditableAreasConfiguration"/> configuration class.
    /// </summary>
    [DataContract(Namespace = "", Name = "Zone")]
    public sealed class ZoneConfiguration
    {
        /// <summary>
        /// Identifier of the widget zone.
        /// </summary>
        [DataMember]
        [JsonProperty("identifier")]
        public Guid Identifier { get; set; }


        /// <summary>
        /// List of widgets within the zone.
        /// </summary>
        [DataMember]
        [JsonProperty("widgets")]
        public List<WidgetConfiguration> Widgets { get; private set; }


        /// <summary>
        /// Creates an instance of <see cref="ZoneConfiguration"/> class.
        /// </summary>
        public ZoneConfiguration()
        {
            Widgets = new List<WidgetConfiguration>();
        }
    }
}
