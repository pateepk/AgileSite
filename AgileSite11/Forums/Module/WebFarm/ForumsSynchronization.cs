using System;

using CMS.Core;
using CMS.Helpers;
using CMS.Base;
using CMS.SiteProvider;
using CMS.DataEngine;

namespace CMS.Forums
{
    /// <summary>
    /// Web farm synchronization for Forums
    /// </summary>
    internal class ForumsSynchronization
    {
        #region "Variables"

        private static bool? mSynchronizeForumAttachments = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets value that indicates whether file synchronization for forum attachments is enabled.
        /// </summary>
        public static bool SynchronizeForumAttachments
        {
            get
            {
                if (mSynchronizeForumAttachments == null)
                {
                    return (CoreServices.WebFarm.SynchronizeFiles && SettingsKeyInfoProvider.GetBoolValue("CMSWebFarmSynchronizeForumAttachments", "CMSWebFarmSynchronizeForumAttachments", true));
                }

                return mSynchronizeForumAttachments.Value;
            }
            set
            {
                mSynchronizeForumAttachments = value;
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
                Type = ForumsTaskType.UpdateForumAttachment,
                Execute = UpdateForumAttachment,
                Condition = CheckSynchronizeForumAttachments
            });
            WebFarmHelper.RegisterTask(new WebFarmTask
            {
                Type = ForumsTaskType.DeleteForumAttachment,
                Execute = DeleteForumAttachment,
                Condition = CheckSynchronizeDeleteForumAttachments
            });
        }


        /// <summary>
        /// Returns true if the synchronization of the forum attachments is allowed
        /// </summary>
        private static bool CheckSynchronizeForumAttachments(IWebFarmTask task)
        {
            return SynchronizeForumAttachments;
        }


        /// <summary>
        /// Returns true if the synchronization of the deletion of the forum attachments is allowed
        /// </summary>
        private static bool CheckSynchronizeDeleteForumAttachments(IWebFarmTask task)
        {
            return SynchronizeForumAttachments && CoreServices.WebFarm.SynchronizeDeleteFiles;
        }


        /// <summary>
        /// Updates the forum attachment
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void UpdateForumAttachment(string target, string[] data, BinaryData binaryData)
        {
            if (data.Length != 4)
            {
                throw new ArgumentException(String.Format("[ForumsSynchronization.UpdateForumAttachment]: Expected 4 data arguments, but received {0}", data.Length));
            }

            // Get the parameters
            string siteName = data[0];
            string guid = data[1];
            string fileName = data[2];
            string extension = data[3];

            // Save the binary data to the disk
            if (binaryData.Data != null)
            {
                ForumAttachmentInfoProvider.SaveAttachmentFileToDisk(siteName, guid, fileName, extension, binaryData.Data, true);
            }
            else
            {
                SiteInfo si = SiteInfoProvider.GetSiteInfo(siteName);
                if (si != null)
                {
                    // Drop the cache dependencies
                    CacheHelper.TouchKey("forumattachment|" + guid.ToLowerCSafe() + "|" + si.SiteID.ToString());
                }
            }
        }



        /// <summary>
        /// Updates the forum attachment
        /// </summary>
        /// <param name="target">Task target</param>
        /// <param name="data">Task data</param>
        /// <param name="binaryData">Task binary data</param>
        private static void DeleteForumAttachment(string target, string[] data, BinaryData binaryData)
        {
            if (data.Length != 3)
            {
                throw new ArgumentException(String.Format("[ForumsSynchronization.DeleteForumAttachment]: Expected 3 data arguments, but received {0}", data.Length));
            }

            // Get the parameters
            string siteName = data[0];
            string attachmentFileName = data[1];
            bool deleteDirectory = ValidationHelper.GetBoolean(data[2], false);

            // Delete attachment file
            ForumAttachmentInfoProvider.DeleteAttachmentFile(siteName, attachmentFileName, deleteDirectory, true);
        }

        #endregion
    }
}
