using CMS.Helpers;

namespace CMS.Search
{
    /// <summary>
    /// Enumeration for index statuses.
    /// </summary>
    public enum IndexStatusEnum
    {
        /// <summary>
        /// Unknown index status.
        /// </summary>
        [EnumStringRepresentation("UNKNOWN")]
        UNKNOWN = -1,

        /// <summary>
        /// New index status.
        /// </summary>
        [EnumDefaultValue]
        [EnumStringRepresentation("NEW")]
        NEW = 0,

        /// <summary>
        /// Ready index/optimize status.
        /// </summary>
        [EnumStringRepresentation("READY")]
        READY = 1,

        /// <summary>
        /// Rebuilding index status.
        /// </summary>
        [EnumStringRepresentation("REBUILDING")]
        REBUILDING = 2,

        /// <summary>
        /// Error status.
        /// </summary>
        [EnumStringRepresentation("ERROR")]
        ERROR = 3,

        /// <summary>
        /// Optimizing status.
        /// </summary>
        [EnumStringRepresentation("OPTIMIZING")]
        OPTIMIZING = 4
    }
}