using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.Helpers;
using CMS.Synchronization;
using CMS.Base;
using CMS.DataEngine;
using CMS.Core;

namespace CMS.MediaLibrary
{
    /// <summary>
    /// Synchronization helper class for the media library module.
    /// </summary>
    internal class MediaSynchronization
    {
        private static bool? mSynchronizeMediaFiles;


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


        /// <summary>
        /// Initializes the tasks for media files synchronization
        /// </summary>
        public static void Init()
        {
            // Web farm synchronization
            WebFarmHelper.RegisterTask<UpdateMediaWebFarmTask>();
            WebFarmHelper.RegisterTask<DeleteFileMediaWebFarmTask>();
            WebFarmHelper.RegisterTask<CopyFileMediaWebFarmTask>();
            WebFarmHelper.RegisterTask<MoveFileMediaWebFarmTask>();
            WebFarmHelper.RegisterTask<DeleteFilePreviewMediaWebFarmTask>();
            WebFarmHelper.RegisterTask<CreateFolderMediaWebFarmTask>();
            WebFarmHelper.RegisterTask<RenameFolderMediaWebFarmTask>();
            WebFarmHelper.RegisterTask<CopyFolderMediaWebFarmTask>();
            WebFarmHelper.RegisterTask<CloneFolderMediaWebFarmTask>();
            WebFarmHelper.RegisterTask<MoveFolderMediaWebFarmTask>();
            WebFarmHelper.RegisterTask<DeleteFolderMediaWebFarmTask>();

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
    }
}
