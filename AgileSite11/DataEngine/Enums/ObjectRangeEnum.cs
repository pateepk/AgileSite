namespace CMS.DataEngine
{
    /// <summary>
    /// Enum representing object range (in the scale of site vs. global object).
    /// </summary>
    public enum ObjectRangeEnum
    {
        /// <summary>
        /// No objects included by default
        /// </summary>
        None = 0,

        /// <summary>
        /// All objects (both, site and global) included by default
        /// </summary>
        All = 1,

        /// <summary>
        /// Site objects only included by default
        /// </summary>
        Site = 2,

        /// <summary>
        /// Global objects only included by default
        /// </summary>
        Global = 3,

        /// <summary>
        /// Default scope
        /// </summary>
        Default = 4
    }
}