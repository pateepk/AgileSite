using System;

namespace CMS.SiteProvider
{
    /// <summary>
    /// Site events
    /// </summary>
    public static class SiteEvents
    {
        #region "Import"

        /// <summary>
        /// Fires when the site with all the document structure and object is being deleted
        /// </summary>
        public static SiteDeletionHandler Delete = new SiteDeletionHandler { Name = "SiteEvents.Delete" };

        #endregion
    }
}