using System.Collections.Generic;
using System.Runtime.Serialization;

namespace CMS.Newsletters.Issues.Widgets.Configuration
{
    /// <summary>
    /// Represents configuration of widgets and zones within the <see cref="IssueInfo"/> instance.
    /// </summary>
    [DataContract(Namespace = "", Name = "Configuration")]
    internal sealed class ZonesConfiguration
    {
        /// <summary>
        /// List of the zones configurations contained within the <see cref="IssueInfo"/> instance.
        /// </summary>
        [DataMember]
        public List<Zone> Zones
        {
            get;
            private set;
        }


        /// <summary>
        /// Creates an instance of <see cref="ZonesConfiguration"/> class.
        /// </summary>
        public ZonesConfiguration()
        {
            Zones = new List<Zone>();
        }
    }
}
