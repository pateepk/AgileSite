using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.Forums
{
    /// <summary>
    /// Web farm synchronization for Forums
    /// </summary>
    internal class ForumsSynchronization
    {
        private static bool? mSynchronizeForumAttachments = null;


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


        /// <summary>
        /// Initializes the tasks for media files synchronization
        /// </summary>
        public static void Init()
        {
            WebFarmHelper.RegisterTask<UpdateForumAttachmentWebFarmTask>();
            WebFarmHelper.RegisterTask<DeleteForumAttachmentWebFarmTask>();
        }
    }
}
