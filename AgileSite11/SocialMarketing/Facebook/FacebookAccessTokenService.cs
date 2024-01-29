using System;
using System.ComponentModel;
using System.Linq;
using System.Text;

using CMS;
using CMS.SocialMarketing.Internal;

using Facebook;

[assembly: RegisterImplementation(typeof(IFacebookAccessTokenService), typeof(FacebookAccessTokenService), Priority = CMS.Core.RegistrationPriority.SystemDefault)]

namespace CMS.SocialMarketing.Internal
{
    /// <summary>
    /// Provides method for getting Facebook access token. This service is for internal use only.
    /// </summary>
    /// <exclude />
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Class was not intended for public use and will be removed in the next version.")]
    public sealed class FacebookAccessTokenService : IFacebookAccessTokenService
    {
        /// <summary>
        /// Gets Facebook access token for given <paramref name="applicationID"/> and <paramref name="applicationSecret"/>.
        /// </summary>
        /// <param name="applicationID">Application ID provided by the Facebook</param>
        /// <param name="applicationSecret">Application secret provided by the Facebook</param>
        /// <exception cref="ArgumentNullException"><paramref name="applicationID"/> is null -or- <paramref name="applicationSecret"/> is null</exception>
        /// <returns>Access token authorizing </returns>
        public string GetAccessToken(string applicationID, string applicationSecret)
        {
            if (applicationID == null)
            {
                throw new ArgumentNullException("applicationID");
            }

            if (applicationSecret == null)
            {
                throw new ArgumentNullException("applicationSecret");
            }
            
            var facebookClient = new FacebookClient();
            dynamic result = facebookClient.Get("oauth/access_token", new
            {
                client_id = applicationID,
                client_secret = applicationSecret,
                grant_type = "client_credentials"
            });
            
            return result.access_token;
        }
    }
}
