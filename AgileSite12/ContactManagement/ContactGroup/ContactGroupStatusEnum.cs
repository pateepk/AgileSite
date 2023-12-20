using System;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Types of contact group status used by ContactGroupInfo object.
    /// </summary>
    public enum ContactGroupStatusEnum
    {
        /// <summary>
        /// Unspecified
        /// </summary>
        Unspecified = -1,

        /// <summary>
        /// Ready
        /// </summary>
        Ready = 0,

        /// <summary>
        /// Rebuilding
        /// </summary>
        Rebuilding = 1,

        /// <summary>
        /// Condition changed - rebuild is required
        /// </summary>
        ConditionChanged = 2
    }
}