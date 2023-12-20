using System;
using System.Collections.Generic;
using System.Text;

using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;

using Facebook;

namespace CMS.ExternalAuthentication
{
    /// <summary>
    /// Helper class providing methods for correct Facebook Connect initialization.
    /// </summary>
    public static class FacebookConnectHelper
    {
        #region "Methods"

        /// <summary>
        /// Indicates if Facebook Connect is enabled for specified site.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool GetFacebookEnabled(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSEnableFacebookConnect");
        }


        /// <summary>
        /// Returns api key.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static string GetFacebookApiKey(string siteName)
        {
            return SettingsKeyInfoProvider.GetValue(siteName + ".CMSFacebookConnectApiKey");
        }


        /// <summary>
        /// Returns application secret.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static string GetFacebookAppSecretKey(string siteName)
        {
            return SettingsKeyInfoProvider.GetValue(siteName + ".CMSFacebookApplicationSecret");
        }


        /// <summary>
        /// Indicates if Facebook Connect is available/enabled on specified site.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool FacebookIsAvailable(string siteName)
        {
            // Check all necessary settings values
            bool enabled = GetFacebookEnabled(siteName);

            string apiKey = GetFacebookApiKey(siteName);
            string secret = GetFacebookAppSecretKey(siteName);

            return enabled && !String.IsNullOrEmpty(apiKey) && !String.IsNullOrEmpty(secret);
        }


        /// <summary>
        /// Validate FB access token against https://graph.facebook.com
        /// </summary>
        /// <param name="accesstoken">Access token to validate</param>
        /// <param name="appSecret">App secret of the current site for providing better security.</param>
        /// <param name="facebookUserId">Returns Facebook user ID</param>
        public static bool ValidateFBAccessToken(string accesstoken, string appSecret, out string facebookUserId)
        {
            facebookUserId = null;
            if (String.IsNullOrEmpty(accesstoken))
            {
                return false;
            }

            try
            {
                FacebookClient client = new FacebookClient(accesstoken);

                var parameters = new
                {
                    fields = "id",
                    appsecret_proof = SecurityHelper.GetHMACSHA2Hash(accesstoken, Encoding.UTF8.GetBytes(appSecret))
                };

                IDictionary<string, object> response = client.Get("me", parameters) as IDictionary<string, object>;

                if (ValidationHelper.IsLong(response["id"]))
                {
                    facebookUserId = response["id"].ToString();
                    return true;
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("CMSFacebookConnectHelper.ValidateFBAccessToken", "EXCEPTION", ex);
            }

            return false;
        }

        #endregion
    }
}
