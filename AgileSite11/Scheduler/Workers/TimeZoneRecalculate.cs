using System;

using CMS.Base;
using CMS.Globalization;

namespace CMS.Scheduler
{
    /// <summary>
    /// Provides an ITask interface to recalculate DST time.
    /// </summary>
    public class TimeZoneRecalculate : ITask
    {
        /// <summary>
        /// Executes the publish action.
        /// </summary>
        /// <param name="task">Task to process</param>
        public string Execute(TaskInfo task)
        {
            try
            {
                // This task will run on each instance in the staging environment so there's no need to synchronize its changes 
                using (new CMSActionContext { LogSynchronization = false })
                {
                    TimeZoneInfoProvider.GenerateTimeZoneRules();
                }
            }
            catch (Exception ex)
            {
                return (ex.Message);
            }

            return String.Empty;
        }
    }
}