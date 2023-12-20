using CMS.Core;
using CMS.DataEngine;
using CMS.IO;

namespace CMS.MediaLibrary
{
    /// <summary>
    /// Web farm task used to create media folders.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class CreateFolderMediaWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Gets or sets name of the site which destination library is assigned to.
        /// </summary>
        public string SiteName { get; set; }


        /// <summary>
        /// Gets or sets identifier of the library where folder will be created.
        /// </summary>
        public int DestinationLibraryId { get; set; }


        /// <summary>
        /// Gets or sets relative path where folder will be created.
        /// </summary>
        public string DestinationPath { get; set; }


        /// <summary>
        /// Initializes a new instance of <see cref="CreateFolderMediaWebFarmTask"/>.
        /// </summary>
        public CreateFolderMediaWebFarmTask()
        {
            TaskTarget = "updatemediafolder";
        }


        /// <summary>
        /// Returns true whether the synchronization for creating media folders is allowed and provided storage is not shared.
        /// </summary>
        public override bool ConditionMethod()
        {
            return MediaSynchronization.SynchronizeMediaFiles && !StorageHelper.IsSharedStorage(TaskFilePath);
        }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="MediaLibraryInfoProvider.CreateMediaLibraryFolder(string, int, string, bool, bool)"/> method while passing it current class' properties as its parameters.
        /// </summary>
        public override void ExecuteTask()
        {
            MediaLibraryInfoProvider.CreateMediaLibraryFolder(SiteName, DestinationLibraryId, DestinationPath, true);
        }
    }
}
