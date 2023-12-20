using CMS.Base;
using CMS.DataEngine;

namespace CMS.Membership
{
    /// <summary>
    /// Authentication service
    /// </summary>
    internal class AuthenticationService : IAuthenticationService
    {
        /// <summary>
        /// Currently authenticated user
        /// </summary>
        public IUserInfo CurrentUser
        {
            get
            {
                if (!DatabaseHelper.IsDatabaseAvailable)
                {
                    return null;
                }

                return MembershipContext.AuthenticatedUser;
            }
        }


        /// <summary>
        /// Get user info of given user.
        /// </summary>
        /// <param name="userName">name of given user</param>
        /// <returns>user info of given user</returns>
        public IUserInfo GetUser(string userName)
        {
            return UserInfoProvider.GetUserInfo(userName);
        }
    }
}
