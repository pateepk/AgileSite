namespace CMS.ContinuousIntegration
{
    /// <summary>
    /// Log item action type used for <see cref="LogItem"/> object.
    /// </summary>
    public enum LogItemActionTypeEnum
    {
        /// <summary>
        /// Unknown action type
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Update action
        /// </summary>
        Update = 1,

        /// <summary>
        /// Delete action
        /// </summary>
        Delete = 2        
    }
}
