using CMS.Helpers;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Widget zone type enumeration.
    /// </summary>
    public enum WidgetZoneTypeEnum
    {
        /// <summary>
        /// All zone types (special value to get the zone XML).
        /// </summary>
        All = -1,

        /// <summary>
        /// Standard zone.
        /// </summary>
        [EnumStringRepresentation("none")]
        None = 0,

        /// <summary>
        /// User personalization.
        /// </summary>
        [EnumStringRepresentation("user")]
        User = 1,

        /// <summary>
        /// Editor personalization.
        /// </summary>
        [EnumStringRepresentation("editor")]
        Editor = 2,

        /// <summary>
        /// Group personalization.
        /// </summary>
        [EnumStringRepresentation("group")]
        Group = 3,

        /// <summary>
        /// Dashboard.
        /// </summary>
        [EnumStringRepresentation("dashboard")]
        Dashboard = 4
    }
}