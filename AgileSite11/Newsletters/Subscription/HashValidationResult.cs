namespace CMS.Newsletters
{
    /// <summary>
    /// Hash validation result.
    /// </summary>
    public enum HashValidationResult
    {
        /// <summary>
        /// Represents that hash was successfully approved.
        /// </summary>
        Success = 0,

        /// <summary>
        /// Hash did not pass security validation.
        /// </summary>
        Failed = 1,

        /// <summary>
        /// Subscription wasn't found.
        /// </summary>
        NotFound = 2,

        /// <summary>
        /// Interval for hash validation has exceeded.
        /// </summary>
        TimeExceeded = 3,
    }
}