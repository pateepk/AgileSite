using CMS.DataEngine;

namespace CMS.MediaLibrary
{
    /// <summary>
    /// Encapsulates settings for staging and integration bus.
    /// </summary>
    internal class LogMediaLibraryChangeSettings : AbstractSynchronizationSettings
    {
        /// <summary>
        /// <see cref="MediaLibraryInfo"/>'s ID.
        /// </summary>
        public int LibraryID
        {
            get;
            set;
        }

        /// <summary>
        /// Source path of the synchronized file.
        /// </summary>
        public string SourcePath
        {
            get;
            set;
        }


        /// <summary>
        /// Target path of the synchronized file.
        /// </summary>
        public string TargetPath
        {
            get;
            set;
        }


        /// <summary>
        /// Synchronized file site name.
        /// </summary>
        public string SiteName
        {
            get;
            set;
        }
    }
}
