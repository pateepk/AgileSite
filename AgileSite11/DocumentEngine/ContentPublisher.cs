using System;

using CMS.EventLog;
using CMS.Scheduler;
using CMS.SiteProvider;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Provides an ITask interface for the content publishing.
    /// </summary>
    public class ContentPublisher : ITask
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
                return null;
            }

            try
            {
                // Get site info
                SiteInfo si = SiteInfoProvider.GetSiteInfo(task.TaskSiteID);
                if (si == null)
                {
                    return "Task site not found.";
                }

                TreeProvider tree = new TreeProvider();
                tree.UpdateUser = false;
                VersionManager vm = VersionManager.GetInstance(tree);
                // Publish documents
                vm.PublishAllScheduled(si.SiteName);
                return null;
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