using CMS.Base;
using CMS.Core;
using CMS.IO;

namespace CMS.MediaLibrary
{
    /// <summary>
    /// Web farm task used to copy media folders.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class CopyFolderMediaWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Gets or sets relative path where the file will be copied.
        /// </summary>
        public string DestinationPath { get; set; }


        /// <summary>
        /// Gets or sets relative path from where the file will be copied.
        /// </summary>
        public string SourcePath { get; set; }


        /// <summary>
        /// Gets or sets name of the site which destination library is assigned to.
        /// </summary>
        public string SiteName { get; set; }


        /// <summary>
        /// Gets or sets identifier of the library where the file will be copied.
        /// </summary>
        public int DestinationLibraryId { get; set; }


        /// <summary>
        /// Initializes a new instance of <see cref="CopyFolderMediaWebFarmTask"/>
        /// </summary>
        public CopyFolderMediaWebFarmTask()
        {
            TaskTarget = "copymediafolder";
        }

        /// <summary>
        /// Returns true whether the synchronization for copying media folders is allowed and provided storage is not shared.
        /// </summary>
        public override bool ConditionMethod()
        {
            return MediaSynchronization.SynchronizeMediaFiles && !StorageHelper.IsSharedStorage(MediaLibraryInfoProvider.GetMediaLibraryFolderPath(DestinationLibraryId));
        }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="MediaLibraryInfoProvider.CopyMediaLibraryFolder(string, int, string, string, int, bool, bool)"/> method while passing it current class' properties as its parameters.
        /// </summary>
        public override void ExecuteTask()
        {
            MediaLibraryInfoProvider.CopyMediaLibraryFolder(SiteName, DestinationLibraryId, SourcePath, DestinationPath, CMSActionContext.CurrentUser.UserID, true);
        }
    }
}
