using CMS.Helpers;

namespace CMS.Globalization
{
    /// <summary>
    /// Defines timezone types. 
    /// </summary>
    public enum TimeZoneTypeEnum : int
    {
        /// <summary>
        /// Indicates whether time zone type is inherited.
        /// </summary>
        [EnumStringRepresentation("inherit")]
        [EnumDefaultValue()]
        Inherit = 0,

        /// <summary>
        /// Server timezone type.
        /// </summary>
        [EnumStringRepresentation("server")]
        Server = 1,

        /// <summary>
        ///  WebSite timezone type.
        /// </summary>
        [EnumStringRepresentation("website")]
        WebSite = 2,

        /// <summary>
        /// User timezone type.
        /// </summary>
        [EnumStringRepresentation("user")]
        User = 3,

        /// <summary>
        /// Custom timezone type.
        /// </summary>
        [EnumStringRepresentation("custom")]
        Custom = 4
    }
}
