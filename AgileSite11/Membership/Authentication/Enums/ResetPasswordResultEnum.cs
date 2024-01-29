namespace CMS.Membership
{
    /// <summary>
    /// Reset password enumeration.
    /// </summary>
    public enum ResetPasswordResultEnum
    {
        /// <summary>
        /// Success.
        /// </summary>
        Success = 0,

        /// <summary>
        /// Time exceeded.
        /// </summary>
        TimeExceeded = 1,

        /// <summary>
        /// Validation failed.
        /// </summary>
        ValidationFailed = 2
    }
}