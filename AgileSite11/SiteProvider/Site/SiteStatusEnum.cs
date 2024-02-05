using CMS.Helpers;

namespace CMS.SiteProvider
{
    /// <summary>
    /// Site status enumeration.
    /// </summary>
    public enum SiteStatusEnum : int
    {
        /// <summary>
        /// Running site
        /// </summary>
        [EnumStringRepresentation("RUNNING")]
        Running = 0,

        /// <summary>
        /// Stopped site
        /// </summary>
        [EnumStringRepresentation("STOPPED")]
        Stopped = 1,
    }
}