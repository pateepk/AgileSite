using System;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.IO;

namespace CMS.ExternalAuthentication
{
    /// <summary>
    /// Helper class providing methods for correct Facebook Connect initialization.
    /// </summary>
    public static class FacebookConnectHelper
    {
        #region "Internal classes and Enums

        /// <summary>
        /// Internal class for parsing Facebook response (JSON)
        /// </summary>
        private class FBID
        {
            public string id
            {
                get;
                set;
            }
        }
        
        #endregion


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
        /// Returns content of the given response.
        /// </summary>
        /// <param name="response">Response</param>
        /// <param name="maxLength">Max length to read.</param>
        private static string GetResponseContent(HttpWebResponse response, int maxLength)
        {
            if (response == null)
            {
                return null;
            }

            using (StreamReader stream = StreamReader.New(response.GetResponseStream()))
            {
                char[] buffer = new char[maxLength];
                int count = stream.Read(buffer, 0, maxLength);
                if (count > 0)
                {
                    return new String(buffer, 0, count);
                }
            }
            return null;
        }


        /// <summary>
        /// <para>
        /// Validate FB access token against https://graph.facebook.com
        /// </para>
        /// <para>
        /// This member was added in hotfix 8 (version 11.0.8). Do not use this member when writing hotfix independent code.
        /// </para>
        /// </summary>
        /// <param name="accesstoken">Access token to validate</param>
        /// <param name="appSecret">Facebook application secret key for improving security.</param>
        /// <param name="facebookUserId">Returns Facebook user ID</param>
        public static bool ValidateFBAccessToken(string accesstoken, string appSecret, out string facebookUserId)
        {
            facebookUserId = null;
            if (String.IsNullOrEmpty(accesstoken))
            {
                return false;
            }

            string url = String.Format("https://graph.facebook.com/me?fields=id&access_token={0}&appsecret_proof={1}", 
                accesstoken,
                SecurityHelper.GetHMACSHA2Hash(accesstoken, Encoding.UTF8.GetBytes(appSecret)));
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            string responseString;

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    responseString = GetResponseContent(response, 8192);
                }
            }
            catch
            {
                return false;
            }

            try
            {
                // Parse Facebook user ID from server response (JSON)
                JavaScriptSerializer ser = new JavaScriptSerializer();
                FBID fbid = ser.Deserialize<FBID>(responseString);
                if ((fbid != null) && ValidationHelper.IsLong(fbid.id))
                {
                    facebookUserId = fbid.id;
                    return true;
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("CMSFacebookConnectHelper.ValidateFBAccessToken", "EXCEPTION", ex);
            }

            return false;
        }


        /// <summary>
        /// Validate FB access token against https://graph.facebook.com
        /// </summary>
        /// <param name="accesstoken">Access token to validate</param>
        /// <param name="facebookUserId">Returns Facebook user ID</param>
        public static bool ValidateFBAccessToken(string accesstoken, out string facebookUserId)
        {
            return ValidateFBAccessToken(accesstoken, String.Empty, out facebookUserId);
        }

        #endregion
    }
}
