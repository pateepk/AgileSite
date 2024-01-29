using CMS.Helpers;

namespace CMS.Search
{
    /// <summary>
    /// Search task type enum.
    /// </summary>
    public enum SearchTaskTypeEnum
    {
        /// <summary>
        /// Update.
        /// </summary>
        [EnumStringRepresentation("update")]
        Update = 0,

        /// <summary>
        /// Delete.
        /// </summary>
        [EnumStringRepresentation("delete")]
        Delete = 1,

        /// <summary>
        /// Rebuild.
        /// </summary>
        [EnumStringRepresentation("rebuild")]
        Rebuild = 2,

        /// <summary>
        /// Optimize.
        /// </summary>
        [EnumStringRepresentation("optimize")]
        Optimize = 3,

        /// <summary>
        /// Universal process task.
        /// </summary>
        [EnumStringRepresentation("process")]
        Process = 4
    }
}