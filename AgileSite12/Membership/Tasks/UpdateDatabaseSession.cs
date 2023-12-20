using System;

using CMS.Helpers;
using CMS.Scheduler;

namespace CMS.Membership
{
    /// <summary>
    /// Provides an ITask interface to remove expired sessions form hashtables and database.
    /// </summary>
    public class UpdateDatabaseSession : IWorkerTask
    {
        /// <summary>
        /// Overload method which is executed by scheduler.
        /// </summary>
        public string Execute()
        {
            try
            {
                // If session management with db repository is used
                if (SessionManager.OnlineUsersEnabled && SessionManager.StoreOnlineUsersInDatabase)
                {
                    SessionManager.UpdateDatabaseSession();

                    if (!SchedulingHelper.RunSchedulerWithinRequest)
                    {
                        WebFarmHelper.CreateTask(new UpdateDatabaseSessionWebFarmTask());
                    }
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