using System;

namespace CMS.Core
{
    /// <summary>
    /// Default performance counter implementation
    /// </summary>
    internal class DefaultPerformanceCounter : IPerformanceCounter
    {
        /// <summary>
        /// Increments global and site counter. If the parameter siteName is null, increments only global counter.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public void Increment(string siteName)
        {
        }


        /// <summary>
        /// Decrements global and site counter. If the parameter siteName is null, decrements only global counter.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public void Decrement(string siteName)
        {
        }


        /// <summary>
        /// Sets raw value of global or site counter. If the parameter siteName is null, sets global counter.
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="siteName">Site name</param>
        public void SetValue(long value, string siteName)
        {
        }


        /// <summary>
        /// Gets value of global or site counter. If the parameter siteName is null, get global counter value.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="updateLastLog">Indicates if date time of last log value should be updated.</param>
        public long GetValue(string siteName, bool updateLastLog = false)
        {
            return 0;
        }


        /// <summary>
        /// Clears global and sites counters.
        /// </summary>
        public void Clear()
        {
        }


        /// <summary>
        /// Clears last log date time.
        /// </summary>
        public void ClearLastLog()
        {
        }


        /// <summary>
        /// Gets time of last log.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public DateTime GetLastLog(string siteName)
        {
            return DateTime.Now;
        }


        /// <summary>
        /// Resets global and sites values.
        /// </summary>
        /// <param name="resetSitesValues">Indicates if sites counters should be reset.</param>
        public void Reset(bool resetSitesValues)
        {
        }
    }
}
