using CMS.Core;
using CMS.DataEngine;
using CMS.IO;

namespace CMS.MediaLibrary
{
    /// <summary>
    /// Web farm task used to move media files.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class MoveFileMediaWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Gets or sets name of the site which destination library is assigned to.
        /// </summary>
        public string SiteName { get; set; }


        /// <summary>
        /// Gets or sets identifier of the library from where file will be moved.
        /// </summary>
        public int SourceLibraryId { get; set; }


        /// <summary>
        /// Gets or sets identifier of the library where file will be moved.
        /// </summary>
        public int DestinationLibraryId { get; set; }


        /// <summary>
        /// Gets or sets relative path from where file will be moved.
        /// </summary>
        public string SourcePath { get; set; }


        /// <summary>
        /// Gets or sets relative path where file will be moved.
        /// </summary>
        public string DestinationPath { get; set; }


        /// <summary>
        /// Initializes a new instance of <see cref="MoveFileMediaWebFarmTask"/>
        /// </summary>
        public MoveFileMediaWebFarmTask()
        {
            TaskTarget = "movemediafile";
        }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="MediaFileInfoProvider.MoveMediaFile(string, int, int, string, string, bool)"/> method while passing it current class' properties as its parameters.
        /// </summary>
        public override void ExecuteTask()
        {

            MediaFileInfoProvider.MoveMediaFile(SiteName, SourceLibraryId, DestinationLibraryId, SourcePath, DestinationPath, true);
        }


        /// <summary>
        /// Returns true whether the synchronization for moving media files is allowed and provided storage is not shared.
        /// </summary>
        public override bool ConditionMethod()
        {
            return MediaSynchronization.SynchronizeMediaFiles && !StorageHelper.IsSharedStorage(MediaLibraryInfoProvider.GetMediaLibraryFolderPath(DestinationLibraryId));
        }
    }
}
