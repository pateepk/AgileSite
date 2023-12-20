using CMS.Core;
using CMS.DataEngine;
using CMS.IO;

namespace CMS.MediaLibrary
{
    /// <summary>
    /// Web farm task used to rename media folders.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class RenameFolderMediaWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Gets or sets relative path where folder will be renamed.
        /// </summary>
        public string DestinationPath { get; set; }


        /// <summary>
        /// Gets or sets relative path from where folder will be renamed.
        /// </summary>
        public string SourcePath { get; set; }


        /// <summary>
        /// Gets or sets name of the site which library is assigned to.
        /// </summary>
        public string SiteName { get; set; }


        /// <summary>
        /// Gets or sets identifier of the library where folder will be renamed.
        /// </summary>
        public int DestinationLibraryId { get; set; }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="MediaLibraryInfoProvider.RenameMediaLibraryFolder(string, int, string, string, bool, bool)"/> method while passing it current class' properties as its parameters.
        /// </summary>
        public override void ExecuteTask()
        {
            MediaLibraryInfoProvider.RenameMediaLibraryFolder(SiteName, DestinationLibraryId, SourcePath, DestinationPath, true);
        }


        /// <summary>
        /// Returns true whether the synchronization for renaming media folders is allowed and provided storage is not shared.
        /// </summary>
        public override bool ConditionMethod()
        {
            return MediaSynchronization.SynchronizeMediaFiles && !StorageHelper.IsSharedStorage(MediaLibraryInfoProvider.GetMediaLibraryFolderPath(DestinationLibraryId));
        }
    }
}
