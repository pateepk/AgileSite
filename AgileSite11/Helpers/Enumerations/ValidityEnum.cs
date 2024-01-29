namespace CMS.Helpers
{
    /// <summary>
    /// Validity period enumeration.
    /// </summary>
    public enum ValidityEnum
    {
        /// <summary>
        /// Validity in days.
        /// </summary>
        Days = 0,

        /// <summary>
        /// Validity in weeks.
        /// </summary>
        Weeks = 1,

        /// <summary>
        /// Validity in months.
        /// </summary>
        Months = 2,

        /// <summary>
        /// Validity in years.
        /// </summary>
        Years = 3,

        /// <summary>
        /// Validity until specific date and time.
        /// </summary>
        Until = 4,
    }
}