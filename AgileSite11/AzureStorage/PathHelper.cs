using CMS.Base;

namespace CMS.AzureStorage
{
    /// <summary>
    /// Contains helper members for accessing to blob storage, cache and temp.
    /// </summary>
    public static class PathHelper
    {
        private static string mTempPath;
        private static string mCachePath;


        /// <summary>
        /// Gets or sets path to local storage for temp.
        /// </summary>
        public static string TempPath
        {
            get
            {
                return mTempPath ?? (mTempPath = GetPathToTempDirectory(SettingsHelper.AppSettings["CMSAzureTempPath"], "AzureTemp"));
            }
            set
            {
                mTempPath = value;
            }
        }


        /// <summary>
        /// Gets or sets path to local storage for cache.
        /// </summary>
        public static string CachePath
        {
            get
            {
                return mCachePath ?? (mCachePath = GetPathToTempDirectory(SettingsHelper.AppSettings["CMSAzureCachePath"], "AzureCache"));
            }
            set
            {
                mCachePath = value;
            }
        }


        /// <summary>
        /// Returns absolute path to temp. Also ensures that the directory exists.
        /// </summary>
        /// <param name="tempAbsolutePath">Absolute path to temp.</param>
        /// <param name="relativeDirectoryPath">Path to directory relative to default temp root.</param>
        /// <remarks>Relative directory path is used only when absolute path is not specified.</remarks>
        private static string GetPathToTempDirectory(string tempAbsolutePath, string relativeDirectoryPath)
        {
            if (string.IsNullOrEmpty(tempAbsolutePath))
            {
                tempAbsolutePath = SystemContext.WebApplicationPhysicalPath + "\\App_Data\\" + relativeDirectoryPath;
            }

            if (!System.IO.Directory.Exists(tempAbsolutePath))
            {
                System.IO.Directory.CreateDirectory(tempAbsolutePath);
            }

            return tempAbsolutePath.TrimEnd('\\');
        }
    }
}
