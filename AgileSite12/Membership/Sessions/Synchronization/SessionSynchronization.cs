using CMS.Helpers;

namespace CMS.Membership
{
    /// <summary>
    /// Web farm synchronization for session
    /// </summary>
    internal class SessionSynchronization
    {
        /// <summary>
        /// Initializes the tasks for media files synchronization
        /// </summary>
        public static void Init()
        {
            WebFarmHelper.RegisterTask<AddUserToKickedListWebFarmTask>();
            WebFarmHelper.RegisterTask<UpdateDatabaseSessionWebFarmTask>();
            WebFarmHelper.RegisterTask<RemoveUserFromKickedList>(true);
            WebFarmHelper.RegisterTask<RemoveSessionWebFarmTask>(true);
            WebFarmHelper.RegisterTask<RemoveAuthenticatedUserWebFarmTask>(true);
        }
    }
}
