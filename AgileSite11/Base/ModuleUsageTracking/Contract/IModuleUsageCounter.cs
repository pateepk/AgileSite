namespace CMS.Base
{
    /// <summary>
    /// Provides access to module usage counters that are used for feature usage tracking.
    /// </summary>
    public interface IModuleUsageCounter
    {
        /// <summary>
        /// Gets current value of the specified counter.
        /// </summary>
        /// <param name="counterName">Name of the counter.</param>
        long GetValue(string counterName);


        /// <summary>
        /// Increments value of the specified counter.
        /// </summary>
        /// <param name="counterName">Name of the counter.</param>
        void Increment(string counterName);


        /// <summary>
        /// Clears value of the specified counter.
        /// </summary>
        /// <param name="counterName">Name of the counter.</param>
        void Clear(string counterName);
    }
}
