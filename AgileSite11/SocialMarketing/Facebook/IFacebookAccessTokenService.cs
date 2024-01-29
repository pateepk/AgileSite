using System;
using System.ComponentModel;

namespace CMS.SocialMarketing.Internal
{
    /// <summary>
    /// Provides method for getting Facebook access token. This service is for internal use only.
    /// </summary>
    /// <exclude />
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Interface was not intended for public use and will be removed in the next version.")]
    public interface IFacebookAccessTokenService
    {
        /// <summary>
        /// Gets Facebook access token for given <paramref name="applicationID"/> and <paramref name="applicationSecret"/>.
        /// </summary>
        /// <param name="applicationID">Application ID provided by the Facebook</param>
        /// <param name="applicationSecret">Application secret provided by the Facebook</param>
        /// <returns>Access token authorizing </returns>
        string GetAccessToken(string applicationID, string applicationSecret);
    }
}