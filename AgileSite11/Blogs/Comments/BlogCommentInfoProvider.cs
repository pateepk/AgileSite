using System;
using System.Data;
using System.Security.Principal;

using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Membership;
using CMS.Search;
using CMS.Base;

namespace CMS.Blogs
{
    /// <summary>
    /// Class providing BlogCommentInfo management.
    /// </summary>
    public class BlogCommentInfoProvider : AbstractInfoProvider<BlogCommentInfo, BlogCommentInfoProvider>
    {
        #region "Properties"

        private static bool mEnableEmails = true;

        /// <summary>
        /// Indicates if e-mails are allowed to be sent to subscribers and moderators, by default it is set to True.
        /// </summary>
        public static bool EnableEmails
        {
            get
            {
                return mEnableEmails && CMSActionContext.CurrentSendEmails;
            }
            set
            {
                mEnableEmails = value;
            }
        }

        #endregion


        #region "Public static methods"

        /// <summary>
        /// Returns the BlogCommentInfo structure for the specified blogComment.
        /// </summary>
        /// <param name="blogCommentId">BlogComment id</param>
        public static BlogCommentInfo GetBlogCommentInfo(int blogCommentId)
        {
            return ProviderObject.GetInfoById(blogCommentId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified blog comment.
        /// </summary>
        /// <param name="blogComment">Blog comment to set</param>
        public static void SetBlogCommentInfo(BlogCommentInfo blogComment)
        {
            ProviderObject.SetBlogCommentInfoInternal(blogComment);
        }


        /// <summary>
        /// Deletes specified blogComment.
        /// </summary>
        /// <param name="blogCommentObj">BlogComment object</param>
        public static void DeleteBlogCommentInfo(BlogCommentInfo blogCommentObj)
        {
            ProviderObject.DeleteBlogCommentInfoInternal(blogCommentObj);
        }


        /// <summary>
        /// Deletes specified blogComment.
        /// </summary>
        /// <param name="blogCommentId">BlogComment id</param>
        public static void DeleteBlogCommentInfo(int blogCommentId)
        {
            BlogCommentInfo blogCommentObj = GetBlogCommentInfo(blogCommentId);
            DeleteBlogCommentInfo(blogCommentObj);
        }


        /// <summary>
        /// Returns object query for blog comments.
        /// </summary>
        public static ObjectQuery<BlogCommentInfo> GetBlogComments()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns all comments.
        /// </summary>
        /// <param name="where">Where condition to filter data</param>
        /// <param name="orderBy">Order by statement</param>
        /// <param name="topN">Specifies number of returned records</param>
        /// <param name="columns">Data columns to return</param>
        [Obsolete("Use CMS.DataEngine.ObjectQuery<BlogCommentInfo> instead")]
        public static DataSet GetAllComments(string where, string orderBy = null, int topN = -1, string columns = null)
        {
            return GetBlogComments()
                        .TopN(topN)
                        .Columns(columns)
                        .Where(where)
                        .OrderBy(orderBy);
        }


        /// <summary>
        /// Returns all comments.
        /// </summary>
        /// <param name="where">Where condition to filter data</param>
        /// <param name="orderBy">Order by statement</param>
        /// <param name="topN">Specifies number of returned records</param>
        /// <param name="columns">Data columns to return</param>
        /// <param name="offset">Index of first record to get</param>
        /// <param name="maxRecords">Maximum number of records to get. If maxRecords is zero or less, all records are returned (no paging is used)</param>
        /// <param name="totalRecords">Returns total records</param>
        [Obsolete("Use CMS.DataEngine.ObjectQuery<BlogCommentInfo> instead")]
        public static DataSet GetAllComments(string where, string orderBy, int topN, string columns, int offset, int maxRecords, ref int totalRecords)
        {
            var query = GetBlogComments()
                        .TopN(topN)
                        .Columns(columns)
                        .Where(where)
                        .OrderBy(orderBy);

            query.Offset = offset;
            query.MaxRecords = maxRecords;

            var result = query.TypedResult;
            totalRecords = query.TotalRecords;

            return result;
        }


        /// <summary>
        /// Returns DataSet with all owner comments waiting for approval.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="ownerID">Owner ID</param>
        [Obsolete("Use CMS.DataEngine.ObjectQuery<BlogCommentInfo> instead")]
        public static DataSet GetCommentsWaitingForApproval(string siteName, int ownerID)
        {
            return ProviderObject.GetCommentsWaitingForApprovalInternal(siteName, ownerID);
        }


        /// <summary>
        /// Returns DataSet with all comments which belongs to the specified user or which are moderated by specified user.
        /// </summary>
        /// <param name="ownerId">Blog owner (user id)</param>
        /// <param name="moderator">Blog moderator (user name)</param>
        /// <param name="where">Where condition which specifies comments to be returned</param>
        /// <param name="blogWhere">Where condition which specifies blogs the comments should be returned from</param>
        /// <param name="columns">Columns to be selected</param>
        /// <param name="topN">Selects only top N items</param>
        /// <param name="order">Order by</param>
        /// <param name="offset">Index of first record to get</param>
        /// <param name="maxRecords">Maximum number of records to get. If maxRecords is zero or less, all records are returned (no paging is used)</param>
        /// <param name="totalRecords">Returns total records</param>
        /// <param name="siteName">Site name of the blog</param>
        public static DataSet GetComments(int ownerId, string moderator, string where, string blogWhere, string columns, int topN, string order, int offset, int maxRecords, ref int totalRecords, string siteName)
        {
            return ProviderObject.GetCommentsInternal(ownerId, moderator, where, blogWhere, columns, topN, order, offset, maxRecords, ref totalRecords, siteName);
        }


        /// <summary>
        /// Returns DataSet with post comments which are not marked as spam.
        /// </summary>
        /// <param name="postDocumentId">Post documentID</param>
        /// <param name="onlyApproved">Indicates if only approved comments should be returned</param>
        /// <param name="includingSpam">Indicates if comments which are marked as SPAM should be included in the result</param>
        public static DataSet GetPostComments(int postDocumentId, bool onlyApproved, bool includingSpam = false)
        {
            return ProviderObject.GetPostCommentsInternal(postDocumentId, onlyApproved, includingSpam);
        }


        /// <summary>
        /// Returns count of the post comments.
        /// </summary>
        /// <param name="postDocumentId">Post documentID</param>
        /// <param name="onlyApproved">Indicates if only approved comments should be included in the count</param>
        /// <param name="includingSpam">Indicates if comments which are marked as SPAM should be included in the count</param>
        public static int GetPostCommentsCount(int postDocumentId, bool onlyApproved, bool includingSpam = false)
        {
            return ProviderObject.GetPostCommentsCountInternal(postDocumentId, onlyApproved, includingSpam);
        }


        /// <summary>
        /// Sends a notification e-mail to blog post subscribers, to blog moderators and to blog owner.
        /// </summary>
        /// <param name="comment">Blog comment data</param>
        /// <param name="toSubscribers">Indicates if notification email should be sent to blog post subscribers</param>
        /// <param name="toModerators">Indicates if notification email should be sent to blog moderators</param>
        /// <param name="toBlogOwner">Indicates if notification email should be sent to blog owner</param>
        public static void SendNewCommentNotification(BlogCommentInfo comment, bool toSubscribers, bool toModerators, bool toBlogOwner)
        {
            ProviderObject.SendNewCommentNotificationInternal(comment, toSubscribers, toModerators, toBlogOwner);
        }


        /// <summary>
        /// Return all blog comments for specified document
        /// </summary>
        /// <param name="documentId">Document ID</param>
        public static DataSet GetApprovedComments(int documentId)
        {
            return GetBlogComments()
                        .WhereEquals("CommentPostDocumentID", documentId)
                        .WhereTrue("CommentApproved");
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Returns WHERE condition for the comments selection according to the specified parameters.
        /// </summary>
        /// <param name="postDocumentId">Post document ID</param>
        /// <param name="onlyApproved">Indicates if only approved comments should be included</param>
        /// <param name="includingSpam">Indicates if comments which are marked as SPAM should be included</param>
        internal static WhereCondition GetCommentsWhereCondition(int postDocumentId, bool onlyApproved, bool includingSpam)
        {
            WhereCondition condition = new WhereCondition().WhereEquals("CommentPostDocumentID", postDocumentId);

            // Select comments which are marked as spam
            if (!includingSpam)
            {
                condition.WhereEqualsOrNull("CommentIsSpam", false);
            }

            // Select only approved comments
            if (onlyApproved)
            {
                condition.WhereTrue("CommentApproved");
            }

            return condition;
        }


        /// <summary>
        /// Returns sitename with dependence on selected document id.
        /// </summary>
        /// <param name="documentId">Document id</param>
        private static string GetCommentSiteName(int documentId)
        {
            var site = TreePathUtils.GetDocumentSite(documentId);
            return (site != null) ? site.SiteName : null;
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Sets (updates or inserts) specified blog comment.
        /// </summary>
        /// <param name="blogComment">Blog comment to set</param>
        protected virtual void SetBlogCommentInfoInternal(BlogCommentInfo blogComment)
        {
            // Check license
            if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, "") != "")
            {
                LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.Blogs);
            }

            if (blogComment != null)
            {
                bool? addPoints = null;
                bool emailToSubscribers = false;
                bool emailToModerators = false;
                bool emailToOwner = false;

                // Set comment info
                blogComment.SetValue("CommentInfo", blogComment.CommentInfo.GetData());

                if (blogComment.CommentID > 0)
                {
                    // Get old comment data                
                    BlogCommentInfo oldBlogComment = GetBlogCommentInfo(blogComment.CommentID);
                    if (oldBlogComment != null)
                    {
                        // Rejected -> Approved
                        if ((!oldBlogComment.CommentApproved) && (blogComment.CommentApproved))
                        {
                            addPoints = true;
                            emailToSubscribers = true;
                        }
                        // Approved -> Rejected
                        else if ((oldBlogComment.CommentApproved) && (!blogComment.CommentApproved))
                        {
                            addPoints = false;
                        }
                    }

                    // Update existing comment
                    blogComment.Generalized.UpdateData();
                }
                else
                {
                    emailToOwner = true;

                    if (blogComment.CommentApproved)
                    {
                        addPoints = true;
                        emailToSubscribers = true;
                    }
                    else
                    {
                        emailToModerators = true;
                    }

                    // Save new comment
                    blogComment.Generalized.InsertData();
                }

                // Send new comment notification to subscribers, moderators and blog owner
                SendNewCommentNotification(blogComment, emailToSubscribers, emailToModerators, emailToOwner);

                if (addPoints != null)
                {
                    string siteName = GetCommentSiteName(blogComment.CommentPostDocumentID);
                    if (!String.IsNullOrEmpty(siteName))
                    {
                        // Add activity points 
                        BadgeInfoProvider.UpdateActivityPointsToUser(ActivityPointsEnum.BlogCommentPost, blogComment.CommentUserID, siteName, (bool)addPoints);
                    }
                }

                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);
                TreeNode node = tree.SelectSingleDocument(blogComment.CommentPostDocumentID);

                // Update search index for given document
                if ((node != null) && DocumentHelper.IsSearchTaskCreationAllowed(node))
                {
                    SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Update, TreeNode.OBJECT_TYPE, SearchFieldsConstants.ID, node.GetSearchID(), node.DocumentID);
                }
            }
            else
            {
                throw new Exception("[BlogCommentInfoProvider.SetBlogCommentInfo]: No BlogCommentInfo object set.");
            }
        }


