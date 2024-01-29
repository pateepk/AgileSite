using System;

using CMS.Base;

namespace CMS.SiteProvider
{
    /// <summary>
    /// Site deletion event arguments
    /// </summary>
    public class SiteDeletionEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Deletion settings
        /// </summary>
        public SiteDeletionSettings Settings
        {
            get;
            set;
        }


        /// <summary>
        /// Progress log
        /// </summary>
        public IProgress<SiteDeletionStatusMessage> ProgressLog
        {
            get;
            set;
        }
    }
}