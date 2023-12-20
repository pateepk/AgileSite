using CMS;
using CMS.Base;

[assembly: RegisterImplementation(typeof(IAuthenticationService), typeof(DefaultAuthenticationService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Base
{
    /// <summary>
    /// Interface for authentication service
    /// </summary>
    public interface IAuthenticationService
    {
        /// <summary>
        /// Currently authenticated user
        /// </summary>
        IUserInfo CurrentUser
        {
            get;
        }


        /// <summary>
        /// Get user info of given user.
        /// </summary>
        /// <param name="userName">name of given user</param>
        /// <returns>user info of given user</returns>
        IUserInfo GetUser(string userName);
    }
}
