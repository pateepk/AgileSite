namespace CMS.Community
{
    /// <summary>
    /// Memebr status enumeration.
    /// </summary>
    public enum GroupMemberStatus
    {
        /// <summary>
        /// Member is approved.
        /// </summary>
        Approved = 0,

        /// <summary>
        /// Member is rejected.
        /// </summary>
        Rejected = 1,

        /// <summary>
        /// Member waiting for approval.
        /// </summary>
        WaitingForApproval = 2,

        /// <summary>
        /// Member is group admin, this status is read only.
        /// </summary>
        GroupAdmin = 3
    }
}