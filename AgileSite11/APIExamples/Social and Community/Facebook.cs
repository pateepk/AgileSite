using System;

using CMS.SocialMarketing;
using CMS.SiteProvider;
using CMS.Base;

namespace APIExamples
{
    /// <summary>
    /// Holds Facebook API examples.
    /// </summary>
    /// <pageTitle>Facebook</pageTitle>
    internal class FacebookMain
    {
        /// <summary>
        /// Holds Facebook app API examples.
        /// </summary>
        /// <groupHeading>Facebook apps</groupHeading>
        private class FacebookApps
        {
            /// <heading>Creating a Facebook app</heading>
            private void CreateFacebookApp()
            {
                // Specifies the app's credentials
                // Replace the values with the App ID and App Secret for your own Facebook app
                // See the documentation for creating a new Facebook app: http://kentico.com/CMSPages/DocLinkMapper.ashx?version=latest&link=sm_connecting
                string appId = "yourAppID";
                string appSecret = "yourAppSecret";

                // Creates a new Facebook app object
                FacebookApplicationInfo newApp = new FacebookApplicationInfo();

                // Sets the properties of the Facebook app
                newApp.FacebookApplicationDisplayName = "New app";
                newApp.FacebookApplicationName = "NewApp";
                newApp.FacebookApplicationSiteID = SiteContext.CurrentSiteID;
                newApp.FacebookApplicationConsumerKey = appId;
                newApp.FacebookApplicationConsumerSecret = appSecret;

                // Saves the Facebook app to the database
                FacebookApplicationInfoProvider.SetFacebookApplicationInfo(newApp);
            }

            /// <heading>Updating a Facebook app</heading>
            private void GetAndUpdateFacebookApp()
            {
                // Gets the Facebook app
                FacebookApplicationInfo app = FacebookApplicationInfoProvider.GetFacebookApplicationInfo("NewApp", SiteContext.CurrentSiteID);

                if (app != null)
                {
                    // Updates the properties of the app
                    app.FacebookApplicationDisplayName = app.FacebookApplicationDisplayName.ToLowerCSafe();

                    // Saves the modified app to the database
                    FacebookApplicationInfoProvider.SetFacebookApplicationInfo(app);
                }
            }


            /// <heading>Deleting a Facebook app</heading>
            private void DeleteFacebookApp()
            {
                // Gets the Facebook app
                FacebookApplicationInfo app = FacebookApplicationInfoProvider.GetFacebookApplicationInfo("NewApp", SiteContext.CurrentSiteID);

                if (app != null)
                {
                    // Deletes the Facebook app from the database
                    FacebookApplicationInfoProvider.DeleteFacebookApplicationInfo(app);
                }
            }
        }


        /// <summary>
        /// Holds Facebook page API examples.
        /// </summary>
        /// <groupHeading>Facebook pages</groupHeading>
        private class FacebookPages
        {
            /// <heading>Creating a Facebook page</heading>
            private void CreateFacebookPage()
            {
                // Specifies the properties for accessing the page
                // Replace the values with the page URL and page access token from your own Facebook page
                // To find a page access token, visit https://developers.facebook.com/tools/explorer and select your app in the list
                // See the Facebook documentation for details https://developers.facebook.com/docs/pages/access-tokens
                string pageUrl = "yourPageURL";
                string pageAccessToken = "yourPageAccessToken";

                // Gets the ID of the app you want to associate with the page
                FacebookApplicationInfo app = FacebookApplicationInfoProvider.GetFacebookApplicationInfo("NewApp", SiteContext.CurrentSiteID);

                int appId = app.FacebookApplicationID;

                // Creates a new Facebook page object
                FacebookAccountInfo page = new FacebookAccountInfo();

                // Sets the properties for the page
                page.FacebookAccountDisplayName = "New page";
                page.FacebookAccountName = "NewPage";
                page.FacebookAccountSiteID = SiteContext.CurrentSiteID;
                page.FacebookAccountFacebookApplicationID = appId;

                // Uses FacebookHelper to get the ID that Facebook has given to the page
                // The ID is used to identify the page later when posting on its wall 
                // Throws an exception if loading of the ID fails
                string facebookPageId;
                if (!FacebookHelper.TryGetFacebookPageId(pageUrl, app, out facebookPageId))
                {
                    throw new Exception("[FacebookApiExamples.CreateFacebookPage]: Failed to get PageID from Facebook. 'pageUrl' is not a valid Facebook Page Url.");
                }

                // Sets the ID and access token for the page
                page.FacebookPageIdentity = new FacebookPageIdentityData(pageUrl, facebookPageId);
                page.FacebookPageAccessToken = new FacebookPageAccessTokenData(pageAccessToken, null);

                // Saves the Facebook page to the database
                FacebookAccountInfoProvider.SetFacebookAccountInfo(page);
            }


