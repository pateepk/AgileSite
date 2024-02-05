namespace CMS.Protection
{
    /// <summary>
    /// Defines the bad word action.
    /// </summary>
    public enum BadWordActionEnum
    {
        /// <summary>
        /// No action is performed.
        /// </summary>
        None = 0,

        /// <summary>
        /// Removes bad word.
        /// </summary>
        Remove = 1,

        /// <summary>
        /// Replaces bad word with replacement.
        /// </summary>
        Replace = 2,

        /// <summary>
        /// Reports abuse.
        /// </summary>
        ReportAbuse = 3,

        /// <summary>
        /// Requests moderation.
        /// </summary>
        RequestModeration = 4,

        /// <summary>
        /// Denies bad word.
        /// </summary>
        Deny = 5
    }
}