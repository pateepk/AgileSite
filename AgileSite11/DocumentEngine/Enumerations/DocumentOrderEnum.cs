namespace CMS.DocumentEngine
{
    /// <summary>
    /// Document order enumeration (for new document).
    /// </summary>
    public enum DocumentOrderEnum : int
    {
        /// <summary>
        /// Unknown order.
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// Alphabetical order.
        /// </summary>
        Alphabetical = 0,

        /// <summary>
        /// First document.
        /// </summary>
        First = 1,

        /// <summary>
        /// Last document.
        /// </summary>
        Last = 2
    }
}