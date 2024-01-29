using System;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Globalization;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Scheduler;

using LinqToTwitter;

namespace CMS.SocialMarketing
{
    /// <summary>
    /// Provides management of Twitter posts.
    /// </summary>
    public class TwitterPostInfoProvider : AbstractInfoProvider<TwitterPostInfo, TwitterPostInfoProvider>
    {
        #region "Public constants"

        /// <summary>
        /// Error code represents unknown error when publishing the post.
        /// </summary>
        public const int ERROR_CODE_UNKNOWN_ERROR = -1;


        /// <summary>
        /// Error code represents unexpected error with Twitter account (account doesn't exist or it isn't valid).
        /// </summary>
        public const int ERROR_CODE_INVALID_ACCOUNT = -2;


        /// <summary>
        /// Error code represents unexpected error with Twitter application (application doesn't exist or it isn't valid).
        /// </summary>
        public const int ERROR_CODE_INVALID_APPLICATION = -3;


        /// <summary>
        /// Error code represents the post belongs to the document that doesn't exist.
        /// </summary>
        public const int ERROR_CODE_DOCUMENT_NOT_EXIST = -4;


        /// <summary>
        /// Scheduled posts which are late on publishing for more than POST_DELAY_TOLERANCE are reported as faulty.
        /// </summary>
        public static readonly TimeSpan POST_DELAY_TOLERANCE = TimeSpan.FromMinutes(5);

        #endregion


        #region "Private constants"

