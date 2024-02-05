using System;

using CMS.Base;
using CMS.EventLog;
using CMS.SiteProvider;

namespace CMS.ModuleUsageTracking
{
    /// <summary>
    /// Provides access to module usage counters that are used for feature usage tracking.
    /// </summary>
    class ModuleUsageCounter : IModuleUsageCounter
    {
        /// <summary>
        /// Gets current value of the specified counter.
        /// </summary>
        /// <param name="counterName">Name of the counter.</param>
        public long GetValue(string counterName)
        {
            var counter = ModuleUsageCounterInfoProvider.GetModuleUsageCounterInfo(counterName);
            return counter != null ? counter.ModuleUsageCounterValue : 0;
        }


        /// <summary>
        /// Increments value of the specified counter.
        /// </summary>
        /// <param name="counterName">Name of the counter.</param>
        public void Increment(string counterName)
        {
            try
            {
                ModuleUsageCounterInfoProvider.IncrementModuleUsageCounterInfo(counterName);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogWarning("ModuleUsageCounter", "INCREMENT", ex, SiteContext.CurrentSiteID, String.Format("Module usage counter '{0}' was not incremented.", counterName));
            }
        }


        /// <summary>
        /// Clears value of the specified counter.
        /// </summary>
        /// <param name="counterName">Name of the counter.</param>
        public void Clear(string counterName)
        {
            try
            { 
                ModuleUsageCounterInfoProvider.ClearModuleUsageCounterInfo(counterName);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogWarning("ModuleUsageCounter", "CLEAR", ex, SiteContext.CurrentSiteID, String.Format("Module usage counter '{0}' was not cleared.", counterName));
            }
        }
    }
}