            /// <heading>Updating a Facebook page</heading>
            private void GetAndUpdateFacebookPage()
            {
                // Gets the Facebook page
                FacebookAccountInfo page = FacebookAccountInfoProvider.GetFacebookAccountInfo("NewPage", SiteContext.CurrentSiteID);

                if (page != null)
                {
                    // Updates the page's properties
                    page.FacebookAccountDisplayName = page.FacebookAccountDisplayName.ToLowerCSafe();

                    // Savez the changes to the database
                    FacebookAccountInfoProvider.SetFacebookAccountInfo(page);
                }
            }


            /// <heading>Deleting a Facebook page</heading>
            private void DeleteFacebookPage()
            {
                // Gets the Facebook page
                FacebookAccountInfo page = FacebookAccountInfoProvider.GetFacebookAccountInfo("NewPage", SiteContext.CurrentSiteID);

                if (page != null)
                {
                    // Deletes the Facebook page
                    FacebookAccountInfoProvider.DeleteFacebookAccountInfo(page);
                }
            }
        }


        /// <summary>
        /// Holds Facebook post API examples.
        /// </summary>
        /// <groupHeading>Facebook posts</groupHeading>
        private class FacebookPosts
        {
            /// <heading>Creating a Facebook post</heading>
            private void CreateFacebookPost()
            {
                // Gets the page to which the post is tied
                FacebookAccountInfo page = FacebookAccountInfoProvider.GetFacebookAccountInfo("NewPage", SiteContext.CurrentSiteID);

                if (page == null)
                {
                    throw new Exception("[FacebookApiExamples.CreateFacebookPost]: Page 'New page' was not found.");
                }

                // Creates a new Facebook post object
                FacebookPostInfo post = new FacebookPostInfo();

                // Sets the properties of the post
                post.FacebookPostFacebookAccountID = page.FacebookAccountID;
                post.FacebookPostSiteID = SiteContext.CurrentSiteID;
                post.FacebookPostText = "Sample post text.";

                // Sets the publish time for the post
                post.FacebookPostScheduledPublishDateTime = DateTime.Now + TimeSpan.FromMinutes(5);

                // Specifies that the post is not tied to a document
                post.FacebookPostDocumentGUID = null;

                // Saves the Facebook post to the database
                FacebookPostInfoProvider.SetFacebookPostInfo(post);
            }


            /// <heading>Updating a Facebook post</heading>
            private void GetAndUpdateFacebookPost()
            {
                // Gets the page to which the post is tied
                FacebookAccountInfo page = FacebookAccountInfoProvider.GetFacebookAccountInfo("NewPage", SiteContext.CurrentSiteID);

                if (page == null)
                {
                    throw new Exception("[FacebookApiExamples.GetAndUpdateFacebookPost]: Page 'New page' was not found.");
                }

                // Gets the first Facebook post on the page
                FacebookPostInfo post = FacebookPostInfoProvider.GetFacebookPostInfosByAccountId(page.FacebookAccountID).FirstObject;

                if (post != null)
                {
                    // Updates the text of the post
                    post.FacebookPostText = post.FacebookPostText + " Edited.";

                    // Saves the changes to the database
                    FacebookPostInfoProvider.SetFacebookPostInfo(post);
                }
            }


            //// <heading>Deleting Facebook posts</heading>
            private void DeleteFacebookPosts()
            {
                // Gets the page to which the posts are tied
                FacebookAccountInfo page = FacebookAccountInfoProvider.GetFacebookAccountInfo("NewPage", SiteContext.CurrentSiteID);

                if (page == null)
                {
                    throw new Exception("[FacebookApiExamples.DeleteFacebookPosts]: Page 'New page' was not found.");
                }

                // Gets all Facebook posts on the page
                var posts = FacebookPostInfoProvider.GetFacebookPostInfosByAccountId(page.FacebookAccountID);

                // Deletes the Facebook post from Kentico and from Facebook
                foreach (FacebookPostInfo deletePost in posts)
                {
                    FacebookPostInfoProvider.DeleteFacebookPostInfo(deletePost);
                }
            }


            /// <heading>Publishing a Facebook post</heading>
            private void PublishPostToFacebook()
            {
                // Gets the page to which the post is tied
                FacebookAccountInfo page = FacebookAccountInfoProvider.GetFacebookAccountInfo("NewPage", SiteContext.CurrentSiteID);

                if (page == null)
                {
                    throw new Exception("[FacebookApiExamples.PublishPostToFacebook]: Page 'New page' was not found.");
                }

                // Gets the first Facebook post on the page
                FacebookPostInfo post = FacebookPostInfoProvider.GetFacebookPostInfosByAccountId(page.FacebookAccountID).FirstObject;

                if (post == null)
                {
                    throw new Exception("[FacebookApiExamples.PublishPostToFacebook]: No posts were created, or they have been deleted.");
                }

                // Publishes the post. The post is scheduled to be published if its FacebookPostScheduledPublishDateTime value is set in the future.
                FacebookPostInfoProvider.PublishFacebookPost(post.FacebookPostID);
            }
        }
    }
}
