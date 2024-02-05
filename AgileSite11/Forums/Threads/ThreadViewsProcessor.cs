using System;

using CMS.EventLog;
using CMS.Scheduler;

namespace CMS.Forums
{
    /// <summary>
    /// Provides an ITask interface for automatic thread views processing (saving the data to DB).
    /// </summary>
    public class ThreadViewsProcessor : ITask
    {
        /// <summary>
        /// Executes the save action.
        /// </summary>
        /// <param name="task">Task to process</param>
        public string Execute(TaskInfo task)
        {
            try
            {
                ThreadViewCounter.SaveViewCounts();
                return null;
            }
            catch (Exception e)
            {
                // Log the exception
                EventLogProvider.LogException("ThreadViewsProcessor", "EXCEPTION", e);

                return e.Message;
            }
        }
    }
}