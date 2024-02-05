namespace CMS.Base
{
    /// <summary>
    /// Default authentication service
    /// </summary>
    internal class DefaultAuthenticationService : IAuthenticationService
    {
        /// <summary>
        /// Currently authenticated user
        /// </summary>
        public IUserInfo CurrentUser
        {
            get
            {
                return null;
            }
        }


        /// <summary>
        /// Get user info of given user.
        /// </summary>
        /// <param name="userName">name of given user</param>
        /// <returns>user info of given user</returns>
        public IUserInfo GetUser(string userName)
        {
            return null;
        }
    }
}
