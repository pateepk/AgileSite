using System;

namespace CMS.Membership
{
    /// <summary>
    /// Provides an ITask interface to update database.
    /// </summary>
    public class RemoveExpiredSessions : IWorkerTask
    {
        /// <summary>
        /// Overload method which is executed by scheduler.
        /// </summary>
        public string Execute()
        {
            try
            {
                if (SessionManager.OnlineUsersEnabled)
                {
                    SessionManager.RemoveExpiredSessions(SessionManager.StoreOnlineUsersInDatabase, true);
                }

                return null;
            }

            catch (Exception ex)
            {
                return (ex.Message);
            }
        }
    }
}