namespace CMS.Membership
{
    /// <summary>
    /// Web farm tasks for membership operations
    /// </summary>
    public class MembershipTaskType
    {
        /// <summary>
        /// Updates (inserts) avatar.
        /// </summary>
        public const string UpdateAvatar = "UPDATEAVATAR";

        /// <summary>
        /// Avatar delete operation.
        /// </summary>
        public const string DeleteAvatar = "DELETEAVATAR";
    }
}
