using System;

using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Membership
{
    /// <summary>
    /// Web farm task used to update membership avatars.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class UpdateAvatarWebFarmTask : WebFarmTaskBase
    {
        private static bool? mSynchronizeAvatars = null;


        /// <summary>
        /// Gets or sets value that indicates whether file synchronization for avatars is enabled.
        /// </summary>
        public static bool SynchronizeAvatars
        {
            get
            {
                if (mSynchronizeAvatars == null)
                {
                    return (CoreServices.WebFarm.SynchronizeFiles && SettingsKeyInfoProvider.GetBoolValue("CMSWebFarmSynchronizeAvatars", "CMSWebFarmSynchronizeAvatars", true));
                }

                return mSynchronizeAvatars.Value;
            }
            set
            {
                mSynchronizeAvatars = value;
            }
        }


        /// <summary>
        /// Gets or sets extension of the file to be updated.
        /// </summary>
        public string FileExtension { get; set; }


        /// <summary>
        /// Gets or sets identifier of the file to be updated.
        /// </summary>
        public Guid FileGuid { get; set; }


        /// <summary>
        /// Gets or sets name of the file to be updated.
        /// </summary>
        public string FileName { get; set; }


        /// <summary>
        /// Initializes a new instance of <see cref="UpdateAvatarWebFarmTask"/>.
        /// </summary>
        public UpdateAvatarWebFarmTask()
        {
            TaskTarget = "AvatarUpdate";
        }


        /// <summary>
        /// Returns true if the synchronization of the BizForm files is allowed
        /// </summary>
        public override bool ConditionMethod()
        {
            return SynchronizeAvatars;
        }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="AvatarInfoProvider.SaveAvatarFileToDisk(string, string, string, BinaryData, bool)"/> method while passing it current class' properties as its parameters.
        /// </summary>
        public override void ExecuteTask()
        {
            // Save the binary data to the disk
            if (TaskBinaryData.Data != null)
            {
                AvatarInfoProvider.SaveAvatarFileToDisk(FileGuid.ToString(), FileName, FileExtension, TaskBinaryData.Data, true);
            }
            else
            {
                // Drop the cache dependencies
                AvatarInfoProvider.UpdatePhysicalFileLastWriteTime(FileName, FileExtension);
                CacheHelper.TouchKey($"avatarfile|{FileGuid}", false, false);
            }
        }
    }
}
