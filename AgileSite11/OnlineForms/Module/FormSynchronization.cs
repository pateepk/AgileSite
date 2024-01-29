using System;

using CMS.Core;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.IO;
using CMS.Base;
using CMS.DataEngine;

namespace CMS.OnlineForms
{
    /// <summary>
    /// Web farm synchronization for Documents
    /// </summary>
    internal class FormSynchronization
    {
        #region "Variables"

        private static bool? mSynchronizeBizFormFiles;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets value that indicates whether file synchronization for BizForm files is enabled.
        /// </summary>
        public static bool SynchronizeBizFormFiles
        {
            get
            {
                if (mSynchronizeBizFormFiles == null)
                {
                    return (CoreServices.WebFarm.SynchronizeFiles && SettingsKeyInfoProvider.GetBoolValue("CMSWebFarmSynchronizeBizFormFiles", "CMSWebFarmSynchronizeBizFormFiles", true));
                }

                return mSynchronizeBizFormFiles.Value;
            }
            set
            {
                mSynchronizeBizFormFiles = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Initializes the tasks for forms synchronization
        /// </summary>
        public static void Init()
        {
            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = FormTaskType.UpdateBizFormFile,
                Execute = UpdateBizFormFile,
                Condition = CheckSynchronizeBizFormFiles
            });
            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = FormTaskType.DeleteBizFormFile,
                Execute = DeleteBizFormFile,
                Condition = CheckSynchronizeDeleteBizFormFiles
            });

            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = FormTaskType.ClearBizFormTypeInfos,
                Execute = ClearBizFormTypeInfo,
                IsMemoryTask = true
            });
            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = FormTaskType.InvalidateBizFormTypeInfo,
                Execute = InvalidateBizFormTypeInfo,
                IsMemoryTask = true
            });
        }


        /// <summary>
        /// Returns true if the synchronization of the BizForm files is allowed
        /// </summary>
        private static bool CheckSynchronizeBizFormFiles(IWebFarmTask task)
        {
            return SynchronizeBizFormFiles;
        }


        /// <summary>
        /// Returns true if the synchronization of the deletion of the BizForm files is allowed
        /// </summary>
        private static bool CheckSynchronizeDeleteBizFormFiles(IWebFarmTask task)
        {
            return SynchronizeBizFormFiles && CoreServices.WebFarm.SynchronizeDeleteFiles;
        }


        /// <summary>
        /// Updates the BizForm file
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void UpdateBizFormFile(string target, string[] data, BinaryData binaryData)
        {
            if (data.Length != 2)
            {
                throw new ArgumentException(String.Format("[FormSynchronization.UpdateBizFormFile]: Expected 2 data arguments, but received {0}", data.Length));
            }

            string fileName = FormHelper.GetBizFormFilesFolderPath(data[0]) + data[1];
            DirectoryHelper.EnsureDiskPath(fileName, SystemContext.WebApplicationPhysicalPath);
            StorageHelper.SaveFileToDisk(fileName, binaryData);
        }


        /// <summary>
        /// Deletes the BizForm file
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void DeleteBizFormFile(string target, string[] data, BinaryData binaryData)
        {
            if (data.Length != 2)
            {
                throw new ArgumentException(String.Format("[FormSynchronization.DeleteBizFormFile]: Expected 2 data arguments, but received {0}", data.Length));
            }

            string fileName = data[1];
            string siteName = data[0];

            string filePath = FormHelper.GetBizFormFilesFolderPath(siteName) + fileName;
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }


        /// <summary>
        /// Clears the BizForm item type info
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void ClearBizFormTypeInfo(string target, string[] data, BinaryData binaryData)
        {
            BizFormItemProvider.Clear(false);
        }


        /// <summary>
        /// Invalidates the BizForm item type info
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void InvalidateBizFormTypeInfo(string target, string[] data, BinaryData binaryData)
        {
            BizFormItemProvider.InvalidateTypeInfo(ValidationHelper.GetString(data, String.Empty), false);
        }

        #endregion
    }
}
