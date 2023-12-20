using CMS.Core;
using CMS.DataEngine;
using CMS.IO;

namespace CMS.MediaLibrary
{
    /// <summary>
    /// Web farm task used to clone media files.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class CopyFileMediaWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Gets or sets relative path where the file will be cloned.
        /// </summary>
        public string DestinationPath { get; set; }


        /// <summary>
        /// Gets or sets relative path from where the file will be cloned.
        /// </summary>
        public string SourcePath { get; set; }


        /// <summary>
        /// Gets or sets name of the site which destination library is assigned to.
        /// </summary>
        public string SiteName { get; set; }


        /// <summary>
        /// Gets or sets identifier of the library where the file will be cloned.
        /// </summary>
        public int DestinationLibraryId { get; set; }


        /// <summary>
        /// Initializes a new instance of <see cref="CopyFileMediaWebFarmTask"/>.
        /// </summary>
        public CopyFileMediaWebFarmTask()
        {
            TaskTarget = "copymediafile";
        }


        /// <summary>
        /// Returns true whether the synchronization for copying media files is allowed and provided storage is not shared.
        /// </summary>
        public override bool ConditionMethod()
        {
            return MediaSynchronization.SynchronizeMediaFiles && !StorageHelper.IsSharedStorage(MediaLibraryInfoProvider.GetMediaLibraryFolderPath(DestinationLibraryId));
        }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="MediaFileInfoProvider.CopyMediaFile(string, int, string, string, bool, int)"/> method while passing it current class' properties as its parameters.
        /// </summary>
        public override void ExecuteTask()
        {
            MediaFileInfoProvider.CopyMediaFile(SiteName, DestinationLibraryId, SourcePath, DestinationPath, true);
        }
    }
}
