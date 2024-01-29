using System;
using System.Linq;
using System.Net;

using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Globalization;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Scheduler;


namespace CMS.SocialMarketing
{    
    /// <summary>
    /// Provides management of LinkedIn posts.
    /// </summary>
    public class LinkedInPostInfoProvider : AbstractInfoProvider<LinkedInPostInfo, LinkedInPostInfoProvider>
    {
        #region "Public constants"

        /// <summary>
        /// Error code representing unknown error when publishing the post.
        /// </summary>
        public const int ERROR_CODE_UNKNOWN_ERROR = -1;


        /// <summary>
        /// Error code representing unexpected error with LinkedIn account (account doesn't exist or its access token isn't valid).
        /// </summary>
        public const int ERROR_CODE_INVALID_ACCOUNT = -2;


        /// <summary>
        /// Error code representing the post belongs to the document that doesn't exist.
        /// </summary>
        public const int ERROR_CODE_DOCUMENT_DOES_NOT_EXIST = -3;


        /// <summary>
        /// Error code representing post with too long comment.
        /// </summary>
        public const int ERROR_CODE_COMMENT_TOO_LONG = -4;


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
        /// Initializes a new instance of the LinkedInPostInfoProvider class.
        /// </summary>
        public LinkedInPostInfoProvider()
            : base(LinkedInPostInfo.TYPEINFO, new HashtableSettings
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
        /// Returns a query for all the LinkedInPostInfo objects.
        /// </summary>
        public static ObjectQuery<LinkedInPostInfo> GetLinkedInPosts()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns LinkedInPostInfo with specified ID.
        /// </summary>
        /// <param name="id">LinkedInPostInfo ID</param>
        public static LinkedInPostInfo GetLinkedInPostInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Returns LinkedInPostInfo with specified GUID.
        /// </summary>
        /// <param name="guid">LinkedInPostInfo GUID</param>                
        public static LinkedInPostInfo GetLinkedInPostInfo(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Sets (updates or inserts) specified LinkedInPostInfo.
        /// </summary>
        /// <param name="infoObj">LinkedInPostInfo to be set</param>
        public static void SetLinkedInPostInfo(LinkedInPostInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified LinkedInPostInfo.
        /// </summary>
        /// <param name="infoObj">LinkedInPostInfo to be deleted</param>
        /// <exception cref="LinkedInPartialDeleteException">Thrown when the deleted post has already been published on LiknedIn. In such case the post is deleted locally only.</exception>
        public static void DeleteLinkedInPostInfo(LinkedInPostInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes LinkedInPostInfo with specified ID.
        /// </summary>
        /// <param name="id">LinkedInPostInfo ID</param>
        /// <exception cref="LinkedInPartialDeleteException">Thrown when the deleted post has already been published on LiknedIn. In such case the post is deleted locally only.</exception>
        public static void DeleteLinkedInPostInfo(int id)
        {
            LinkedInPostInfo infoObj = GetLinkedInPostInfo(id);
            DeleteLinkedInPostInfo(infoObj);
        }

        #endregion


        #region "Public methods - Advanced"


        /// <summary>
        /// Returns a query for all the LinkedInPostInfo objects of a specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        public static ObjectQuery<LinkedInPostInfo> GetLinkedInPosts(SiteInfoIdentifier siteId)
        {
            return ProviderObject.GetLinkedInPostsInternal(siteId);
        }


        /// <summary>
        /// Retrieves an object query of LinkedIn posts for the specified LinkedIn account (company profile) identifier, and returns it.
        /// </summary>
        /// <param name="linkedInAccountId">LinkedIn account (company profile) identifier.</param>
        /// <returns>An object query of LinkedIn posts for the specified account (company profile).</returns>
        public static ObjectQuery<LinkedInPostInfo> GetLinkedInPostInfosByAccountId(int linkedInAccountId)
        {
            return ProviderObject.GetLinkedInPostInfosByAccountIdInternal(linkedInAccountId);
        }


        /// <summary>
        /// Retrieves an object query of LinkedIn posts for the specified document identifier, and returns it.
        /// </summary>
        /// <param name="documentGuid">Document identifier.</param>
        /// <param name="documentSiteId">Document site identifier.</param>
        /// <returns>An object query of LinkedIn posts for the specified document.</returns>
        public static ObjectQuery<LinkedInPostInfo> GetLinkedInPostInfosByDocumentGuid(Guid documentGuid, int documentSiteId)
        {
            return ProviderObject.GetLinkedInPostInfosByDocumentGuidInternal(documentGuid, documentSiteId);
        }


        /// <summary>
        /// Publishes the LinkedIn post on the appropriate LinkedIn account (company profile) in proper time.
        /// Uses scheduler for future posts.
        /// When modifying scheduled posts, always call the <see cref="TryCancelScheduledPublishLinkedInPost(int)"/> first.
        /// </summary>
        /// <seealso cref="TryCancelScheduledPublishLinkedInPost(int)"/>
        /// <seealso cref="TryCancelScheduledPublishLinkedInPost(LinkedInPostInfo)"/>
        /// <param name="postInfoId">Identifier of the LinkedInPostInfo that will be published.</param>
        /// <exception cref="Exception">When post does not exist, has already been published to LinkedIn or the account is not valid.</exception>
        /// <exception cref="ArgumentException">Thrown when authorization or LinkedIn's company ID resulting from accountId is null, or when comment is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when comment is longer than <see cref="LinkedInHelper.COMPANY_SHARE_COMMENT_MAX_LENGTH"/></exception>
        /// <exception cref="LinkedInApiUnauthorizedException">Thrown when protocol error occurs because the request is not authorized properly.</exception>
        /// <exception cref="LinkedInApiException">Thrown when protocol error occurs.</exception>
        public static void PublishLinkedInPost(int postInfoId)
        {
            ProviderObject.PublishLinkedInPostInternal(postInfoId);
        }


        /// <summary>
        /// Tries to cancel scheduled publish of LinkedIn post. If successful, the post can be modified using <see cref="PublishLinkedInPost"/>.
        /// (Has to be called before modification since LinkedIn does not allow published posts to be modified).
        /// </summary>
        /// <param name="postInfoId">Identifier of the LinkedInPostInfo that shall be canceled.</param>
        /// <returns>True if successfully canceled, false if the post has already been published.</returns>
        public static bool TryCancelScheduledPublishLinkedInPost(int postInfoId)
        {
            LinkedInPostInfo post = GetLinkedInPostInfo(postInfoId);
            return ProviderObject.TryCancelScheduledPublishLinkedInPostInternal(post);
        }


        /// <summary>
        /// Tries to cancel scheduled publish of LinkedIn post. If successful, the post can be modified using <see cref="PublishLinkedInPost"/>.
        /// (Has to be called before modification since LinkedIn does not allow published posts to be modified).
        /// </summary>
        /// <param name="postInfo">The LinkedInPostInfo that shall be canceled.</param>
        /// <returns>True if successfully canceled, false if the post has already been published.</returns>
        public static bool TryCancelScheduledPublishLinkedInPost(LinkedInPostInfo postInfo)
        {
            return ProviderObject.TryCancelScheduledPublishLinkedInPostInternal(postInfo);
        }


        /// <summary>
        /// Gets localized message that describes the post's publish state.
        /// </summary>
        /// <param name="post">LinkedIn post.</param>
        /// <param name="user">User whose time zone will be used when formating date time information.</param>
        /// <param name="site">Site whose time zone will be used when formating date time information.</param>
        /// <param name="shortMessage">Indicates if short message has to be used instead of detail message.</param>
        /// <returns>Localized status message.</returns>
        public static string GetPostPublishStateMessage(LinkedInPostInfo post, IUserInfo user = null, ISiteInfo site = null, bool shortMessage = false)
        {
            return ProviderObject.GetPostPublishStateMessageInternal(post, user, site, shortMessage);
        }
        
        #endregion


        #region "Internal interface"

        /// <summary>
        /// Retrieves an object query of LinkedIn posts which have outdated insights for the specified LinkedIn account (company profile) identifier, and returns it.
        /// Insights are considered outdated when they haven't been updated today.
        /// </summary>
        /// <param name="accountId">LinkedIn account (page) identifier.</param>
        /// <returns>An object query of LinkedIn posts which have outdated insights for the specified account (company profile).</returns>
        internal static ObjectQuery<LinkedInPostInfo> GetLinkedInPostInfosWithOutdatedInsights(int accountId)
        {
            return ProviderObject.GetObjectQuery()
                .WhereEquals("LinkedInPostLinkedInAccountID", accountId)
                .WhereNotEmpty("LinkedInPostUpdateKey")
                .Where(new WhereCondition()
                    .WhereNull("LinkedInPostInsightsLastUpdated")
                    .Or()
                    .Where("LinkedInPostInsightsLastUpdated", QueryOperator.LessThan, DateTime.Now.Date)
                );
        }


        /// <summary>
        /// Publishes the LinkedIn post on the appropriate LinkedIn account (company profile) immediately.
        /// </summary>
        /// <param name="postInfoId">Identifier of the LinkedInPostInfo that will be published.</param>
        /// <exception cref="Exception">When post does not exist, has already been published to LinkedIn or the account is not valid.</exception>
        /// <exception cref="ArgumentException">Thrown when authorization or LinkedIn's company ID resulting from accountId is null, or when comment is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when comment is longer than <see cref="LinkedInHelper.COMPANY_SHARE_COMMENT_MAX_LENGTH"/></exception>
        /// <exception cref="LinkedInApiUnauthorizedException">Thrown when protocol error occurs because the request is not authorized properly.</exception>
        /// <exception cref="LinkedInApiException">Thrown when protocol error occurs.</exception>
        internal static void PublishLinkedInPostToLinkedIn(int postInfoId)
        {
            ProviderObject.PublishLinkedInPostToLinkedInInternal(postInfoId);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(LinkedInPostInfo info)
        {
            if (info == null)
            {
                return;
            }

            TaskInfo scheduledTask = TaskInfoProvider.GetTaskInfo(String.Format(SocialMarketingPostPublishingTask.TASK_CODENAME_FORMAT_LINKEDIN, info.LinkedInPostID), info.LinkedInPostSiteID);
            if (scheduledTask != null)
            {
                TaskInfoProvider.DeleteTaskInfo(scheduledTask);
            }

            bool alreadyPosted = !String.IsNullOrEmpty(info.LinkedInPostUpdateKey);

            base.DeleteInfo(info);

            if (alreadyPosted)
            {
                throw new LinkedInPartialDeleteException("The LinkedIn post has already been posted on LinkedIn. Only the local copy of the post has been deleted.");
            }
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Returns a query for all the LinkedInPostInfo objects of a specified site.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        protected virtual ObjectQuery<LinkedInPostInfo> GetLinkedInPostsInternal(SiteInfoIdentifier siteId)
        {
            return GetObjectQuery().OnSite(siteId);
        }


        /// <summary>
        /// Retrieves an object query of LinkedIn posts for the specified LinkedIn account (company profile) identifier, and returns it.
        /// </summary>
        /// <param name="linkedInAccountId">LinkedIn account (company profile) identifier.</param>
        /// <returns>An object query of LinkedIn posts for the specified account (company profile).</returns>
        protected virtual ObjectQuery<LinkedInPostInfo> GetLinkedInPostInfosByAccountIdInternal(int linkedInAccountId)
        {
            return ProviderObject.GetObjectQuery().Where("LinkedInPostLinkedInAccountID", QueryOperator.Equals, linkedInAccountId);
        }


        /// <summary>
        /// Retrieves an object query of LinkedIn posts for the specified document identifier, and returns it.
        /// </summary>
        /// <param name="documentGuid">Document identifier.</param>
        /// <param name="documentSiteId">Document site identifier.</param>
        /// <returns>An object query of LinkedIn posts for the specified document.</returns>
        protected virtual ObjectQuery<LinkedInPostInfo> GetLinkedInPostInfosByDocumentGuidInternal(Guid documentGuid, int documentSiteId)
        {
            var query = ProviderObject.GetObjectQuery().WhereEquals("LinkedInPostDocumentGUID", documentGuid);
            if (documentSiteId > 0)
            {
                query.WhereEquals("LinkedInPostSiteID", documentSiteId);
            }

            return query;
        }


        /// <summary>
        /// Publishes the LinkedIn post on the appropriate LinkedIn account (company profile) in proper time.
        /// Uses scheduler for future posts.
        /// </summary>
        /// <param name="postInfoId">Identifier of the LinkedInPostInfo that will be published.</param>
        /// <exception cref="Exception">When post does not exist, has already been published to LinkedIn or the account is not valid.</exception>
        /// <exception cref="ArgumentException">Thrown when authorization or LinkedIn's company ID resulting from accountId is null, or when comment is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when comment is longer than <see cref="LinkedInHelper.COMPANY_SHARE_COMMENT_MAX_LENGTH"/></exception>
        /// <exception cref="LinkedInApiUnauthorizedException">Thrown when protocol error occurs because the request is not authorized properly.</exception>
        /// <exception cref="LinkedInApiException">Thrown when protocol error occurs.</exception>
        protected virtual void PublishLinkedInPostInternal(int postInfoId)
        {
            LinkedInPostInfo postInfo = GetLinkedInPostInfo(postInfoId);

            // Post does not exist or has already been publised
            if ((postInfo == null) || !String.IsNullOrWhiteSpace(postInfo.LinkedInPostUpdateKey))
            {
                throw new Exception(String.Format("[LinkedInPostInfoProvider.PublishLinkedInPostInternal]: LinkedIn post with ID {0} does not exist or has been already published.", postInfoId));
            }

            if (!postInfo.LinkedInPostScheduledPublishDateTime.HasValue || (DateTime.Compare(postInfo.LinkedInPostScheduledPublishDateTime.Value, DateTime.Now + IMMEDIATE_PUBLISH_TOLERANCE) <= 0))
            {
                // Post gets published immediately
                PublishLinkedInPostToLinkedInInternal(postInfoId);
            }
            else
            {
                // Post has to have a scheduled task
                TaskInterval interval = new TaskInterval
                {
                    StartTime = postInfo.LinkedInPostScheduledPublishDateTime.Value,
                    Period = SchedulingHelper.PERIOD_ONCE
                };

                TaskInfo scheduledTask = new TaskInfo
                {
                    TaskDisplayName = "{$sm.linkedin.post.scheduledtask.name$}",
                    TaskName = String.Format(SocialMarketingPostPublishingTask.TASK_CODENAME_FORMAT_LINKEDIN, postInfo.LinkedInPostID),
                    TaskAssemblyName = "CMS.SocialMarketing",
                    TaskClass = "CMS.SocialMarketing.SocialMarketingPostPublishingTask",
                    TaskDeleteAfterLastRun = true,
                    TaskSiteID = postInfo.LinkedInPostSiteID,
                    TaskAllowExternalService = true,
                    TaskUseExternalService = true,
                    TaskRunInSeparateThread = true,
                    TaskEnabled = true,
                    TaskType = ScheduledTaskTypeEnum.System,
                    TaskData = postInfo.LinkedInPostID.ToString(),
                    TaskInterval = SchedulingHelper.EncodeInterval(interval)
                };

                // Set task activation time
                scheduledTask.TaskNextRunTime = SchedulingHelper.GetFirstRunTime(interval);

                TaskInfoProvider.SetTaskInfo(scheduledTask);
            }
        }


        /// <summary>
        /// Tries to cancel scheduled publish of LinkedIn post. If successful, the post can be modified using <see cref="PublishLinkedInPost"/>.
        /// (Has to be called before modification since LinkedIn does not allow published posts to be modified).
        /// </summary>
        /// <param name="postInfo">The LinkedInPostInfo that shall be canceled.</param>
        /// <returns>True if successfully canceled (or neither post or scheduled task does not exist), false if the post has already been published (or the task is already running).</returns>
        protected virtual bool TryCancelScheduledPublishLinkedInPostInternal(LinkedInPostInfo postInfo)
        {
            if ((postInfo == null) || !postInfo.LinkedInPostScheduledPublishDateTime.HasValue)
            {
                return true;
            }

            if (!String.IsNullOrEmpty(postInfo.LinkedInPostUpdateKey))
            {
                return false;
            }

            TaskInfo scheduledTask = TaskInfoProvider.GetTaskInfo(String.Format(SocialMarketingPostPublishingTask.TASK_CODENAME_FORMAT_LINKEDIN, postInfo.LinkedInPostID), postInfo.LinkedInPostSiteID);
            if (scheduledTask != null)
            {
                TaskInfoProvider.DeleteTaskInfo(scheduledTask);
                return true;
            }
            else if (String.IsNullOrEmpty(postInfo.LinkedInPostUpdateKey))
            {
                // The scheduled task has been deleted by user via UI before post publishing.
                return true;
            }
            return false;
        }


        /// <summary>
        /// Publishes the LinkedIn post on the appropriate LinkedIn account (company profile). Throws an exception if something goes wrong.
        /// </summary>
        /// <param name="postInfoId">Identifier of the LinkedInPostInfo that will be published.</param>
        /// <exception cref="Exception">When post does not exist, has already been published to LinkedIn or the account is not valid.</exception>
        /// <exception cref="ArgumentException">Thrown when authorization or LinkedIn's company ID resulting from accountId is null, or when comment is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when comment is longer than <see cref="LinkedInHelper.COMPANY_SHARE_COMMENT_MAX_LENGTH"/></exception>
        /// <exception cref="LinkedInApiUnauthorizedException">Thrown when protocol error occurs because the request is not authorized properly.</exception>
        /// <exception cref="LinkedInApiException">Thrown when protocol error occurs.</exception>
        protected virtual void PublishLinkedInPostToLinkedInInternal(int postInfoId)
        {
            using (var scope = BeginTransaction())
            {
                LinkedInPostInfo post = GetLinkedInPostInfo(postInfoId);
                if (post == null || !String.IsNullOrWhiteSpace(post.LinkedInPostUpdateKey))
                {
                    throw new Exception(String.Format("[LinkedInPostInfoProvider.PublishLinkedInPostToLinkedInInternal]: LinkedIn post with ID {0} does not exist or has been already published.", postInfoId));
                }

                try
                {
                    post.LinkedInPostErrorCode = 0;
                    post.LinkedInPostErrorMessage = null;

                    LinkedInAccountInfo account = LinkedInAccountInfoProvider.GetLinkedInAccountInfo(post.LinkedInPostLinkedInAccountID);
                    if ((account == null)
                        || String.IsNullOrWhiteSpace(account.LinkedInAccountAccessToken))
                    {
                        post.LinkedInPostErrorCode = ERROR_CODE_INVALID_ACCOUNT;
                        throw new Exception(String.Format("[LinkedInPostInfoProvider.PublishLinkedInPostToLinkedInInternal]: LinkedIn account with ID {0} does not exist or its access token is invalid.", post.LinkedInPostLinkedInAccountID));
                    }

                    // Resolve macros in the post content.
                    if (post.LinkedInPostDocumentGUID.HasValue)
                    {
                        TreeNode document = new ObjectQuery<TreeNode>().WithGuid(post.LinkedInPostDocumentGUID.Value).OnSite(post.LinkedInPostSiteID).TopN(1).FirstOrDefault();
                        if (document == null)
                        {
                            // Related document does not exist so the post won't be published.
                            post.LinkedInPostErrorCode = ERROR_CODE_DOCUMENT_DOES_NOT_EXIST;
                            return;
                        }

                        MacroResolver resolver = MacroResolver.GetInstance();
                        resolver.AddAnonymousSourceData(document);
                        post.LinkedInPostComment = resolver.ResolveMacros(post.LinkedInPostComment);
                    }

                    // Shorten URLs in the post content.
                    if (post.LinkedInPostURLShortenerType != URLShortenerTypeEnum.None)
                    {
                        post.LinkedInPostComment = URLShortenerHelper.ShortenURLsInText(post.LinkedInPostComment, post.LinkedInPostURLShortenerType, post.LinkedInPostSiteID);
                    }

                    post.LinkedInPostUpdateKey = LinkedInHelper.PublishShareOnLinkedInCompanyProfile(account.LinkedInAccountID, post.LinkedInPostComment);
                    post.LinkedInPostHTTPStatusCode = (int)HttpStatusCode.Created;
                    post.LinkedInPostPublishedDateTime = DateTime.Now;

                }
                catch (ArgumentOutOfRangeException ex)
                {
                    post.LinkedInPostErrorCode = ERROR_CODE_COMMENT_TOO_LONG;
                    post.LinkedInPostErrorMessage = ex.Message;
                    throw;
                }
                catch (ArgumentException ex)
                {
                    post.LinkedInPostErrorCode = ERROR_CODE_INVALID_ACCOUNT;
                    post.LinkedInPostErrorMessage = ex.Message;
                    throw;
                }
                catch (LinkedInApiException ex)
                {
                    // Note: LinkedInApiUnauthorizedException is handled here as well
                    post.LinkedInPostHTTPStatusCode = ex.HttpStatus;
                    post.LinkedInPostErrorCode = ex.ErrorCode;
                    post.LinkedInPostErrorMessage = ex.Message;
                    throw;
                }
                catch (Exception ex)
                {
                    post.LinkedInPostErrorCode = ERROR_CODE_UNKNOWN_ERROR;
                    post.LinkedInPostErrorMessage = ex.Message;
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
        /// <param name="post">LinkedIn post.</param>
        /// <param name="user">User whose time zone will be used when formating date time information.</param>
        /// <param name="site">Site whose time zone will be used when formating date time information.</param>
        /// <param name="shortMessage">Indicates if short message has to be used instead of detail message.</param>
        /// <returns>Localized status message.</returns>
        protected virtual string GetPostPublishStateMessageInternal(LinkedInPostInfo post, IUserInfo user = null, ISiteInfo site = null, bool shortMessage = false)
        {
            if (post.IsPublished)
            {
                string publishedTime = TimeZoneHelper.ConvertToUserTimeZone(post.LinkedInPostPublishedDateTime.Value, true, user, site);
                string resourceString = shortMessage ? "sm.linkedin.posts.msg.published.short" : "sm.linkedin.posts.msg.published";

                return String.Format(ResHelper.GetString(resourceString), publishedTime);
            }

            // Error occured
            if (post.IsFaulty)
            {
                return shortMessage ? ResHelper.GetString("sm.linkedin.posts.msg.failed") : GetLinkedInPostErrorMessage(post.LinkedInPostHTTPStatusCode, post.LinkedInPostErrorCode, post.LinkedInPostErrorMessage);
            }

            // Post relates to some document
            if (post.LinkedInPostDocumentGUID.HasValue)
            {
                TreeNode document = new ObjectQuery<TreeNode>().WithGuid(post.LinkedInPostDocumentGUID.Value).OnSite(post.LinkedInPostSiteID).TopN(1).FirstOrDefault();
                if (document == null)
                {
                    // Post won't be published because related document doesn't exist
                    return ResHelper.GetString(shortMessage ? "sm.linkedin.posts.msg.wontbepublished.short" : "sm.linkedin.posts.msg.wontbepublished");
                }
                else if (post.LinkedInPostPostAfterDocumentPublish && !post.LinkedInPostScheduledPublishDateTime.HasValue)
                {
                    // Post will be posted to LinkedIn when the related document gets published
                    return ResHelper.GetString(shortMessage ? "sm.linkedin.posts.msg.postondocumentpublish.short" : "sm.linkedin.posts.msg.postondocumentpublish");
                }
            }

            // Post is scheduled
            if (post.LinkedInPostScheduledPublishDateTime.HasValue)
            {
                string scheduledTime = TimeZoneHelper.ConvertToUserTimeZone(post.LinkedInPostScheduledPublishDateTime.Value, true, user, site);
                if (DateTime.Compare(post.LinkedInPostScheduledPublishDateTime.Value, DateTime.Now - POST_DELAY_TOLERANCE) <= 0)
                {
                    return shortMessage ? ResHelper.GetString("sm.linkedin.posts.msg.failed") : String.Format(ResHelper.GetString("sm.linkedin.posts.msg.schedulederror"), scheduledTime);
                }

                string resourceString = shortMessage ? "sm.linkedin.posts.msg.scheduled.short" : "sm.linkedin.posts.msg.scheduled";

                return String.Format(ResHelper.GetString(resourceString), scheduledTime);
            }

            return String.Empty;
        }


        /// <summary>
        /// Gets localized error message that describes the given LinkedIn's error code.
        /// </summary>
        /// <param name="httpStatusCode">HTTP status code returned when publishing to LinkedIn.</param>
        /// <param name="errorCode">Error code that LinkedIn returned in the response body. Or custom error (negative value)</param>
        /// <param name="errorMessage">Error message that LinkedIn returned in the response body.</param>
        /// <returns>Localized error message.</returns>
        protected virtual string GetLinkedInPostErrorMessage(int httpStatusCode, int errorCode, string errorMessage)
        {
            // Try to determine the cause by HTTP status code first
            if (httpStatusCode == 201)
            {
                // Status code 201 (Created) is returned in case of successful creation
                return ResHelper.GetString("sm.linkedin.posts.msg.created");
            }

            if ((httpStatusCode) == 400 && (errorMessage != null))
            {
                if (errorMessage.EqualsCSafe("Do not post duplicate content"))
                {
                    // Double post of same comment within short time period
                    return ResHelper.GetString("sm.linkedin.posts.msg.duplicatestatuserror");
                }
                if (errorMessage.StartsWithCSafe("Invalid value {") && errorMessage.EndsWithCSafe("} in key {id}"))
                {
                    // Invalid LinkedIn's company ID
                    return ResHelper.GetString("sm.linkedin.posts.msg.invalidcompanyid");
                }
            }

            if (httpStatusCode == 401)
            {
                // The root cause is not being authorized properly
                if (errorMessage != null)
                {
                    // Try to determine the cause more precisely
                    if (errorMessage.StartsWithCSafe("[unauthorized]. No consumer found for key"))
                    {
                        return ResHelper.GetString("sm.linkedin.posts.msg.unauthorized.noconsumerforkey");
                    }
                    if (errorMessage.StartsWithCSafe("[unauthorized]. The token used in the OAuth request is not valid."))
                    {
                        return ResHelper.GetString("sm.linkedin.posts.msg.unauthorized.invalidusertoken");
                    }
                    if (errorMessage.StartsWithCSafe("[unauthorized]. OAU:"))
                    {
                        return ResHelper.GetString("sm.linkedin.posts.msg.unauthorized.invalidsecret");
                    }
                }

                // Or return just common information about any element of the OAuth tuple to be invalid
                return ResHelper.GetString("sm.linkedin.posts.msg.unauthorized");
            }

            // Try to determine the cause by LinkedIn's error code. The negative code numbers are used by us.
            switch (errorCode)
            {
                // Invalid account
                case ERROR_CODE_INVALID_ACCOUNT:
                    return ResHelper.GetString("sm.linkedin.posts.msg.accounterror");

                // Invalid document
                case ERROR_CODE_DOCUMENT_DOES_NOT_EXIST:
                    return ResHelper.GetString("sm.linkedin.posts.msg.documenterror");

                // Share's comment too long
                case ERROR_CODE_COMMENT_TOO_LONG:
                    return String.Format(ResHelper.GetString("sm.linkedin.posts.msg.commenttoolong"), LinkedInHelper.COMPANY_SHARE_COMMENT_MAX_LENGTH);

                case ERROR_CODE_UNKNOWN_ERROR:
                    return ResHelper.GetString("sm.linkedin.posts.msg.unknownerror");
            }

            return ResHelper.GetString("sm.linkedin.posts.msg.unknownerror");
        }
        
        #endregion		
    }
}