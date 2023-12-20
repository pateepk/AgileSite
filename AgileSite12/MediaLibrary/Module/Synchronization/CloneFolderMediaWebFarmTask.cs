using System;

using CMS.Core;
using CMS.IO;
using CMS.Membership;

namespace CMS.MediaLibrary
{
    /// <summary>
    /// Web farm task used to clone media folders.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class CloneFolderMediaWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Gets or sets the identifier of the library from where the folder will be cloned.
        /// </summary>
        public int SourceLibraryId { get; set; }


        /// <summary>
        /// Gets or sets the identifier of the library where the folder will be cloned.
        /// </summary>
        public int DestinationLibraryId { get; set; }


        /// <summary>
        /// Initializes a new instance of <see cref="CloneFolderMediaWebFarmTask"/>.
        /// </summary>
        public CloneFolderMediaWebFarmTask()
        {
            TaskTarget = "clonemediafolder";
        }


        /// <summary>
        /// Returns true whether the synchronization for cloning media folders is allowed and provided storage is not shared.
        /// </summary>
        public override bool ConditionMethod()
        {

            return MediaSynchronization.SynchronizeMediaFiles && !StorageHelper.IsSharedStorage(MediaLibraryInfoProvider.GetMediaLibraryFolderPath(DestinationLibraryId));
        }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="MediaLibraryHelper.CopyRecursive(int, int, DirectoryInfo, string, string, int, string, bool, int, bool, DataEngine.CloneSettings, DataEngine.CloneResult)"/>
        /// method while passing it current class' properties as its parameters.
        /// </summary>
        public override void ExecuteTask()
        {
            string sourcePath = MediaLibraryInfoProvider.GetMediaLibraryFolderPath(SourceLibraryId);
            string destinationPath = MediaLibraryInfoProvider.GetMediaLibraryFolderPath(DestinationLibraryId);

            MediaLibraryHelper.CopyRecursive(SourceLibraryId, DestinationLibraryId, DirectoryInfo.New(sourcePath), destinationPath, sourcePath, 0, String.Empty, false, MembershipContext.AuthenticatedUser.UserID, true);
        }
    }
}
