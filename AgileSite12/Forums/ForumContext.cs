using System.Collections;
using System.Data;

using CMS.Base;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;

namespace CMS.Forums
{
    /// <summary>
    /// Forum context.
    /// </summary>
    public class ForumContext : AbstractContext<ForumContext>
    {
        #region "Variables"

        private static string mForumQuery = "ForumID";

        private static string mThreadQuery = "ThreadID";

        private static string mReplyQuery = "ReplyTo";

        private static string mSubscriptionQuery = "SubscribeTo";

        #endregion


        #region "Security methods"

        /// <summary>
        /// Returns hashtable with current user forums in which is moderator, key = forum id.
        /// </summary>
        /// <param name="userId">User ID</param>
        private static Hashtable GetUserModeratedForums(int userId)
        {
            Hashtable forums = RequestStockHelper.GetItem("ForumContext_CurrentUserModeratedForums" + userId) as Hashtable;
            if (forums == null)
            {
                forums = new Hashtable();
                DataSet ds = ForumModeratorInfoProvider.GetUserModeratedForums(userId);
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        forums[ValidationHelper.GetInteger(dr["ForumID"], 0)] = null;
                    }
                }
                RequestStockHelper.Add("ForumContext_CurrentUserModeratedForums" + userId, forums);
            }
            return forums;
        }


        /// <summary>
        /// Returns true if current user is moderator in at least one forum.
        /// </summary>
        public static bool UserIsModeratorInSomeForum
        {
            get
            {
                return (GetUserModeratedForums(MembershipContext.AuthenticatedUser.UserID).Count > 0);
            }
        }


        /// <summary>
        /// Returns true if specified user is group admin for selected forum.
        /// </summary>
        /// <param name="forumId">Forum ID</param>
        /// <param name="user">User info</param>
        public static bool IsCommunityGroupAdmin(int forumId, UserInfo user)
        {
            // Check whether user info is defined
            if (user == null)
            {
                return false;
            }

            // Try convert user object to current user info
            CurrentUserInfo cUser = user as CurrentUserInfo;

            // If isn't current user info, create new one
            if (cUser == null)
            {
                cUser = new CurrentUserInfo(user, true);
            }

            // Check whether is group admin
            ForumInfo fi = ForumInfoProvider.GetForumInfo(forumId);
            if (fi != null)
            {
                ForumGroupInfo fgi = ForumGroupInfoProvider.GetForumGroupInfo(fi.ForumGroupID);
                if (fgi != null)
                {
                    if (fgi.GroupGroupID > 0)
                    {
                        return cUser.IsGroupAdministrator(fgi.GroupGroupID);
                    }
                }
            }

            return false;
        }


        /// <summary>
        /// Returns true if current user is group admin for selected forum.
        /// </summary>
        /// <param name="forumId">Forum ID</param>
        public static bool IsCommunityGroupAdmin(int forumId)
        {
            return IsCommunityGroupAdmin(forumId, MembershipContext.AuthenticatedUser);
        }


        /// <summary>
        /// Returns true if specified user is moderator of current forum.
        /// </summary>
        /// <param name="forumID">Forum ID</param>
        /// <param name="communityGroupId">Community group ID</param>
        /// <param name="user">User info object</param>
        public static bool UserIsModerator(int forumID, int communityGroupId, UserInfo user)
        {
            // Check whether user info is defined
            if (user == null)
            {
                return false;
            }

            // Try convert user object to current user info
            CurrentUserInfo cUser = user as CurrentUserInfo;

            // If isn't current user info, create new one
            if (cUser == null)
            {
                cUser = new CurrentUserInfo(user, true);
            }

            // Public user can't be a moderator
            if (!AuthenticationHelper.IsAuthenticated())
            {
                return false;
            }

            // Global admin is moderator by default
            if (cUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin))
            {
                return true;
            }
            var forumIsModerated = false;
            var forum = ForumInfoProvider.GetForumInfo(forumID);
            if (forum != null)
            {
                forumIsModerated = forum.ForumModerated;
            }

            // Check whether current user is moderator
            bool isModerator = GetUserModeratedForums(cUser.UserID).Contains(forumID);
            if (isModerator && forumIsModerated)
            {
                return true;
            }

            // Check modify permission
            if (communityGroupId == 0)
            {
                object result = RequestStockHelper.GetItem("ForumContextIsModerator" + cUser.UserID);
                if (result == null)
                {
                    result = cUser.IsAuthorizedPerResource("cms.forums", "modify");
                    RequestStockHelper.Add("ForumContextIsModerator" + cUser.UserID, result);
                }

                return ValidationHelper.GetBoolean(result, false);
            }

            // Check whether current user is current group moderator if exists
            if (communityGroupId > 0)
            {
                return IsCommunityGroupAdmin(forumID);
            }

            return false;
        }


        /// <summary>
        /// Returns true if current user is moderator of current forum.
        /// </summary>
        /// <param name="forumID">Forum ID</param>
        /// <param name="communityGroupId">Community group ID</param>
        public static bool UserIsModerator(int forumID, int communityGroupId)
        {
            return UserIsModerator(forumID, communityGroupId, MembershipContext.AuthenticatedUser);
        }


        /// <summary>
        /// Checks if current post, forum or group is belong to current site. You have to insert at least one ID other parameters could be zero.
        /// </summary>
        /// <param name="groupID">Forum group id object.</param>
        /// <param name="forumID">Forum id object.</param>
        /// <param name="postID">Post id object.</param>
        public static void CheckSite(int groupID, int forumID, int postID)
        {
            // Get forum ID if post ID is input
            if (postID > 0)
            {
                ForumPostInfo fpi = ForumPostInfoProvider.GetForumPostInfo(postID);
                if (fpi != null)
                {
                    forumID = fpi.PostForumID;
                }
            }

            // Get group ID if forum ID is input
            if (forumID > 0)
            {
                ForumInfo fi = ForumInfoProvider.GetForumInfo(forumID);
                if (fi != null)
                {
                    groupID = fi.ForumGroupID;
                }
            }

            // Get group object
            if (groupID > 0)
            {
                ForumGroupInfo fgi = ForumGroupInfoProvider.GetForumGroupInfo(groupID);
                if (fgi != null)
                {
                    // Check whether site id of edited object is belongs to current site
                    if (fgi.GroupSiteID != SiteContext.CurrentSiteID)
                    {
                        var url = AdministrationUrlHelper.GetInformationUrl(ResHelper.GetString("general.notassigned"));
                        URLHelper.SeeOther(url);
                    }
                }
            }
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets Discussion macro helper for current forum.
        /// </summary>
        public static DiscussionMacroResolver CurrentForumDiscussionMacroHelper
        {
            get
            {
                DiscussionMacroResolver dmh = RequestStockHelper.GetItem(ForumID.ToString() + "_dmh") as DiscussionMacroResolver;

                if (dmh == null)
                {
                    if (CurrentForum != null)
                    {
                        dmh = new DiscussionMacroResolver();
                        dmh.EnableBold = CurrentForum.ForumEnableFontBold;
                        dmh.EnableItalics = CurrentForum.ForumEnableFontItalics;
                        dmh.EnableStrikeThrough = CurrentForum.ForumEnableFontStrike;
                        dmh.EnableUnderline = CurrentForum.ForumEnableFontUnderline;
                        dmh.EnableCode = CurrentForum.ForumEnableCodeSnippet;
                        dmh.EnableColor = CurrentForum.ForumEnableFontColor;
                        dmh.EnableImage = CurrentForum.ForumEnableImage || CurrentForum.ForumEnableAdvancedImage;
                        dmh.EnableQuote = CurrentForum.ForumEnableQuote;
                        dmh.EnableURL = CurrentForum.ForumEnableURL || CurrentForum.ForumEnableAdvancedURL;
                        dmh.MaxImageSideSize = CurrentForum.ForumImageMaxSideSize;
                        dmh.QuotePostText = ResHelper.GetString("DiscussionMacroResolver.QuotePostText");
                        dmh.UseNoFollowForLinks = HTMLHelper.UseNoFollowForUsersLinks(SiteContext.CurrentSiteName);

                        if (CurrentForum.ForumHTMLEditor)
                        {
                            dmh.EncodeText = false;
                            dmh.ConvertLineBreaksToHTML = false;
                        }
                        else
                        {
                            dmh.EncodeText = true;
                            dmh.ConvertLineBreaksToHTML = true;
                        }

                        RequestStockHelper.Add(ForumID.ToString() + "_dmh", dmh);
                    }
                }

                return dmh;
            }
        }


        /// <summary>
        /// Gets the current forum mode.
        /// </summary>
        public static ForumStateEnum CurrentState
        {
            get
            {
                // Check whether the forum search takes place
                if ((QueryHelper.GetString("searchtext", "") != "") || (QueryHelper.GetString("searchusername", "") != ""))
                {
                    return ForumStateEnum.Search;
                }

                // Check whether current group is in current site
                if (CurrentGroup != null)
                {
                    if (CurrentGroup.GroupSiteID != SiteID)
                    {
                        return ForumStateEnum.AccessDenied;
                    }
                }
                else if (CurrentForum != null)
                {
                    ForumGroupInfo chgi = ForumGroupInfoProvider.GetForumGroupInfo(CurrentForum.ForumGroupID);
                    if ((chgi != null) && (chgi.GroupSiteID != SiteID))
                    {
                        return ForumStateEnum.AccessDenied;
                    }
                }


                // Check whether thread and forum is available
                if ((CurrentThread != null) && (CurrentForum != null))
                {
                    // Check whether thread is assigned to the forum
                    if (CurrentThread.PostForumID == CurrentForum.ForumID)
                    {
                        // Check reply to post
                        if (CurrentReplyThread != null)
                        {
                            // Check whether reply thread is assigned to the current forum
                            if ((CurrentReplyThread.PostForumID == CurrentForum.ForumID) && ((CurrentThread == null) || (CurrentReplyThread.PostIDPath.Contains(CurrentThread.PostIDPath))))
                            {
                                // Return thread reply
                                return ForumStateEnum.ReplyToPost;
                            }
                        }

                        // Check subscription to post
                        if (CurrentSubscribeThread != null)
                        {
                            // Check whether subscribe thread is assigned to the current forum
                            if ((CurrentSubscribeThread.PostForumID == CurrentForum.ForumID) && ((CurrentThread == null) || (CurrentSubscribeThread.PostIDPath.Contains(CurrentThread.PostIDPath))))
                            {
                                // Return Subscribe to post
                                return ForumStateEnum.SubscribeToPost;
                            }
                        }


                        // Check edit post mode
                        if ((CurrentMode == ForumMode.Edit) && (CurrentPost != null))
                        {
                            // Check whether post belongs to the current thread
                            if (CurrentPost.PostIDPath.Contains(CurrentThread.PostIDPath))
                            {
                                return ForumStateEnum.EditPost;
                            }
                        }


                        // Check edit post mode
                        if ((CurrentMode == ForumMode.Attachment) && (CurrentPost != null))
                        {
                            // Check whether post belongs to the current thread
                            if (CurrentPost.PostIDPath.Contains(CurrentThread.PostIDPath))
                            {
                                return ForumStateEnum.Attachments;
                            }
                        }

                        // Check topic move
                        if ((CurrentMode == ForumMode.TopicMove) && (CurrentPost != null))
                        {
                            // Check whether post belongs to the current thread
                            if (CurrentPost.PostIDPath.Contains(CurrentThread.PostIDPath))
                            {
                                return ForumStateEnum.TopicMove;
                            }
                        }

                        return ForumStateEnum.Thread;
                    }
                }

                // Check whether forum is available
                if (CurrentForum != null)
                {
                    // Check whether group is available
                    if (CurrentGroup != null)
                    {
                        // Check whether forum is asigned to the specified group
                        if (CurrentForum.ForumGroupID == CurrentGroup.GroupID)
                        {
                            // new thread
                            if (QueryHelper.Contains(mReplyQuery) && (QueryHelper.GetInteger(mReplyQuery, 0) == 0))
                            {
                                // New thread
                                return ForumStateEnum.NewThread;
                            }

                            // new subscribe
                            if (QueryHelper.Contains(mSubscriptionQuery) && (QueryHelper.GetInteger(mSubscriptionQuery, 0) == 0))
                            {
                                // New subscribe

                                return ForumStateEnum.NewSubscription;
                            }

                            // Threads mode
                            return ForumStateEnum.Threads;
                        }
                    }
                    else
                    {
                        // new thread
                        if (QueryHelper.Contains(mReplyQuery))
                        {
                            // New thread

                            return ForumStateEnum.NewThread;
                        }

                        // new subscribe
                        if (QueryHelper.Contains(mSubscriptionQuery))
                        {
                            // New subscribe

                            return ForumStateEnum.NewSubscription;
                        }

                        // Threads mode
                        return ForumStateEnum.Threads;
                    }
                }

                if (CurrentGroup != null)
                {
                    // Forum mode
                    return ForumStateEnum.Forums;
                }

                // Unknown
                return ForumStateEnum.Unknown;
            }
        }


        /// <summary>
        /// Gets current forum mode.
        /// </summary>
        public static ForumMode CurrentMode
        {
            get
            {
                if (CurrentForum != null)
                {
                    var currentUser = MembershipContext.AuthenticatedUser;
                    bool isModerator = UserIsModerator(CurrentForum.ForumID, CommunityGroupID);
                    bool isPostAuthor = false;

                    if ((CurrentPost != null) && AuthenticationHelper.IsAuthenticated())
                    {
                        isPostAuthor = (CurrentPost.PostUserID == currentUser.UserID);
                    }

                    switch (QueryHelper.GetString("mode", "").ToLowerCSafe())
                    {
                        case "edit":
                            if (isModerator || (isPostAuthor && CurrentForum.ForumAuthorEdit))
                            {
                                return ForumMode.Edit;
                            }
                            break;
                        case "attachment":
                            if ((currentUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin) || (CurrentForum.AllowAttachFiles != SecurityAccessEnum.Nobody)) && (isModerator || (isPostAuthor && ForumInfoProvider.IsAuthorizedPerForum(CurrentForum.ForumID, CurrentForum.ForumGroupID, "AttachFiles", CurrentForum.AllowAttachFiles, MembershipContext.AuthenticatedUser))))
                            {
                                return ForumMode.Attachment;
                            }
                            break;
                        case "quote":
                            if (CurrentForum.ForumEnableQuote)
                            {
                                return ForumMode.Quote;
                            }
                            break;
                        case "topicmove":
                            if (isModerator)
                            {
                                return ForumMode.TopicMove;
                            }
                            break;
                        default:
                            return ForumMode.Unknown;
                    }
                }

                return ForumMode.Unknown;
            }
        }


        /// <summary>
        /// Gets or sets the site ID.
        /// </summary>
        public static int SiteID
        {
            get
            {
                int siteId = ValidationHelper.GetInteger(RequestStockHelper.GetItem("SiteID"), 0);
                if (siteId == 0)
                {
                    siteId = SiteContext.CurrentSiteID;
                    RequestStockHelper.Add("SiteID", siteId);
                }

                return siteId;
            }
            set
            {
                RequestStockHelper.Add("SiteID", value);
            }
        }


        /// <summary>
        /// Gets or sets the group id based on current settings.
        /// </summary>
        public static int GroupID
        {
            get
            {
                return ValidationHelper.GetInteger(RequestStockHelper.GetItem("ForumGroupID"), 0);
            }
            set
            {
                RequestStockHelper.Add("ForumGroupID", value);
            }
        }


        /// <summary>
        /// Gets or sets the forum id base on current url or settings.
        /// </summary>
        public static int ForumID
        {
            get
            {
                if (ValidationHelper.GetInteger(RequestStockHelper.GetItem("ForumID"), 0) == 0)
                {
                    RequestStockHelper.Add("ForumID", QueryHelper.GetInteger(mForumQuery, 0));
                }

                return ValidationHelper.GetInteger(RequestStockHelper.GetItem("ForumID"), 0);
            }
            set
            {
                RequestStockHelper.Add("ForumID", value);
            }
        }


        /// <summary>
        /// Gets or sets community group id.
        /// </summary>
        public static int CommunityGroupID
        {
            get
            {
                return ValidationHelper.GetInteger(RequestStockHelper.GetItem("CommunityGroupID"), 0);
            }
            set
            {
                RequestStockHelper.Add("CommunityGroupID", value);
            }
        }


        /// <summary>
        /// Gets or sets the thread id based on current url or settings.
        /// </summary>
        public static int ThreadID
        {
            get
            {
                int threadId = ValidationHelper.GetInteger(RequestStockHelper.GetItem("ThreadID"), 0);
                if (threadId == 0)
                {
                    threadId = QueryHelper.GetInteger(mThreadQuery, 0);
                    // Backward compatibility
                    if (threadId == 0)
                    {
                        threadId = ValidationHelper.GetInteger(QueryHelper.GetString("Thread", "").TrimStart('0'), 0);
                    }

                    if (threadId > 0)
                    {
                        RequestStockHelper.Add("ThreadID", threadId);
                    }
                }
                return threadId;
            }
            set
            {
                RequestStockHelper.Add("ThreadID", value);
                RequestStockHelper.Remove(value.ToString() + "_CurrentPostInfo");
            }
        }


        /// <summary>
        /// Gets or sets the forum id base on current url or settings.
        /// </summary>
        public static int ReplyThreadID
        {
            get
            {
                if (ValidationHelper.GetInteger(RequestStockHelper.GetItem("ReplyThreadID"), 0) == 0)
                {
                    RequestStockHelper.Add("ReplyThreadID", QueryHelper.GetInteger("ReplyTo", 0));
                }

                return ValidationHelper.GetInteger(RequestStockHelper.GetItem("ReplyThreadID"), 0);
            }
            set
            {
                RequestStockHelper.Add("ReplyThreadID", value);
                RequestStockHelper.Remove("CurrentReplyThreadInfo");
            }
        }


        /// <summary>
        /// Gets or sets the forum id base on current url or settings.
        /// </summary>
        public static int SubscribeThreadID
        {
            get
            {
                if (ValidationHelper.GetInteger(RequestStockHelper.GetItem("SubscribeThreadID"), 0) == 0)
                {
                    RequestStockHelper.Add("SubscribeThreadID", QueryHelper.GetInteger("SubscribeTo", 0));
                }

                return ValidationHelper.GetInteger(RequestStockHelper.GetItem("SubscribeThreadID"), 0);
            }
            set
            {
                RequestStockHelper.Add("SubscribeThreadID", value);
                RequestStockHelper.Remove("CurrentSubscribeThreadInfo");
            }
        }


        /// <summary>
        /// Returns curren reply thread info.
        /// </summary>
        public static ForumPostInfo CurrentSubscribeThread
        {
            get
            {
                ForumPostInfo fpi = RequestStockHelper.GetItem("CurrentSubscribeThreadInfo") as ForumPostInfo;

                if (fpi == null)
                {
                    fpi = ForumPostInfoProvider.GetForumPostInfo(SubscribeThreadID);
                    if ((fpi == null) || (CurrentForum == null) || (CurrentThread == null) || (CurrentThread.PostId != ForumPostInfoProvider.GetPostRootFromIDPath(fpi.PostIDPath)) || (fpi.PostForumID != CurrentForum.ForumID) || (!fpi.PostApproved && !UserIsModerator(CurrentForum.ForumID, CommunityGroupID)))
                    {
                        return null;
                    }
                    RequestStockHelper.Add("CurrentSubscribeThreadInfo", fpi);
                }

                return fpi;
            }
        }


        /// <summary>
        /// Returns curren reply thread info.
        /// </summary>
        public static ForumPostInfo CurrentReplyThread
        {
            get
            {
                ForumPostInfo fpi = RequestStockHelper.GetItem("CurrentReplyThreadInfo") as ForumPostInfo;
                if (fpi == null)
                {
                    fpi = ForumPostInfoProvider.GetForumPostInfo(ReplyThreadID);
                    if ((fpi == null) || (CurrentForum == null) || (CurrentThread == null) || (CurrentThread.PostId != ForumPostInfoProvider.GetPostRootFromIDPath(fpi.PostIDPath)) || (fpi.PostForumID != CurrentForum.ForumID) || (!fpi.PostApproved && !UserIsModerator(CurrentForum.ForumID, CommunityGroupID)))
                    {
                        return null;
                    }

                    RequestStockHelper.Add("CurrentReplyThreadInfo", fpi);
                }
                return fpi;
            }
        }


        /// <summary>
        /// Returns current forum.
        /// </summary>
        [RegisterProperty]
        public static ForumInfo CurrentForum
        {
            get
            {
                ForumInfo fi = null;

                if (ForumID > 0)
                {
                    fi = RequestStockHelper.GetItem(ForumID.ToString() + "_CurrentForumInfo") as ForumInfo;
                    if (fi == null)
                    {
                        fi = ForumInfoProvider.GetForumInfo(ForumID);
                        if (fi != null)
                        {
                            if (((CurrentGroup != null) && (CurrentGroup.GroupID != fi.ForumGroupID)) || (!fi.ForumOpen && !UserIsModerator(ForumID, CommunityGroupID)))
                            {
                                fi = null;
                            }
                        }
                        RequestStockHelper.Add(ForumID.ToString() + "_CurrentForumInfo", fi);
                    }
                }
                // Ad-hoc forum
                else if (ForumID < 0)
                {
                    fi = new ForumInfo();
                    fi.ForumID = 0;
                    fi.ForumSiteID = SiteID;
                    fi.ForumDocumentID = DocumentContext.CurrentDocument.DocumentID;
                    fi.ForumGroupID = ForumGroupInfoProvider.GetAdHocGroupInfo(SiteID).GroupID;
                }

                return fi;
            }
        }


        /// <summary>
        /// Returns current thread.
        /// </summary>
        [RegisterProperty]
        public static ForumPostInfo CurrentThread
        {
            get
            {
                if (ThreadID > 0)
                {
                    ForumPostInfo fpi = RequestStockHelper.GetItem(ThreadID.ToString() + "_CurrentPostInfo") as ForumPostInfo;

                    if (fpi == null)
                    {
                        fpi = ForumPostInfoProvider.GetForumPostInfo(ThreadID);
                        if ((fpi == null) || (CurrentForum == null) || ((fpi.PostForumID != CurrentForum.ForumID) || (!fpi.PostApproved && !UserIsModerator(CurrentForum.ForumID, CommunityGroupID) && (fpi.PostUserID != MembershipContext.AuthenticatedUser.UserID))))
                        {
                            return null;
                        }
                        RequestStockHelper.Add(ThreadID.ToString() + "_CurrentPostInfo", fpi);
                    }

                    return fpi;
                }
                return null;
            }
        }


        /// <summary>
        /// Returns current post for edit or attachment.
        /// </summary>
        [RegisterProperty]
        public static ForumPostInfo CurrentPost
        {
            get
            {
                ForumPostInfo fpi = RequestStockHelper.GetItem("CurrentPost") as ForumPostInfo;

                if (fpi == null)
                {
                    fpi = ForumPostInfoProvider.GetForumPostInfo(QueryHelper.GetInteger("PostID", 0));
                    if ((fpi == null) || (CurrentForum == null) || (CurrentThread == null) || (CurrentThread.PostId != ForumPostInfoProvider.GetPostRootFromIDPath(fpi.PostIDPath)) || (fpi.PostForumID != CurrentForum.ForumID) || (!fpi.PostApproved && !UserIsModerator(CurrentForum.ForumID, CommunityGroupID) && (fpi.PostUserID != MembershipContext.AuthenticatedUser.UserID)))
                    {
                        return null;
                    }
                    RequestStockHelper.Add("CurrentPost", fpi);
                }

                return fpi;
            }
        }


        /// <summary>
        /// Returns current thread.
        /// </summary>
        public static ForumGroupInfo CurrentGroup
        {
            get
            {
                if (GroupID > 0)
                {
                    ForumGroupInfo fgi = RequestStockHelper.GetItem(GroupID.ToString() + "_ForumGroupInfo") as ForumGroupInfo;
                    if (fgi == null)
                    {
                        fgi = ForumGroupInfoProvider.GetForumGroupInfo(GroupID);
                        RequestStockHelper.Add(GroupID.ToString() + "_ForumGroupInfo", fgi);
                    }

                    return fgi;
                }

                return null;
            }
        }

        #endregion


        #region "Abstract context implementation"

        /// <summary>
        /// Registers the properties of this object
        /// </summary>
        protected sealed override void RegisterProperties()
        {
            base.RegisterProperties();

            RegisterProperty<ForumGroupInfo>("CurrentGroup", m => GetCurrentGroup());
        }


        /// <summary>
        /// Gets the current forum group
        /// </summary>
        private static object GetCurrentGroup()
        {
            if ((CurrentGroup == null) && (CurrentForum != null))
            {
                return ForumGroupInfoProvider.GetForumGroupInfo(CurrentForum.ForumGroupID);
            }

            return CurrentGroup;
        }

        #endregion
    }
}