using System;

namespace CMS.Core
{
    /// <summary>
    /// Interface for a performance counter
    /// </summary>
    public interface IPerformanceCounter
    {
        /// <summary>
        /// Increments global and site counter. If the parameter siteName is null, increments only global counter.
        /// </summary>
        /// <param name="siteName">Site name</param>
        void Increment(string siteName);
        

        /// <summary>
        /// Decrements global and site counter. If the parameter siteName is null, decrements only global counter.
        /// </summary>
        /// <param name="siteName">Site name</param>
        void Decrement(string siteName);


        /// <summary>
        /// Sets raw value of global or site counter. If the parameter siteName is null, sets global counter.
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="siteName">Site name</param>
        void SetValue(long value, string siteName);


        /// <summary>
        /// Gets value of global or site counter. If the parameter siteName is null, get global counter value.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="updateLastLog">Indicates if date time of last log value should be updated.</param>
        long GetValue(string siteName, bool updateLastLog = false);
        

        /// <summary>
        /// Clears global and sites counters.
        /// </summary>
        void Clear();


        /// <summary>
        /// Clears last log date time.
        /// </summary>
        void ClearLastLog();


        /// <summary>
        /// Gets time of last log.
        /// </summary>
        /// <param name="siteName">Site name</param>
        DateTime GetLastLog(string siteName);


        /// <summary>
        /// Resets global and sites values.
        /// </summary>
        /// <param name="resetSitesValues">Indicates if sites counters should be reset.</param>
        void Reset(bool resetSitesValues);
    }
}
