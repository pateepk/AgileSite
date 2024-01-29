namespace CMS.Membership
{
    /// <summary>
    /// Web farm tasks for session operations
    /// </summary>
    public class SessionTaskType
    {
        /// <summary>
        /// Adds user to the kicked users
        /// </summary>
        public const string AddUserToKicked = "ADDUSERTOKICKED";

        /// <summary>
        /// Removes users from the kicked users 
        /// </summary>
        public const string RemoveUserFromKicked = "REMOVEUSERFROMKICKED";

        /// <summary>
        /// Removes the user session
        /// </summary>
        public const string RemoveUser = "REMOVEUSER";

        /// <summary>
        /// Removes the authenticated user session
        /// </summary>
        public const string RemoveAuthenticatedUser = "REMOVEAUTHENTICATEDUSER";

        /// <summary>
        /// Updates the database session
        /// </summary>
        public const string UpdateDatabaseSession = "UPDATEDATABASESESSION";
    }
}
