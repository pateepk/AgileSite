using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

using CMS.Base;
using CMS.EventLog;
using CMS.Helpers;

using Facebook;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CMS.SocialMarketing
{
    /// <summary>
    /// Helper methods for working with Facebook social network.
    /// </summary>
    public class FacebookHelper : AbstractHelper<FacebookHelper>
    {
        #region "Private constants"

        /// <summary>
        /// Uri of Facebook Graph API without trailing slash.
        /// </summary>
        private const string GRAPH_API = "https://graph.facebook.com/";


        /// <summary>
        /// Insights for single Facebook object ID URL.
        /// Pass ID as parameter 0.
        /// </summary>
        private const string GRAPH_API_INSIGHTS = GRAPH_API + "{0}/insights/{1}";


        /// <summary>
        /// Insights for multiple Facebook object IDs URL.
        /// Pass comma separated list of IDs as parameter 0.
        /// </summary>
        private const string GRAPH_API_INSIGHTS_MULTIPLE_ID = GRAPH_API + "insights/{1}?ids={0}";


        /// <summary>
        /// Comments summary for multiple Facebook object IDs URL.
        /// Pass comma separated list of IDs as parameter 0.
        /// </summary>
        private const string GRAPH_API_COMMENTS_SUMMARY_MULTIPLE_ID = GRAPH_API + "comments?ids={0}&summary=1&limit=1";


        /// <summary>
        /// Likes summary for multiple Facebook object IDs URL.
        /// Pass comma separated list of IDs as parameter 0.
        /// </summary>
        private const string GRAPH_API_LIKES_SUMMARY_MULTIPLE_ID = GRAPH_API + "likes?ids={0}&summary=1&limit=1";


        /// <summary>
        /// Multiple Facebook objects URL.
        /// Pass comma separated list of IDs as parameter 0.
        /// </summary>
        private const string GRAPH_API_OBJECT_MULTIPLE_ID = GRAPH_API + "?ids={0}";


        /// <summary>
        /// Session key for page access token.
        /// </summary>
        public const string PAGE_ACCESS_TOKEN_SESSION_KEY = "CMSFacebookPageAccessToken";

        #endregion


        #region "Public constants"

        /// <summary>
        /// URL for getting access token.
        /// </summary>
        public const string OAUTH_ACCESS_TOKEN_URL = GRAPH_API + "oauth/access_token?client_id={0}&redirect_uri={1}&client_secret={2}&code={3}";


        /// <summary>
        /// URL for user authorization.
        /// </summary>
        public const string OAUTH_AUTHORIZE_URL = GRAPH_API + "oauth/authorize?client_id={0}&redirect_uri={1}&scope=manage_pages,publish_pages,read_insights";

        #endregion


        #region "public methods"

        /// <summary>
        /// Publishes the new post on the Facebook page and returns the Facebook post ID.
        /// </summary>
        /// <param name="pageId">Facebook's page identifier. Post will be published on that page.</param>
        /// <param name="pageAccessToken">Access token that will be used for publishing on the page.</param>
        /// <param name="postText">Text of the post that will be published.</param>
        /// <param name="appSecret">Facebook application secret key for improving security.</param>
        /// <returns>Facebook identifier of the new post.</returns>
        /// <exception cref="FacebookApiException">FacebookApiException is thrown when an error occurs while communicating with Facebook.</exception>
        public static string PublishPostOnFacebookPage(string pageId, string pageAccessToken, string postText, string appSecret)
        {
            return HelperObject.PublishPostOnFacebookPageInternal(pageId, pageAccessToken, postText, appSecret);
        }


        /// <summary>
        /// Deletes the post on the Facebook page.
        /// </summary>
        /// <param name="postId">Facebook (external) identifier of the post.</param>
        /// <param name="pageAccessToken">Access token that will be used for deleting the post.</param>
        /// <param name="appSecret">Facebook application secret key for improving security.</param>
        /// <exception cref="FacebookApiException">FacebookApiException is thrown when an error occurs while communicating with Facebook.</exception>
        /// <exception cref="Exception">Exception is thrown if the post could not be deleted.</exception>
        public static void DeletePostOnFacebookPage(string postId, string pageAccessToken, string appSecret)
        {
            HelperObject.DeletePostOnFacebookPageInternal(postId, pageAccessToken, appSecret);
        }


        /// <summary>
        /// Parses an OAuth access token in JSON format to <see cref="OAuthAccessToken"/>.
        /// </summary>
        /// <param name="oAuthAccessToken">OAuth access token JSON string.</param>
        /// <returns>Parsed token.</returns>
        public static OAuthAccessToken ParseOAuthAccessToken(string oAuthAccessToken)
        {
            return HelperObject.ParseOAuthAccessTokenInternal(oAuthAccessToken);
        }


        /// <summary>
        /// <para>
        /// Downloads account info from https://graph.facebook.com/me/accounts/ and finds page access token in it.
        /// </para>
        /// <para>
        /// Returns null if page access token could not be retrieved.
        /// </para>
        /// </summary>
        /// <param name="accessToken">Application access token acquired via login bounce.</param>
        /// <param name="pageId">Facebook page identifier to find page access token for.</param>
        /// <param name="appSecret">Facebook application secret key for improving security.</param>
        public static string GetPageAccessToken(string accessToken, string pageId, string appSecret)
        {
            return HelperObject.GetPageAccessTokenInternal(accessToken, pageId, appSecret);
        }

        #endregion


        #region "Internal interface"

        /// <summary>
        /// Retrieves Facebook Insights for object with given Facebook object ID.
        /// </summary>
        /// <param name="objectId">Facebook object ID.</param>
        /// <param name="metrics">Metrics to be included in result data.</param>
        /// <param name="accessToken">Access token for Facebook object.</param>
        /// <param name="appSecret">Facebook application secret key for improving security.</param>
        /// <returns>Facebook Graph API response.</returns>
        /// <exception cref="FacebookApiException">FacebookApiException is thrown when an error occurs while communicating with Facebook.</exception>
        internal static dynamic RetrieveFacebookInsights(string objectId, IEnumerable<string> metrics, string accessToken, string appSecret)
        {
            string requestUri = String.Format(GRAPH_API_INSIGHTS, objectId, metrics.Join(","));
            return HelperObject.GetFacebookResponse(requestUri, accessToken, appSecret);
        }


        /// <summary>
        /// Retrieves Facebook Insights for objects with given Facebook object IDs.
        /// </summary>
        /// <param name="ids">Facebook object IDs.</param>
        /// <param name="metrics">Metrics to be retrieved.</param>
        /// <param name="accessToken">Access token for Facebook objects.</param>
        /// <param name="appSecret">Facebook application secret key for improving security.</param>
        /// <returns>Facebook Graph API response.</returns>
        /// <exception cref="FacebookApiException">FacebookApiException is thrown when an error occurs while communicating with Facebook.</exception>
        internal static dynamic RetrieveFacebookInsights(IEnumerable<string> ids, IEnumerable<string> metrics, string accessToken, string appSecret)
        {
            string requestUri = String.Format(GRAPH_API_INSIGHTS_MULTIPLE_ID, ids.Join(","), metrics.Join(","));
            return HelperObject.GetFacebookResponse(requestUri, accessToken, appSecret);
        }


        /// <summary>
        /// Retrieves Facebook Comments for objects with given Facebook object IDs.
        /// </summary>
        /// <param name="ids">Facebook object IDs.</param>
        /// <param name="accessToken">Access token for Facebook objects.</param>
        /// <param name="appSecret">Facebook application secret key for improving security.</param>
        /// <returns>Facebook Graph API response.</returns>
        /// <exception cref="FacebookApiException">FacebookApiException is thrown when an error occurs while communicating with Facebook.</exception>
        internal static dynamic RetrieveFacebookCommentsSummary(IEnumerable<string> ids, string accessToken, string appSecret)
        {
            string requestUri = String.Format(GRAPH_API_COMMENTS_SUMMARY_MULTIPLE_ID, ids.Join(","));
            return HelperObject.GetFacebookResponse(requestUri, accessToken, appSecret);
        }


        /// <summary>
        /// Retrieves Facebook Likes for objects with given Facebook object IDs.
        /// </summary>
        /// <param name="ids">Facebook object IDs.</param>
        /// <param name="accessToken">Access token for Facebook objects.</param>
        /// <param name="appSecret">Facebook application secret key for improving security.</param>
        /// <returns>Facebook Graph API response.</returns>
        /// <exception cref="FacebookApiException">FacebookApiException is thrown when an error occurs while communicating with Facebook.</exception>
        internal static dynamic RetrieveFacebookLikesSummary(IEnumerable<string> ids, string accessToken, string appSecret)
        {
            string requestUri = String.Format(GRAPH_API_LIKES_SUMMARY_MULTIPLE_ID, ids.Join(","));
            return HelperObject.GetFacebookResponse(requestUri, accessToken, appSecret);
        }


        /// <summary>
        /// Retrieves Facebook objects with given IDs.
        /// </summary>
        /// <param name="ids">Facebook object IDs.</param>
        /// <param name="accessToken">Access token for Facebook objects.</param>
        /// <param name="appSecret">Facebook application secret key for improving security.</param>
        /// <returns>Facebook Graph API response.</returns>
        /// <exception cref="FacebookApiException">FacebookApiException is thrown when an error occurs while communicating with Facebook.</exception>
        internal static dynamic RetrieveFacebookObjects(IEnumerable<string> ids, string accessToken, string appSecret)
        {
            string requestUri = String.Format(GRAPH_API_OBJECT_MULTIPLE_ID, ids.Join(","));
            return HelperObject.GetFacebookResponse(requestUri, accessToken, appSecret);
        }

        #endregion


        #region "Internal implementation"

        /// <summary>
        /// Publishes the new post on the Facebook page and returns the Facebook post ID.
        /// </summary>
        /// <param name="pageId">Facebook's page identifier. Post will be published on that page.</param>
        /// <param name="pageAccessToken">Access token that will be used for publishing on the page.</param>
        /// <param name="postText">Text of the post that will be published.</param>
        /// <param name="appSecret">Facebook application secret key for improving security.</param>
        /// <returns>Facebook identifier of the new post.</returns>
        /// <exception cref="FacebookApiException">FacebookApiException is thrown when an error occurs while communicating with Facebook.</exception>
        protected virtual string PublishPostOnFacebookPageInternal(string pageId, string pageAccessToken, string postText, string appSecret)
        {
            var client = new FacebookClient(pageAccessToken);

            var parameters = new 
            {
                message = postText,
                appsecret_proof = SecurityHelper.GetHMACSHA2Hash(pageAccessToken, Encoding.UTF8.GetBytes(appSecret))
            };

            IDictionary<string, object> response = (IDictionary<string, object>)client.Post($"/{pageId}/feed", parameters);

            return (string)response["id"];
        }


        /// <summary>
        /// Deletes the post on the Facebook page.
        /// </summary>
        /// <param name="postId">Facebook (external) identifier of the post.</param>
        /// <param name="pageAccessToken">Access token that will be used for deleting the post.</param>
        /// <param name="appSecret">Facebook application secret key for improving security.</param>
        /// <exception cref="FacebookApiException">FacebookApiException is thrown when an error occurs while communicating with Facebook.</exception>
        /// <exception cref="Exception">Exception is thrown if the post could not be deleted.</exception>
        protected virtual void DeletePostOnFacebookPageInternal(string postId, string pageAccessToken, string appSecret)
        {
            FacebookClient client = new FacebookClient(pageAccessToken);
            bool deleted = false;

            try
            {
                byte[] keyByte = Encoding.UTF8.GetBytes(appSecret);
                var fbResponse = client.Delete(postId, new { appsecret_proof = SecurityHelper.GetHMACSHA2Hash(pageAccessToken, keyByte) });

                if (!Boolean.TryParse(fbResponse.ToString(), out deleted))
                {
                    // The response is a JsonObject (API v2.1).
                    deleted = HasRequestSucceeded((JsonObject)fbResponse);
                }
            }
            catch (FacebookApiException ex)
            {
                // Error code 100 means invalid argument - the post does not exist or has been already deleted - this error is ignored.
                if (ex.ErrorCode != 100)
                {
                    throw;
                }
                deleted = true;
            }

            if (!deleted)
            {
                throw new Exception("[FacebookHelper.DeletePostOnFacebookPage]: Post was not deleted.");
            }
        }


        /// <summary>
        /// Parses an OAuth access token in JSON format to <see cref="OAuthAccessToken"/>.
        /// </summary>
        /// <param name="oAuthAccessToken">OAuth access token JSON string.</param>
        /// <returns>Parsed token.</returns>
        protected virtual OAuthAccessToken ParseOAuthAccessTokenInternal(string oAuthAccessToken)
        {
            return JsonConvert.DeserializeObject<OAuthAccessToken>(oAuthAccessToken);
        }


        /// <summary>
        /// <para>
        /// Downloads account info from https://graph.facebook.com/me/accounts/ and finds page access token in it.
        /// </para>
        /// <para>
        /// Returns null if page access token could not be retrieved.
        /// </para>
        /// </summary>
        /// <param name="accessToken">Application access token acquired via login bounce.</param>
        /// <param name="pageId">Facebook page identifier to find page access token for.</param>
        /// <param name="appSecret">Facebook application secret key for improving security.</param>
        protected virtual string GetPageAccessTokenInternal(string accessToken, string pageId, string appSecret)
        {
            try
            {
                string appSecretHash = SecurityHelper.GetHMACSHA2Hash(accessToken, Encoding.UTF8.GetBytes(appSecret));

                FacebookClient client = new FacebookClient(accessToken);
                var accountsInfo = client.Get("me/accounts", new { appsecret_proof = appSecretHash }).ToString();
                
                // Get page access token
                string pageAccessToken = HelperObject.GetPageAccessTokenFromAccountsInfo(accountsInfo, pageId);

                if (!String.IsNullOrEmpty(pageAccessToken))
                {
                    return pageAccessToken;
                }
                else
                {
                    Exception e = new Exception("[FacebookProvider.GetPageAccessToken]: Could not find page access token in accounts info. Most likely missing permissions to list it properly.");
                    EventLogProvider.LogException("FacebookProvider", "Access token initialization", e);
                }
            }
            catch (WebException e)
            {
                // Log exception
                EventLogProvider.LogException("FacebookAccessToken", "PageToken", e);
            }

            return null;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Gets Facebook Graph API response for given URL and access token.
        /// </summary>
        /// <param name="url">URL for GET request.</param>
        /// <param name="accessToken">Access token for request.</param>
        /// <param name="appSecret">Facebook application secret key for improving security.</param>
        /// <returns>Facebook Graph API response.</returns>
        private dynamic GetFacebookResponse(string url, string accessToken, string appSecret)
        {
            FacebookClient client = new FacebookClient(accessToken);
            var key = Encoding.UTF8.GetBytes(appSecret);

            return client.Get(url, new { appsecret_proof = SecurityHelper.GetHMACSHA2Hash(accessToken, key) });
        }


        /// <summary>
        /// Finds access token for page account in account information stored in serialized array of json objects.
        /// </summary>
        /// <param name="json">Account info listed from https://graph.facebook.com/me/accounts</param>
        /// <param name="pageId">Facebook page identifier</param>
        private string GetPageAccessTokenFromAccountsInfo(string json, string pageId)
        {
            // Parse JSON to retrieve error details
            var responseJson = JObject.Parse(json);
            var data = responseJson["data"];

            // Try to get page access token from acount info
            foreach (var item in data)
            {
                if (item["id"].ToString() == pageId)
                {
                    return item["access_token"].ToString();
                }
            }

            // If execution comes here no token for given pageId was found
            return null;
        }

        /// <summary>
        /// Checks whether a Facebook request succeeded. The Facebook response is in JSON format. 
        /// </summary>
        /// <param name="response">Facebook response</param>
        private static bool HasRequestSucceeded(JsonObject response)
        {
            string json = response.ToString();

            // Parse JSON to retrieve value of success token.
            JObject responseJson = JObject.Parse(json);
            JToken successToken = responseJson.SelectToken("success");

            if (successToken != null)
            {
                return (bool)successToken;
            }

            return false;
        }

        #endregion

    }

}