namespace CMS.Helpers
{
    /// <summary>
    /// Trailing slash enumeration.
    /// </summary>
    public enum TrailingSlashEnum
    {
        /// <summary>
        /// Proceed URLs with or without slash at the and.
        /// </summary>
        DontCare,

        /// <summary>
        /// Never proceed URLs with slash at the end.
        /// </summary>
        Never,

        /// <summary>
        /// Always proceed URLs with slash at the end.
        /// </summary>
        Always
    }
}