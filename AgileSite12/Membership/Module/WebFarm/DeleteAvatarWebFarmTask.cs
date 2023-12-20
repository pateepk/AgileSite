using System;

using CMS.Core;

namespace CMS.Membership
{
    /// <summary>
    /// Web farm task used to delete membership avatars.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class DeleteAvatarWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Gets or sets extension of the file to be deleted.
        /// </summary>
        public string FileExtension { get; set; }


        /// <summary>
        /// Gets or sets identifier of the file to be deleted.
        /// </summary>
        public Guid FileGuid { get; set; }


        /// <summary>
        /// Gets or sets a value that indicates whether directory will be deleted.
        /// </summary>
        public bool DeleteDirectory { get; set; }


        /// <summary>
        /// Initializes a new instance of <see cref="DeleteAvatarWebFarmTask"/>.
        /// </summary>
        public DeleteAvatarWebFarmTask()
        {
            TaskTarget = "AvatarDelete";
        }


        /// <summary>
        /// Returns true if the synchronization of the deletion of the BizForm files is allowed
        /// </summary>
        public override bool ConditionMethod()
        {
            return MembershipSynchronization.SynchronizeAvatars && CoreServices.WebFarm.SynchronizeDeleteFiles;
        }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="AvatarInfoProvider.DeleteAvatarFile(string, string, bool, bool)"/> method while passing it current class' properties as its parameters.
        /// </summary>
        public override void ExecuteTask()
        {
            AvatarInfoProvider.DeleteAvatarFile(FileGuid.ToString(), FileExtension, DeleteDirectory, true);
        }
    }
}
