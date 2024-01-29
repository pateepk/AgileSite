using System.Threading;
using System.Web;

using CMS.Base;
using CMS.Helpers;
using CMS.Routing.Web;
using CMS.Scheduler;
using CMS.SiteProvider;

[assembly: RegisterHttpHandler("CMSPages/Scheduler.ashx", typeof(SchedulingHandler), Order = 1)]

namespace CMS.Scheduler
{
    /// <summary>
    /// Handler for creating end executing new scheduling tasks.
    /// </summary>
    public class SchedulingHandler : IHttpHandler
    {
        /// <summary>
        /// Creates and executes new scheduler thread.
        /// </summary>
        public void ProcessRequest(HttpContext context)
        {
            if (!DebugHelper.DebugScheduler)
            {
                // Disable the debugging
                DebugHelper.DisableDebug();
            }

            context.Response.Cache.SetNoStore();
            
            // Run the tasks
            SchedulingExecutorParameters schedulingParams = new SchedulingExecutorParameters { SiteName = SiteContext.CurrentSiteName, ServerName = WebFarmHelper.ServerName };
            ThreadStart threadStartObj = new ThreadStart(schedulingParams.ExecuteScheduledTasks);
            
            // Create synchronous thread
            CMSThread schedulerThread = new CMSThread(threadStartObj, true, ThreadModeEnum.Sync);
            schedulerThread.Start();

            // Checking constant
            context.Response.Write(SchedulingHelper.SCHEDULER_PING_CONTENT);
        }


        /// <summary>
        /// Gets whether this handler can be reused for other request; always returns <c>false</c>.
        /// </summary>
        /// <value>Always <c>false</c></value>
        public bool IsReusable => false;
    }
}
