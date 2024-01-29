using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.Base;
using CMS.ContinuousIntegration.Internal;
using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Membership;
using CMS.Search;

namespace CMS.Forums
{
    using TypedDataSet = InfoDataSet<ForumPostInfo>;
    
    /// <summary>
    /// Class providing ForumPostInfo management.
    /// </summary>
    public class ForumPostInfoProvider : AbstractInfoProvider<ForumPostInfo, ForumPostInfoProvider>
    {
        #region "Variables"

        private static int mForumPostIdLength = -1;
        private static int mMaxPostLevel = -1;

        #endregion


        #region "Properties"

        /// <summary>
        /// Post ID Path length. Default = 8 '00000001/000000001'
        /// </summary>
        public static int ForumPostIdLength
        {
            get
            {
                if (mForumPostIdLength < 0)
                {
                    mForumPostIdLength = ValidationHelper.GetInteger(SettingsHelper.AppSettings["ForumPostIDPathLength"], 8);
                }

                return mForumPostIdLength;
            }
        }


        /// <summary>
        /// Returns maximal available post level.
        /// </summary>
        public static int MaxPostLevel
        {
            get
            {
                if (mMaxPostLevel < 0)
                {
                    // 450 = maximum indexable size for SQL server, 8+1 = ID and slash, -1 zero based
                    // Default value is 49
                    mMaxPostLevel = (450 / (ForumPostIdLength + 1)) - 1;
                }

                return mMaxPostLevel;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns object with specified GUID.
        /// </summary>
        /// <param name="guid">Object GUID</param>
        public static ForumPostInfo GetForumPostInfoByGUID(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Returns the ForumPostInfo structure for the specified forumPost.
        /// </summary>
        /// <param name="forumPostId">ForumPost id</param>
        public static ForumPostInfo GetForumPostInfo(int forumPostId)
        {
            return ProviderObject.GetInfoById(forumPostId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified forumPost.
        /// </summary>
        /// <param name="forumPost">ForumPost to set</param>
        public static void SetForumPostInfo(ForumPostInfo forumPost)
        {
            SetForumPostInfo(forumPost, null, null);
        }


        /// <summary>
        /// Sets (updates or inserts) specified forumPost.
        /// </summary>
        /// <param name="forumPost">ForumPost to set</param>
        /// <param name="baseUrl">Default base URL</param>
        /// <param name="unsubscriptionUrl">Default unsubscription URL</param>
        public static void SetForumPostInfo(ForumPostInfo forumPost, string baseUrl, string unsubscriptionUrl)
        {
            if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, "") != "")
            {
                LicenseHelper.RequestFeature(RequestContext.CurrentDomain, FeatureEnum.Forums);
            }

            ProviderObject.SetForumPostInfoInternal(forumPost, baseUrl, unsubscriptionUrl);
        }


        /// <summary>
        /// Override to ensure SiteID is set
        /// </summary>
        /// <param name="info"></param>
        protected override void SetInfo(ForumPostInfo info)
        {
            if (info.SiteId <= 0)
            {
                info.SetSiteIdFromForum();
            }
            base.SetInfo(info);
        }


        /// <summary>
        /// Update thread and child-posts counts of specified post.
        /// </summary>
        /// <param name="postInfo">Forum post which thread and parent intended for recount</param>
        public static void UpdateCounts(ForumPostInfo postInfo)
        {
            if (postInfo != null)
            {
                // Parent is not thread post
                if (postInfo.PostLevel > 1)
                {
                    // Update parent posts count
                    UpdatePostCounts(postInfo.PostParentID);
                }

                // Update thread posts count
                UpdateThreadCounts(postInfo.PostIDPath);
            }
        }


        /// <summary>
        /// Update direct-child-posts counts of specified post.
        /// </summary>
        /// <param name="postId">ID of the forum post to recount</param>
        public static void UpdatePostCounts(int postId)
        {
            ProviderObject.UpdatePostCountsInternal(postId);
        }


        /// <summary>
        /// Update thread sub-posts counts and last post information.
        /// </summary>
        /// <param name="path">Post path, first part determines the thread</param>
        public static void UpdateThreadCounts(string path)
        {
            ProviderObject.UpdateThreadCountsInternal(path);
        }


        /// <summary>
        /// Returns PostIdPath in correct format.
        /// </summary>
        /// <param name="path">Posts path</param>
        /// <param name="postId">Post ID</param>
        public static string GetPath(string path, int postId)
        {
            if (path == null)
            {
                path = "";
            }

            //Convert to string
            string mStrPostId = postId.ToString();
            // Check number count
            if (mStrPostId.Length > ForumPostIdLength)
            {
                throw new Exception("[ForumPostInfoProvider.GetPath]: PostIdPath is longer then " + ForumPostIdLength.ToString() + " counts.");
            }
            // Insert zero needed
            for (int i = mStrPostId.Length; i < ForumPostIdLength; i++)
            {
                mStrPostId = "0" + mStrPostId;
            }

            path += "/" + mStrPostId;

            return path;
        }


        /// <summary>
        /// Deletes specified forumPost.
        /// </summary>
        /// <param name="forumPostObj">ForumPost object</param>
        public static void DeleteForumPostInfo(ForumPostInfo forumPostObj)
        {
            ProviderObject.DeleteForumPostInfoInternal(forumPostObj);
        }


        /// <summary>
        /// Deletes specified forumPost.
        /// </summary>
        /// <param name="forumPostId">ForumPost id</param>
        public static void DeleteForumPostInfo(int forumPostId)
        {
            ForumPostInfo forumPostObj = GetForumPostInfo(forumPostId);
            DeleteForumPostInfo(forumPostObj);
        }


        /// <summary>
        /// Returns forum post DataSet.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="topN">Select top n columns</param>
        /// <param name="columns">Columns which should be selected</param>
        [Obsolete("Use method GetForumPosts() instead")]
        public static TypedDataSet GetForumPosts(string where, string orderBy = null, int topN = 0, string columns = null)
        {
            return ProviderObject.GetForumPostsInternal(where, orderBy, topN, columns);
        }


        /// <summary>
        /// Gets all forum posts as object query
        /// </summary>
        public static ObjectQuery<ForumPostInfo> GetForumPosts()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Creates complete where condition from parameters.
        /// </summary>
        /// <param name="forumId">Forum ID</param>
        /// <param name="path">Posts path</param>
        /// <param name="where">Where condition</param>
        /// <param name="maxRelativeLevel">Maximal relative level</param>
        /// <param name="selectOnlyApproved">Select only approved posts</param>
        public static string GetCompleteWhere(int forumId, string path, string where, int maxRelativeLevel, bool selectOnlyApproved)
        {
            if ((where != null) && (where.Trim()) != "")
            {
                where = "(" + where + ")";
            }

            // Forum ID
            where = SqlHelper.AddWhereCondition(where, "PostForumID = " + forumId);

            // Remove null value
            path = ValidationHelper.GetString(path, String.Empty);

            // Path
            if ((path != String.Empty) && (path != "/%"))
            {
                where = SqlHelper.AddWhereCondition(where, "PostIDPath LIKE N'" + SqlHelper.GetSafeQueryString(path, false) + "'");
            }

            // Level
            int baseLevel = path.Split('/').GetUpperBound(0);
            if (maxRelativeLevel >= 0)
            {
                int tmpNodeLevel = baseLevel + maxRelativeLevel - 1;
                where = SqlHelper.AddWhereCondition(where, "PostLevel <= " + tmpNodeLevel.ToString());
            }

            // Approved where condition
            if (selectOnlyApproved)
            {
                where = SqlHelper.AddWhereCondition(where, "PostApproved = 1");
            }

            return where;
        }


        /// <summary>
        /// Returns the DataSet of forum posts in specified subtree of forum posts.
        /// </summary>
        /// <param name="forumId">Forum ID</param>
        /// <param name="path">Posts path</param>
        /// <param name="whereCondition">Where condition</param>
        /// <param name="orderBy">Order specification</param>
        /// <param name="maxRelativeLevel">Maximal relative level</param>
        /// <param name="selectOnlyApproved">Select only approved posts</param>
        /// <param name="topN">Select TOP N posts</param>
        public static TypedDataSet SelectForumPosts(int forumId, string path, string whereCondition, string orderBy, int maxRelativeLevel, bool selectOnlyApproved, int topN)
        {
            return SelectForumPosts(forumId, path, whereCondition, orderBy, maxRelativeLevel, selectOnlyApproved, topN, null);
        }


        /// <summary>
        /// Returns the DataSet of forum posts in specified subtree of forum posts.
        /// </summary>
        /// <param name="forumId">Forum ID</param>
        /// <param name="path">Posts path</param>
        /// <param name="whereCondition">Where condition</param>
        /// <param name="orderBy">Order specification</param>
        /// <param name="maxRelativeLevel">Maximal relative level</param>
        /// <param name="selectOnlyApproved">Select only approved posts</param>
        /// <param name="topN">Select TOP N posts</param>
        /// <param name="columns">List of columns which should be selected</param>
        public static TypedDataSet SelectForumPosts(int forumId, string path, string whereCondition, string orderBy, int maxRelativeLevel, bool selectOnlyApproved, int topN, string columns)
        {
            int totalRecords = 0;
            return SelectForumPosts(forumId, path, whereCondition, orderBy, maxRelativeLevel, selectOnlyApproved, topN, columns, 0, 0, ref totalRecords);
        }


        /// <summary>
        /// Returns the DataSet of forum posts in specified subtree of forum posts.
        /// </summary>
        /// <param name="forumId">Forum ID</param>
        /// <param name="path">Posts path</param>
        /// <param name="whereCondition">Where condition</param>
        /// <param name="orderBy">Order specification</param>
        /// <param name="maxRelativeLevel">Maximal relative level</param>
        /// <param name="selectOnlyApproved">Select only approved posts</param>
        /// <param name="topN">Select TOP N posts</param>
        /// <param name="columns">List of columns which should be selected</param>
        /// <param name="offset">Index of first record to get</param>
        /// <param name="maxRecords">Maximum number of records to get. If maxRecords is zero or less, all records are returned (no paging is used)</param>
        /// <param name="totalRecords">Returns total number of the records</param>
        public static TypedDataSet SelectForumPosts(int forumId, string path, string whereCondition, string orderBy, int maxRelativeLevel, bool selectOnlyApproved, int topN, string columns, int offset, int maxRecords, ref int totalRecords)
        {
            return ProviderObject.SelectForumPostsInternal(forumId, path, whereCondition, orderBy, maxRelativeLevel, selectOnlyApproved, topN, columns, offset, maxRecords, ref totalRecords);
        }


        /// <summary>
        /// Returns the DataSet of forum posts in specified subtree of forum posts.
        /// </summary>
        /// <param name="forumId">Forum ID</param>
        /// <param name="path">Posts path</param>
        /// <param name="whereCondition">Where condition</param>
        /// <param name="orderBy">Order specification</param>
        /// <param name="maxRelativeLevel">Maximal relative level</param>
        /// <param name="selectOnlyApproved">Select only approved posts</param>
        public static TypedDataSet SelectForumPosts(int forumId, string path, string whereCondition, string orderBy, int maxRelativeLevel, bool selectOnlyApproved)
        {
            return SelectForumPosts(forumId, path, whereCondition, orderBy, maxRelativeLevel, selectOnlyApproved, 0);
        }


        /// <summary>
        /// Returns the DataSet of forum posts in specified subtree of forum posts.
        /// </summary>
        /// <param name="forumId">Forum ID</param>
        /// <param name="path">Posts path</param>
        /// <param name="whereCondition">Where condition</param>
        /// <param name="orderBy">Order specification</param>
        /// <param name="maxRelativeLevel">Maximal relative level</param>
        public static TypedDataSet SelectForumPosts(int forumId, string path, string whereCondition, string orderBy, int maxRelativeLevel)
        {
            return SelectForumPosts(forumId, path, whereCondition, orderBy, maxRelativeLevel, true, 0);
        }


        /// <summary>
        /// Returns the DataSet of forum posts in specified subtree of forum posts.
        /// </summary>
        /// <param name="forumId">Forum ID</param>
        /// <param name="path">Posts path</param>
        /// <param name="whereCondition">Where condition</param>
        /// <param name="orderBy">Order specification</param>
        public static TypedDataSet SelectForumPosts(int forumId, string path, string whereCondition, string orderBy)
        {
            return SelectForumPosts(forumId, path, whereCondition, orderBy, -1, true, 0);
        }


        /// <summary>
        /// Returns the DataSet of forum posts in specified subtree of forum posts.
        /// </summary>
        /// <param name="forumId">Forum ID</param>
        /// <param name="path">Posts path</param>
        /// <param name="whereCondition">Where condition</param>
        public static TypedDataSet SelectForumPosts(int forumId, string path, string whereCondition)
        {
            return SelectForumPosts(forumId, path, whereCondition, null, -1, true, 0);
        }


        /// <summary>
        /// Returns the DataSet of forum posts in specified subtree of forum posts.
        /// </summary>
        /// <param name="forumId">Forum ID</param>
        /// <param name="path">Posts path</param>
        public static TypedDataSet SelectForumPosts(int forumId, string path)
        {
            return SelectForumPosts(forumId, path, null, null, -1, true, 0);
        }


        /// <summary>
        /// Returns the DataSet of forum posts joined with user, user settings and other tables.
        /// </summary>
        /// <param name="whereCondition">Where condition</param>
        /// <param name="orderBy">Order specification</param>
        /// <param name="topN">Select TOP N posts</param>
        /// <param name="columns">List of columns which should be selected</param>
        public static TypedDataSet SelectForumPosts(string whereCondition, string orderBy, int topN, string columns)
        {
            return ProviderObject.SelectForumPostsInternal(whereCondition, orderBy, topN, columns);
        }


        /// <summary>
        /// Returns post, forum, forum group joined tables with dependence on.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by condition</param>
        /// <param name="topN">TOP N</param>
        /// <param name="columns">Columns to select</param>
        public static DataSet Search(string where, string orderBy, int topN, string columns)
        {
            return ProviderObject.SearchInternal(where, orderBy, topN, columns);
        }


        /// <summary>
        /// Returns all the child posts of the post.
        /// </summary>
        /// <param name="postId">Post ID</param>
        public static TypedDataSet GetChildPosts(int postId)
        {
            return ProviderObject.GetChildPostsInternal(postId);
        }


        /// <summary>
        /// Sends the new post acknowledgement to the users subscribed 
        /// to the forum or forum thread where the post has been inserted.
        /// </summary>
        /// <param name="postId">Post ID</param>
        public static void SendPostEmails(int postId)
        {
            SendPostEmails(postId, null, null);
        }


        /// <summary>
        /// Sends the new post acknowledgement to the users subscribed 
        /// to the forum or forum thread where the post has been inserted.
        /// </summary>
        /// <param name="postId">Post ID</param>
        /// <param name="baseUrl">Default base URL</param>
        /// <param name="unsubscriptionUrl">Default unsubscription URL</param>
        public static void SendPostEmails(int postId, string baseUrl, string unsubscriptionUrl)
        {
            ThreadEmailSender tca = new ThreadEmailSender(postId, false, baseUrl, unsubscriptionUrl);
        }


        /// <summary>
        /// Send e-mail to moderators.
        /// </summary>
        /// <param name="postId">Post ID</param>
        public static void SendModeratorsEmail(int postId)
        {
            SendModeratorsEmail(postId, null, null);
        }


        /// <summary>
        /// Send e-mail to moderators.
        /// </summary>
        /// <param name="postId">Post ID</param>
        /// <param name="baseUrl">Default base URL</param>
        /// <param name="unsubscriptionUrl">Default unsubscription URL</param>
        public static void SendModeratorsEmail(int postId, string baseUrl, string unsubscriptionUrl)
        {
            ThreadEmailSender tca = new ThreadEmailSender(postId, true, baseUrl, unsubscriptionUrl);
        }


        /// <summary>
        /// Stick thread.
        /// </summary>
        /// <param name="fpi">Forum post info</param>
        public static void StickThread(ForumPostInfo fpi)
        {
            ProviderObject.StickThreadInternal(fpi);
        }


        /// <summary>
        /// Stick thread.
        /// </summary>
        /// <param name="fpi">Forum post info</param>
        public static void UnstickThread(ForumPostInfo fpi)
        {
            ProviderObject.UnstickThreadInternal(fpi);
        }


        /// <summary>
        /// Moves sticky thread up.
        /// </summary>
        /// <param name="fpi">Forum post info</param>
        public static void MoveStickyThreadUp(ForumPostInfo fpi)
        {
            ProviderObject.MoveStickyThreadUpInternal(fpi);
        }


        /// <summary>
        /// Moves sticky thread down.
        /// </summary>
        /// <param name="fpi">Forum post info</param>
        public static void MoveStickyThreadDown(ForumPostInfo fpi)
        {
            ProviderObject.MoveStickyThreadDownInternal(fpi);
        }


        /// <summary>
        /// Counts number of attachments of post specified by postID and saves it into database.
        /// </summary>
        public static void UpdatePostAttachmentCount(int postId)
        {
            ProviderObject.UpdatePostAttachmentCountInternal(postId);
        }


        /// <summary>
        /// Increments the post views with the specified number.
        /// </summary>
        /// <param name="postId">ID of the post the views of which are updated</param>
        /// <param name="viewCount">Number of views which will be added</param>
        public static void IncrementPostViews(int postId, int viewCount)
        {
            ProviderObject.IncrementPostViewsInternal(postId, viewCount);
        }


        /// <summary>
        /// Creates new thread from the given post.
        /// </summary>
        /// <param name="postInfo">Post to be separated from current thread</param>
        public static void SplitThread(ForumPostInfo postInfo)
        {
            ProviderObject.SplitThreadInternal(postInfo);
        }


        /// <summary>
        /// Moves the thread to other forum.
        /// </summary>
        /// <param name="postInfo">Post to be moved</param>
        /// <param name="forumId">ID of the forum where the post will be moved</param>
        public static void MoveThread(ForumPostInfo postInfo, int forumId)
        {
            ProviderObject.MoveThreadInternal(postInfo, forumId);
        }


        /// <summary>
        /// Returns root post ID for given PostIDPath.
        /// </summary>
        /// <param name="path">PostIDPath the root of which should be returned</param>
        public static int GetPostRootFromIDPath(string path)
        {
            if (!String.IsNullOrEmpty(path))
            {
                string trimmedPath = path.TrimStart('/');
                int pos = trimmedPath.IndexOf("/", StringComparison.Ordinal);
                if (pos > 0)
                {
                    return ValidationHelper.GetInteger(trimmedPath.Substring(0, pos), 0);
                }
                else
                {
                    return ValidationHelper.GetInteger(trimmedPath, 0);
                }
            }
            return 0;
        }


        /// <summary>
        /// Returns link to selected post.
        /// </summary>
        /// <param name="postIdPath">Post id path</param>
        /// <param name="forumId">Forum id</param>
        /// <param name="encodeQueryString">Indicates if query string should be encoded</param>
        public static string GetPostURL(string postIdPath, int forumId, bool encodeQueryString)
        {
            if (!String.IsNullOrEmpty(postIdPath) && (forumId > 0))
            {
                // Get thread id from post id path
                int threadId = GetPostRootFromIDPath(postIdPath);

                if (threadId > 0)
                {
                    string url = string.Empty;

                    // Get forum info
                    ForumInfo fi = ForumInfoProvider.GetForumInfo(forumId);
                    if (fi != null)
                    {
                        url = URLHelper.ResolveUrl(fi.ForumBaseUrl);
                    }
                    if (String.IsNullOrEmpty(url))
                    {
                        url = RequestContext.CurrentURL;
                        url = URLHelper.RemoveParameterFromUrl(url, "searchtext");
                    }
                    url = URLHelper.UpdateParameterInUrl(url, "ForumId", forumId.ToString());
                    url = URLHelper.UpdateParameterInUrl(url, "ThreadId", threadId.ToString());
                    if (encodeQueryString)
                    {
                        url = HTMLHelper.EncodeForHtmlAttribute(url);
                    }
                    return url;
                }
            }

            return string.Empty;
        }


        /// <summary>
        /// Returns '0' if current user is authorized to view current search document.
        /// </summary>
        /// <param name="doc">Search document object</param>
        public static int FilterSearchResults(ILuceneSearchDocument doc)
        {
            return FilterSearchResults(doc, null);
        }


        /// <summary>
        /// Returns '0' if specified user is authorized to view current search document.
        /// </summary>
        /// <param name="doc">Search document object</param>
        /// <param name="user">User info</param>
        public static int FilterSearchResults(ILuceneSearchDocument doc, UserInfo user)
        {
            // If user is not defined use current user
            if (user == null)
            {
                user = MembershipContext.AuthenticatedUser;
            }

            // Check whether user is global admin
            if (user.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin))
            {
                return 0;
            }

            int forumId = SearchValueConverter.StringToInt(doc.Get("postforumid"));
            int groupId = 0;

            // Get forum info for current document
            ForumInfo fi = ForumInfoProvider.GetForumInfo(forumId);
            if (fi != null)
            {
                // Get forum group info for current document
                ForumGroupInfo fgi = ForumGroupInfoProvider.GetForumGroupInfo(fi.ForumGroupID);
                if (fgi != null)
                {
                    groupId = fgi.GroupGroupID;
                }

                // If current user is moderator for current post forum or is authorized, add current document to the filtered results 
                if (ForumContext.UserIsModerator(fi.ForumID, groupId, user) || ForumInfoProvider.IsAuthorizedPerForum(fi.ForumID, groupId, "AccessToForum", fi.AllowAccess, user))
                {
                    return 0;
                }
            }

            // Remove result item
            return 1;
        }


        /// <summary>
        /// Returns forum posts for specified document
        /// </summary>
        /// <param name="documentId">Document ID</param>
        public static TypedDataSet GetDocumentPosts(int documentId)
        {
            var fi = ForumInfoProvider.GetForumInfoByDocument(documentId);
            if ((fi != null) && (fi.ForumAccess < 100000))
            {
                return SelectForumPosts(fi.ForumID, "/%");
            }

            return null;
        }


        /// <summary>
        /// Updates data for all records given by where condition
        /// </summary>
        /// <param name="updateExpression">Update expression, e.g. "Value = Value * 2"</param>
        /// <param name="where">Where condition</param>
        /// <param name="parameters">Parameters</param>
        internal static void UpdateData(string updateExpression, string where, QueryDataParameters parameters)
        {
            ProviderObject.UpdateData(updateExpression, parameters, where);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Sets (updates or inserts) specified forumPost.
        /// </summary>
        /// <param name="forumPost">ForumPost to set</param>
        /// <param name="baseUrl">Default base URL</param>
        /// <param name="unsubscriptionUrl">Default unsubscription URL</param>
        protected virtual void SetForumPostInfoInternal(ForumPostInfo forumPost, string baseUrl, string unsubscriptionUrl)
        {
            if (forumPost != null)
            {
                // Set default stick order if is not set
                forumPost.PostStickOrder = forumPost.PostStickOrder;
                // Set post info
                forumPost.SetValue("PostInfo", forumPost.PostInfo.GetData());
                // Set attachments count
                forumPost.PostAttachmentCount = forumPost.PostAttachmentCount;

                bool? addPoints = null;

                if (forumPost.PostId > 0)
                {
                    ForumPostInfo oldPostInfo = GetForumPostInfo(forumPost.PostId);
                    if (oldPostInfo != null)
                    {
                        // Rejected -> Approved
                        if (!oldPostInfo.PostApproved && forumPost.PostApproved)
                        {
                            addPoints = true;
                        }
                        // Approve -> Reject
                        else if (oldPostInfo.PostApproved && !forumPost.PostApproved)
                        {
                            addPoints = false;
                        }
                    }

                    SetInfo(forumPost);

                    if (oldPostInfo != null)
                    {
                        // Update thread posts count when restoring posts with Continuous Integration after split operation
                        if (forumPost.PostForumID == oldPostInfo.PostForumID && forumPost.PostParentID != oldPostInfo.PostParentID)
                        {
                            var idPath = oldPostInfo.PostIDPath;
                            var slashIndex = idPath.IndexOf("/", 1, StringComparison.Ordinal);
                            var parentIdPath = slashIndex > 0 ? idPath.Substring(0, slashIndex) : idPath;
                            UpdateThreadCounts(parentIdPath);
                        }
                    }

                    if (addPoints == true)
                    {
                        // Send Emails
                        SendPostEmails(forumPost.PostId, baseUrl, unsubscriptionUrl);
                    }

                    // Updates QuestionSolved fields in forum
                    ForumInfoProvider.UpdateQuestionSolved(forumPost.PostForumID, forumPost.PostIDPath);

                    // Set new Thread, posts, last user and time value
                    ForumInfoProvider.UpdateForumCounts(forumPost.PostForumID);

                    // Update thread and direct sub-posts direct count
                    UpdateCounts(forumPost);
                }
                else
                {
                    forumPost.PostIDPath = "";
                    forumPost.PostLevel = 0;

                    if (forumPost.PostApproved)
                    {
                        addPoints = true;
                    }

                    // Insert the post
                    SetInfo(forumPost);

                    // Set new Thread, posts, last user and time value
                    ForumInfoProvider.UpdateForumCounts(forumPost.PostForumID);

                    // Update thread and direct sub-posts direct count
                    UpdateCounts(forumPost);

                    // Invalidate parent forum to reload cached instance
                    var parentForum = ForumInfoProvider.GetForumInfo(forumPost.PostForumID);
                    parentForum?.Generalized.Invalidate(false);

                    // Send e-mails to moderators if it is required
                    SendModeratorsEmail(forumPost.PostId, baseUrl, unsubscriptionUrl);

                    // Send Emails
                    SendPostEmails(forumPost.PostId, baseUrl, unsubscriptionUrl);
                }

                if (addPoints != null)
                {
                    string siteName = ForumInfoProvider.GetForumSiteName(forumPost.PostForumID);
                    if (!String.IsNullOrEmpty(siteName))
                    {
                        BadgeInfoProvider.UpdateActivityPointsToUser(ActivityPointsEnum.ForumPost, forumPost.PostUserID, siteName, (bool)addPoints);
                    }
                }

                // Update search index
                if (SearchIndexInfoProvider.SearchEnabled)
                {
                    // Get ad-hoc forum
                    ForumInfo fi = ForumInfoProvider.GetForumInfo(forumPost.PostForumID);
                    if ((fi != null) && (fi.ForumDocumentID > 0))
                    {
                        TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);
                        TreeNode node =  tree.SelectSingleDocument(fi.ForumDocumentID);

                        // Update search index for given document
                        if ((node != null) && DocumentHelper.IsSearchTaskCreationAllowed(node))
                        {
                            SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Update, TreeNode.OBJECT_TYPE, SearchFieldsConstants.ID, node.GetSearchID(), node.DocumentID);
                        }
                    }
                    // Not-AdHoc forum
                    else
                    {
                        // Register update task. Post is inserted or approved
                        if (forumPost.PostApproved)
                        {
                            SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Update, ForumInfo.OBJECT_TYPE, SearchFieldsConstants.ID, forumPost.PostId.ToString(), forumPost.PostId);
                        }
                        // Post was rejected
                        else
                        {
                            SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Delete, ForumInfo.OBJECT_TYPE, SearchFieldsConstants.ID, forumPost.PostId.ToString(), forumPost.PostId);
                        }
                    }
                }
            }
            else
            {
                throw new Exception("[ForumPostInfoProvider.SetForumPostInfo]: No ForumPostInfo object set.");
            }
        }


