using System;
using System.Linq;

using CMS.Core;
using CMS.Helpers;
using CMS.Base;
using CMS.DataEngine;

namespace CMS.Membership
{
    /// <summary>
    /// Web farm synchronization for membership
    /// </summary>
    internal class MembershipSynchronization
    {
        #region "Variables"

        private static bool? mSynchronizeAvatars = null;

        #endregion


        #region "Properties"

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

        #endregion


        #region "Methods"

        /// <summary>
        /// Initializes the tasks for media files synchronization
        /// </summary>
        public static void Init()
        {
            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = MembershipTaskType.UpdateAvatar,
                Execute = UpdateAvatar,
                Condition = CheckSynchronizeAvatars
            });
            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = MembershipTaskType.DeleteAvatar,
                Execute = DeleteAvatar,
                Condition = CheckSynchronizeDeleteAvatars
            });
        }


        /// <summary>
        /// Returns true if the synchronization of the BizForm files is allowed
        /// </summary>
        private static bool CheckSynchronizeAvatars(IWebFarmTask task)
        {
            return SynchronizeAvatars;
        }


        /// <summary>
        /// Returns true if the synchronization of the deletion of the BizForm files is allowed
        /// </summary>
        private static bool CheckSynchronizeDeleteAvatars(IWebFarmTask task)
        {
            return SynchronizeAvatars && CoreServices.WebFarm.SynchronizeDeleteFiles;
        }


        /// <summary>
        /// Updates the Avatar
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void UpdateAvatar(string target, string[] data, BinaryData binaryData)
        {
            if (data.Length != 3)
            {
                throw new ArgumentException(String.Format("[MembershipSynchronization.UpdateAvatar]: Expected 3 data arguments, but received {0}", data.Length));
            }

            // Get the parameters
            string guid = data[0];
            string fileName = data[1];
            string extension = data[2];

            // Save the binary data to the disk
            if (binaryData.Data != null)
            {
                AvatarInfoProvider.SaveAvatarFileToDisk(guid, fileName, extension, binaryData.Data, true);
            }
            else
            {
                // Drop the cache dependencies
                AvatarInfoProvider.UpdatePhysicalFileLastWriteTime(fileName, extension);
                CacheHelper.TouchKey("avatarfile|" + guid.ToLowerCSafe(), false, false);
            }
        }


        /// <summary>
        /// Deletes the Avatar
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void DeleteAvatar(string target, string[] data, BinaryData binaryData)
        {
            if (data.Length != 3)
            {
                throw new ArgumentException(String.Format("[MembershipSynchronization.DeleteAvatar]: Expected 3 data arguments, but received {0}", data.Length));
            }

            // Get the parameters
            string guid = data[0];
            string extension = data[1];
            bool deleteDirectory = ValidationHelper.GetBoolean(data[2], false);

            // Delete the file
            AvatarInfoProvider.DeleteAvatarFile(guid, extension, deleteDirectory, true);
        }

        #endregion
    }
}
