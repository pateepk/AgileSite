using CMS.Base;

namespace CMS.SiteProvider
{
    /// <summary>
    /// Site deletion status message object used for reporting progress of site deletion for <see cref="SiteDeletionLog"/>.
    /// </summary>
    public class SiteDeletionStatusMessage
    {
        /// <summary>
        /// Status
        /// </summary>
        public LogStatusEnum Status
        {
            get;
            set;
        }

        /// <summary>
        /// Message
        /// </summary>
        public string Message
        {
            get;
            set;
        }
    }
}