        /// <summary>
        /// Update direct-child-posts counts of specified post.
        /// </summary>
        /// <param name="postId">ID of the forum post to recount</param>
        protected virtual void UpdatePostCountsInternal(int postId)
        {
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@PostID", postId);

            ConnectionHelper.ExecuteQuery("Forums.ForumPost.UpdatePostCounts", parameters);
        }


        /// <summary>
        /// Update thread sub-posts counts and last post information.
        /// </summary>
        /// <param name="path">Post path, first part determines the thread</param>
        protected virtual void UpdateThreadCountsInternal(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            // Try find other path separator
            var slashIndex = path.IndexOf("/", 1, StringComparison.Ordinal);
            if (slashIndex > 0)
            {
                path = path.Remove(slashIndex);
            }

            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@Path", path);

            ConnectionHelper.ExecuteQuery("Forums.ForumPost.UpdateThreadCounts", parameters);
        }


        /// <summary>
        /// Deletes specified forumPost.
        /// </summary>
        /// <param name="forumPostObj">ForumPost object</param>
        protected virtual void DeleteForumPostInfoInternal(ForumPostInfo forumPostObj)
        {
            if (forumPostObj != null)
            {
                int forumId = forumPostObj.PostForumID;
                int userId = forumPostObj.PostUserID;
                bool approved = forumPostObj.PostApproved;
                string siteName = ForumInfoProvider.GetForumSiteName(forumId);

                // Delete object
                DeleteInfo(forumPostObj);

                // Update child posts counts of thread and parent
                UpdateCounts(forumPostObj);

                // Set new Thread, posts, last user and time value
                ForumInfoProvider.UpdateForumCounts(forumId);

                // Invalidate parent forum to reload cached instance
                var parentForum = ForumInfoProvider.GetForumInfo(forumId);
                parentForum?.Generalized.Invalidate(false);

                // Update forum activity points
                if ((userId > 0) && (approved))
                {
                    if (!String.IsNullOrEmpty(siteName))
                    {
                        BadgeInfoProvider.UpdateActivityPointsToUser(ActivityPointsEnum.ForumPost, userId, siteName, false);
                    }
                }

                // Update user counts
                UserInfoProvider.UpdateUserCounts(ActivityPointsEnum.ForumPost, 0, 0);

                //Update search index
                if (SearchIndexInfoProvider.SearchEnabled)
                {
                    // Get ad-hoc forum
                    ForumInfo fi = ForumInfoProvider.GetForumInfo(forumPostObj.PostForumID);
                    if ((fi != null) && (fi.ForumDocumentID > 0))
                    {
                        TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);
                        TreeNode node = tree.SelectSingleDocument(fi.ForumDocumentID);

                        // Update search index for given document
                        if ((node != null) && DocumentHelper.IsSearchTaskCreationAllowed(node))
                        {
                            SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Update, TreeNode.OBJECT_TYPE, SearchFieldsConstants.ID, node.GetSearchID(), node.DocumentID);
                        }
                    }
                    else
                    {
                        SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Delete, ForumInfo.OBJECT_TYPE, SearchFieldsConstants.ID, forumPostObj.PostId.ToString(), forumPostObj.PostId);
                    }
                }
            }
        }


        /// <summary>
        /// Returns forum post DataSet.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="topN">Select top n columns</param>
        /// <param name="columns">Columns which should be selected</param>
        [Obsolete("Use method GetForumPosts() instead")]
        protected virtual TypedDataSet GetForumPostsInternal(string where, string orderBy, int topN, string columns)
        {
            return GetForumPosts().Where(where).OrderBy(orderBy).TopN(topN).Columns(columns).BinaryData(true).TypedResult;
        }


        /// <summary>
        /// Returns the DataSet of forum posts in specified subtree of forum posts.
        /// </summary>
        /// <param name="forumId">Forum ID</param>
        /// <param name="path">Posts path</param>
        /// <param name="whereCondition">Where condition</param>
        /// <param name="orderBy">Order specification</param>
        /// <param name="maxRelativeLevel">Maximal relative level</param>
        /// <param name="selectOnlyApproved">Select only approved posts</param>
        /// <param name="topN">Select TOP N posts</param>
        /// <param name="columns">List of columns which should be selected</param>
        /// <param name="offset">Index of first record to get</param>
        /// <param name="maxRecords">Maximum number of records to get. If maxRecords is zero or less, all records are returned (no paging is used)</param>
        /// <param name="totalRecords">Returns total number of the records</param>
        protected virtual TypedDataSet SelectForumPostsInternal(int forumId, string path, string whereCondition, string orderBy, int maxRelativeLevel, bool selectOnlyApproved, int topN, string columns, int offset, int maxRecords, ref int totalRecords)
        {
            if (forumId <= 0)
            {
                return null;
            }

            string where = GetCompleteWhere(forumId, path, whereCondition, maxRelativeLevel, selectOnlyApproved);

            // Set default columns value if columns value is not defined
            if (String.IsNullOrEmpty(ValidationHelper.GetString(columns, String.Empty).Trim()))
            {
                columns = "[Forums_ForumPost].*";
            }

            var parameters = GetQueryDataParameters();

            // Get the data
            return ConnectionHelper.ExecuteQuery("Forums.ForumPost.SelectPosts", parameters, where, orderBy, topN, columns, offset, maxRecords, ref totalRecords).As<ForumPostInfo>();
        }


        /// <summary>
        /// Returns the DataSet of forum posts joined with user, user settings and other tables.
        /// </summary>
        /// <param name="whereCondition">Where condition</param>
        /// <param name="orderBy">Order specification</param>
        /// <param name="topN">Select TOP N posts</param>
        /// <param name="columns">List of columns which should be selected</param>
        protected virtual TypedDataSet SelectForumPostsInternal(string whereCondition, string orderBy, int topN, string columns)
        {
            var parameters = GetQueryDataParameters();

            // Get the data
            return ConnectionHelper.ExecuteQuery("Forums.ForumPost.SelectPosts", parameters, whereCondition, orderBy, topN, columns).As<ForumPostInfo>();
        }


        /// <summary>
        /// Returns post, forum, forum group joined tables with dependence on.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by condition</param>
        /// <param name="topN">TOP N</param>
        /// <param name="columns">Columns to select</param>
        protected virtual DataSet SearchInternal(string where, string orderBy, int topN, string columns)
        {
            return ConnectionHelper.ExecuteQuery("Forums.ForumPost.SearchForum", null, where, orderBy, topN, columns);
        }


        /// <summary>
        /// Returns all the child posts of the post.
        /// </summary>
        /// <param name="postId">Post ID</param>
        protected virtual TypedDataSet GetChildPostsInternal(int postId)
        {
            ForumPostInfo fpi = GetForumPostInfo(postId);
            if (fpi == null)
            {
                return null;
            }

            var parameters = GetQueryDataParameters();
            parameters.Add("@Path", fpi.PostIDPath);

            return ConnectionHelper.ExecuteQuery("Forums.ForumPost.GetChildPosts", parameters).As<ForumPostInfo>();
        }


        /// <summary>
        /// Stick thread.
        /// </summary>
        /// <param name="fpi">Forum post info</param>
        protected virtual void StickThreadInternal(ForumPostInfo fpi)
        {
            // Check whether post exists
            if (fpi != null)
            {
                QueryDataParameters parameters = new QueryDataParameters();
                parameters.Add("@PostForumID", fpi.PostForumID);
                parameters.Add("@PostID", fpi.PostId);

                var updatedPosts = ConnectionHelper.ExecuteQuery("Forums.ForumPost.stickthread", parameters);
                EnsureContinuousIntegrationReserialization(updatedPosts);
            }
        }


        /// <summary>
        /// Stick thread.
        /// </summary>
        /// <param name="fpi">Forum post info</param>
        protected virtual void UnstickThreadInternal(ForumPostInfo fpi)
        {
            // Check whether post exists
            if (fpi != null)
            {
                QueryDataParameters parameters = new QueryDataParameters();
                parameters.Add("@PostForumID", fpi.PostForumID);
                parameters.Add("@PostID", fpi.PostId);

                var updatedPosts = ConnectionHelper.ExecuteQuery("Forums.ForumPost.unstickthread", parameters);
                EnsureContinuousIntegrationReserialization(updatedPosts);
            }
        }


        /// <summary>
        /// Moves sticky thread up.
        /// </summary>
        /// <param name="fpi">Forum post info</param>
        protected virtual void MoveStickyThreadUpInternal(ForumPostInfo fpi)
        {
            // Check whether post exists
            if (fpi != null)
            {
                fpi.Generalized.MoveObjectDown();
            }
        }


        /// <summary>
        /// Moves sticky thread down.
        /// </summary>
        /// <param name="fpi">Forum post info</param>
        protected virtual void MoveStickyThreadDownInternal(ForumPostInfo fpi)
        {
            // Check whether post exists
            if (fpi != null)
            {
                fpi.Generalized.MoveObjectUp();
            }
        }


        /// <summary>
        /// Counts number of attachments of post specified by postID and saves it into database.
        /// </summary>
        protected virtual void UpdatePostAttachmentCountInternal(int postId)
        {
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@PostID", postId);

            DataSet ds = ConnectionHelper.ExecuteQuery("Forums.ForumAttachment.count", parameters);

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                // Get count
                int count = ValidationHelper.GetInteger(ds.Tables[0].Rows[0]["Count"], 0);

                // Get forum post
                ForumPostInfo fpi = GetForumPostInfo(postId);
                if (fpi != null)
                {
                    // Update database
                    fpi.PostAttachmentCount = count;
                    SetForumPostInfo(fpi);
                }
            }
        }


        /// <summary>
        /// Increments the post views with the specified number.
        /// </summary>
        /// <param name="postId">ID of the post the views of which are updated</param>
        /// <param name="viewCount">Number of views which will be added</param>
        protected virtual void IncrementPostViewsInternal(int postId, int viewCount)
        {
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@PostID", postId);
            parameters.Add("@ViewCount", viewCount);

            ConnectionHelper.ExecuteQuery("forums.forumpost.logthreadview", parameters);
        }


        /// <summary>
        /// Creates new thread from the given post.
        /// </summary>
        /// <param name="postInfo">Post to be separated from current thread</param>
        protected virtual void SplitThreadInternal(ForumPostInfo postInfo)
        {
            if (postInfo != null)
            {
                // Move only if the post is child
                if (postInfo.PostParentID > 0)
                {
                    // Get the prefix which has to be removed
                    string prefix = postInfo.PostIDPath;
                    int lastIndex = prefix.LastIndexOf("/", StringComparison.Ordinal);

                    // Update root post
                    postInfo.PostParentID = 0;
                    postInfo.PostLevel = 0;
                    postInfo.PostIDPath = postInfo.PostIDPath.Substring(lastIndex);
                    SetForumPostInfo(postInfo);

                    QueryDataParameters parameters = new QueryDataParameters();
                    parameters.Add("@Prefix", prefix);
                    parameters.Add("@PrefixLevel", prefix.Split(new [] { '/' }).Length - 2);
                    parameters.Add("@PrefixLength", lastIndex);

                    var updatedPosts = ConnectionHelper.ExecuteQuery("forums.forumpost.splitthread", parameters);
                    EnsureContinuousIntegrationReserialization(updatedPosts);

                    // Root prefix - path of the thread (old parent thread)
                    string rootPrefix = prefix.Substring(0, prefix.IndexOf("/", 1, StringComparison.Ordinal));

                    UpdateThreadCounts(postInfo.PostIDPath);
                    UpdateThreadCounts(rootPrefix);

                    // Update relevant forum posts
                    if (SearchIndexInfoProvider.SearchEnabled)
                    {
                        DataSet ds = GetForumPosts().WhereEquals("PostApproved", 1).WhereStartsWith("PostIDPath", postInfo.PostIDPath).Column("PostID");
                        if (!DataHelper.DataSourceIsEmpty(ds))
                        {
                            foreach (DataRow dr in ds.Tables[0].Rows)
                            {
                                SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Update, ForumInfo.OBJECT_TYPE, SearchFieldsConstants.ID, Convert.ToString(dr["PostID"]), Convert.ToInt32(dr["PostID"]), false);
                            }
                            SearchTaskInfoProvider.ProcessTasks();
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Moves the thread to other forum.
        /// </summary>
        /// <param name="postInfo">Post to be moved</param>
        /// <param name="forumId">ID of the forum where the post will be moved</param>
        protected virtual void MoveThreadInternal(ForumPostInfo postInfo, int forumId)
        {
            if (postInfo != null)
            {
                int oldForumId = postInfo.PostForumID;

                var values = new Dictionary<string, object>
                {
                    {"PostForumID", forumId}
                };

                // Continuous Integration serialization is handled by UpdateData
                var whereCondition = new WhereCondition().WhereStartsWith("PostIDPath", postInfo.PostIDPath);
                UpdateData(whereCondition, values);

                ForumInfoProvider.UpdateForumCounts(forumId);
                ForumInfoProvider.UpdateForumCounts(oldForumId);

                // Update relevant forum posts
                if (SearchIndexInfoProvider.SearchEnabled)
                {
                    DataSet ds = GetForumPosts().WhereEquals("PostApproved", 1).WhereStartsWith("PostIDPath", postInfo.PostIDPath).Column("PostId");
                    if (!DataHelper.DataSourceIsEmpty(ds))
                    {
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Update, ForumInfo.OBJECT_TYPE, SearchFieldsConstants.ID, Convert.ToString(dr["PostID"]), Convert.ToInt32(dr["PostID"]), false);
                        }
                        SearchTaskInfoProvider.ProcessTasks();
                    }
                }
            }
        }


        /// <summary>
        /// Handles the necessary continuous integration actions after a bulk update action.
        /// </summary>
        /// <param name="queryResult"></param>
        private void EnsureContinuousIntegrationReserialization(DataSet queryResult)
        {
            if (queryResult.Tables.Count == 1)
            {
                var updatedPostsIds = queryResult.Tables[0].AsEnumerable().Select(row => row.Field<int>(TypeInfo.IDColumn)).ToList();
                RepositoryBulkOperations.StoreObjects(TypeInfo, new WhereCondition().WhereIn(TypeInfo.IDColumn, updatedPostsIds));
            }
        }


        private static QueryDataParameters GetQueryDataParameters()
        {
            var parameters = new QueryDataParameters();
            parameters.EnsureDataSet<ForumPostInfo>();

            return parameters;
        }

        #endregion
    }
}