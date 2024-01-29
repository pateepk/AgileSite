using System;
using System.Linq;
using System.Threading.Tasks;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.SiteProvider;

using LinqToTwitter;

namespace CMS.SocialMarketing
{
    /// <summary>
    /// Provides helper method for Twitter social network.
    /// </summary>
    public class TwitterHelper : AbstractHelper<TwitterHelper>
    {
        #region "Private variables"

        private static TwitterConfiguration mTwitterConfiguration = null;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets Twitter configuration. 
        /// </summary>
        public static TwitterConfiguration TwitterConfiguration
        {
            get
            {
                return mTwitterConfiguration ?? (mTwitterConfiguration = GetTwitterConfiguration());
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Publishes the new tweet on the Twitter channel and returns the Twitter's tweet ID.
        /// </summary>
        /// <param name="consumerKey">The consumer key that will be used for authentication.</param>
        /// <param name="consumerSecret">The consumer secret that will be used for authentication.</param>
        /// <param name="accessToken">The access token that will be used for authentication.</param>
        /// <param name="accessTokenSecret">The access token secret that will be used for authentication.</param>
        /// <param name="postText">Text that will be published.</param>
        /// <returns>Twitter identifier of the new tweet.</returns>
        public static string PublishTweet(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret, string postText)
        {
            return HelperObject.PublishTweetInternal(consumerKey, consumerSecret, accessToken, accessTokenSecret, postText);
        }


        /// <summary>
        /// Deletes tweet with given ID from Twitter.
        /// </summary>
        /// <param name="consumerKey">The consumer key that will be used for authentication.</param>
        /// <param name="consumerSecret">The consumer secret that will be used for authentication.</param>
        /// <param name="accessToken">The access token that will be used for authentication.</param>
        /// <param name="accessTokenSecret">The access token secret that will be used for authentication.</param>
        /// <param name="tweetId">Identifier of the tweet on Twitter.</param>
        public static void DeleteTweet(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret, string tweetId)
        {
            HelperObject.DeleteTweetInternal(consumerKey, consumerSecret, accessToken, accessTokenSecret, tweetId);
        }


        /// <summary>
        /// Retrieves the Twitter user identifier for the specified credentials, and returns it.
        /// </summary>
        /// <param name="consumerKey">The consumer key that will be used for authentication.</param>
        /// <param name="consumerSecret">The consumer secret that will be used for authentication.</param>
        /// <param name="accessToken">The access token that will be used for authentication.</param>
        /// <param name="accessTokenSecret">The access token secret that will be used for authentication.</param>
        /// <returns>The Twitter user identifier for the specified credentials.</returns>
        public static string GetTwitterUserId(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret)
        {
            return HelperObject.GetTwitterUserIdInternal(consumerKey, consumerSecret, accessToken, accessTokenSecret);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Publishes the new tweet on the Twitter channel and returns the Twitter's tweet ID.
        /// </summary>
        /// <param name="consumerKey">The consumer key that will be used for authentication.</param>
        /// <param name="consumerSecret">The consumer secret that will be used for authentication.</param>
        /// <param name="accessToken">The access token that will be used for authentication.</param>
        /// <param name="accessTokenSecret">The access token secret that will be used for authentication.</param>
        /// <param name="postText">Text that will be published.</param>
        /// <returns>Twitter identifier of the new tweet.</returns>
        protected virtual string PublishTweetInternal(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret, string postText)
        {
            PinAuthorizer authorizer = new PinAuthorizer
            {
                CredentialStore = new InMemoryCredentialStore
                {
                    ConsumerKey = consumerKey,
                    ConsumerSecret = consumerSecret,
                    OAuthTokenSecret = accessTokenSecret,
                    OAuthToken = accessToken
                }
            };

            using (TwitterContext context = new TwitterContext(authorizer))
            {
                Status status = context.TweetAsync(postText).Result;

                return status.StatusID.ToString();
            }
        }


        /// <summary>
        /// Deletes tweet with given ID from Twitter.
        /// </summary>
        /// <param name="consumerKey">The consumer key that will be used for authentication.</param>
        /// <param name="consumerSecret">The consumer secret that will be used for authentication.</param>
        /// <param name="accessToken">The access token that will be used for authentication.</param>
        /// <param name="accessTokenSecret">The access token secret that will be used for authentication.</param>
        /// <param name="tweetId">Identifier of the tweet on Twitter.</param>
        protected virtual void DeleteTweetInternal(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret, string tweetId)
        {
            PinAuthorizer authorizer = new PinAuthorizer
            {
                CredentialStore = new InMemoryCredentialStore
                {
                    ConsumerKey = consumerKey,
                    ConsumerSecret = consumerSecret,
                    OAuthTokenSecret = accessTokenSecret,
                    OAuthToken = accessToken
                }
            };

            DeleteTweetAsync(authorizer, UInt64.Parse(tweetId)).Wait();
        }


        /// <summary>
        /// Deletes tweet with given ID from Twitter.
        /// </summary>
        private async Task DeleteTweetAsync(PinAuthorizer authorizer, UInt64 tweetId)
        {
            using (TwitterContext context = new TwitterContext(authorizer))
            {
                try
                {
                    await context.DeleteTweetAsync(tweetId).ConfigureAwait(false);
                }
                catch (TwitterQueryException ex) when (IsPostNotFoundErrorCode(ex.ErrorCode))
                {
                    // Ignore not existing post
                }
            }
        }


        /// <summary>
        /// Retrieves the Twitter user identifier for the specified credentials, and returns it.
        /// </summary>
        /// <param name="consumerKey">The consumer key that will be used for authentication.</param>
        /// <param name="consumerSecret">The consumer secret that will be used for authentication.</param>
        /// <param name="accessToken">The access token that will be used for authentication.</param>
        /// <param name="accessTokenSecret">The access token secret that will be used for authentication.</param>
        /// <returns>The Twitter user identifier for the specified credentials.</returns>
        protected virtual string GetTwitterUserIdInternal(string consumerKey, string consumerSecret, string accessToken, string accessTokenSecret)
        {
            IAuthorizer authorizer = new SingleUserAuthorizer
            {
                CredentialStore = new SingleUserInMemoryCredentialStore
                {
                    ConsumerKey = consumerKey,
                    ConsumerSecret = consumerSecret,
                    AccessToken = accessToken,
                    AccessTokenSecret = accessTokenSecret
                }
            };
            TwitterContext context = new TwitterContext(authorizer);
            Account account = context.Account.Where(x => x.Type == AccountType.VerifyCredentials).Single();

            return account.User.UserIDResponse;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Gets Twitter configuration using Twitter API. If it is not possible to get configuration, default configuration will be used.
        /// </summary>
        /// <returns>Twitter configuration.</returns>
        private static TwitterConfiguration GetTwitterConfiguration()
        {
            var configuration = new TwitterConfiguration();

            if (LicenseKeyInfoProvider.IsFeatureAvailable(RequestContext.CurrentDomain, FeatureEnum.SocialMarketing))
            {
                foreach (var account in TwitterAccountInfoProvider.GetTwitterAccounts(SiteContext.CurrentSiteID))
                {
                    try
                    {
                        configuration = GetConfigurationFromTwitter(account);
                        break;
                    }
                    catch
                    {
                        // It is not possible to get configuration from Twitter API, try to use another Twitter account
                    }
                }   
            }

            return configuration;
        }


        /// <summary>
        /// Gets configuration using Twitter API with credentials of given Twitter account. If it is not possible to get configuration, exception is thrown.
        /// </summary>
        /// <param name="account">Twitter account with credentials that will be used when communication with API.</param>
        /// <returns>Actual Twitter configuration.</returns>
        private static TwitterConfiguration GetConfigurationFromTwitter(TwitterAccountInfo account)
        {
            var app = TwitterApplicationInfoProvider.GetTwitterApplicationInfo(account.TwitterAccountTwitterApplicationID);

            PinAuthorizer authorizer = new PinAuthorizer
            {
                CredentialStore = new InMemoryCredentialStore
                {
                    ConsumerKey = app.TwitterApplicationConsumerKey,
                    ConsumerSecret = app.TwitterApplicationConsumerSecret,
                    OAuthTokenSecret = account.TwitterAccountAccessTokenSecret,
                    OAuthToken = account.TwitterAccountAccessToken
                }
            };

            using (TwitterContext context = new TwitterContext(authorizer))
            {
                var helpResult = (from test in context.Help where test.Type == HelpType.Configuration select test).SingleOrDefault();
                Configuration c = helpResult.Configuration;

                TwitterConfiguration configuration = new TwitterConfiguration();
                configuration.ShortUrlLength = c.ShortUrlLength;
                configuration.ShortUrlLengthHttps = c.ShortUrlLengthHttps;

                return configuration;
            }
        }


        /// <summary>
        /// Indicates whether twitter post with given ID was not found based on response error code.
        /// </summary>
        /// <param name="errorCode">Error code returned by <see cref="TwitterQueryException"/></param>
        /// <remarks>This can occur when the user manually deletes the post on the twitter, instead of deleting it through CMS API.</remarks>
        /// <returns><c>true</c> when post not found; otherwise <c>false</c>.</returns>
        internal static bool IsPostNotFoundErrorCode(int errorCode)
        {
            return errorCode == 34 || errorCode == 144;
        }

        #endregion

    }
}
