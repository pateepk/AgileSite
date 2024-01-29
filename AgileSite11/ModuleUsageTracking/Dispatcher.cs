using System;
using System.Linq;
using System.Text;
using System.Threading;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Scheduler;

namespace CMS.ModuleUsageTracking
{
    /// <summary>
    /// Class used by scheduler to execute module usage tracking task.
    /// </summary>
    public class Dispatcher : ITask
    {
        internal const string USAGE_TRACKING_ENABLED_KEY = "CMSModuleUsageTrackingEnabled";


        /// <summary>
        /// Sends modules statistical data.
        /// </summary>
        /// <param name="task">Task data</param>
        public string Execute(TaskInfo task)
        {
            // Execute task only if enabled
            if (SettingsKeyInfoProvider.GetBoolValue(USAGE_TRACKING_ENABLED_KEY))
            {
                // Disable logging on production environment. This module should not bother customers with logs.
                using (new CMSActionContext { LogEvents = SystemContext.DevelopmentMode, AllowAsyncActions = true })
                {
                    // Start async processing
                    var action = new CMSThread(ProcessData, new ThreadSettings { Priority = ThreadPriority.Lowest, CreateLog = true});
                    action.Start();
               } 
            }

            return String.Empty;
        }


        /// <summary>
        /// Executes module usage data processing.
        /// </summary>
        private void ProcessData()
        {
            try
            {
                // Send module usage data to server
                ModuleUsageProvider.ProcessData();
            }
            catch (Exception ex)
            {
                CoreServices.EventLog.LogException(ModuleUsageTrackingModule.MODULE_USAGE_EVENTS_SOURCE, "SENDDATA", ex);
            }
        }
    }
}
