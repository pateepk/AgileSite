using CMS.Core;
using CMS.DataEngine;
using CMS.IO;

namespace CMS.MediaLibrary
{
    /// <summary>
    /// Web farm task used to delete media files.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class DeleteFileMediaWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Gets or sets identifier of the site which destination library is assigned to.
        /// </summary>
        public int SiteId { get; set; }


        /// <summary>
        /// Gets or sets identifier of the library where file will be deleted.
        /// </summary>
        public int DestinationLibraryId { get; set; }


        /// <summary>
        /// Gets or sets relative path where file will be deleted.
        /// </summary>
        public string DestinationPath { get; set; }


        /// <summary>
        /// Gets or sets a value that indicates if media deletion will be executed only on files.
        /// </summary>
        public bool ApplyOnlyOnFiles { get; set; }


        /// <summary>
        /// Initializes a new instance of <see cref="DeleteFileMediaWebFarmTask"/>.
        /// </summary>
        public DeleteFileMediaWebFarmTask()
        {
            TaskTarget = "mediafiledelete";
        }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="MediaFileInfoProvider.DeleteMediaFile(int, int, string, bool, bool)"/> method while passing it current class' properties as its parameters.
        /// </summary>
        public override void ExecuteTask()
        {
            MediaFileInfoProvider.DeleteMediaFile(SiteId, DestinationLibraryId, DestinationPath, ApplyOnlyOnFiles, true);
        }


        /// <summary>
        /// Returns true whether the synchronization for deleting media files is allowed and provided storage is not shared.
        /// </summary>
        public override bool ConditionMethod()
        {
            return CoreServices.WebFarm.SynchronizeDeleteFiles && MediaSynchronization.SynchronizeMediaFiles && !StorageHelper.IsSharedStorage(TaskFilePath);
        }
    }
}
