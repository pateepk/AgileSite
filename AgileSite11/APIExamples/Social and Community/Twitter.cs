using System;

using CMS.SocialMarketing;
using CMS.SiteProvider;
using CMS.Base;

namespace APIExamples
{
    /// <summary>
    /// Holds Twitter API examples.
    /// </summary>
    /// <pageTitle>Twitter</pageTitle>
    internal class TwitterMain
    {
        /// <summary>
        /// Holds Twitter app API examples.
        /// </summary>
        /// <groupHeading>Twitter apps</groupHeading>
        private class TwitterApps
        {
            /// <heading>Creating a Twitter app</heading>
            private void CreateTwitterApp()
            {
                // Specifies the app's credentials
                // Replace the values with the credentials for your own configured Twitter app
                // See the documentation for creating a new Twitter app: http://kentico.com/CMSPages/DocLinkMapper.ashx?version=latest&link=sm_connecting
                string consumerKey = "yourConsumerKey";
                string consumerSecret = "yourConsumerSecret";

                // Creates a new Twitter app object
                TwitterApplicationInfo app = new TwitterApplicationInfo();

                // Sets the properties of the app
                app.TwitterApplicationDisplayName = "New Twitter app";
                app.TwitterApplicationName = "NewTwitterApp";

                app.TwitterApplicationConsumerKey = consumerKey;
                app.TwitterApplicationConsumerSecret = consumerSecret;

                app.TwitterApplicationSiteID = SiteContext.CurrentSiteID;

                // Saves the Twitter app to the database
                TwitterApplicationInfoProvider.SetTwitterApplicationInfo(app);
            }


            /// <heading>Updating a Twitter app</heading>
            private void GetAndUpdateTwitterApp()
            {
                // Gets the app
                TwitterApplicationInfo app = TwitterApplicationInfoProvider.GetTwitterApplicationInfo("NewTwitterApp", SiteContext.CurrentSiteName);

                if (app != null)
                {
                    // Updates the app
                    app.TwitterApplicationDisplayName = app.TwitterApplicationDisplayName.ToLowerCSafe();

                    // Saves the modified app to the database
                    TwitterApplicationInfoProvider.SetTwitterApplicationInfo(app);
                }
            }


            /// <heading>Deleting a Twitter app</heading>
            private void DeleteTwitterApp()
            {
                // Gets the app
                TwitterApplicationInfo app = TwitterApplicationInfoProvider.GetTwitterApplicationInfo("NewTwitterApp", SiteContext.CurrentSiteName);

                if (app != null)
                {
                    // Deletes the app
                    TwitterApplicationInfoProvider.DeleteTwitterApplicationInfo(app);
                }
            }
        }

        /// <summary>
        /// Holds Twitter channel API examples.
        /// </summary>
        /// <groupHeading>Twitter channels</groupHeading>
        private class TwitterChannel
        {
            /// <heading>Creating a Twitter channel</heading>
            private void CreateTwitterChannel()
            {
                // Specifies the properties for accessing a Twitter channel
                // Replace the values with the access credentials for your own configured Twitter channel
                string accessToken = "yourAccessToken";
                string accessTokenSecret = "yourAccessTokenSecret";
                
                // Gets the app to which the channel is tied
                TwitterApplicationInfo app = TwitterApplicationInfoProvider.GetTwitterApplicationInfo("NewTwitterApp", SiteContext.CurrentSiteName);

                if (app == null)
                {
                    throw new Exception("[ApiExamples.CreateTwitterChannel]: Application 'NewTwitterApp' was not found.");
                }

                // Creates a new channel object
                TwitterAccountInfo channel = new TwitterAccountInfo();

                // Sets the properties for the channel
                channel.TwitterAccountDisplayName = "New Twitter channel";
                channel.TwitterAccountName = "NewTwitterChannel";

                channel.TwitterAccountAccessToken = accessToken;
                channel.TwitterAccountAccessTokenSecret = accessTokenSecret;
                channel.TwitterAccountSiteID = SiteContext.CurrentSiteID;
                channel.TwitterAccountTwitterApplicationID = app.TwitterApplicationID;

                // Saves the channel to the database
                TwitterAccountInfoProvider.SetTwitterAccountInfo(channel);
            }


            /// <heading>Updating a Twitter channel</heading>
            private void GetAndUpdateTwitterChannel()
            {
                // Gets the channel
                TwitterAccountInfo channel = TwitterAccountInfoProvider.GetTwitterAccountInfo("NewTwitterChannel", SiteContext.CurrentSiteID);

                if (channel != null)
                {
                    // Updates the properties of the channel
                    channel.TwitterAccountDisplayName = channel.TwitterAccountDisplayName.ToLowerCSafe();

                    // Saves the changes to the databse
                    TwitterAccountInfoProvider.SetTwitterAccountInfo(channel);
                }
            }


