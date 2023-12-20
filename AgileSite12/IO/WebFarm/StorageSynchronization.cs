using System;
using System.Collections.Generic;

using CMS.Core;
using CMS.Base;

namespace CMS.IO
{
    /// <summary>
    /// Storage task logging
    /// </summary>
    internal class StorageSynchronization
    {
        #region "Variables"

        /// <summary>
        /// Lookup table for physical folders that are automatically synchronized over the web farm
        /// </summary>
        private static List<string> mWebFarmSynchronizedFolders;

        #endregion


        #region "Properties"

        /// <summary>
        /// Lookup table for physical folders that are automatically synchronized over the web farm
        /// </summary>
        public static List<string> WebFarmSynchronizedFolders
        {
            get
            {
                if (mWebFarmSynchronizedFolders == null)
                {
                    var folders = new List<string>();
                    folders.Add("/App_Themes/");
                    folders.Add("/CMSVirtualFiles/");
                    folders.Add("/App_Data/VersionHistory/Attachments/");

                    var siteUtilsFolder = SettingsHelper.AppSettings["CMSSiteUtilsFolderPath"] ?? "/CMSSiteUtils/";
                    folders.Add(siteUtilsFolder.TrimStart('~').TrimEnd('/') + "/Import/");

                    mWebFarmSynchronizedFolders = folders;
                }

                return mWebFarmSynchronizedFolders;
            }
        }

        #endregion

        
        #region "Methods"

        /// <summary>
        /// Returns true if the given relative path is synchronized for web farm
        /// </summary>
        public static bool IsWebFarmSynchronizedPath(string path)
        {
            if (!CoreServices.WebFarm.SynchronizePhysicalFiles)
            {
                return false;
            }

            if (path.StartsWith("~/", StringComparison.Ordinal))
            {
                path = path.Substring(1);
            }

            // Check synchronized paths
            foreach (string folder in WebFarmSynchronizedFolders)
            {
                if (path.StartsWith(folder, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Logs the directory delete task for the web farm server
        /// </summary>
        /// <param name="path">Path</param>
        public static void LogDirectoryDeleteTask(string path)
        {
            if (CMSActionContext.CurrentLogWebFarmTasks)
            {
                // Check if the file can be synchronized
                path = StorageHelper.GetWebApplicationRelativePath(path);
                if (!String.IsNullOrEmpty(path) && IsWebFarmSynchronizedPath(path))
                {
                    CoreServices.WebFarm.CreateIOTask(new DeleteFolderWebFarmTask { Path = path, TaskFilePath = path });
                }
            }
        }


        /// <summary>
        /// Logs the file delete task for the web farm server
        /// </summary>
        /// <param name="path">Path</param>
        public static void LogDeleteFileTask(string path)
        {
            if (CMSActionContext.CurrentLogWebFarmTasks)
            {
                // Check if the file can be synchronized
                path = StorageHelper.GetWebApplicationRelativePath(path);
                if (!String.IsNullOrEmpty(path) && IsWebFarmSynchronizedPath(path))
                {
                    CoreServices.WebFarm.CreateIOTask(new DeleteFileWebFarmTask { Path = path, TaskFilePath = path });
                }
            }
        }


        /// <summary>
        /// Logs the file update task for the web farm server
        /// </summary>
        /// <param name="path">Path</param>
        public static void LogUpdateFileTask(string path)
        {
            if (CMSActionContext.CurrentLogWebFarmTasks)
            {
                // Check if the file can be synchronized
                string relativePath = StorageHelper.GetWebApplicationRelativePath(path);
                if (!String.IsNullOrEmpty(relativePath) && IsWebFarmSynchronizedPath(relativePath))
                {
                    if (File.Exists(path))
                    {
                        // Create the file task
                        using (var str = File.OpenRead(path))
                        {
                            CoreServices.WebFarm.CreateIOTask(new UpdateFileWebFarmTask {
                                Path = relativePath,
                                TaskFilePath = path,
                                TaskBinaryData = str,
                            });
                        }
                    }
                }
            }
        }

        #endregion
    }
}
