namespace CMS.Helpers
{
    /// <summary>
    /// Type of TextPart match against other string enumeration.
    /// </summary>
    public enum ComparisonStatus : int
    {
        /// <summary>
        /// Unknown status, TextPart was processed yet.
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// No match was found.
        /// </summary>
        NoMatch = 0,

        /// <summary>
        /// Match found successfuly.
        /// </summary>
        Match = 1
    }
}