            /// <heading>Deleting a Twitter channel</heading>
            private void DeleteTwitterChannel()
            {
                // Gets the channel
                TwitterAccountInfo channel = TwitterAccountInfoProvider.GetTwitterAccountInfo("NewTwitterChannel", SiteContext.CurrentSiteID);

                if (channel != null)
                {
                    // Deletes the channel
                    TwitterAccountInfoProvider.DeleteTwitterAccountInfo(channel);
                }
            }
        }


        /// <summary>
        /// Holds Twitter post API examples.
        /// </summary>
        /// <groupHeading>Twitter posts</groupHeading>
        private class TwitterPosts
        {
            /// <heading>Creating a Tweet</heading>
            private void CreateTweet()
            {
                // Gets the channel to which the tweet is tied
                TwitterAccountInfo channel = TwitterAccountInfoProvider.GetTwitterAccountInfo("NewTwitterChannel", SiteContext.CurrentSiteID);

                if (channel == null)
                {
                    throw new Exception("[ApiExamples.CreateTwitterPost]: Account 'NewTwitterChannel' was not found.");
                }

                // Creates a new Twitter post object
                TwitterPostInfo tweet = new TwitterPostInfo();

                // Sets the properties for the tweet
                tweet.TwitterPostTwitterAccountID = channel.TwitterAccountID;
                tweet.TwitterPostSiteID = SiteContext.CurrentSiteID;
                tweet.TwitterPostText = "Sample tweet text.";

                // Specifies the publish time for the tweet
                tweet.TwitterPostScheduledPublishDateTime = DateTime.Now + TimeSpan.FromMinutes(5);

                // Saves the tweet to the database
                TwitterPostInfoProvider.SetTwitterPostInfo(tweet);
            }


            /// <heading>Updating a Tweet</heading>
            private void GetAndUpdateTwitterPost()
            {
                // Gets the channel to which the tweet is tied
                TwitterAccountInfo channel = TwitterAccountInfoProvider.GetTwitterAccountInfo("NewTwitterChannel", SiteContext.CurrentSiteID);

                if (channel == null)
                {
                    throw new Exception("[ApiExamples.GetAndUpdateTwitterPost]: Account 'NewTwitterChannel' was not found.");
                }

                // Gets the first tweet tied to the channel
                TwitterPostInfo tweet = TwitterPostInfoProvider.GetTwitterPostInfoByAccountId(channel.TwitterAccountID).FirstObject;

                if (tweet == null)
                {
                    throw new Exception("[ApiExamples.GetAndUpdateTwitterPost]: There are no posts tied to 'NewTwitterChannel'.");
                }

                // Updates the text of the tweet
                tweet.TwitterPostText = tweet.TwitterPostText + " Edited.";

                // Saves the changed tweet to the database
                TwitterPostInfoProvider.SetTwitterPostInfo(tweet);
            }


            /// <heading>Deleting Tweets</heading>
            private void DeleteTweets()
            {
                // Gets the channel to which the tweets are tied
                TwitterAccountInfo channel = TwitterAccountInfoProvider.GetTwitterAccountInfo("NewTwitterChannel", SiteContext.CurrentSiteID);

                if (channel == null)
                {
                    throw new Exception("[ApiExamples.DeleteTwitterPosts]: Account 'NewTwitterChannel' has not been found.");
                }

                // Gets all tweets tied to the channel
                var tweets = TwitterPostInfoProvider.GetTwitterPostInfoByAccountId(channel.TwitterAccountID);

                // Deletes the tweets from Kentico and from Twitter
                foreach (TwitterPostInfo tweet in tweets)
                {
                    TwitterPostInfoProvider.DeleteTwitterPostInfo(tweet);
                }
            }


            /// <heading>Publishing a Tweet</heading>
            private void PublishTweetToTwitter()
            {
                // Gets the channel to which the tweet is tied
                TwitterAccountInfo channel = TwitterAccountInfoProvider.GetTwitterAccountInfo("NewTwitterChannel", SiteContext.CurrentSiteID);

                if (channel == null)
                {
                    throw new Exception("[ApiExamples.PublishTweetToTwitter]: Account 'NewTwitterChannel' was not found.");
                }

                // Gets the first tweet tied to the channel
                TwitterPostInfo tweet = TwitterPostInfoProvider.GetTwitterPostInfoByAccountId(channel.TwitterAccountID).FirstObject;

                if (tweet == null)
                {
                    throw new Exception("[ApiExamples.PublishTweetToTwitter]: There are no posts tied to 'NewTwitterChannel'.");
                }

                // Publishes the tweet. The tweet is scheduled for publishing if its TwitterPostScheduledPublishDateTime value is set in the future.
                TwitterPostInfoProvider.PublishTwitterPost(tweet.TwitterPostID);
            }
        }
    }
}
