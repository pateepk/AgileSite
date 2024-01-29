using System;
using System.Linq;
using System.Text;

using CMS.Helpers;

namespace CMS.WebFarmSync
{
    /// <summary>
    /// Web farm server status enum.
    /// </summary>
    public enum WebFarmServerStatusEnum
    {
        /// <summary>
        /// Healthy means that the server is responding in a timely manner.
        /// </summary>
        [EnumDefaultValue]
        [EnumStringRepresentation("Healthy")]
        Healthy = 0,

        /// <summary>
        /// Transitioning means that the server status cannot be properly determined.
        /// This can happen when web farm server starts or his status changes
        /// from healthy to not responding and vice versa.
        /// </summary>
        [EnumStringRepresentation("Transitioning")]
        Transitioning = 1,

        /// <summary>
        /// Not responding means that the server is not responding at all.
        /// </summary>
        [EnumStringRepresentation("NotResponding")]
        NotResponding = 2,

        /// <summary>
        /// Server was automatically disabled (memory tasks removed and stopped generating new ones).
        /// </summary>
        [EnumStringRepresentation("AutoDisabled")]
        AutoDisabled = 3,
    }
}
