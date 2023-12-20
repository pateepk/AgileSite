using CMS.Core;
using CMS.DataEngine;
using CMS.IO;

namespace CMS.MediaLibrary
{
    /// <summary>
    /// Web farm task used to delete media folders.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class DeleteFolderMediaWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Gets or sets name of the site which destination library is assigned to.
        /// </summary>
        public string SiteName { get; set; }


        /// <summary>
        /// Gets or sets relative path where file will be deleted.
        /// </summary>
        public string DestinationPath { get; set; }


        /// <summary>
        /// Gets or sets identifier of the library where file will be deleted.
        /// </summary>
        public int DestinationLibraryId { get; set; }


        /// <summary>
        /// Initializes a new instance of <see cref="DeleteFolderMediaWebFarmTask"/>.
        /// </summary>
        public DeleteFolderMediaWebFarmTask()
        {
            TaskTarget = "deletemediafolder";
        }


        /// <summary>
        /// Returns true whether the synchronization for deleting media files is allowed and provided storage is not shared.
        /// </summary>
        public override bool ConditionMethod()
        {
            return CoreServices.WebFarm.SynchronizeDeleteFiles && MediaSynchronization.SynchronizeMediaFiles && !StorageHelper.IsSharedStorage(TaskFilePath);
        }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="MediaLibraryInfoProvider.DeleteMediaLibraryFolder(string, string, bool, bool)"/> method while passing it current class' properties as its parameters.
        /// </summary>
        public override void ExecuteTask()
        {
            if (DestinationLibraryId != 0)
            {
                MediaLibraryInfoProvider.DeleteMediaLibraryFolder(SiteName, DestinationLibraryId, DestinationPath, true);
            }

            MediaLibraryInfoProvider.DeleteMediaLibraryFolder(SiteName, DestinationPath, true);
        }
    }
}
