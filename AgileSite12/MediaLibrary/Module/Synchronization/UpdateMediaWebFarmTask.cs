using System;

using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.IO;

namespace CMS.MediaLibrary
{
    /// <summary>
    /// Web farm task used to upload media files.
    /// </summary>
    public class UpdateMediaWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Initializes a new instance of <see cref="UpdateMediaWebFarmTask"/>.
        /// </summary>
        public UpdateMediaWebFarmTask()
        {
            TaskTarget = "mediafileupload";
        }


        /// <summary>
        /// Gets or sets name of the site which media library is assigned to.
        /// </summary>
        public string SiteName { get; set; }


        /// <summary>
        /// Gets or sets media library root folder.
        /// </summary>
        public string LibraryFolder { get; set; }


        /// <summary>
        /// Gets or sets library sub folder path.
        /// </summary>
        public string LibrarySubFolderPath { get; set; }


        /// <summary>
        /// Gets or sets name of the file to be updated.
        /// </summary>
        public string FileName { get; set; }


        /// <summary>
        /// Gets or sets extension of the file to be updated.
        /// </summary>
        public string FileExtension { get; set; }


        /// <summary>
        /// Gets or sets guid identifier of the file to be updated.
        /// </summary>
        public Guid FileGuid { get; set; }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="MediaFileInfoProvider.SaveFileToDisk(string, string, string, string, string, Guid, BinaryData, bool, bool)"/> method while passing it current class' properties as its parameters.
        /// </summary>
        public override void ExecuteTask()
        {
            // Save the binary data to the disk
            if (TaskBinaryData.Data != null)
            {
                MediaFileInfoProvider.SaveFileToDisk(SiteName, LibraryFolder, LibrarySubFolderPath, FileName, FileExtension, FileGuid, TaskBinaryData, true, false);
            }
            else
            {
                // Drop the cache dependencies if method run under synchronization
                CacheHelper.TouchKey("mediafile|" + FileGuid);
                CacheHelper.TouchKey("mediafilepreview|" + FileGuid);
            }
        }


        /// <summary>
        /// Returns true whether the synchronization for updating media files is allowed and provided storage is not shared.
        /// </summary>
        public override bool ConditionMethod()
        {
            return MediaSynchronization.SynchronizeMediaFiles && !StorageHelper.IsSharedStorage(TaskFilePath);
        }
    }
}
