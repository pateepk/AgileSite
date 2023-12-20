using CMS.Core;
using CMS.Scheduler;

namespace CMS.Membership
{
    /// <summary>
    /// Web farm task used to get sessions from hashtables and updates them into the database.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    /// <seealso cref="SessionDatabaseUpdater.UpdateItems"/>
    public class UpdateDatabaseSessionWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Processes the web farm task by invoking the <see cref="SessionManager.UpdateDatabaseSession"/> method.
        /// </summary>
        public override void ExecuteTask()
        {
            if (!SchedulingHelper.RunSchedulerWithinRequest && SessionManager.OnlineUsersEnabled && SessionManager.StoreOnlineUsersInDatabase)
            {
                SessionManager.UpdateDatabaseSession();
            }
        }
    }
}
