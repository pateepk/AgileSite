using CMS.Core;
using CMS.Helpers;

namespace CMS.DataEngine
{
    /// <summary>
    /// Web farm task used to update meta files.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class UpdateMetaFileWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Gets or sets name of the site which the media library is assigned to.
        /// </summary>
        public string SiteName { get; set; }


        /// <summary>
        /// Gets or sets identifier of the file to be updated.
        /// </summary>
        public string FileGuid { get; set; }


        /// <summary>
        /// Gets or sets extension of the file to be updated.
        /// </summary>
        public string FileExtension { get; set; }


        /// <summary>
        /// Gets or sets a value that indicates if files in destination folder with mask '[guid]*.*' should be deleted.
        /// </summary>
        public bool DeleteOldFiles { get; set; }


        /// <summary>
        /// Gets or sets name of the file to be updated.
        /// </summary>
        public string FileName { get; set; }


        /// <summary>
        /// Initializes a new instance of <see cref="UpdateMetaFileWebFarmTask"/>.
        /// </summary>
        public UpdateMetaFileWebFarmTask()
        {
            TaskTarget = "metafileupload";
        }


        /// <summary>
        /// Returns true if the synchronization of the metafiles is allowed.
        /// </summary>
        public override bool ConditionMethod()
        {
            return DataTasks.SynchronizeMetaFiles;
        }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="MetaFileInfoProvider.SaveFileToDisk(string, string, string, string, BinaryData, bool, bool)"/> method.
        /// </summary>
        public override void ExecuteTask()
        {
            // Save the binary data to the disk
            if (TaskBinaryData.Data != null)
            {
                MetaFileInfoProvider.SaveFileToDisk(SiteName, FileGuid, FileName, FileExtension, TaskBinaryData.Data, DeleteOldFiles, true);
            }
            else
            {
                // Drop the cache dependencies
                MetaFileInfoProvider.UpdatePhysicalFileLastWriteTime(SiteName, FileName, FileExtension);
                CacheHelper.TouchKey("metafile|" + FileGuid, false, false);
            }
        }
    }
}
