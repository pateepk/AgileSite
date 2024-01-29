using System;
using System.Linq;
using System.Text;

namespace CMS.Newsletters
{
    /// <summary>
    /// Subscription approval result.
    /// </summary>
    public enum ApprovalResult
    {
        /// <summary>
        /// Represents that subscription was found and successfully approved.
        /// </summary>
        Success = 0,

        /// <summary>
        /// Represents that subscription has failed.
        /// </summary>
        Failed = 1,

        /// <summary>
        /// Represents that subscription wasn't found.
        /// </summary>
        NotFound = 2,

        /// <summary>
        /// Represents that subscription interval for approving request has exceeded.
        /// </summary>
        TimeExceeded = 3,

        /// <summary>
        /// Represents that subscription is already approved.
        /// </summary>
        AlreadyApproved = 4
    }
}