        /// <summary>
        /// Post publishing tolerance interval - posts published within (now + IMMEDIATE_PUBLISH_TOLERANCE) are published immediately as if no delay was set.
        /// </summary>
        private readonly TimeSpan IMMEDIATE_PUBLISH_TOLERANCE = TimeSpan.FromMinutes(1);

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the TwitterPostInfoProvider class.
        /// </summary>
        public TwitterPostInfoProvider() : base(TwitterPostInfo.TYPEINFO, new HashtableSettings
                {
                    ID = true,
                    Load = LoadHashtableEnum.None,
                    UseWeakReferences = true
                })
        {

        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the TwitterPostInfo objects.
        /// </summary>
        public static ObjectQuery<TwitterPostInfo> GetTwitterPosts()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Retrieves a Twitter post with the specified identifier, and returns it.
        /// </summary>
        /// <param name="postId">Twitter post identifier.</param>
        /// <returns>A Twitter post with the specified identifier, if found; otherwise, null.</returns>      
        public static TwitterPostInfo GetTwitterPostInfo(int postId)
        {
            return ProviderObject.GetInfoById(postId);
        }


        /// <summary>
        /// Updates or creates the specified Twitter post.
        /// </summary>
        /// <param name="post">Twitter post to be updated or created.</param>
        public static void SetTwitterPostInfo(TwitterPostInfo post)
        {
            ProviderObject.SetInfo(post);
        }


        /// <summary>
        /// Deletes the specified Twitter post from both the Twitter and the CMS database.
        /// </summary>
        /// <param name="post">Twitter post to be deleted.</param>
        public static void DeleteTwitterPostInfo(TwitterPostInfo post)
        {
            ProviderObject.DeleteTwitterPostInfoInternal(post);
        }


        /// <summary>
        /// Deletes the Twitter post with specified identifier from both the Twitter and the CMS database.
        /// </summary>
        /// <param name="postId">Twitter post identifier.</param>
        public static void DeleteTwitterPostInfo(int postId)
        {
            TwitterPostInfo postObj = GetTwitterPostInfo(postId);
            DeleteTwitterPostInfo(postObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns a query for all the TwitterPostInfo objects of a specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        public static ObjectQuery<TwitterPostInfo> GetTwitterPosts(SiteInfoIdentifier siteId)
        {
            return ProviderObject.GetTwitterPostsInternal(siteId);
        }


        /// <summary>
        /// Retrieves an object query of Twitter posts for the specified Twitter account (channel) identifier, and returns it.
        /// </summary>
        /// <param name="twitterAccountId">Twitter account (channel) identifier.</param>
        /// <returns>An object query of Twitter posts for the specified account (channel).</returns>
        public static ObjectQuery<TwitterPostInfo> GetTwitterPostInfoByAccountId(int twitterAccountId)
        {
            return ProviderObject.GetTwitterPostInfoByAccountIdInternal(twitterAccountId);
        }


        /// <summary>
        /// Retrieves an object query of Twitter posts for the specified document identifier, and returns it.
        /// </summary>
        /// <param name="documentGuid">Document identifier.</param>
        /// <param name="documentSiteId">Document site identifier.</param>
        /// <returns>An object query of Twitter posts for the specified document.</returns>
        public static ObjectQuery<TwitterPostInfo> GetTwitterPostInfosByDocumentGuid(Guid documentGuid, int documentSiteId)
        {
            return ProviderObject.GetTwitterPostInfosByDocumentGuidInternal(documentGuid, documentSiteId);
        }


        /// <summary>
        /// Publishes the Twitter post on the appropriate Twitter channel. Throws an exception if something goes wrong.
        /// Uses scheduler for future posts.
        /// When modifying scheduled posts, always call the <see cref="TryCancelScheduledPublishTwitterPost(int)"/> first.
        /// </summary>
        /// <seealso cref="TryCancelScheduledPublishTwitterPost(int)"/>
        /// <seealso cref="TryCancelScheduledPublishTwitterPost(TwitterPostInfo)"/>
        /// <param name="postInfoId">Identifier of the TwitterPostInfo that will be published.</param>
        public static void PublishTwitterPost(int postInfoId)
        {
            ProviderObject.PublishTwitterPostInternal(postInfoId);
        }


        /// <summary>
        /// Tries to cancel scheduled publish of Twitter post. If successful, the post can be modified using <see cref="PublishTwitterPost"/>.
        /// (Has to be called before modification since Twitter does not allow published posts to be modified).
        /// There is no need to call this method when deleting a post since posts can be deleted even after publication.
        /// </summary>
        /// <param name="postInfoId">Identifier of the TwitterPostInfo that shall be canceled.</param>
        /// <returns>True if successfully canceled, false if the post has already been published.</returns>
        public static bool TryCancelScheduledPublishTwitterPost(int postInfoId)
        {
            TwitterPostInfo twitterPost = GetTwitterPostInfo(postInfoId);
            return ProviderObject.TryCancelScheduledPublishTwitterPostInternal(twitterPost);
        }


        /// <summary>
        /// Tries to cancel scheduled publish of Twitter post. If successful, the post can be modified using <see cref="PublishTwitterPost"/>.
        /// (Has to be called before modification since Twitter does not allow published posts to be modified).
        /// There is no need to call this method when deleting a post since posts can be deleted even after publication.
        /// </summary>
        /// <param name="postInfo">The TwitterPostInfo that shall be canceled.</param>
        /// <returns>True if successfully canceled, false if the post has already been published.</returns>
        public static bool TryCancelScheduledPublishTwitterPost(TwitterPostInfo postInfo)
        {
            return ProviderObject.TryCancelScheduledPublishTwitterPostInternal(postInfo);
        }


        /// <summary>
        /// Gets localized message that describes the post's publish state.
        /// </summary>
        /// <param name="post">Twitter post.</param>
        /// <param name="user">User whose time zone will be used when formating date time information.</param>
        /// <param name="site">Site whose time zone will be used when formating date time information.</param>
        /// <param name="shortMessage">Indicates if short message has to be used instead of detail message.</param>
        /// <returns>Localized status message.</returns>
        public static string GetPostPublishStateMessage(TwitterPostInfo post, IUserInfo user = null, ISiteInfo site = null, bool shortMessage = false)
        {
            return ProviderObject.GetPostPublishStateMessageInternal(post, user, site, shortMessage);
        }
        
        #endregion


        #region "Internal interface"

        /// <summary>
        /// Publishes the Twitter post on the appropriate Twitter channel immediately.
        /// </summary>
        /// <param name="postInfoId">Identifier of the TwitterPostInfo that will be published.</param>
        /// <exception cref="Exception">When post does not exist, has already been published to Twitter or the account is not valid.</exception>
        internal static void PublishTwitterPostToTwitter(int postInfoId)
        {
            ProviderObject.PublishTwitterPostToTwitterInternal(postInfoId);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Deletes the specified Twitter post from both the Twitter and the CMS database.
        /// Deletes scheduled task as well.
        /// </summary>
        /// <param name="post">Twitter post to be deleted.</param>  
        /// <exception cref="Exception">When twitter account or application not found or tweet not deleted successfully.</exception>
        protected virtual void DeleteTwitterPostInfoInternal(TwitterPostInfo post)
        {
            if (post == null)
            {
                return;
            }

            TaskInfo scheduledTask = TaskInfoProvider.GetTaskInfo(String.Format(SocialMarketingPostPublishingTask.TASK_CODENAME_FORMAT_TWITTER, post.TwitterPostID), post.TwitterPostSiteID);
            if (scheduledTask != null)
            {
                TaskInfoProvider.DeleteTaskInfo(scheduledTask);
            }

            if (!String.IsNullOrEmpty(post.TwitterPostExternalID))
            {
                TwitterAccountInfo account = TwitterAccountInfoProvider.GetTwitterAccountInfo(post.TwitterPostTwitterAccountID);
                if (account == null)
                {
                    throw new Exception(String.Format("[TwitterPostInfoProvider.DeleteTwitterPostInfoInternal]: Twitter account with ID {0} not found.", post.TwitterPostTwitterAccountID));
                }

                TwitterApplicationInfo application = TwitterApplicationInfoProvider.GetTwitterApplicationInfo(account.TwitterAccountTwitterApplicationID);
                if (application == null)
                {
                    throw new Exception(String.Format("[TwitterPostInfoProvider.DeleteTwitterPostInfoInternal]: Twitter application with ID {0} not found.", account.TwitterAccountTwitterApplicationID));
                }

                TwitterHelper.DeleteTweet(application.TwitterApplicationConsumerKey, application.TwitterApplicationConsumerSecret, account.TwitterAccountAccessToken, account.TwitterAccountAccessTokenSecret, post.TwitterPostExternalID);
            }

            DeleteInfo(post);
        }	

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Returns a query for all the TwitterPostInfo objects of a specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        protected virtual ObjectQuery<TwitterPostInfo> GetTwitterPostsInternal(int siteId)
        {
            return GetObjectQuery().OnSite(siteId);
        }    
        
        
        /// <summary>
        /// Retrieves an object query of Twitter posts for the specified Twitter account (channel) identifier, and returns it.
        /// </summary>
        /// <param name="twitterAccountId">Twitter account (channel) identifier.</param>
        /// <returns>An object query of Twitter posts for the specified account (channel).</returns>
        protected virtual ObjectQuery<TwitterPostInfo> GetTwitterPostInfoByAccountIdInternal(int twitterAccountId)
        {
            return GetObjectQuery().Where("TwitterPostTwitterAccountID", QueryOperator.Equals, twitterAccountId);
        }


        /// <summary>
        /// Retrieves an object query of Twitter posts for the specified document identifier, and returns it.
        /// </summary>
        /// <param name="documentGuid">Document identifier.</param>
        /// <param name="documentSiteId">Document site identifier.</param>
        /// <returns>An object query of Twitter posts for the specified document.</returns>
        protected virtual ObjectQuery<TwitterPostInfo> GetTwitterPostInfosByDocumentGuidInternal(Guid documentGuid, int documentSiteId)
        {
            var query = GetObjectQuery().WhereEquals("TwitterPostDocumentGUID", documentGuid);
            if (documentSiteId > 0)
            {
                query.WhereEquals("TwitterPostSiteID", documentSiteId);
            }

            return query;
        }


        /// <summary>
        /// Publishes the Twitter post on the appropriate Twitter channel.
        /// Uses scheduler for future posts.
        /// </summary>
        /// <param name="postInfoId">Identifier of the TwitterPostInfo that will be published.</param>
        /// <exception cref="Exception">When post does not exist, has already been published to Twitter or the account is not valid.</exception>
        protected virtual void PublishTwitterPostInternal(int postInfoId)
        {
            TwitterPostInfo twitterPost = GetTwitterPostInfo(postInfoId);

            // Post does not exist or has already been published
            if ((twitterPost == null) || !String.IsNullOrWhiteSpace(twitterPost.TwitterPostExternalID))
            {
                throw new Exception(String.Format("[TwitterPostInfoProvider.PublishTwitterPostInternal]: Twitter post with ID {0} does not exist or has been already published.", postInfoId));
            }

            if (!twitterPost.TwitterPostScheduledPublishDateTime.HasValue || (DateTime.Compare(twitterPost.TwitterPostScheduledPublishDateTime.Value, DateTime.Now + IMMEDIATE_PUBLISH_TOLERANCE) <= 0))
            {
                // Post gets published immediately
                PublishTwitterPostToTwitterInternal(postInfoId);
            }
            else
            {
                // Post has to have a scheduled task
                TaskInterval interval = new TaskInterval
                {
                    StartTime = twitterPost.TwitterPostScheduledPublishDateTime.Value,
                    Period = SchedulingHelper.PERIOD_ONCE
                };

                TaskInfo scheduledTask = new TaskInfo
                {
                    TaskDisplayName = "{$sm.twitter.post.scheduledtask.name$}",
                    TaskName = String.Format(SocialMarketingPostPublishingTask.TASK_CODENAME_FORMAT_TWITTER, twitterPost.TwitterPostID),
                    TaskAssemblyName = "CMS.SocialMarketing",
                    TaskClass = "CMS.SocialMarketing.SocialMarketingPostPublishingTask",
                    TaskDeleteAfterLastRun = true,
                    TaskSiteID = twitterPost.TwitterPostSiteID,
                    TaskAllowExternalService = true,
                    TaskUseExternalService = true,
                    TaskRunInSeparateThread = true,
                    TaskEnabled = true,
                    TaskType = ScheduledTaskTypeEnum.System,
                    TaskData = twitterPost.TwitterPostID.ToString(),
                    TaskInterval = SchedulingHelper.EncodeInterval(interval)
                };

                // Set task activation time
                scheduledTask.TaskNextRunTime = SchedulingHelper.GetFirstRunTime(interval);

                TaskInfoProvider.SetTaskInfo(scheduledTask);
            }
        }


        /// <summary>
        /// Tries to cancel scheduled publish of Twitter post. If successful, the post can be modified using <see cref="PublishTwitterPost"/>.
        /// (Has to be called before modification since Twitter does not allow published posts to be modified).
        /// There is no need to call this method when deleting a post since posts can be deleted even after publication.
        /// </summary>
        /// <param name="postInfo">The TwitterPostInfo that shall be canceled.</param>
        /// <returns>True if successfully canceled (or neither post or scheduled task does not exist), false if the post has already been published (or the task is already running).</returns>
        protected virtual bool TryCancelScheduledPublishTwitterPostInternal(TwitterPostInfo postInfo)
        {
            if ((postInfo == null) || !postInfo.TwitterPostScheduledPublishDateTime.HasValue)
            {
                return true;
            }

            if (!String.IsNullOrEmpty(postInfo.TwitterPostExternalID))
            {
                return false;
            }

            TaskInfo scheduledTask = TaskInfoProvider.GetTaskInfo(String.Format(SocialMarketingPostPublishingTask.TASK_CODENAME_FORMAT_TWITTER, postInfo.TwitterPostID), postInfo.TwitterPostSiteID);
            if (scheduledTask != null)
            {
                TaskInfoProvider.DeleteTaskInfo(scheduledTask);
                return true;
            }
            else if (String.IsNullOrEmpty(postInfo.TwitterPostExternalID))
            {
                // The scheduled task has been deleted by user via UI before post publishing.
                return true;
            }
            return false;
        }


        /// <summary>
        /// Publishes the Twitter post on the appropriate Twitter channel. Throws an exception if something goes wrong.
        /// </summary>
        /// <param name="postInfoId">Identifier of the TwitterPostInfo that will be published.</param>
        /// <exception cref="Exception">When post does not exist, has already been published to Twitter or the account is not valid.</exception>
        protected virtual void PublishTwitterPostToTwitterInternal(int postInfoId)
        {
            using (var scope = BeginTransaction())
            {
                TwitterPostInfo post = GetTwitterPostInfo(postInfoId);
                if ((post == null) || !String.IsNullOrWhiteSpace(post.TwitterPostExternalID))
                {
                    throw new Exception(String.Format("[TwitterPostInfoProvider.PublishTwitterPostToTwitterInternal]: Twitter post with ID {0} does not exist or has been already published.", postInfoId));
                }

                try
                {
                    post.TwitterPostErrorCode = null;

                    TwitterAccountInfo account = TwitterAccountInfoProvider.GetTwitterAccountInfo(post.TwitterPostTwitterAccountID);
                    if (account == null)
                    {
                        post.TwitterPostErrorCode = ERROR_CODE_INVALID_ACCOUNT;
                        throw new Exception(String.Format("[TwitterPostInfoProvider.PublishTwitterPostToTwitterInternal]: Twitter account with ID {0} does not exist.", post.TwitterPostTwitterAccountID));
                    }

                    TwitterApplicationInfo app = TwitterApplicationInfoProvider.GetTwitterApplicationInfo(account.TwitterAccountTwitterApplicationID);
                    if (app == null)
                    {
                        post.TwitterPostErrorCode = ERROR_CODE_INVALID_APPLICATION;
                        throw new Exception(String.Format("[TwitterPostInfoProvider.PublishTwitterPostToTwitterInternal]: Twitter application with ID {0} does not exist.", account.TwitterAccountTwitterApplicationID));
                    }

                    // Resolve macros in the post content.
                    if (post.TwitterPostDocumentGUID.HasValue)
                    {
                        TreeNode document = new ObjectQuery<TreeNode>().WithGuid(post.TwitterPostDocumentGUID.Value).OnSite(post.TwitterPostSiteID).TopN(1).FirstOrDefault();
                        if (document == null)
                        {
                            // Related document does not exist so the tweet won't be published.
                            post.TwitterPostErrorCode = ERROR_CODE_DOCUMENT_NOT_EXIST;
                            return;
                        }

                        MacroResolver resolver = MacroResolver.GetInstance();
                        resolver.AddAnonymousSourceData(document);
                        post.TwitterPostText = resolver.ResolveMacros(post.TwitterPostText);
                    }

                    // Shorten URLs in the post content.
                    if (post.TwitterPostURLShortenerType != URLShortenerTypeEnum.None)
                    {
                        post.TwitterPostText = URLShortenerHelper.ShortenURLsInText(post.TwitterPostText, post.TwitterPostURLShortenerType, post.TwitterPostSiteID);
                    }

                    post.TwitterPostExternalID = TwitterHelper.PublishTweet(app.TwitterApplicationConsumerKey, app.TwitterApplicationConsumerSecret, account.TwitterAccountAccessToken, account.TwitterAccountAccessTokenSecret, post.TwitterPostText);
                    post.TwitterPostPublishedDateTime = DateTime.Now;

                }
                catch (TwitterQueryException ex)
                {
                    post.TwitterPostErrorCode = ex.ErrorCode;
                    throw;
                }
                catch
                {
                    post.TwitterPostErrorCode = ERROR_CODE_UNKNOWN_ERROR;
                    throw;
                }
                finally
                {
                    post.Update();
                    scope.Commit();
                }
            }
        }


        /// <summary>
        /// Gets localized message that describes the post's publish state.
        /// </summary>
        /// <param name="post">Twitter post.</param>
        /// <param name="user">User whose time zone will be used when formating date time information.</param>
        /// <param name="site">Site whose time zone will be used when formating date time information.</param>
        /// <param name="shortMessage">Indicates if short message has to be used instead of detail message.</param>
        /// <returns>Localized status message.</returns>
        protected virtual string GetPostPublishStateMessageInternal(TwitterPostInfo post, IUserInfo user = null, ISiteInfo site = null, bool shortMessage = false)
        {
            if (post.IsPublished)
            {
                string publishedTime = TimeZoneHelper.ConvertToUserTimeZone(post.TwitterPostPublishedDateTime.Value, true, user, site);
                string resourceString = shortMessage ? "sm.twitter.posts.msg.published.short" : "sm.twitter.posts.msg.published";

                return String.Format(ResHelper.GetString(resourceString), publishedTime);
            }

            // Error occurred
            if (post.TwitterPostErrorCode.HasValue)
            {
                return shortMessage ? ResHelper.GetString("sm.twitter.posts.msg.failed") : GetTwitterPostErrorMessage(post.TwitterPostErrorCode.Value);
            }

            // Post relates to some document
            if (post.TwitterPostDocumentGUID.HasValue)
            {
                TreeNode document = new ObjectQuery<TreeNode>().WithGuid(post.TwitterPostDocumentGUID.Value).OnSite(post.TwitterPostSiteID).TopN(1).FirstOrDefault();
                if (document == null)
                {
                    // Post won't be published because related document doesn't exist
                    return ResHelper.GetString(shortMessage ? "sm.twitter.posts.msg.wontbepublished.short" : "sm.twitter.posts.msg.wontbepublished");
                }
                else if (post.TwitterPostPostAfterDocumentPublish && !post.TwitterPostScheduledPublishDateTime.HasValue)
                {
                    // Post will be posted to Twitter when the related document gets published
                    return ResHelper.GetString(shortMessage ? "sm.twitter.posts.msg.postondocumentpublish.short" : "sm.twitter.posts.msg.postondocumentpublish");
                }
            }

            // Post is scheduled
            if (post.TwitterPostScheduledPublishDateTime.HasValue)
            {
                string scheduledTime = TimeZoneHelper.ConvertToUserTimeZone(post.TwitterPostScheduledPublishDateTime.Value, true, user, site);
                if (DateTime.Compare(post.TwitterPostScheduledPublishDateTime.Value, DateTime.Now - POST_DELAY_TOLERANCE) <= 0)
                {
                    return shortMessage ? ResHelper.GetString("sm.twitter.posts.msg.failed") : String.Format(ResHelper.GetString("sm.twitter.posts.msg.schedulederror"), scheduledTime);
                }

                string resourceString = shortMessage ? "sm.twitter.posts.msg.scheduled.short" : "sm.twitter.posts.msg.scheduled";

                return String.Format(ResHelper.GetString(resourceString), scheduledTime);
            }

            return String.Empty;
        }


        /// <summary>
        /// Gets localized error message that describes the given Twitter's error code.
        /// </summary>
        /// <param name="errorCode">Error code that Twitter returned.</param>
        /// <returns>Localized error message.</returns>
        protected virtual string GetTwitterPostErrorMessage(int errorCode)
        {
            switch (errorCode)
            {
                // Authentication errors
                case 32:
                case 64:
                case 89:
                case 135:
                case 215:
                case 231:
                    return ResHelper.GetString("sm.twitter.posts.msg.authenticationerror");

                // Tweet does not exist on twitter, however is still present in CMS DB
                case 144:
                    return ResHelper.GetString("sm.twitter.posts.msg.postdoesnotexist");

                // Server side errors
                case 34:
                case 130:
                case 131:
                    return ResHelper.GetString("sm.twitter.posts.msg.servererror");

                // API calls limit error
                case 88:
                    return ResHelper.GetString("sm.twitter.posts.msg.apilimiterror");

                // Status is over 280 characters
                case 186:
                    return ResHelper.GetString("sm.twitter.posts.msg.tweettoolong");

                // Duplicate status error
                case 187:
                    return ResHelper.GetString("sm.twitter.posts.msg.duplicatestatuserror");

                // Invalid account
                case ERROR_CODE_INVALID_ACCOUNT:
                    return ResHelper.GetString("sm.twitter.posts.msg.accounterror");

                // Invalid application
                case ERROR_CODE_INVALID_APPLICATION:
                    return ResHelper.GetString("sm.twitter.posts.msg.applicationerror");

                // Invalid document
                case ERROR_CODE_DOCUMENT_NOT_EXIST:
                    return ResHelper.GetString("sm.twitter.posts.msg.documenterror");

                default:
                    // Unknown errors
                    return ResHelper.GetString("sm.twitter.posts.msg.unknownerror");
            }
        }
        
        #endregion		
    }
}