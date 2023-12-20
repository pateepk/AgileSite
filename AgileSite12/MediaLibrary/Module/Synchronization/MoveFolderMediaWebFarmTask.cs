using CMS.Core;
using CMS.DataEngine;
using CMS.IO;

namespace CMS.MediaLibrary
{
    /// <summary>
    /// Web farm task used to move  media folders.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class MoveFolderMediaWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Gets or sets relative destination path where folder will be moved.
        /// </summary>
        public string DestinationPath { get; set; }


        /// <summary>
        /// Gets or sets relative source path from where folder will be moved.
        /// </summary>
        public string SourcePath { get; set; }


        /// <summary>
        /// Gets or sets name of the site which destination library will be assigned to.
        /// </summary>
        public string SiteName { get; set; }


        /// <summary>
        /// Gets or sets identifier of the library where folder will be moved.
        /// </summary>
        public int DestinationLibraryId { get; set; }


        /// <summary>
        /// Initializes a new instance of <see cref="MoveFolderMediaWebFarmTask"/>.
        /// </summary>
        public MoveFolderMediaWebFarmTask()
        {
            TaskTarget = "movemediafolder";
        }


        /// <summary>
        /// Returns true whether the synchronization for moving media folders is allowed and provided storage is not shared.
        /// </summary>
        public override bool ConditionMethod()
        {
            return MediaSynchronization.SynchronizeMediaFiles && !StorageHelper.IsSharedStorage(MediaLibraryInfoProvider.GetMediaLibraryFolderPath(DestinationLibraryId));
        }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="MediaLibraryInfoProvider.MoveMediaLibraryFolder(string, int, string, string, bool, bool)"/> method while passing it current class' properties as its parameters.
        /// </summary>
        public override void ExecuteTask()
        {
            MediaLibraryInfoProvider.MoveMediaLibraryFolder(SiteName, DestinationLibraryId, SourcePath, DestinationPath, true);
        }
    }
}
