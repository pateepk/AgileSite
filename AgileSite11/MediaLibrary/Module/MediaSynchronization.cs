using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.Core;
using CMS.Helpers;
using CMS.Synchronization;
using CMS.Base;
using CMS.DataEngine;

namespace CMS.MediaLibrary
{
    /// <summary>
    /// Synchronization helper class for the media library module.
    /// </summary>
    internal class MediaSynchronization
    {
        #region "Variables"

        private static bool? mSynchronizeMediaFiles;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets value that indicates whether file synchronization for media files is enabled.
        /// </summary>
        public static bool SynchronizeMediaFiles
        {
            get
            {
                if (mSynchronizeMediaFiles == null)
                {
                    return (CoreServices.WebFarm.SynchronizeFiles && SettingsKeyInfoProvider.GetBoolValue("CMSWebFarmSynchronizeMediaFiles", "CMSWebFarmSynchronizeMediaFiles", true));
                }

                return mSynchronizeMediaFiles.Value;
            }
            set
            {
                mSynchronizeMediaFiles = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Initializes the tasks for media files synchronization
        /// </summary>
        public static void Init()
        {
            // Web farm synchronization
            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = MediaTaskType.UpdateMediaFile,
                Execute = UpdateMediaFile,
                Condition = CheckSynchronizeMediaFiles
            });
            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = MediaTaskType.DeleteMediaFile,
                Execute = DeleteMediaFile,
                Condition = CheckSynchronizeDeleteMediaFiles
            });
            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = MediaTaskType.CopyMediaFile,
                Execute = CopyMediaFile,
                Condition = CheckSynchronizeMediaFiles
            });
            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = MediaTaskType.MoveMediaFile,
                Execute = MoveMediaFile,
                Condition = CheckSynchronizeMediaFiles
            });
            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = MediaTaskType.DeleteMediaFilePreview,
                Execute = DeleteMediaFilePreview,
                Condition = CheckSynchronizeDeleteMediaFiles
            });

            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = MediaTaskType.CreateMediaFolder,
                Execute = CreateMediaFolder,
                Condition = CheckSynchronizeMediaFiles
            });
            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = MediaTaskType.RenameMediaFolder,
                Execute = RenameMediaFolder,
                Condition = CheckSynchronizeMediaFiles
            });
            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = MediaTaskType.CopyMediaFolder,
                Execute = CopyMediaFolder,
                Condition = CheckSynchronizeMediaFiles
            });
            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = MediaTaskType.MoveMediaFolder,
                Execute = MoveMediaFolder,
                Condition = CheckSynchronizeMediaFiles
            });
            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = MediaTaskType.DeleteMediaFolder,
                Execute = DeleteMediaFolder,
                Condition = CheckSynchronizeDeleteMediaFiles
            });

            // Staging synchronization
            StagingEvents.ProcessTask.Before += ProcessTask_Before;
        }


        /// <summary>
        /// Staging event handler for processing media folder tasks.
        /// </summary>
        private static void ProcessTask_Before(object sender, StagingSynchronizationEventArgs e)
        {
            switch (e.TaskType)
            {
                // Process media folder tasks
                case TaskTypeEnum.CreateMediaFolder:
                case TaskTypeEnum.MoveMediaFolder:
                case TaskTypeEnum.CopyMediaFolder:
                case TaskTypeEnum.RenameMediaFolder:
                case TaskTypeEnum.DeleteMediaFolder:
                case TaskTypeEnum.DeleteMediaRootFolder:
                    ProcessMediaFolder(e.TaskType, e.TaskData, e.SyncManager);

                    // Set 'TaskHandled' flag to not continue in processing general tasks 
                    e.TaskHandled = true;
                    break;
            }
        }


        /// <summary>
        /// Returns true if the synchronization of the media files is allowed
        /// </summary>
        private static bool CheckSynchronizeMediaFiles(IWebFarmTask task)
        {
            return SynchronizeMediaFiles;
        }


        /// <summary>
        /// Returns true if the synchronization of the deletion of the media files is allowed
        /// </summary>
        private static bool CheckSynchronizeDeleteMediaFiles(IWebFarmTask task)
        {
            return SynchronizeMediaFiles && CoreServices.WebFarm.SynchronizeDeleteFiles;
        }


        /// <summary>
        /// Process media folder task.
        /// </summary>
        /// <param name="taskType">Task type</param>
        /// <param name="ds">DataSet with folder data</param>
        /// <param name="syncManager">current SyncManager instance</param>
        private static void ProcessMediaFolder(TaskTypeEnum taskType, DataSet ds, ISyncManager syncManager)
        {
            // Get folder table
            DataTable folderTable = ds.Tables["FolderData"];
            if (folderTable == null)
            {
                throw new Exception("[MediaSynchronization.ProcessMediaFolder]: Missing media folder data.");
            }

            // Get folder data
            DataRow folderDR = folderTable.Rows[0];
            string sourceFolder = DataHelper.GetStringValue(folderDR, "SourcePath");
            string targetFolder = DataHelper.GetStringValue(folderDR, "TargetPath");
            int libraryId = DataHelper.GetIntValue(folderDR, "LibraryID");

            if (taskType != TaskTypeEnum.DeleteMediaRootFolder)
            {
                // Get the translation table
                DataTable transTable = ds.Tables[TranslationHelper.TRANSLATION_TABLE];
                TranslationHelper th = new TranslationHelper(transTable);

                if (syncManager.ProceedWithTranslations(th))
                {
                    // Translate library ID
                    libraryId = th.GetNewID(PredefinedObjectType.MEDIALIBRARY, libraryId, "LibraryName", syncManager.SiteID, "LibrarySiteID", null, "LibraryGroupID");
                }

                // Missing media library info - this is not error for delete task
                if ((libraryId == 0) && (taskType != TaskTypeEnum.DeleteMediaFolder))
                {
                    throw new Exception("[MediaSynchronization.ProcessMediaFolder]: Missing parent media library information.");
                }
            }

            switch (taskType)
            {
                // Create media library folder
                case TaskTypeEnum.CreateMediaFolder:
                    MediaLibraryInfoProvider.CreateMediaLibraryFolder(syncManager.SiteName, libraryId, sourceFolder, false, syncManager.LogTasks);
                    break;

                // Move media library folder
                case TaskTypeEnum.MoveMediaFolder:
                    MediaLibraryInfoProvider.MoveMediaLibraryFolder(syncManager.SiteName, libraryId, sourceFolder, targetFolder, false, syncManager.LogTasks);
                    break;

                // Copy media library folder
                case TaskTypeEnum.CopyMediaFolder:
                    Dictionary<Guid, Guid> fileGUIDs = GetFileGUIDsList(ds.Tables["FileGUIDs"]);
                    MediaLibraryInfoProvider.CopyMediaLibraryFolder(syncManager.SiteName, libraryId, sourceFolder, targetFolder, CMSActionContext.CurrentUser.UserID, false, syncManager.LogTasks, fileGUIDs);
                    break;

                // Rename media library folder
                case TaskTypeEnum.RenameMediaFolder:
                    MediaLibraryInfoProvider.RenameMediaLibraryFolder(syncManager.SiteName, libraryId, sourceFolder, targetFolder, false, syncManager.LogTasks);
                    break;

                // Delete media library folder
                case TaskTypeEnum.DeleteMediaFolder:
                    if (libraryId != 0)
                    {
                        MediaLibraryInfoProvider.DeleteMediaLibraryFolder(syncManager.SiteName, libraryId, sourceFolder, false, syncManager.LogTasks);
                    }
                    break;

                // Delete media library root folder
                case TaskTypeEnum.DeleteMediaRootFolder:
                    MediaLibraryInfoProvider.DeleteMediaLibraryFolder(syncManager.SiteName, sourceFolder, false, syncManager.LogTasks);
                    break;
            }
        }


        /// <summary>
        /// Updates the media file
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void UpdateMediaFile(string target, string[] data, BinaryData binaryData)
        {
            if (data.Length != 6)
            {
                throw new ArgumentException(String.Format("[MediaSynchronization.UpdateMediaFile]: Expected 6 data arguments, but received {0}", data.Length));
            }

            // Get the parameters
            string siteName = data[0];
            string libraryFolder = data[1];
            string librarySubFolderPath = data[2];
            string fileName = data[3];
            string extension = data[4];
            string guid = data[5];

            // Save the binary data to the disk
            if (binaryData.Data != null)
            {
                MediaFileInfoProvider.SaveFileToDisk(siteName, libraryFolder, librarySubFolderPath, fileName, extension, ValidationHelper.GetGuid(guid, Guid.Empty), binaryData.Data, true, false);
            }
            else
            {
                // Drop the cache dependencies if method run under synchronization
                CacheHelper.TouchKey("mediafile|" + guid.ToLowerCSafe());
                CacheHelper.TouchKey("mediafilepreview|" + guid.ToLowerCSafe());
            }
        }


        /// <summary>
        /// Deletes the media file
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void DeleteMediaFile(string target, string[] data, BinaryData binaryData)
        {
            if (data.Length != 4)
            {
                throw new ArgumentException(String.Format("[MediaSynchronization.DeleteMediaFile]: Expected 4 data arguments, but received {0}", data.Length));
            }

            // Get the parameters
            int siteId = ValidationHelper.GetInteger(data[0], 0);
            int libraryId = ValidationHelper.GetInteger(data[1], 0);
            string filePath = data[2];
            bool onlyFile = ValidationHelper.GetBoolean(data[3], false);

            // Delete the file
            MediaFileInfoProvider.DeleteMediaFile(siteId, libraryId, filePath, onlyFile, true);
        }


        /// <summary>
        /// Copies the media file
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void CopyMediaFile(string target, string[] data, BinaryData binaryData)
        {
            if (data.Length != 4)
            {
                throw new ArgumentException(String.Format("[MediaSynchronization.CopyMediaFile]: Expected 4 data arguments, but received {0}", data.Length));
            }

            // Get the parameters
            string siteName = data[0];
            int libraryId = ValidationHelper.GetInteger(data[1], 0);
            string originalPath = data[2];
            string newPath = data[3];

            // Copy the file
            MediaFileInfoProvider.CopyMediaFile(siteName, libraryId, originalPath, newPath, true);

        }


        /// <summary>
        /// Moves the media file
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void MoveMediaFile(string target, string[] data, BinaryData binaryData)
        {
            if (data.Length != 5)
            {
                throw new ArgumentException(String.Format("[MediaSynchronization.MoveMediaFile]: Expected 5 data arguments, but received {0}", data.Length));
            }

            // Get the parameters
            string siteName = data[0];
            int originalLibraryId = ValidationHelper.GetInteger(data[1], 0);
            int newLibraryId = ValidationHelper.GetInteger(data[2], 0);
            string originalPath = data[3];
            string newPath = data[4];

            // Move the file
            MediaFileInfoProvider.MoveMediaFile(siteName, originalLibraryId, newLibraryId, originalPath, newPath, true);
        }


        /// <summary>
        /// Deletes the media file preview
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void DeleteMediaFilePreview(string target, string[] data, BinaryData binaryData)
        {
            if (data.Length != 3)
            {
                throw new ArgumentException(String.Format("[MediaSynchronization.DeleteMediaFilePreview]: Expected 3 data arguments, but received {0}", data.Length));
            }

            // Get the parameters
            string siteName = data[0];
            int libraryId = ValidationHelper.GetInteger(data[1], 0);
            string filePath = data[2];

            // Delete the preview file
            MediaFileInfoProvider.DeleteMediaFilePreview(siteName, libraryId, filePath, true);
        }


        /// <summary>
        /// Creates the media folder
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void CreateMediaFolder(string target, string[] data, BinaryData binaryData)
        {
            if (data.Length != 3)
            {
                throw new ArgumentException(String.Format("[MediaSynchronization.CreateMediaFolder]: Expected 3 data arguments, but received {0}", data.Length));
            }

            // Get the parameters
            string siteName = data[0];
            int libraryId = ValidationHelper.GetInteger(data[1], 0);
            string newFolderPath = data[2];

            // Create folder
            MediaLibraryInfoProvider.CreateMediaLibraryFolder(siteName, libraryId, newFolderPath, true);
        }


        /// <summary>
        /// Renames the media folder
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void RenameMediaFolder(string target, string[] data, BinaryData binaryData)
        {
            if (data.Length != 4)
            {
                throw new ArgumentException(String.Format("[MediaSynchronization.RenameMediaFolder]: Expected 4 data arguments, but received {0}", data.Length));
            }

            // Get the parameters
            string siteName = data[0];
            int libraryId = ValidationHelper.GetInteger(data[1], 0);
            string folderPath = data[2];
            string newFolderPath = data[3];

            // Rename folder
            MediaLibraryInfoProvider.RenameMediaLibraryFolder(siteName, libraryId, folderPath, newFolderPath, true);
        }


        /// <summary>
        /// Copies the media folder
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void CopyMediaFolder(string target, string[] data, BinaryData binaryData)
        {
            if (data.Length != 4)
            {
                throw new ArgumentException(String.Format("[MediaSynchronization.CopyMediaFolder]: Expected 4 data arguments, but received {0}", data.Length));
            }

            // Get the parameters
            string siteName = data[0];
            int libraryId = ValidationHelper.GetInteger(data[1], 0);
            string originalPath = data[2];
            string newPath = data[3];

            // Copy the folder
            MediaLibraryInfoProvider.CopyMediaLibraryFolder(siteName, libraryId, originalPath, newPath, CMSActionContext.CurrentUser.UserID, true);
        }


        /// <summary>
        /// Moves the media folder
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void MoveMediaFolder(string target, string[] data, BinaryData binaryData)
        {
            if (data.Length != 4)
            {
                throw new ArgumentException(String.Format("[MediaSynchronization.MoveMediaFolder]: Expected 4 data arguments, but received {0}", data.Length));
            }

            // Get the parameters
            string siteName = data[0];
            int libraryId = ValidationHelper.GetInteger(data[1], 0);
            string originalPath = data[2];
            string newPath = data[3];

            // Copy the folder
            MediaLibraryInfoProvider.MoveMediaLibraryFolder(siteName, libraryId, originalPath, newPath, true);
        }


        /// <summary>
        /// Deletes the media folder
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void DeleteMediaFolder(string target, string[] data, BinaryData binaryData)
        {
            if (data.Length == 3)
            {
                // Get the parameters
                string siteName = data[0];
                int libraryId = ValidationHelper.GetInteger(data[1], 0);
                string folderPath = data[2];

                // Delete the folder
                MediaLibraryInfoProvider.DeleteMediaLibraryFolder(siteName, libraryId, folderPath, true);
            }
            else if (data.Length == 2)
            {
                // Get the parameters
                string siteName = data[0];
                string folderPath = data[1];

                // Delete the folder
                MediaLibraryInfoProvider.DeleteMediaLibraryFolder(siteName, folderPath, true);
            }
            else
            {
                throw new ArgumentException(String.Format("[MediaSynchronization.DeleteMediaFolder]: Expected 2 or 3 data arguments, but received {0}", data.Length));
            }
        }


        /// <summary>
        /// Gets dictionary with source GUID : target GUID pairs
        /// </summary>
        /// <param name="dt">Data table with data</param>
        private static Dictionary<Guid, Guid> GetFileGUIDsList(DataTable dt)
        {
            if (dt == null)
            {
                return null;
            }

            return dt.AsEnumerable()
                     .ToDictionary(row => row["SourceGUID"].ToGuid(Guid.Empty),
                         row => row["TargetGUID"].ToGuid(Guid.Empty));
        }

        #endregion
    }
}
