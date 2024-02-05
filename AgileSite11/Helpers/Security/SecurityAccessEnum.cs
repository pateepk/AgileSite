namespace CMS.Helpers
{
    /// <summary>
    /// General access enumeration.
    /// </summary>
    public enum SecurityAccessEnum : int
    {
        /// <summary>
        /// All users.
        /// </summary>
        AllUsers = 0,

        /// <summary>
        /// Authenticated users.
        /// </summary>
        AuthenticatedUsers = 1,

        /// <summary>
        /// Authorized roles.
        /// </summary>
        AuthorizedRoles = 2,

        /// <summary>
        /// Group members.
        /// </summary>
        GroupMembers = 3,

        /// <summary>
        /// Nobody.
        /// </summary>
        Nobody = 4,

        /// <summary>
        /// Owner.
        /// </summary>
        Owner = 5,

        /// <summary>
        /// Admnin of the current group.
        /// </summary>
        GroupAdmin = 6,

        /// <summary>
        /// Global administrator.
        /// </summary>
        GlobalAdmin = 7
    }
}