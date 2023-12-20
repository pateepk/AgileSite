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
        /// Initializes the tasks for media files synchronization
        /// </summary>
        public static void Init()
        {
            WebFarmHelper.RegisterTask<UpdateAvatarWebFarmTask>();
            WebFarmHelper.RegisterTask<DeleteAvatarWebFarmTask>();
        }
    }
}
