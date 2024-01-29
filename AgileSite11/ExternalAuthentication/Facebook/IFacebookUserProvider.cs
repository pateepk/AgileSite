using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.ExternalAuthentication.Facebook
{
    /// <summary>
    /// Provider allows you to get information about Facebook user.
    /// </summary>
    public interface IFacebookUserProvider
    {
        /// <summary>
        /// Gets information about Facebook user. 
        /// </summary>
        /// <param name="facebookUserId">Facebook user's ID.</param>
        /// <returns>Facebook user data class</returns>
        FacebookUserProfile GetFacebookUser(string facebookUserId);


        /// <summary>
        /// Gets information about Facebook user. 
        /// </summary>
        /// <param name="facebookUserId">Facebook user's ID.</param>
        /// <param name="accessToken">User's access token.</param>
        /// <returns>Facebook user data class</returns>
        FacebookUserProfile GetFacebookUser(string facebookUserId, string accessToken);
    }
}
