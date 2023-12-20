using System;
using System.Data;

using CMS.EventLog;
using CMS.Scheduler;
using CMS.SiteProvider;

namespace CMS.Synchronization
{
    /// <summary>
    /// Automatic staging worker task.
    /// </summary>
    public class StagingWorker : ITask
    {
        /// <summary>
        /// Executes the publish action.
        /// </summary>
        /// <param name="task">Task to process</param>
        public string Execute(TaskInfo task)
        {
            // Only execute site specific tasks
            if ((task == null) || (task.TaskSiteID <= 0))
            {
                return "Only site-specific task is allowed to run content synchronization.";
            }

            try
            {
                // Get site info
                SiteInfo si = SiteInfoProvider.GetSiteInfo(task.TaskSiteID);
                if (si != null)
                {
                    // Get the tasks
                    DataSet ds = StagingTaskInfoProvider.SelectDocumentTaskList(si.SiteID, 0, "/", null, null, 0, "TaskID");
                    new StagingTaskRunner().RunSynchronization(ds);

                    return null;
                }
                else
                {
                    return "Task site not found.";
                }
            }
            catch (Exception e)
            {
                // Log the exception
                EventLogProvider.LogException("Content", "EXCEPTION", e);

                return e.Message;
            }
        }
    }
}