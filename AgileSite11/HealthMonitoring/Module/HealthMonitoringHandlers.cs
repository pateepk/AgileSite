using System;
using System.Linq;
using System.Text;

using CMS.Base;

namespace CMS.HealthMonitoring
{
    /// <summary>
    /// Event handlers for health monitoring
    /// </summary>
    internal class HealthMonitoringHandlers
    {
        /// <summary>
        /// Initializes the event handlers
        /// </summary>
        public static void InitHandlers()
        {
            if (SystemContext.IsCMSRunningAsMainApplication)
            {
                RequestEvents.RunEndRequestTasks.Execute += EnsurePerformanceCounterTimer;
            }
        }


        /// <summary>
        /// Ensures the performance counter timer for health monitoring
        /// </summary>
        private static void EnsurePerformanceCounterTimer(object sender, EventArgs e)
        {
            // Run performance timer
            HealthMonitoringHelper.EnsurePerformanceCounterTimer();
        }
    }
}
