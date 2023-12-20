using System;

using CMS.Base;
using CMS.Core;


namespace CMS.IO
{
    /// <summary>
    /// Web farm synchronization for IO
    /// </summary>
    internal static class StorageTasks
    {
        /// <summary>
        /// Web application physical path for synchronizing physical files.
        /// </summary>
        private static string mWebFarmApplicationPhysicalPath;


        /// <summary>
        /// Web application physical path for synchronizing physical files.
        /// </summary>
        public static string WebFarmApplicationPhysicalPath
        {
            get
            {
                if (mWebFarmApplicationPhysicalPath == null)
                {
                    mWebFarmApplicationPhysicalPath = SettingsHelper.AppSettings["CMSWebFarmApplicationPhysicalPath"].ToString(string.Empty);
                }

                // Use web app physical path when setting is empty
                if (String.IsNullOrEmpty(mWebFarmApplicationPhysicalPath))
                {
                    return SystemContext.WebApplicationPhysicalPath;
                }

                return mWebFarmApplicationPhysicalPath;
            }
        }


        /// <summary>
        /// Initializes the tasks
        /// </summary>
        public static void Init()
        {
            CoreServices.WebFarm.RegisterTask<UpdateFileWebFarmTask>();
            CoreServices.WebFarm.RegisterTask<DeleteFileWebFarmTask>();
            CoreServices.WebFarm.RegisterTask<DeleteFolderWebFarmTask>();
        }
    }
}
