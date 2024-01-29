namespace CMS.Base
{
    /// <summary>
    /// Provides access to module usage counters that are used for feature usage tracking.
    /// </summary>
    /// <remarks>
    /// This is default empty implementation which doesn't store any value in the database. 
    /// Module usage tracking module registers its own implementation of the interface when installed.
    /// </remarks>
    internal class DefaultModuleUsageCounter : IModuleUsageCounter
    {
        /// <summary>
        /// Gets current value of the specified counter.
        /// </summary>
        /// <param name="counterName">Name of the counter.</param>
        public long GetValue(string counterName)
        {
            return 0;
        }


        /// <summary>
        /// Increments value of the specified counter.
        /// </summary>
        /// <param name="counterName">Name of the counter.</param>
        public void Increment(string counterName)
        {
        }


        /// <summary>
        /// Clears value of the specified counter.
        /// </summary>
        /// <param name="counterName">Name of the counter.</param>
        public void Clear(string counterName)
        {
        }
    }
}
