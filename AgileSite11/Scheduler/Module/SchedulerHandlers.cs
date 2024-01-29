using System;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.Helpers;

namespace CMS.Scheduler
{
    /// <summary>
    /// Event handlers for the scheduler module
    /// </summary>
    internal class SchedulerHandlers
    {
        /// <summary>
        /// Initializes the handlers
        /// </summary>
        public static void Init()
        {
            if (SystemContext.IsCMSRunningAsMainApplication)
            {
                ApplicationEvents.PostStart.Execute += ReInitCorruptedTasks;
                RequestEvents.RunEndRequestTasks.Execute += RunEndRequestScheduler;
            }
        }


        /// <summary>
        /// Runs the scheduler in the end request mode
        /// </summary>
        private static void RunEndRequestScheduler(object sender, EventArgs eventArgs)
        {
            if (!RequestHelper.IsAsyncPostback())
            {
                // Attempt to run the scheduler
                RunEndRequestScheduler();
            }
        }


        /// <summary>
        /// Attempts to run the scheduler request.
        /// </summary>
        private static void RunEndRequestScheduler()
        {
            // Scheduler is disabled
            if (!SchedulingHelper.EnableScheduler)
            {
                return;
            }

            // Process scheduler only on content or system pages
            if (RequestContext.IsContentPage || (RequestContext.CurrentStatus == RequestStatusEnum.SystemPage))
            {
                // Run scheduler
                SchedulingHelper.RunEndRequestScheduler();
            }
        }


        /// <summary>
        /// Re-init all scheduled task which are corrupted.
        /// </summary>
        private static void ReInitCorruptedTasks(object sender, EventArgs eventArgs)
        {
            // Re-initialize tasks which were stopped by application end
            var debugs = DebugHelper.DisableSchedulerDebug();

            SchedulingExecutor.ReInitCorruptedTasks();

            DebugHelper.RestoreDebugSettings(debugs);
        }
    }
}
