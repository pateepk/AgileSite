using System;
using System.Collections.Concurrent;
using System.Threading;

using CMS.Core;

namespace CMS.HealthMonitoring
{
    /// <summary>
    /// Performance counter.
    /// </summary>
    internal sealed class CMSPerformanceCounter : IPerformanceCounter
    {
        private long globalCounter = 0;
        private readonly ConcurrentDictionary<string, long> sitesCounters = new ConcurrentDictionary<string, long>();
        private readonly ConcurrentDictionary<string, DateTime> lastLogs = new ConcurrentDictionary<string, DateTime>();
        private readonly object locker = new object();
        private DateTime lastLog = DateTime.MinValue;


        /// <summary>
        /// Increments global and site counter. If the parameter siteName is null, increments only global counter.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public void Increment(string siteName)
        {
            // Global counter
            Interlocked.Increment(ref globalCounter);

            // Site counter
            if (!string.IsNullOrEmpty(siteName))
            {
                sitesCounters.AddOrUpdate(siteName, 1, (key, value) => value + 1);
            }
        }


        /// <summary>
        /// Decrements global and site counter. If the parameter siteName is null, decrements only global counter.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public void Decrement(string siteName)
        {
            // Global counter
            Interlocked.Decrement(ref globalCounter);

            // Site counter
            if (!string.IsNullOrEmpty(siteName))
            {
                sitesCounters.AddOrUpdate(siteName, 0, (key, value) => value - 1);
            }
        }


        /// <summary>
        /// Sets raw value of global or site counter. If the parameter siteName is null, sets global counter.
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="siteName">Site name</param>
        public void SetValue(long value, string siteName)
        {
            // Global counter
            if (String.IsNullOrEmpty(siteName))
            {
                globalCounter = value;
                return;
            }

            // Site counter
            sitesCounters[siteName] = value;
        }


        /// <summary>
        /// Gets value of global or site counter. If the parameter siteName is null, get global counter value.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="updateLastLog">Indicates if date time of last log value should be updated.</param>
        public long GetValue(string siteName, bool updateLastLog = false)
        {
            if (updateLastLog)
            {
                lastLog = DateTime.Now;
            }

            // Global counter
            if (string.IsNullOrEmpty(siteName))
            {
                return Interlocked.Read(ref globalCounter);
            }

            lock (locker)
            {
                if (updateLastLog)
                {
                    // Add last log time
                    lastLogs[siteName] = DateTime.Now;
                }

                // Site counter
                long counter;
                return sitesCounters.TryGetValue(siteName, out counter) ? counter : 0;
            }
        }


        /// <summary>
        /// Gets time of last log.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public DateTime GetLastLog(string siteName)
        {
            // Global counter
            if (string.IsNullOrEmpty(siteName))
            {
                return lastLog;
            }

            // Site counter
            DateTime result;
            return lastLogs.TryGetValue(siteName, out result)
                ? result
                : DateTime.MinValue;
        }


        /// <summary>
        /// Clears global and sites counters.
        /// </summary>
        public void Clear()
        {
            globalCounter = 0;
            sitesCounters.Clear();
        }


        /// <summary>
        /// Clears last log date time.
        /// </summary>
        public void ClearLastLog()
        {
            // Global
            lastLog = DateTime.MinValue;

            // Sites
            foreach (string siteName in sitesCounters.Keys)
            {
                lastLogs[siteName] = DateTime.MinValue;
            }
        }


        /// <summary>
        /// Resets global and sites values.
        /// </summary>
        /// <param name="resetSitesValues">Indicates if sites counters should be reset.</param>
        public void Reset(bool resetSitesValues)
        {
            globalCounter = 0;

            if (resetSitesValues)
            {
                foreach (string siteName in sitesCounters.Keys)
                {
                    SetValue(0, siteName);
                }
            }
        }
    }
}