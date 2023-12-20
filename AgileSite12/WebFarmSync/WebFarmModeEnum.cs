using System;
using System.Linq;
using System.Text;

using CMS.Helpers;

namespace CMS.WebFarmSync
{
    /// <summary>
    /// Represents web farm setup used in application.
    /// </summary>
    public enum WebFarmModeEnum
    {
        /// <summary>
        /// Web farm synchronization is disabled.
        /// </summary>
        [EnumStringRepresentation("Disabled")]
        [EnumOrder(1)]
        [EnumDefaultValue]
        Disabled = 0,

        /// <summary>
        /// Web farm servers are managed automatically by system.
        /// </summary>
        [EnumStringRepresentation("Automatic")]
        [EnumOrder(2)]
        Automatic = 1,

        /// <summary>
        /// Web farm servers are configured manually by system administrator.
        /// </summary>
        [EnumStringRepresentation("Manual")]
        [EnumOrder(3)]
        Manual = 2
    }
}
