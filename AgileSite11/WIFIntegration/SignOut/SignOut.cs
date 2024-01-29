using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Web;

using CMS.Helpers;
using CMS.Membership;

namespace CMS.WIFIntegration
{
    /// <summary>
    /// Class that handles the operations for WIF sign-out.
    /// </summary>
    internal class SignOut : AbstractWIFAuthentication
    {
        #region "Constants"

        /// <summary>
        /// Query parameter name sent when signing out from CMS on request from identity provider.
        /// </summary>
        private const string SIGN_OUT_REQUEST_QUERY_NAME = "wa";


        /// <summary>
        /// Query parameter value sent when signing out from CMS on request from identity provider.
        /// </summary>
        private const string SIGN_OUT_REQUEST_QUERY_VALUE = "wsignoutcleanup1.0";

        #endregion


        #region "Public methods"

        /// <summary>
        /// Sign Out user from identity provider.
        /// </summary>
        public void RequestSignOut()
        {
            // Check if module enabled
            if (!Settings.Enabled || WIFContext.RequestIsSignOut)
            {
                return;
            }

            // Set HTTP Referrer header
            SetReferrer();
            
            // Redirect to identity provider
            var signOutUrl = GetSignOutUrl(Settings.IdentityProviderURL, Settings.Realm, URLHelper.GetApplicationUrl());
            URLHelper.ResponseRedirect(signOutUrl);
        }


        /// <summary>
        /// Signs out user on request from identity provider.
        /// </summary>
        public void ProcessSignOutRequest()
        {
            if (Settings.Enabled && IsSignOutRequest())
            {
                WIFContext.RequestIsSignOut = true;
                AuthenticationHelper.SignOut();
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Whether request should sign out user.
        /// </summary>
        private bool IsSignOutRequest()
        {
            return QueryHelper.GetString(SIGN_OUT_REQUEST_QUERY_NAME, "") == SIGN_OUT_REQUEST_QUERY_VALUE;
        }

        #endregion
    }
}