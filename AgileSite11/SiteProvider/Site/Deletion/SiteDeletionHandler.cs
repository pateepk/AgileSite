using System;

using CMS.Base;

namespace CMS.SiteProvider
{
    /// <summary>
    /// Site deletion handler
    /// </summary>
    public class SiteDeletionHandler : AdvancedHandler<SiteDeletionHandler, SiteDeletionEventArgs>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="deletionSettings">Deletion settings</param>
        /// <param name="progresslog">Progress log</param>
        public SiteDeletionHandler StartEvent(SiteDeletionSettings deletionSettings, IProgress<SiteDeletionStatusMessage> progresslog)
        {
            var e = new SiteDeletionEventArgs
            {
                Settings = deletionSettings,
                ProgressLog = progresslog
            };

            return StartEvent(e);
        }
    }
}