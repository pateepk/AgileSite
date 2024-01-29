namespace CMS.Membership
{
    /// <summary>
    /// Constants for system role names
    /// </summary>
    public class RoleName
    {
        #region "Generic roles constants"

        /// <summary>
        /// Special role which automatically covers all users.
        /// </summary>
        public const string EVERYONE = "_everyone_";


        /// <summary>
        /// Special role which automatically covers all authenticated users.
        /// </summary>
        public const string AUTHENTICATED = "_authenticated_";


        /// <summary>
        /// Special role which automatically covers all not authenticated users.
        /// </summary>
        public const string NOTAUTHENTICATED = "_notauthenticated_";

        #endregion
    }
}