        /// <summary>
        /// Deletes specified blogComment.
        /// </summary>
        /// <param name="blogCommentObj">BlogComment object</param>
        protected virtual void DeleteBlogCommentInfoInternal(BlogCommentInfo blogCommentObj)
        {
            if (blogCommentObj != null)
            {
                int userId = blogCommentObj.CommentUserID;
                int documentId = blogCommentObj.CommentPostDocumentID;

                blogCommentObj.Generalized.DeleteData();

                if (userId > 0)
                {
                    string siteName = GetCommentSiteName(documentId);
                    if (!String.IsNullOrEmpty(siteName))
                    {
                        // Add activity points 
                        BadgeInfoProvider.UpdateActivityPointsToUser(ActivityPointsEnum.BlogCommentPost, userId, siteName, false);
                    }
                }

                if (SearchIndexInfoProvider.SearchEnabled)
                {
                    TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);
                    TreeNode node = tree.SelectSingleDocument(blogCommentObj.CommentPostDocumentID);

                    // Update search index for given document
                    if ((node != null) && DocumentHelper.IsSearchTaskCreationAllowed(node))
                    {
                        SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Update, TreeNode.OBJECT_TYPE, SearchFieldsConstants.ID, node.GetSearchID(), node.DocumentID);
                    }
                }
            }
        }


        /// <summary>
        /// Returns DataSet with all owner comments waiting for approval.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="ownerID">Owner ID</param>
        [Obsolete("Use CMS.DataEngine.ObjectQuery<BlogCommentInfo> instead")]
        protected virtual DataSet GetCommentsWaitingForApprovalInternal(string siteName, int ownerID)
        {
            var data = BlogHelper.GetBlogPosts(siteName, "/%", TreeProvider.ALL_CULTURES, false, "NodeOwner = " + ownerID, "", false);
            if (DataHelper.DataSourceIsEmpty(data))
            {
                return null;
            }

            var postIds = DataHelper.GetIntegerValues(data.Tables[0], "DocumentID");
            if (postIds.Count <= 0)
            {
                return null;
            }

            var condition = new WhereCondition()
                .WhereIn("CommentPostDocumentID", postIds)
                .WhereEqualsOrNull("CommentApproved", 0);

            return GetBlogComments().Where(condition);
        }


        /// <summary>
        /// Returns DataSet with all comments which belongs to the specified user or which are moderated by specified user.
        /// </summary>
        /// <param name="ownerId">Blog owner (user id)</param>
        /// <param name="moderator">Blog moderator (user name)</param>
        /// <param name="where">Where condition which specifies comments to be returned</param>
        /// <param name="blogWhere">Where condition which specifies blogs the comments should be returned from</param>
        /// <param name="columns">Columns to be selected</param>
        /// <param name="topN">Selects only top N items</param>
        /// <param name="order">Order by</param>
        /// <param name="offset">Index of first record to get</param>
        /// <param name="maxRecords">Maximum number of records to get. If maxRecords is zero or less, all records are returned (no paging is used)</param>
        /// <param name="totalRecords">Returns total records</param>
        /// <param name="siteName">Site name of the blog</param>
        protected virtual DataSet GetCommentsInternal(int ownerId, string moderator, string where, string blogWhere, string columns, int topN, string order, int offset, int maxRecords, ref int totalRecords, string siteName)
        {
            var commentsWhere = GetCommentsWhere(ownerId, moderator, where, blogWhere, siteName);
            if (commentsWhere == null)
            {
                totalRecords = 0;
                return null;
            }

            var query = GetBlogComments()
                .Where(commentsWhere)
                .OrderBy(order)
                .TopN(topN)
                .Columns(SqlHelper.ParseColumnList(columns));

            query.Offset = offset;
            query.MaxRecords = maxRecords;

            var data = query.Result;
            totalRecords = query.TotalRecords;

            return data;
        }


        private static WhereCondition GetCommentsWhere(int ownerId, string moderator, string where, string blogWhere, string siteName)
        {
            var pathCondition = GetBlogsPathsWhereCondition(ownerId, moderator, blogWhere, siteName);
            if (pathCondition == null)
            {
                return null;
            }

            var postsQuery = GetBlogPostsQuery(pathCondition, siteName);

            return new WhereCondition()
                .WhereIn("CommentPostDocumentID", postsQuery.AsIDQuery())
                .Where(where);
        }


        private static DocumentQuery GetBlogPostsQuery(WhereCondition pathCondition, string siteName)
        {
            var tree = new TreeProvider();
            var postsQuery = tree.SelectNodes("cms.blogpost")
                                 .All()
                                 .Where(pathCondition);

            if (!string.IsNullOrEmpty(siteName))
            {
                postsQuery.OnSite(siteName);
            }

            return postsQuery;
        }


        private static WhereCondition GetBlogsPathsWhereCondition(int ownerId, string moderator, string blogWhere, string siteName)
        {
            var blogsCondition = BlogHelper.GetBlogsWhere(ownerId, moderator, blogWhere);

            var tree = new TreeProvider();
            var blogPathsQuery = tree.SelectNodes("cms.blog")
                                     .Distinct()
                                     .Column("NodeAliasPath")
                                     .AllCultures()
                                     .CombineWithDefaultCulture(false)
                                     .Where(blogsCondition)
                                     .Published(false);

            if (!string.IsNullOrEmpty(siteName))
            {
                blogPathsQuery.OnSite(siteName);
            }

            var blogPaths = blogPathsQuery.GetListResult<string>();
            if (blogPaths.Count <= 0)
            {
                return null;
            }

            var pathCondition = new WhereCondition();
            foreach (var path in blogPaths)
            {
                pathCondition.Or().WhereStartsWith("NodeAliasPath", path.TrimEnd('/') + "/");
            }

            return pathCondition;
        }


        /// <summary>
        /// Returns DataSet with post comments which are not marked as spam.
        /// </summary>
        /// <param name="postDocumentId">Post documentID</param>
        /// <param name="onlyApproved">Indicates if only approved comments should be returned</param>
        /// <param name="includingSpam">Indicates if comments which are marked as SPAM should be included in the result</param>
        protected virtual DataSet GetPostCommentsInternal(int postDocumentId, bool onlyApproved, bool includingSpam)
        {
            // Get comments WHERE condition
            WhereCondition condition = GetCommentsWhereCondition(postDocumentId, onlyApproved, includingSpam);
            return GetBlogComments().Where(condition);
        }


        /// <summary>
        /// Returns count of the post comments.
        /// </summary>
        /// <param name="postDocumentId">Post document ID</param>
        /// <param name="onlyApproved">Indicates if only approved comments should be included in the count</param>
        /// <param name="includingSpam">Indicates if comments which are marked as SPAM should be included in the count</param>
        protected virtual int GetPostCommentsCountInternal(int postDocumentId, bool onlyApproved, bool includingSpam)
        {
            // Get comments WHERE condition
            WhereCondition condition = GetCommentsWhereCondition(postDocumentId, onlyApproved, includingSpam);
            return GetBlogComments().Column("CommentID").Where(condition).Count;
        }


        /// <summary>
        /// Sends a notification e-mail to blog post subscribers, to blog moderators and to blog owner.
        /// </summary>
        /// <param name="comment">Blog comment data</param>
        /// <param name="toSubscribers">Indicates if notification email should be sent to blog post subscribers</param>
        /// <param name="toModerators">Indicates if notification email should be sent to blog moderators</param>
        /// <param name="toBlogOwner">Indicates if notification email should be sent to blog owner</param>
        protected virtual void SendNewCommentNotificationInternal(BlogCommentInfo comment, bool toSubscribers, bool toModerators, bool toBlogOwner)
        {
            if (!EnableEmails)
            {
                return;
            }

            if (!toSubscribers && !toModerators && !toBlogOwner)
            {
                return;
            }

            ThreadEmailSender sender = new ThreadEmailSender(comment);
            sender.SendNewCommentNotification(WindowsIdentity.GetCurrent(), toSubscribers, toModerators, toBlogOwner);
        }

        #endregion
    }
}