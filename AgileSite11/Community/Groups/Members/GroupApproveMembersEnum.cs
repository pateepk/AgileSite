namespace CMS.Community
{
    /// <summary>
    /// Group members approve enumeration.
    /// </summary>
    public enum GroupApproveMembersEnum : int
    {
        /// <summary>
        /// Any site member can join.
        /// </summary>
        AnyoneCanJoin = 0,

        /// <summary>
        /// Only approved members can join.
        /// </summary>
        ApprovedCanJoin = 1,

        /// <summary>
        /// Invited members can join without approval.
        /// </summary>
        InvitedWithoutApproval = 2
    }
}