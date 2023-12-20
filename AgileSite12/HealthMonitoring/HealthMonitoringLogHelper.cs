using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Core;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Base;

namespace CMS.HealthMonitoring
{
    /// <summary>
    /// Class that provides methods for logging to the counters from application or windows service.
    /// </summary>
    public class HealthMonitoringLogHelper
    {
        #region "Variables"

        private static readonly StringSafeDictionary<RegisteredCounter> mRegisteredCounters = new StringSafeDictionary<RegisteredCounter>();

        #endregion


        #region "Delegates and events

        /// <summary>
        /// Custom counter handler.
        /// </summary>
        /// <param name="counter">Counter definition</param>
        /// <returns>Performance counter</returns>
        public delegate IPerformanceCounter LogCustomCounterHandler(Counter counter);


        /// <summary>
        /// Event for custom counter.
        /// </summary>
        public static event LogCustomCounterHandler OnLogCustomCounter;

        #endregion


        #region "Methods"

        /// <summary>
        /// Registers the given counter update method
        /// </summary>
        /// <param name="key">Counter key</param>
        /// <param name="updateMethod">Update method</param>
        public static void RegisterCounter(string key, Action<Counter> updateMethod)
        {
            if (updateMethod == null)
            {
                throw new ArgumentNullException("updateMethod");
            }

            mRegisteredCounters[key] = new RegisteredCounter(updateMethod);
        }


        /// <summary>
        /// Registers the given counter update method
        /// </summary>
        /// <param name="key">Counter key</param>
        /// <param name="underlyingCounter">Performance counter containing up-to-date values.</param>
        public static void RegisterCounter(string key, IPerformanceCounter underlyingCounter)
        {
            if (underlyingCounter == null)
            {
                throw new ArgumentNullException("underlyingCounter");
            }

            mRegisteredCounters[key] = new RegisteredCounter(underlyingCounter);
        }


        /// <summary>
        /// Reads needed data from application and writes it to the counters.
        /// </summary>
        public static void LogApplicationCounters()
        {
            try
            {
                List<Counter> counters = null;

                // Get counters that are supported by application
                if (HealthMonitoringHelper.UseExternalService)
                {
                    counters = HealthMonitoringManager.Counters.Where(c => c.Enabled && !c.Error && !HealthMonitoringManager.IsSystemDatabaseCounter(c.Key)).ToList();
                }
                else
                {
                    // Get all enabled and no error counters
                    counters = HealthMonitoringManager.Counters.Where(c => c.Enabled && !c.Error).ToList();
                }

                LogCountersValues(counters);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("HealthMonitoringLogHelper", "LogApplicationCounters", ex);
            }
        }


        /// <summary>
        /// Logs data that are needed to get from the database.
        /// </summary>
        public static void LogServiceCounters()
        {
            try
            {
                // Get counter that needed data from database
                List<Counter> counters = HealthMonitoringManager.Counters.Where(c => c.Enabled && !c.Error && HealthMonitoringManager.IsSystemDatabaseCounter(c.Key)).ToList();

                LogCountersValues(counters);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("HealthMonitoringLogHelper", "LogServiceCounters", ex);
            }
        }


        /// <summary>
        /// Logs counters from web application.
        /// </summary>
        /// <param name="counterList">Counter list</param>
        private static void LogCountersValues(List<Counter> counterList)
        {
            foreach (Counter counter in counterList)
            {
                string counterKey = counter.Key;

                // Try to call update method from registered counters
                var registered = mRegisteredCounters[counterKey];
                if (registered != null)
                {
                    registered.Update(counter);
                }
                else if (OnLogCustomCounter != null)
                {
                    // Invoke event
                    counter.PerformanceCounter = OnLogCustomCounter(counter);
                }

                // Log values
                counter.Log();
            }

            // Get error counters
            List<Counter> errorCounters = counterList.Where(c => c.Error).ToList();
            if (errorCounters.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (Counter errorCounter in errorCounters)
                {
                    sb.AppendLine(errorCounter.LastErrorMessage);
                }

                sb.AppendLine("This error may be caused by performance counters not being registered in your system.");

                // Log error messages
                EventLogProvider.LogEvent(EventType.ERROR, "HealthMonitoringLogHelper", "LogCountersValues", sb.ToString(), RequestContext.RawURL);
            }
        }


        /// <summary>
        /// Clears application counters except the counters keeping permanent incremental values (cache related, ...)
        /// </summary>
        public static void ClearApplicationCounters()
        {
            CMSThread.RunningThreads.Clear();

            EventLogProvider.Warnings.Clear();
            EventLogProvider.Errors.Clear();

            CacheHelper.Removed.Clear();
            CacheHelper.Expired.Clear();
            CacheHelper.DependencyChanged.Clear();
            CacheHelper.Underused.Clear();

            RequestHelper.TotalPageRequests.Clear();
            RequestHelper.TotalSystemPageRequests.Clear();
            RequestHelper.TotalGetFileRequests.Clear();
            RequestHelper.TotalNonPageRequests.Clear();
            RequestHelper.TotalPageNotFoundRequests.Clear();
            RequestHelper.PendingRequests.Clear();
            RequestHelper.TotalPageRobotsTxtRequests.Clear();

            SqlHelper.RunningQueries.Clear();

            // Clear registered counters
            foreach (RegisteredCounter registered in mRegisteredCounters.TypedValues)
            {
                registered.Clear();
            }
        }

        #endregion
    }
}