using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CMS.Newsletters.Issues.Widgets.Configuration
{
    /// <summary>
    /// Represents the zone within the <see cref="ZonesConfiguration"/> configuration class.
    /// </summary>
    [DataContract(Namespace = "")]
    public sealed class Zone
    {
        /// <summary>
        /// Identifier of the zone.
        /// </summary>
        [DataMember]
        public string Identifier
        {
            get;
            private set;
        }


        /// <summary>
        /// List of widgets within the zone.
        /// </summary>
        [DataMember]
        public List<Widget> Widgets
        {
            get;
            private set;
        }


        /// <summary>
        /// Creates an instance of <see cref="Zone"/> class.
        /// </summary>
        /// <param name="identifier">Zone identifier.</param>
        public Zone(string identifier)
        {
            Identifier = identifier;
            Widgets = new List<Widget>();
        }
    }
}
