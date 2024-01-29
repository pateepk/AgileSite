using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Interface for classes that are used to group staging tasks.
    /// </summary>
    public interface IStagingTaskGroup
    {
        /// <summary>
        /// Task group ID
        /// </summary>
        int TaskGroupID
        {
            get;
            set;
        }


        /// <summary>
        /// Task group description
        /// </summary>
        string TaskGroupDescription
        {
            get;
            set;
        }


        /// <summary>
        /// Task group code name
        /// </summary>
        string TaskGroupCodeName
        {
            get;
            set;
        }


        /// <summary>
        /// Task group guid
        /// </summary>
        Guid TaskGroupGuid
        {
            get;
            set;
        }
    }
}
