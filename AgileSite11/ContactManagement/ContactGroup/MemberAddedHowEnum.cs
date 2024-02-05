using System;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Possible types of how can be contact added to contact group.
    /// </summary>
    public enum MemberAddedHowEnum
    {
        /// <summary>
        /// Contact is added through dynamic macro evaluation.
        /// </summary>
        Dynamic = 1,

        /// <summary>
        /// Contact is added through account which is member of contact group.
        /// </summary>
        Account = 2,

        /// <summary>
        /// Contact is manually added.
        /// </summary>
        Manual = 3
    }
}