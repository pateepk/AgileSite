namespace CMS.SiteProvider
{
    /// <summary>
    /// Site deletion settings configuration
    /// </summary>
    public sealed class SiteDeletionSettings
    {
        /// <summary>
        /// Indicates if meta files should be deleted.
        /// </summary>
        public bool DeleteMetaFiles
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if media files should be deleted.
        /// </summary>
        public bool DeleteMediaFiles
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if attachments should be deleted.
        /// </summary>
        public bool DeleteAttachments
        {
            get;
            set;
        }


        /// <summary>
        /// Deleted site.
        /// </summary>
        public SiteInfo Site
        {
            get;
            set;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public SiteDeletionSettings()
        {
            DeleteMetaFiles = true;
            DeleteMediaFiles = true;
            DeleteAttachments = true;
        }
    }
}