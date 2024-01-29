using System;

using CMS.EventLog;
using CMS.Scheduler;

namespace CMS.Synchronization
{
    /// <summary>
    /// Provides an ITask interface for external integration task synchronization.
    /// </summary>
    public class IntegrationExternalTasksProcessor : ITask
    {
        #region "ITask Members"

        /// <summary>
        /// Executes external integration tasks synchronization.
        /// </summary>
        /// <param name="task">Scheduling task info object</param>
        /// <returns>Possible error</returns>
        public string Execute(Scheduler.TaskInfo task)
        {
            try
            {
                // Get connector name
                string connectorName = task.TaskData;
                if (!string.IsNullOrEmpty(connectorName))
                {
                    // Process pending tasks for specified connector
                    IntegrationHelper.ProcessExternalTasksAsync(connectorName);
                }
                else
                {
                    // Process pending tasks for all connectors
                    IntegrationHelper.ProcessExternalTasksAsync();
                }

                return null;
            }
            catch (Exception ex)
            {
                // Log the exception
                EventLogProvider.LogException("Integration", "EXCEPTION", ex);
                return ex.Message;
            }
        }

        #endregion
    }
}