using System;
using System.Linq;

using CMS.Core;
using CMS.Helpers;
using CMS.IO;
using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Web farm synchronization for IO
    /// </summary>
    internal class StorageTasks
    {
        #region "Variables"

        /// <summary>
        /// Web application physical path for synchronizing physical files.
        /// </summary>
        private static string mWebFarmApplicationPhysicalPath;

        #endregion


        #region "Properties"

        /// <summary>
        /// Web application physical path for synchronizing physical files.
        /// </summary>
        private static string WebFarmApplicationPhysicalPath
        {
            get
            {
                if (mWebFarmApplicationPhysicalPath == null)
                {
                    mWebFarmApplicationPhysicalPath = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSWebFarmApplicationPhysicalPath"], string.Empty);
                }

                // Use web app physical path when setting is empty 
                return DataHelper.GetNotEmpty(mWebFarmApplicationPhysicalPath, SystemContext.WebApplicationPhysicalPath);
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Initializes the tasks
        /// </summary>
        public static void Init()
        {
            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = StorageTaskType.UpdatePhysicalFile,
                Execute = UpdatePhysicalFile,
                Condition = CheckSynchronizePhysicalFiles
            });
            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = StorageTaskType.DeletePhysicalFile,
                Execute = DeletePhysicalFile,
                Condition = CheckSynchronizeDeletePhysicalFiles
            });
            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = StorageTaskType.DeleteFolder,
                Execute = DeleteFolder,
                Condition = CheckSynchronizeDeletePhysicalFiles
            });
        }


        /// <summary>
        /// Returns true if the synchronization of the physical files is allowed
        /// </summary>
        private static bool CheckSynchronizePhysicalFiles(IWebFarmTask task)
        {
            return CoreServices.WebFarm.SynchronizePhysicalFiles;
        }


        /// <summary>
        /// Returns true if the synchronization of the deletion of the physical files is allowed
        /// </summary>
        private static bool CheckSynchronizeDeletePhysicalFiles(IWebFarmTask task)
        {
            return CoreServices.WebFarm.SynchronizePhysicalFiles && CoreServices.WebFarm.SynchronizeDeleteFiles;
        }


        /// <summary>
        /// Deletes the folder
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void DeleteFolder(string target, string[] data, BinaryData binaryData)
        {
            var path = data.FirstOrDefault();
            if (String.IsNullOrEmpty(path))
            {
                return;
            }

            // Get directory path for current application
            string directoryPath = FileHelper.GetFullFolderPhysicalPath(path, WebFarmApplicationPhysicalPath);
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
            }
        }


        /// <summary>
        /// Deletes the physical file
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void DeletePhysicalFile(string target, string[] data, BinaryData binaryData)
        {
            var path = data.FirstOrDefault();
            if (String.IsNullOrEmpty(path))
            {
                return;
            }

            var filePath = FileHelper.GetFullFilePhysicalPath(path, WebFarmApplicationPhysicalPath);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }


        /// <summary>
        /// Updates the physical file
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void UpdatePhysicalFile(string target, string[] data, BinaryData binaryData)
        {
            var path = data.FirstOrDefault();
            if (String.IsNullOrEmpty(path) || (binaryData.Data == null))
            {
                return;
            }

            var filePath = FileHelper.GetFullFilePhysicalPath(data[0], WebFarmApplicationPhysicalPath);
            DirectoryHelper.EnsureDiskPath(filePath, WebFarmApplicationPhysicalPath);
            File.WriteAllBytes(filePath, binaryData.Data);
        }

        #endregion
    }
}