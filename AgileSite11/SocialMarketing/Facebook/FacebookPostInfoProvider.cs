using System;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Globalization;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Scheduler;

using Facebook;

namespace CMS.SocialMarketing
{
    /// <summary>
    /// Provides management of Facebook posts.
    /// </summary>
    public class FacebookPostInfoProvider : AbstractInfoProvider<FacebookPostInfo, FacebookPostInfoProvider>
    {
        #region "Public constants"

        /// <summary>
        /// Error code represents unknown error when publishing the post.
        /// </summary>
        public const int ERROR_CODE_UNKNOWN_ERROR = -1;


        /// <summary>
        /// Error code represents unexpected error with Facebook account (account doesn't exist or its access token isn't valid).
        /// </summary>
        public const int ERROR_CODE_INVALID_ACCOUNT = -2;


        /// <summary>
        /// Error code represents the post belongs to the document that doesn't exist.
        /// </summary>
        public const int ERROR_CODE_DOCUMENT_NOT_EXIST = -3;


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
        /// Initializes a new instance of the FacebookPostInfoProvider class.
        /// </summary>
        public FacebookPostInfoProvider() : base(FacebookPostInfo.TYPEINFO, new HashtableSettings
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
        /// Returns a query for all the FacebookPostInfo objects.
        /// </summary>
        public static ObjectQuery<FacebookPostInfo> GetFacebookPosts()
        {
            return ProviderObject.GetObjectQuery();
        }

        
        /// <summary>
        /// Retrieves a Facebook post with the specified identifier, and returns it.
        /// </summary>
        /// <param name="postId">Facebook post identifier.</param>
        /// <returns>A Facebook post with the specified identifier, if found; otherwise, null.</returns>      
        public static FacebookPostInfo GetFacebookPostInfo(int postId)
        {
            return ProviderObject.GetInfoById(postId);
        }


        /// <summary>
        /// Returns FacebookPostInfo with specified GUID.
        /// </summary>
        /// <param name="guid">FacebookPostInfo GUID</param>                
        public static FacebookPostInfo GetFacebookPostInfo(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Updates or creates the specified Facebook post.
        /// </summary>
        /// <param name="post">Facebook post to be updated or created.</param>
        public static void SetFacebookPostInfo(FacebookPostInfo post)
        {
            ProviderObject.SetInfo(post);
        }


        /// <summary>
        /// Deletes the specified Facebook post. If the post is published on the Facebook, it will be deleted there first.
        /// </summary>
        /// <param name="post">Facebook post to be deleted.</param>         
        /// <exception cref="FacebookApiException">FacebookApiException is thrown when an error occurs while communicating with Facebook.</exception>
        /// <exception cref="Exception">Exception is thrown if the Facebook account doesn't exist or its access token is invalid or post could not be deleted from Facebook.</exception>
        public static void DeleteFacebookPostInfo(FacebookPostInfo post)
        {
            ProviderObject.DeleteInfo(post);
        }

        /// <summary>
        /// Deletes the specified Facebook post. If the post is published on the Facebook, it will be deleted there first.
        /// </summary>
        /// <param name="postId">Facebook post identifier.</param>
        /// <exception cref="FacebookApiException">FacebookApiException is thrown when an error occurs while communicating with Facebook.</exception>
        /// <exception cref="Exception">Exception is thrown if the Facebook account doesn't exist or its access token is invalid or post could not be deleted from Facebook.</exception>
        public static void DeleteFacebookPostInfo(int postId)
        {
            FacebookPostInfo postObj = GetFacebookPostInfo(postId);
            DeleteFacebookPostInfo(postObj);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns a query for all the FacebookPostInfo objects of a specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        public static ObjectQuery<FacebookPostInfo> GetFacebookPosts(SiteInfoIdentifier siteId)
        {
            return ProviderObject.GetFacebookPostsInternal(siteId);
        }


        /// <summary>
        /// Retrieves an object query of Facebook posts for the specified Facebook account (page) identifier, and returns it.
        /// </summary>
        /// <param name="facebookAccountId">Facebook account (page) identifier.</param>
        /// <returns>An object query of Facebook posts for the specified account (page).</returns>
        public static ObjectQuery<FacebookPostInfo> GetFacebookPostInfosByAccountId(int facebookAccountId)
        {
            return ProviderObject.GetFacebookPostInfosByAccountIdInternal(facebookAccountId);
        }


        /// <summary>
        /// Retrieves an object query of Facebook posts for the specified document identifier, and returns it.
        /// </summary>
        /// <param name="documentGuid">Document identifier.</param>
        /// <param name="documentSiteId">Document site identifier.</param>
        /// <returns>An object query of Facebook posts for the specified document.</returns>
        public static ObjectQuery<FacebookPostInfo> GetFacebookPostInfosByDocumentGuid(Guid documentGuid, int documentSiteId)
        {
            return ProviderObject.GetFacebookPostInfosByDocumentGuidInternal(documentGuid, documentSiteId);
        }


        /// <summary>
        /// Publishes the Facebook post on the appropriate Facebook page in proper time.
        /// Uses scheduler for future posts.
        /// When modifying scheduled posts, always call the <see cref="TryCancelScheduledPublishFacebookPost(int)"/> first.
        /// </summary>
        /// <seealso cref="TryCancelScheduledPublishFacebookPost(int)"/>
        /// <seealso cref="TryCancelScheduledPublishFacebookPost(FacebookPostInfo)"/>
        /// <param name="postInfoId">Identifier of the FacebookPostInfo that will be published.</param>
        /// <exception cref="Exception">When post does not exist, has already been published to Facebook or the account is not valid.</exception>
        /// <exception cref="FacebookApiException">When something in communication with Facebook goes wrong.</exception>
        public static void PublishFacebookPost(int postInfoId)
        {
            ProviderObject.PublishFacebookPostInternal(postInfoId);
        }


        /// <summary>
        /// Tries to cancel scheduled publish of Facebook post. If successful, the post can be modified using <see cref="PublishFacebookPost"/>.
        /// (Has to be called before modification since Facebook does not allow published posts to be modified).
        /// There is no need to call this method when deleting a post since posts can be deleted even after publication.
        /// </summary>
        /// <param name="postInfoId">Identifier of the FacebookPostInfo that shall be canceled.</param>
        /// <returns>True if successfully canceled, false if the post has already been published.</returns>
        public static bool TryCancelScheduledPublishFacebookPost(int postInfoId)
        {
            FacebookPostInfo facebookPost = GetFacebookPostInfo(postInfoId);
            return ProviderObject.TryCancelScheduledPublishFacebookPostInternal(facebookPost);
        }


        /// <summary>
        /// Tries to cancel scheduled publish of Facebook post. If successful, the post can be modified using <see cref="PublishFacebookPost"/>.
        /// (Has to be called before modification since Facebook does not allow published posts to be modified).
        /// There is no need to call this method when deleting a post since posts can be deleted even after publication.
        /// </summary>
        /// <param name="postInfo">The FacebookPostInfo that shall be canceled.</param>
        /// <returns>True if successfully canceled, false if the post has already been published.</returns>
        public static bool TryCancelScheduledPublishFacebookPost(FacebookPostInfo postInfo)
        {
            return ProviderObject.TryCancelScheduledPublishFacebookPostInternal(postInfo);
        }


        /// <summary>
        /// Gets localized message that describes the post's publish state.
        /// </summary>
        /// <param name="post">Facebook post.</param>
        /// <param name="user">User whose time zone will be used when formating date time information.</param>
        /// <param name="site">Site whose time zone will be used when formating date time information.</param>
        /// <param name="shortMessage">Indicates if short message has to be used instead of detail message.</param>
        /// <returns>Localized status message.</returns>
        public static string GetPostPublishStateMessage(FacebookPostInfo post, IUserInfo user = null, ISiteInfo site = null, bool shortMessage = false)
        {
            return ProviderObject.GetPostPublishStateMessageInternal(post, user, site, shortMessage);
        }
        
        #endregion


        #region "Internal interface"

        /// <summary>
        /// Retrieves an object query of Facebook posts which has outdated insights for the specified Facebook account (page) identifier, and returns it.
        /// Insights are considered outdated when they haven't been updated today.
        /// </summary>
        /// <param name="facebookAccountId">Facebook account (page) identifier.</param>
        /// <returns>An object query of Facebook posts which has outdated insights for the specified account (page).</returns>
        internal static ObjectQuery<FacebookPostInfo> GetFacebookPostInfosWithOutdatedInsights(int facebookAccountId)
        {
            return ProviderObject.GetObjectQuery()
                .Where("FacebookPostFacebookAccountID", QueryOperator.Equals, facebookAccountId)
                .WhereNotEmpty("FacebookPostExternalID")
                .Where(new WhereCondition()
                    .WhereNull("FacebookPostInsightsLastUpdated")
                    .Or()
                    .Where("FacebookPostInsightsLastUpdated", QueryOperator.LessThan, DateTime.Now.Date)
                );
        }


        /// <summary>
        /// Publishes the Facebook post on the appropriate Facebook page immediately.
        /// </summary>
        /// <param name="postInfoId">Identifier of the FacebookPostInfo that will be published.</param>
        /// <exception cref="Exception">When post does not exist, has already been published to Facebook or the account is not valid.</exception>
        /// <exception cref="FacebookApiException">When something in communication with Facebook goes wrong.</exception>
        internal static void PublishFacebookPostToFacebook(int postInfoId)
        {
            ProviderObject.PublishFacebookPostToFacebookInternal(postInfoId);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(FacebookPostInfo info)
        {
            if (info == null)
            {
                return;
            }

            TaskInfo scheduledTask = TaskInfoProvider.GetTaskInfo(String.Format(SocialMarketingPostPublishingTask.TASK_CODENAME_FORMAT_FACEBOOK, info.FacebookPostID), info.FacebookPostSiteID);
            if (scheduledTask != null)
            {
                TaskInfoProvider.DeleteTaskInfo(scheduledTask);
            }

            if (!String.IsNullOrEmpty(info.FacebookPostExternalID))
            {
                // Post will be deleted from Facebook.
                FacebookAccountInfo account = FacebookAccountInfoProvider.GetFacebookAccountInfo(info.FacebookPostFacebookAccountID);
                if ((account == null)
                    || String.IsNullOrWhiteSpace(account.FacebookPageAccessToken.AccessToken)
                    || (account.FacebookPageAccessToken.Expiration.HasValue && (account.FacebookPageAccessToken.Expiration.Value.AddMinutes(5) < DateTime.UtcNow)))
                {
                    throw new Exception(String.Format("[FacebookPostInfoProvider.DeleteFacebookPostInfoInternal]: Facebook account with ID {0} does not exist or its access token is invalid.", info.FacebookPostFacebookAccountID));
                }

                var appSecret = FacebookApplicationInfoProvider.GetFacebookApplicationInfo(account.FacebookAccountFacebookApplicationID).FacebookApplicationConsumerSecret;
                FacebookHelper.DeletePostOnFacebookPage(info.FacebookPostExternalID, account.FacebookPageAccessToken.AccessToken, appSecret);
            }

            base.DeleteInfo(info);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Returns a query for all the FacebookPostInfo objects of a specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        protected virtual ObjectQuery<FacebookPostInfo> GetFacebookPostsInternal(SiteInfoIdentifier siteId)
        {
            return GetObjectQuery().OnSite(siteId);
        }  


        /// <summary>
        /// Retrieves an object query of Facebook posts for the specified Facebook account (page) identifier, and returns it.
        /// </summary>
        /// <param name="facebookAccountId">Facebook account (page) identifier.</param>
        /// <returns>An object query of Facebook posts for the specified account (page).</returns>
        protected virtual ObjectQuery<FacebookPostInfo> GetFacebookPostInfosByAccountIdInternal(int facebookAccountId)
        {
            return ProviderObject.GetObjectQuery().Where("FacebookPostFacebookAccountID", QueryOperator.Equals, facebookAccountId);
        }


        /// <summary>
        /// Retrieves an object query of Facebook posts for the specified document identifier, and returns it.
        /// </summary>
        /// <param name="documentGuid">Document identifier.</param>
        /// <param name="documentSiteId">Document site identifier.</param>
        /// <returns>An object query of Facebook posts for the specified document.</returns>
        protected virtual ObjectQuery<FacebookPostInfo> GetFacebookPostInfosByDocumentGuidInternal(Guid documentGuid, int documentSiteId)
        {
            var query = ProviderObject.GetObjectQuery().WhereEquals("FacebookPostDocumentGUID", documentGuid);
            if (documentSiteId > 0)
            {
                query.WhereEquals("FacebookPostSiteID", documentSiteId);
            }

            return query;
        }


        /// <summary>
        /// Publishes the Facebook post on the appropriate Facebook page in proper time.
        /// Uses scheduler for future posts.
        /// </summary>
        /// <param name="postInfoId">Identifier of the FacebookPostInfo that will be published.</param>
        /// <exception cref="Exception">When post does not exist, has already been published to Facebook or the account is not valid.</exception>
        /// <exception cref="FacebookApiException">When something in communication with Facebook goes wrong.</exception>
        protected virtual void PublishFacebookPostInternal(int postInfoId)
        {
            FacebookPostInfo facebookPost = GetFacebookPostInfo(postInfoId);

            // Post does not exist or has already been published
            if ((facebookPost == null) || !String.IsNullOrWhiteSpace(facebookPost.FacebookPostExternalID))
            {
                throw new Exception(String.Format("[FacebookPostInfoProvider.PublishFacebookPostInternal]: Facebook post with ID {0} does not exist or has been already published.", postInfoId));
            }

            if (!facebookPost.FacebookPostScheduledPublishDateTime.HasValue || (DateTime.Compare(facebookPost.FacebookPostScheduledPublishDateTime.Value, DateTime.Now + IMMEDIATE_PUBLISH_TOLERANCE) <= 0))
            {
                // Post gets published immediately
                PublishFacebookPostToFacebookInternal(postInfoId);
            }
            else
            {
                // Post has to have a scheduled task
                TaskInterval interval = new TaskInterval
                {
                    StartTime = facebookPost.FacebookPostScheduledPublishDateTime.Value,
                    Period = SchedulingHelper.PERIOD_ONCE
                };

                TaskInfo scheduledTask = new TaskInfo
                {
                    TaskDisplayName = "{$sm.facebook.post.scheduledtask.name$}",
                    TaskName = String.Format(SocialMarketingPostPublishingTask.TASK_CODENAME_FORMAT_FACEBOOK, facebookPost.FacebookPostID),
                    TaskAssemblyName = "CMS.SocialMarketing",
                    TaskClass = "CMS.SocialMarketing.SocialMarketingPostPublishingTask",
                    TaskDeleteAfterLastRun = true,
                    TaskSiteID = facebookPost.FacebookPostSiteID,
                    TaskAllowExternalService = true,
                    TaskUseExternalService = true,
                    TaskRunInSeparateThread = true,
                    TaskEnabled = true,
                    TaskType = ScheduledTaskTypeEnum.System,
                    TaskData = facebookPost.FacebookPostID.ToString(),
                    TaskInterval = SchedulingHelper.EncodeInterval(interval)
                };

                // Set task activation time
                scheduledTask.TaskNextRunTime = SchedulingHelper.GetFirstRunTime(interval);

                TaskInfoProvider.SetTaskInfo(scheduledTask);
            }
        }


        /// <summary>
        /// Tries to cancel scheduled publish of Facebook post. If successful, the post can be modified using <see cref="PublishFacebookPost"/>.
        /// (Has to be called before modification since Facebook does not allow published posts to be modified).
        /// There is no need to call this method when deleting a post since posts can be deleted even after publication.
        /// </summary>
        /// <param name="postInfo">The FacebookPostInfo that shall be canceled.</param>
        /// <returns>True if successfully canceled (or neither post or scheduled task does not exist), false if the post has already been published (or the task is already running).</returns>
        protected virtual bool TryCancelScheduledPublishFacebookPostInternal(FacebookPostInfo postInfo)
        {
            if ((postInfo == null) || !postInfo.FacebookPostScheduledPublishDateTime.HasValue)
            {
                return true;
            }

            if (!String.IsNullOrEmpty(postInfo.FacebookPostExternalID))
            {
                return false;
            }

            TaskInfo scheduledTask = TaskInfoProvider.GetTaskInfo(String.Format(SocialMarketingPostPublishingTask.TASK_CODENAME_FORMAT_FACEBOOK, postInfo.FacebookPostID), postInfo.FacebookPostSiteID);
            if (scheduledTask != null)
            {
                TaskInfoProvider.DeleteTaskInfo(scheduledTask);
                return true;
            }
            else if (String.IsNullOrEmpty(postInfo.FacebookPostExternalID))
            {
                // The scheduled task has been deleted by user via UI before post publishing.
                return true;
            }
            return false;
        }


        /// <summary>
        /// Publishes the Facebook post on the appropriate Facebook page. Throws an exception if something goes wrong.
        /// </summary>
        /// <param name="postInfoId">Identifier of the FacebookPostInfo that will be published.</param>
        /// <exception cref="Exception">When post does not exist, has already been published to Facebook or the account is not valid.</exception>
        /// <exception cref="FacebookApiException">When something in communication with Facebook goes wrong.</exception>
        protected virtual void PublishFacebookPostToFacebookInternal(int postInfoId)
        {
            using (var scope = BeginTransaction())
            {
                FacebookPostInfo post = GetFacebookPostInfo(postInfoId);
                if (post == null || !String.IsNullOrWhiteSpace(post.FacebookPostExternalID))
                {
                    throw new Exception(String.Format("[FacebookPostInfoProvider.PublishFacebookPostToFacebookInternal]: Facebook post with ID {0} does not exist or has been already published.", postInfoId));
                }

                try
                {
                    post.FacebookPostErrorCode = null;
                    post.FacebookPostErrorSubcode = null;

                    FacebookAccountInfo account = FacebookAccountInfoProvider.GetFacebookAccountInfo(post.FacebookPostFacebookAccountID);
                    if ((account == null)
                        || String.IsNullOrWhiteSpace(account.FacebookPageAccessToken.AccessToken)
                        || (account.FacebookPageAccessToken.Expiration.HasValue && (account.FacebookPageAccessToken.Expiration.Value.AddMinutes(5) < DateTime.UtcNow)))
                    {
                        post.FacebookPostErrorCode = ERROR_CODE_INVALID_ACCOUNT;
                        throw new Exception(String.Format("[FacebookPostInfoProvider.PublishFacebookPostToFacebookInternal]: Facebook account with ID {0} does not exist or its access token is invalid.", post.FacebookPostFacebookAccountID));
                    }

                    // Resolve macros in the post content.
                    if (post.FacebookPostDocumentGUID.HasValue)
                    {
                        TreeNode document = new ObjectQuery<TreeNode>().WithGuid(post.FacebookPostDocumentGUID.Value).OnSite(post.FacebookPostSiteID).TopN(1).FirstOrDefault();
                        if (document == null)
                        {
                            // Related document does not exist so the post won't be published.
                            post.FacebookPostErrorCode = ERROR_CODE_DOCUMENT_NOT_EXIST;
                            return;
                        }

                        MacroResolver resolver = MacroResolver.GetInstance();
                        resolver.AddAnonymousSourceData(document);
                        post.FacebookPostText = resolver.ResolveMacros(post.FacebookPostText);
                    }

                    // Shorten URLs in the post content.
                    if (post.FacebookPostURLShortenerType != URLShortenerTypeEnum.None)
                    {
                        post.FacebookPostText = URLShortenerHelper.ShortenURLsInText(post.FacebookPostText, post.FacebookPostURLShortenerType, post.FacebookPostSiteID);
                    }

                    var appSecret = FacebookApplicationInfoProvider.GetFacebookApplicationInfo(account.FacebookAccountFacebookApplicationID).FacebookApplicationConsumerSecret;

                    post.FacebookPostExternalID = FacebookHelper.PublishPostOnFacebookPage(account.FacebookAccountPageID, account.FacebookPageAccessToken.AccessToken, post.FacebookPostText, appSecret);
                    post.FacebookPostPublishedDateTime = DateTime.Now;

                }
                catch (FacebookApiException ex)
                {
                    post.FacebookPostErrorCode = ex.ErrorCode;
                    post.FacebookPostErrorSubcode = ex.ErrorSubcode;
                    throw;
                }
                catch
                {
                    post.FacebookPostErrorCode = ERROR_CODE_UNKNOWN_ERROR;
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
        /// <param name="post">Facebook post.</param>
        /// <param name="user">User whose time zone will be used when formating date time information.</param>
        /// <param name="site">Site whose time zone will be used when formating date time information.</param>
        /// <param name="shortMessage">Indicates if short message has to be used instead of detail message.</param>
        /// <returns>Localized status message.</returns>
        protected virtual string GetPostPublishStateMessageInternal(FacebookPostInfo post, IUserInfo user = null, ISiteInfo site = null, bool shortMessage = false)
        {
            if (post.IsPublished)
            {
                string publishedTime = TimeZoneHelper.ConvertToUserTimeZone(post.FacebookPostPublishedDateTime.Value, true, user, site);
                string resourceString = shortMessage ? "sm.facebook.posts.msg.published.short" : "sm.facebook.posts.msg.published";

                return String.Format(ResHelper.GetString(resourceString), publishedTime);
            }

            // Error occurred
            if (post.FacebookPostErrorCode.HasValue)
            {
                return shortMessage ? ResHelper.GetString("sm.facebook.posts.msg.failed") : GetFacebookPostErrorMessage(post.FacebookPostErrorCode.Value);
            }

            // Post relates to some document
            if (post.FacebookPostDocumentGUID.HasValue)
            {
                TreeNode document = new ObjectQuery<TreeNode>().WithGuid(post.FacebookPostDocumentGUID.Value).OnSite(post.FacebookPostSiteID).TopN(1).FirstOrDefault();
                if (document == null)
                {
                    // Post won't be published because related document doesn't exist
                    return ResHelper.GetString(shortMessage ? "sm.facebook.posts.msg.wontbepublished.short" : "sm.facebook.posts.msg.wontbepublished");
                }
                else if (post.FacebookPostPostAfterDocumentPublish && !post.FacebookPostScheduledPublishDateTime.HasValue)
                {
                    // Post will be posted to Facebook when the related document gets published
                    return ResHelper.GetString(shortMessage ? "sm.facebook.posts.msg.postondocumentpublish.short" : "sm.facebook.posts.msg.postondocumentpublish");
                }
            }

            // Post is scheduled
            if (post.FacebookPostScheduledPublishDateTime.HasValue)
            {
                string scheduledTime = TimeZoneHelper.ConvertToUserTimeZone(post.FacebookPostScheduledPublishDateTime.Value, true, user, site);
                if (DateTime.Compare(post.FacebookPostScheduledPublishDateTime.Value, DateTime.Now - POST_DELAY_TOLERANCE) <= 0)
                {
                    return shortMessage ? ResHelper.GetString("sm.facebook.posts.msg.failed") : String.Format(ResHelper.GetString("sm.facebook.posts.msg.schedulederror"), scheduledTime);
                }

                string resourceString = shortMessage ? "sm.facebook.posts.msg.scheduled.short" : "sm.facebook.posts.msg.scheduled";

                return String.Format(ResHelper.GetString(resourceString), scheduledTime);
            }

            return String.Empty;
        }


        /// <summary>
        /// Gets localized error message that describes the given Facebook's error code.
        /// </summary>
        /// <param name="errorCode">Error code that Facebook returned.</param>
        /// <returns>Localized error message.</returns>
        protected virtual string GetFacebookPostErrorMessage(int errorCode)
        {
            switch (errorCode)
            {
                // OAuth errors
                case 190:
                case 102:
                    return ResHelper.GetString("sm.facebook.posts.msg.oautherror");

                // Duplicate status error
                case 506:
                    return ResHelper.GetString("sm.facebook.posts.msg.duplicatestatuserror");

                // Server side errors
                case 1:
                case 2:
                    return ResHelper.GetString("sm.facebook.posts.msg.servererror");

                // API calls limit errors
                case 4:
                case 17:
                    return ResHelper.GetString("sm.facebook.posts.msg.apilimiterror");

                // Invalid account
                case ERROR_CODE_INVALID_ACCOUNT:
                    return ResHelper.GetString("sm.facebook.posts.msg.accounterror");

                // Invalid document
                case ERROR_CODE_DOCUMENT_NOT_EXIST:
                    return ResHelper.GetString("sm.facebook.posts.msg.documenterror");

                default:
                    // Permission denied errors
                    if (errorCode == 10 || (errorCode >= 200 && errorCode <= 299))
                    {
                        return ResHelper.GetString("sm.facebook.posts.msg.permissionerror");
                    }

                    // Unknown errors
                    return ResHelper.GetString("sm.facebook.posts.msg.unknownerror");
            }
        }
        
        #endregion
    }

}