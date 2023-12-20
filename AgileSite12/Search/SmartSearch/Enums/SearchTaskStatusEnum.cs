using CMS.Helpers;

namespace CMS.Search
{
    /// <summary>
    /// Search task status enumeration.
    /// </summary>
    public enum SearchTaskStatusEnum
    {

        /// <summary>
        /// Ready status.
        /// </summary>
        [EnumStringRepresentation("ready")]
        Ready = 0,


        /// <summary>
        /// In progress status.
        /// </summary>
        [EnumStringRepresentation("inprogress")]
        InProgress = 1,


        /// <summary>
        /// Error status
        /// </summary>
        [EnumStringRepresentation("error")]
        Error = 2

    }
}