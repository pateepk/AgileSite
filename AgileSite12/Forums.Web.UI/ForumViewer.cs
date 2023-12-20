using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Web.UI;

using CMS.Base;
using CMS.Base.Web.UI;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Globalization;
using CMS.Helpers;
using CMS.IO;
using CMS.Localization;
using CMS.Membership;
using CMS.SiteProvider;
using CMS.UIControls;


namespace CMS.Forums.Web.UI
{
    /// <summary>
    /// Summary description for ForumViewer.
    /// </summary>
    public class ForumViewer : CMSUserControl, IPostBackEventHandler
    {
        #region "Variables"

        private int mForumID;
        private int mThreadID;
        private int communityGroupId;

        // Hashtable of threads, loaded on demand
        private Hashtable mThreads;

        // Thread view mode
        private FlatModeEnum mViewMode = FlatModeEnum.Threaded;

        // Indicates whether forum is adHoc

        // Indicates whether page should be redirected after some action

        // Layout name
        private string mForumLayout = "Flat";

        // Base URL for friendly URLs
        private string mFriendlyBaseURL;

        // Friendly URL extension
        private string mFriendlyURLExtension;

        // Enabling friendly URL

        // Maximum side size of post attachment image
        private int mAttachmentImageMaxSideSize = 100;

        // Indicates whether avatars should be displayed
        private bool mEnableAvatars = true;

        // Max side size of avatar image
        private int mAvatarMaxSideSize = 80;

        // Sets the thread view mode
        private FlatModeEnum mForumViewMode = FlatModeEnum.Threaded;

        // Indicates whether image attachment should be displayed like an image

        // Indicates whether user should be redirected to the logon page
        private bool mRedirectUnauthorized = true;

        // Indicates whether user should see the forums in the group list even if he has no access

        // Sets logon page URL
        private string mLogonPageURL;

        // Sets access denied page URL
        private string mAccessDeniedPageURL;

        // Indicates whether paging should be used on thread list page
        private bool mEnableThreadPaging = true;

        // Sets thread page size
        private int mThreadPageSize = 10;

        // Indicates whether paging should be used on posts list page
        private bool mEnablePostsPaging = true;

        // Sets size of posts page
        private int mPostsPageSize = 10;

        // Tree forum show mode
        private ShowModeEnum mShowMode = ShowModeEnum.TreeMode;

        // Indicates whether forum with tree layout should be expanded by default

        // Indicates whether user signature should be displayed
        private bool mEnableSignature = true;

        // Indicates whether user can add the posts to his favorites

        // Sets name of groups in which should be search proceeded
        private string mSearchInGroups = String.Empty;

        // Sets the no results search text
        private string mSearchNoResults = String.Empty;

        // Maximal nesting level of posts
        private int mMaxRelativeLevel = -1;

        // Indicates whether badge should be displayed
        private bool mDisplayBadgeInfo = true;

        // Indicates whether username is link to user profile.
        private bool mRedirectToUserProfile = true;

        // Sets the forums in which should be search performed
        private string mSearchInForums = String.Empty;

        // Sets the  unsubscription URL
        private string mUnsubscriptionURL = String.Empty;

        // Sets the community group id

        // Sets the where condition

        // Forum base URL

        // Indicates whether action scripts should be generated

        // Indicates whether onsite management is allowed
        private bool mEnableOnSiteManagement = true;

        // Indicates whether subscription is allowed
        private bool mEnableSubscription = true;

        // Sets the security access enum
        private SecurityAccessEnum mAbuseReportAccess = SecurityAccessEnum.AuthenticatedUsers;

        // Sets the threads in which should be search performed
        private string mSearchInThreads = String.Empty;

        // Indicates whether logging activity is performed

        /// <summary>
        /// Regular expression to match groups ("sometext") from within the text.
        /// </summary>
        private static Regex mTextGroupsRegExp;

        /// <summary>
        /// Current site name.
        /// </summary>
        private string mSiteName = SiteContext.CurrentSiteName;

        /// <summary>
        /// Current site id.
        /// </summary>
        private int mSiteId = SiteContext.CurrentSiteID;


        

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets or sets the site name which should be used for current control.
        /// </summary>
        public string SiteName
        {
            get
            {
                return mSiteName;
            }
            set
            {
                mSiteName = value;
                if (!String.IsNullOrEmpty(mSiteName))
                {
                    SiteInfo si = SiteInfoProvider.GetSiteInfo(mSiteName);
                    if (si != null)
                    {
                        mSiteId = si.SiteID;
                        ForumContext.SiteID = si.SiteID;
                    }
                }
            }
        }


        /// <summary>
        /// Gets or sets the site id which should be used for current control.
        /// </summary>
        public int SiteID
        {
            get
            {
                return mSiteId;
            }
            set
            {
                mSiteId = value;
                ForumContext.SiteID = value;

                if (mSiteId > 0)
                {
                    SiteInfo si = SiteInfoProvider.GetSiteInfo(mSiteId);
                    if (si != null)
                    {
                        mSiteName = si.SiteName;
                    }
                }
            }
        }


        /// <summary>
        /// Gets or sets the roles split by semicolon, which roles can report abuse, requires  security access state authorize roles.
        /// </summary>
        public string AbuseReportRoles
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the security access enum to abuse report (Authenticated users by default).
        /// </summary>
        public SecurityAccessEnum AbuseReportAccess
        {
            get
            {
                return mAbuseReportAccess;
            }
            set
            {
                mAbuseReportAccess = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether subscription is enabled.
        /// </summary>
        public bool EnableSubscription
        {
            get
            {
                return mEnableSubscription;
            }
            set
            {
                mEnableSubscription = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether onsite management is allowed.
        /// </summary>
        public bool EnableOnSiteManagement
        {
            get
            {
                return mEnableOnSiteManagement;
            }
            set
            {
                mEnableOnSiteManagement = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether action scripts should be generated.
        /// </summary>
        public bool GenerateActionScripts
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the where condition.
        /// </summary>
        public string WhereCondition
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the community group id.
        /// </summary>
        public virtual int CommunityGroupID
        {
            get
            {
                if (communityGroupId == 0)
                {
                    communityGroupId = QueryHelper.GetInteger("CommunityGroupID", 0);
                }

                return communityGroupId;
            }
            set
            {
                communityGroupId = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether username is link to user profile.
        /// User profile path must be defined in site settings
        /// </summary>
        public bool RedirectToUserProfile
        {
            get
            {
                return mRedirectToUserProfile;
            }
            set
            {
                mRedirectToUserProfile = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether badge info should be displayed.
        /// </summary>
        public bool DisplayBadgeInfo
        {
            get
            {
                return mDisplayBadgeInfo;
            }
            set
            {
                mDisplayBadgeInfo = value;
            }
        }


        /// <summary>
        /// Gets or sets the maximal nesting level of posts.
        /// </summary>
        public int MaxRelativeLevel
        {
            get
            {
                return mMaxRelativeLevel;
            }
            set
            {
                mMaxRelativeLevel = value;
            }
        }


        /// <summary>
        /// Gets or sets the text which should be displayed if no result found.
        /// </summary>
        public string SearchNoResults
        {
            get
            {
                return mSearchNoResults;
            }
            set
            {
                mSearchNoResults = value;
            }
        }


        /// <summary>
        /// Gets or sets the groups names in which should be search proceeded, use semicolon like separator.
        /// </summary>
        public string SearchInGroups
        {
            get
            {
                return mSearchInGroups;
            }
            set
            {
                mSearchInGroups = value;
            }
        }


        /// <summary>
        /// Gets or sets the forums in which should be search performed.
        /// </summary>
        public string SearchInForums
        {
            get
            {
                return mSearchInForums;
            }
            set
            {
                mSearchInForums = value;
            }
        }


        /// <summary>
        /// Gets or sets the threads where search should be performed.
        /// </summary>
        public string SearchInThreads
        {
            get
            {
                return mSearchInThreads;
            }
            set
            {
                mSearchInThreads = value;
            }
        }


        /// <summary>
        /// Indicates whether user can add the posts to his favorites.
        /// </summary>
        public bool EnableFavorites
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether user signature should be displayed.
        /// </summary>
        public bool EnableSignature
        {
            get
            {
                return mEnableSignature;
            }
            set
            {
                mEnableSignature = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether forum with tree layout should be expanded.
        /// </summary>
        public bool ExpandTree
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the tree forum show mode.
        /// </summary>
        public ShowModeEnum ShowMode
        {
            get
            {
                return mShowMode;
            }
            set
            {
                mShowMode = value;
            }
        }


        /// <summary>
        /// Gets or sets the size of posts page size.
        /// </summary>
        public int PostsPageSize
        {
            get
            {
                return mPostsPageSize;
            }
            set
            {
                mPostsPageSize = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether paging should be used on posts list page.
        /// </summary>
        public bool EnablePostsPaging
        {
            get
            {
                return mEnablePostsPaging;
            }
            set
            {
                mEnablePostsPaging = value;
            }
        }


        /// <summary>
        /// Gets or sets the size of thread list page.
        /// </summary>
        public int ThreadPageSize
        {
            get
            {
                return mThreadPageSize;
            }
            set
            {
                mThreadPageSize = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether paging should be used on thread list page.
        /// </summary>
        public bool EnableThreadPaging
        {
            get
            {
                return mEnableThreadPaging;
            }
            set
            {
                mEnableThreadPaging = value;
            }
        }


        /// <summary>
        /// Gets or sets the logon page URL.
        /// </summary>
        public string LogonPageURL
        {
            get
            {
                return mLogonPageURL ?? (mLogonPageURL = AuthenticationHelper.GetSecuredAreasLogonPage(SiteContext.CurrentSiteName));
            }
            set
            {
                if (value != "")
                {
                    mLogonPageURL = value;
                }
            }
        }


        /// <summary>
        /// Gets or sets the access denied page URL (URL where the user is redirected when 
        /// trying to access forum for which the user is unauthorized).
        /// </summary>
        public string AccessDeniedPageURL
        {
            get
            {
                return mAccessDeniedPageURL;
            }
            set
            {
                if (value != "")
                {
                    mAccessDeniedPageURL = value;
                }
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether use should be redirected to logon page
        /// if he hasn't appropriate permissions to the requested action.
        /// </summary>
        public bool RedirectUnauthorized
        {
            get
            {
                return mRedirectUnauthorized;
            }
            set
            {
                mRedirectUnauthorized = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether the forums for which the user has no permissions
        /// are visible in the list of forums in forum group.
        /// </summary>
        public bool HideForumForUnauthorized
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the value that indicates whether image attachment should be displayed like an image.
        /// </summary>
        public bool DisplayAttachmentImage
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the thread view mode.
        /// </summary>
        public FlatModeEnum ForumViewMode
        {
            get
            {
                return mForumViewMode;
            }
            set
            {
                mForumViewMode = value;
            }
        }


        /// <summary>
        /// Gets or sets the max side size of avatar image.
        /// </summary>
        public int AvatarMaxSideSize
        {
            get
            {
                return mAvatarMaxSideSize;
            }
            set
            {
                mAvatarMaxSideSize = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether avatars should be displayed.
        /// </summary>
        public bool EnableAvatars
        {
            get
            {
                return mEnableAvatars;
            }
            set
            {
                mEnableAvatars = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether forums should generate friendly URLs.
        /// </summary>
        public bool UseFriendlyURL
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the friendly URL extension.
        /// </summary>
        public string FriendlyURLExtension
        {
            get
            {
                // If friendly URL extension is not defined get default URL extension
                return mFriendlyURLExtension ?? (mFriendlyURLExtension = TreePathUtils.GetUrlExtension());
            }
            set
            {
                mFriendlyURLExtension = value;
            }
        }


        /// <summary>
        /// Gets or sets the unsubscription URL.
        /// </summary>
        public string UnsubscriptionURL
        {
            get
            {
                return mUnsubscriptionURL;
            }
            set
            {
                mUnsubscriptionURL = value;
            }
        }


        /// <summary>
        /// Gets or sets the forum base URL.
        /// </summary>
        public string BaseURL
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets base URL for forum which use friendly URLs, if URL is not set URL is set from current document.
        /// </summary>
        public string FriendlyBaseURL
        {
            get
            {
                if (String.IsNullOrEmpty(mFriendlyBaseURL))
                {
                    if (DocumentContext.CurrentDocument != null)
                    {
                        string url = DocumentURLProvider.GetUrl(DocumentContext.OriginalAliasPath, DocumentContext.CurrentDocument.DocumentUrlPath);
                        string extension = Path.GetExtension(url);

                        if (!String.IsNullOrEmpty(extension))
                        {
                            url = url.Substring(0, url.Length - extension.Length);
                        }

                        mFriendlyBaseURL = ResolveUrl(url);
                    }
                }

                return mFriendlyBaseURL;
            }
            set
            {
                if (value != null)
                {
                    mFriendlyBaseURL = ResolveUrl(value);
                }
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether after action should be used redirect.
        /// </summary>
        public bool UseRedirectAfterAction
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the forum layout.
        /// </summary>
        public string ForumLayout
        {
            get
            {
                return mForumLayout;
            }
            set
            {
                mForumLayout = value;
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether current forum is AdHoc.
        /// </summary>
        public bool IsAdHocForum
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the thread view mode.
        /// </summary>
        public FlatModeEnum ViewMode
        {
            get
            {
                return mViewMode;
            }
            set
            {
                mViewMode = value;
            }
        }


        /// <summary>
        /// Gets order by expression with dependence on current view mode.
        /// </summary>
        public string ThreadOrderBy
        {
            get
            {
                // Check whether view mode is sets by view mode selector
                if (ForumContext.CurrentThread != null)
                {
                    ViewMode = ForumModes.GetFlatMode(ValidationHelper.GetString(SessionHelper.GetValue("CMSForumViewMode"), ViewMode.ToString()));
                    if ((ViewState["ViewMode"] == null) || (ViewMode != (FlatModeEnum)ViewState["ViewMode"]))
                    {
                        ViewState["ViewMode"] = ViewMode;
                    }
                }

                // Set sorting
                switch (ViewMode)
                {
                    case FlatModeEnum.OldestToNewest:
                        return "PostTime";
                    case FlatModeEnum.NewestToOldest:
                        return "PostTime DESC";
                }

                return "PostIDPath, PostTime";
            }
        }


        /// <summary>
        /// Gets or sets the maximum side size of image shown in post attachment list.
        /// </summary>
        public int AttachmentImageMaxSideSize
        {
            get
            {
                return mAttachmentImageMaxSideSize;
            }
            set
            {
                mAttachmentImageMaxSideSize = value;
            }
        }


        /// <summary>
        /// Gets or sets value that indicates whether logging activity is performed.
        /// </summary>
        public bool LogActivity
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the group id based on current settings.
        /// </summary>
        public virtual int GroupID
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the forum id base on current url or settings.
        /// </summary>
        public virtual int ForumID
        {
            get
            {
                if (mForumID == 0)
                {
                    mForumID = QueryHelper.GetInteger("ForumID", 0);
                }

                return mForumID;
            }
            set
            {
                mForumID = value;
            }
        }


        /// <summary>
        /// Gets or sets the thread id based on current url or settings.
        /// </summary>
        public virtual int ThreadID
        {
            get
            {
                if (mThreadID == 0)
                {
                    mThreadID = QueryHelper.GetInteger("ThreadID", 0);

                    // Backward compatibility
                    if (mThreadID == 0)
                    {
                        mThreadID = ValidationHelper.GetInteger(QueryHelper.GetString("Thread", "").TrimStart('0'), 0);
                    }
                }
                return mThreadID;
            }
            set
            {
                mThreadID = value;
            }
        }


        /// <summary>
        /// Groups ("sometext") from within the text regular expression.
        /// </summary>
        private static Regex TextGroupsRegExp
        {
            get
            {
                return mTextGroupsRegExp ?? (mTextGroupsRegExp = RegexHelper.GetRegex("\".*?\"", true));
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public ForumViewer()
        {
            LogActivity = false;
            BaseURL = null;
            DisplayAttachmentImage = false;
            HideForumForUnauthorized = false;
            ExpandTree = false;
            GenerateActionScripts = false;
            WhereCondition = null;
            CommunityGroupID = 0;
            EnableFavorites = false;
            AbuseReportRoles = null;
        }

        #endregion


        #region "Helper methods"

        /// <summary>
        /// Returns data from datarow or datarowview and specified column.
        /// </summary>
        /// <param name="data">Data (DataRowView or DataRow)</param>
        /// <param name="column">Column name</param>
        public object GetData(object data, string column)
        {
            return DataHelper.GetDataContainerItem(data, column);
        }


        /// <summary>
        /// Check if css class name is defined and set CSS class directive.
        /// </summary>
        /// <param name="cssClass">CSS class name</param>
        private string CSSClassNameTransformer(string cssClass)
        {
            // Set CSS class directive if is class name defined
            if (!String.IsNullOrEmpty(cssClass))
            {
                cssClass = " class=\"" + cssClass + "\" ";
            }

            return cssClass;
        }

        #endregion


        #region "Information methods"

        /// <summary>
        /// Returns number of threads in current forum.
        /// </summary>
        /// <param name="data">Container.DataItem</param>
        public int ThreadCount(object data)
        {
            int forumId = ValidationHelper.GetInteger(GetData(data, "ForumID"), 0);

            // Check whether forum exists
            if (forumId > 0)
            {
                // Check whether current user can see unapproved posts
                if (ForumContext.UserIsModerator(forumId, CommunityGroupID))
                {
                    // Get last forum post for current forum
                    return ValidationHelper.GetInteger(GetData(data, "ForumThreadsAbsolute"), 0);
                }
                else
                {
                    // Return default value
                    return ValidationHelper.GetInteger(GetData(data, "ForumThreads"), 0);
                }
            }

            // If forum doesn't exist return 0
            return 0;
        }


        /// <summary>
        /// Returns number of posts in current forum or thread.
        /// </summary>
        /// <param name="data">Container.DataItem</param>
        /// <param name="thread">If is true, returns number of posts for current thread, otherwise returns number of posts  for current forum</param>
        public int PostCount(object data, bool thread)
        {
            // Posts for current forum
            if (!thread)
            {
                // Get forum id from current row
                int forumId = ValidationHelper.GetInteger(GetData(data, "ForumID"), 0);
                // Check whether forum exists
                if (forumId > 0)
                {
                    // Check whether current user can see unapproved posts
                    if (ForumContext.UserIsModerator(forumId, CommunityGroupID))
                    {
                        // Get last forum post for current forum
                        return ValidationHelper.GetInteger(GetData(data, "ForumPostsAbsolute"), 0);
                    }
                    else
                    {
                        // Return default value
                        return ValidationHelper.GetInteger(GetData(data, "ForumPosts"), 0);
                    }
                }
            }
            // Posts for current thread
            else if (ForumContext.CurrentForum != null)
            {
                // Check whether current user can see unapproved posts
                if (ForumContext.UserIsModerator(ForumContext.CurrentForum.ForumID, CommunityGroupID))
                {
                    // Get last forum post for current forum
                    return ValidationHelper.GetInteger(GetData(data, "PostThreadPostsAbsolute"), 0);
                }
                else
                {
                    // Return default value
                    return ValidationHelper.GetInteger(GetData(data, "PostThreadPosts"), 0);
                }
            }


            // If forum doesn't exist return 0
            return 0;
        }


        /// <summary>
        /// Returns number of replies for current thread.
        /// </summary>
        /// <param name="data">Container.DataItem</param>
        public int RepliesCount(object data)
        {
            // Get number of posts
            int posts = PostCount(data, true);
            // Remove thredad from count
            if (posts > 0)
            {
                return posts - 1;
            }
            // Return zero if no reply exists
            return 0;
        }


        /// <summary>
        /// Returns string representation of datetime of last post for current forum or thread.
        /// </summary>
        /// <param name="data">Container.DataItem</param>
        /// <param name="thread">If is true, returns datetime of last post for current thread, otherwise returns datetime of last post  for current forum</param>
        /// <remarks>If forum is empty or for some reason dateTime is not available, "N/A" is returned.</remarks>
        public String LastTime(object data, bool thread)
        {
            // Last time for current forum
            if (!thread)
            {
                // Get forum id from current row
                int forumId = ValidationHelper.GetInteger(GetData(data, "ForumID"), 0);
                // Check whether forum exists
                if (forumId > 0)
                {
                    // Check whether current user can see unapproved posts
                    if (ForumContext.UserIsModerator(forumId, CommunityGroupID))
                    {
                        // Get last forum post for current forum
                        return GetTimezonedDateTime(data, "ForumLastPostTimeAbsolute");
                    }
                    else
                    {
                        // Return default value
                        return GetTimezonedDateTime(data, "ForumLastPostTime");
                    }
                }
            }
            // Last time for current thread
            else if (ForumContext.CurrentForum != null)
            {
                // Check whether current user can see unapproved posts
                if (ForumContext.UserIsModerator(ForumContext.CurrentForum.ForumID, CommunityGroupID))
                {
                    // Get last forum post for current forum
                    return GetTimezonedDateTime(data, "PostThreadLastPostTimeAbsolute");
                }
                else
                {
                    // Return default value
                    return GetTimezonedDateTime(data, "PostThreadLastPostTime");
                }
            }

            //return "N/A" by default
            return ResHelper.GetString("general.na");
        }


        /// <summary>
        /// Returns user name of last post for current forum or thread.
        /// </summary>
        /// <param name="data">Container.DataItem</param>
        /// <param name="thread">If is true, returns user name of last post for current thread, otherwise returns user name of last post  for current forum</param>
        public string LastUser(object data, bool thread)
        {
            // Last time for current forum
            if (!thread)
            {
                // Get forum id from current row
                int forumId = ValidationHelper.GetInteger(GetData(data, "ForumID"), 0);
                // Check whether forum exists
                if (forumId > 0)
                {
                    // Check whether current user can see unapproved posts
                    if (ForumContext.UserIsModerator(forumId, CommunityGroupID))
                    {
                        // Get last forum post for current forum
                        return HTMLHelper.HTMLEncode(ValidationHelper.GetString(GetData(data, "ForumLastPostUserNameAbsolute"), ""));
                    }
                    else
                    {
                        // Return default value
                        return HTMLHelper.HTMLEncode(ValidationHelper.GetString(GetData(data, "ForumLastPostUserName"), ""));
                    }
                }
            }
            // Last time for current thread
            else if (ForumContext.CurrentForum != null)
            {
                // Check whether current user can see unapproved posts
                if (ForumContext.UserIsModerator(ForumContext.CurrentForum.ForumID, CommunityGroupID))
                {
                    // Get last forum post for current forum
                    return HTMLHelper.HTMLEncode(ValidationHelper.GetString(GetData(data, "PostThreadLastPostUserNameAbsolute"), ""));
                }
                else
                {
                    // Return default value
                    return HTMLHelper.HTMLEncode(ValidationHelper.GetString(GetData(data, "PostThreadLastPostUserName"), ""));
                }
            }

            // Return empty string time by default
            return "";
        }

        #endregion


        #region "Design methods"

        /// <summary>
        /// Returns true or false rusult with dependence on bool condition.
        /// </summary>
        /// <param name="operand">Bool condition</param>
        /// <param name="trueResult">True result</param>
        /// <param name="falseResult">False result</param>
        public static string IFCompare(bool operand, string trueResult, string falseResult)
        {
            if (operand)
            {
                return trueResult;
            }

            return falseResult;
        }


        /// <summary>
        /// Generate HTML part with username, if forum is set to display email then generates mailto:
        /// If username should be link to user profile generates link to user profile
        /// </summary>
        /// <param name="data">Data container</param>    
        public string GetUserName(object data)
        {
            string userName = HTMLHelper.HTMLEncode(ValidationHelper.GetString(GetData(data, "PostUserName"), ""));
            string realUserName = HTMLHelper.HTMLEncode(ValidationHelper.GetString(GetData(data, "UserName"), ""));

            if ((RedirectToUserProfile) && (!String.IsNullOrEmpty(realUserName)))
            {
                string memberProfile = GetMemberProfileUrl(realUserName);
                if (!String.IsNullOrEmpty(memberProfile))
                {
                    // Return user name with link to their profile
                    return string.Format("<a href=\"{0}\" title=\"{1}\" class=\"PostUser\">{1}</a>", DocumentURLProvider.GetUrl(memberProfile), userName);
                }
            }

            // Get forum ID
            int forumId = ValidationHelper.GetInteger(GetData(data, "PostForumID"), 0);
            if (forumId == 0)
            {
                forumId = ValidationHelper.GetInteger(GetData(data, "ForumID"), 0);
            }

            // Get forum info
            ForumInfo fi = ForumInfoProvider.GetForumInfo(forumId);
            if ((fi != null) && (fi.ForumDisplayEmails))
            {
                // Get user email
                string email = HTMLHelper.HTMLEncode(ValidationHelper.GetString(GetData(data, "PostUserMail"), ""));
                if (!String.IsNullOrEmpty(email))
                {
                    // And return as link
                    return string.Format("<a href=\"mailto:{0}\" title=\"{1}\" class=\"PostUser\">{1}</a>", email, userName);
                }
            }

            // Return only user name
            return userName;
        }


        /// <summary>
        /// Return text with dependence on input value, if value is null or empty returns not empty text
        /// parameter otherwise returns empy text parameter
        /// </summary>
        /// <param name="value">Input value</param>
        /// <param name="notEmptyText">Not empty text</param>
        /// <param name="emptyText">Empty text</param>
        /// <param name="type">Forum action type</param>
        public string GetNotEmpty(object value, string notEmptyText, string emptyText, ForumActionType type)
        {
            switch (type)
            {
                case ForumActionType.Badge:
                    if (!DisplayBadgeInfo)
                    {
                        return "";
                    }
                    break;
            }

            // Return default value
            if (ValidationHelper.GetString(value, "").Trim() != "")
            {
                return notEmptyText;
            }

            // Input text is not empty
            return emptyText;
        }


        /// <summary>
        /// Generate indent code.
        /// </summary>
        /// <param name="data">Data container</param>
        /// <param name="html">HTML code</param>
        public string GenerateIndentCode(object data, string html)
        {
            // Disable indentation if viewmode is not threaded
            if (ViewMode != FlatModeEnum.Threaded)
            {
                return "";
            }

            // Get indent value
            int postIndent = ValidationHelper.GetInteger(GetData(data, "PostLevel"), 0);
            if (postIndent > 0)
            {
                string result = "";
                for (int i = 0; i < postIndent; i++)
                {
                    result += html;
                }

                return result;
            }

            return "";
        }


        /// <summary>
        /// Returns selected value from thread based on post id path.
        /// </summary>
        /// <param name="postIdPath">Post id path</param>
        /// <param name="column">Column</param>
        private object GetValueFromThread(string postIdPath, string column)
        {
            // Get thread ID
            int threadId = ForumPostInfoProvider.GetPostRootFromIDPath(postIdPath);
            if (mThreads == null)
            {
                mThreads = new Hashtable();
            }

            // Get post info
            ForumPostInfo fpi = mThreads[threadId] as ForumPostInfo;
            if (fpi == null)
            {
                fpi = ForumPostInfoProvider.GetForumPostInfo(threadId);
                mThreads[threadId] = fpi;
            }

            if (fpi != null)
            {
                return GetData(fpi, column);
            }

            return null;
        }


        /// <summary>
        /// Generate code if current post is mark as answer.
        /// </summary>
        /// <param name="data">Container.DataItem</param>
        /// <param name="html">Html or some code</param>
        public string GenerateAnswerCode(object data, string html)
        {
            int isAnswer = ValidationHelper.GetInteger(GetData(data, "PostIsAnswer"), 0);
            int isNotAnswer = ValidationHelper.GetInteger(GetData(data, "PostIsNotAnswer"), 0);
            if ((ForumContext.CurrentForum != null) && (isAnswer > 0))
            {
                // Check if post is marked as an answer
                if (((ForumContext.CurrentForum.ForumType == 2) || (ForumContext.CurrentForum.ForumType == 0 && ValidationHelper.GetInteger(GetValueFromThread(GetData(data, "PostIdPath") as string, "PostType"), 0) == 1))
                    && (ForumContext.CurrentForum.ForumIsAnswerLimit <= (isAnswer - isNotAnswer)))
                {
                    return html;
                }
            }
            return "";
        }


        /// <summary>
        /// Retruns css class.
        /// </summary>
        public string GetImageClass(object data)
        {
            string className;

            // Threads
            if (ValidationHelper.GetInteger(GetData(data, "PostID"), 0) != 0)
            {
                // Locked/Announcment thread
                if (ValidationHelper.GetBoolean(GetData(data, "PostIsLocked"), false))
                {
                    // Announcement
                    if (ValidationHelper.GetInteger(GetData(data, "PostStickOrder"), 0) > 0)
                    {
                        className = "ThreadImageAnnouncement";
                    }
                    else
                    {
                        // Locked
                        className = "ThreadImageLocked";
                    }
                }
                // Sticky thread
                else if (ValidationHelper.GetInteger(GetData(data, "PostStickOrder"), 0) > 0)
                {
                    className = "ThreadImageSticky";
                }
                else
                {
                    // Normal thread
                    className = "ThreadImage";
                }
            }
            // Forums
            else
            {
                className = ValidationHelper.GetBoolean(GetData(data, "ForumIsLocked"), false) ? "ForumImageLocked" : "ForumImage";
            }

            return className;
        }


        /// <summary>
        /// Returns avatar image.
        /// </summary>
        /// <param name="data">Container.DataItem</param>
        public string AvatarImage(object data)
        {
            // Check whether avatars are enabled
            if (!EnableAvatars)
            {
                return string.Empty;
            }

            bool imageSelected = false;

            // Try get avatar html code form request cache
            int userId = ValidationHelper.GetInteger(GetData(data, "PostUserID"), 0);
            string imageUrl = RequestStockHelper.GetItem("forumAvatar_" + userId) as string;
            string aType = SettingsKeyInfoProvider.GetValue(SiteContext.CurrentSiteName + ".CMSAvatarType");

            // Get user choice
            if (aType == AvatarInfoProvider.USERCHOICE)
            {
                UserInfo ui = UserInfoProvider.GetUserInfo(userId);
                if (ui != null)
                {
                    aType = ui.UserSettings.UserAvatarType;
                }
                else
                {
                    // If user doesn't exist choose cms.avatar
                    aType = AvatarInfoProvider.GRAVATAR;
                }
            }

            switch (aType)
            {
                case AvatarInfoProvider.AVATAR:

                    if (!String.IsNullOrEmpty(imageUrl))
                    {
                        return imageUrl;
                    }

                    // Create default url
                    imageUrl = ResolveUrl("~/CMSPages/GetAvatar.aspx?maxsidesize=" + AvatarMaxSideSize + "&avatarguid=");


                    #region "Avatar data are present in the datarow"

                    // Check whether exist user avatar guid
                    Guid avatarGuid = ValidationHelper.GetGuid(GetData(data, "AvatarGUID"), Guid.Empty);
                    if (avatarGuid != Guid.Empty)
                    {
                        imageUrl += avatarGuid;
                        imageSelected = true;
                    }
                    else
                    {
                        // Check old type avatar - backward compatibility
                        string userPicture = ValidationHelper.GetString(GetData(data, "UserPicture"), "");
                        // Old avatar exists
                        if (!String.IsNullOrEmpty(userPicture))
                        {
                            // Get picture filename
                            if (userPicture.Contains("/"))
                            {
                                string filename = userPicture.Remove(userPicture.IndexOfCSafe('/'));
                                string ext = Path.GetExtension(filename);
                                imageUrl += filename.Substring(0, (filename.Length - ext.Length));
                                imageUrl += HTMLHelper.HTMLEncode("&extension=" + ext);
                                imageSelected = true;
                            }
                        }

                        // Try get default avatar
                        if (!imageSelected)
                        {
                            // try retype data as datarow view
                            DataRowView drv = data as DataRowView;
                            DataRow dr = data as DataRow;

                            // If datarowview object exists, return column data
                            if (((drv != null) && (drv.Row.Table.Columns.Contains("UserGender"))) || (dr != null) && (dr.Table.Columns.Contains("UserGender")))
                            {
                                int gender = ValidationHelper.GetInteger(GetData(data, "UserGender"), 0);
                                AvatarInfo ai = AvatarInfoProvider.GetDefaultAvatar((UserGenderEnum)gender);

                                if (ai != null)
                                {
                                    imageUrl += ai.AvatarGUID;
                                    imageSelected = true;
                                }
                            }
                        }
                    }

                    #endregion


                    #region "Avatar data are not in datarow"

                    if (!imageSelected)
                    {
                        AvatarInfo ai;

                        // Get avatar from user info
                        UserSettingsInfo usi = UserSettingsInfoProvider.GetUserSettingsInfoByUser(userId);
                        if (usi != null)
                        {
                            if (usi.UserAvatarID > 0)
                            {
                                ai = AvatarInfoProvider.GetAvatarInfoWithoutBinary(usi.UserAvatarID);
                                if (ai != null)
                                {
                                    imageUrl += ai.AvatarGUID;
                                    imageSelected = true;
                                }
                            }

                            // Check old type avatar - backward compatibility
                            if (!imageSelected)
                            {
                                string userPicture = usi.UserPicture;
                                // Old avatar exists
                                if (!String.IsNullOrEmpty(userPicture))
                                {
                                    // Get picture filename
                                    if (userPicture.Contains("/"))
                                    {
                                        string filename = userPicture.Remove(userPicture.IndexOfCSafe('/'));
                                        string ext = Path.GetExtension(filename);
                                        imageUrl += filename.Substring(0, (filename.Length - ext.Length));
                                        imageUrl += HTMLHelper.HTMLEncode("&extension=" + ext);
                                        imageSelected = true;
                                    }
                                }
                            }

                            // Try get default avatar
                            if (!imageSelected)
                            {
                                ai = AvatarInfoProvider.GetDefaultAvatar((UserGenderEnum)usi.UserGender);

                                if (ai != null)
                                {
                                    imageUrl += ai.AvatarGUID;
                                    imageSelected = true;
                                }
                            }
                        }
                        else
                        {
                            ai = AvatarInfoProvider.GetDefaultAvatar(DefaultAvatarTypeEnum.User);
                            if (ai != null)
                            {
                                imageUrl += ai.AvatarGUID;
                                imageSelected = true;
                            }
                        }
                    }


                    #endregion

                    break;

                case AvatarInfoProvider.GRAVATAR:
                    imageSelected = true;

                    string email;
                    int uiGender;

                    // If user is registered
                    UserInfo ui = UserInfoProvider.GetUserInfo(userId);
                    if (ui != null)
                    {
                        email = ui.Email;
                        uiGender = ui.UserSettings.UserGender;
                    }
                    else
                    {
                        // User is public so try get User data from DataRow
                        email = ValidationHelper.GetString(GetData(data, "PostUserMail"), "");
                        uiGender = ValidationHelper.GetInteger(GetData(data, "UserGender"), 0);
                    }

                    // Create link
                    imageUrl = AvatarInfoProvider.CreateGravatarLink(email, uiGender, AvatarMaxSideSize, SiteContext.CurrentSiteName);

                    break;
            }

            if (imageSelected)
            {
                // Generate img tag
                imageUrl = HTMLHelper.EncodeForHtmlAttribute(imageUrl);
                imageUrl = "<img class=\"AvatarImage\" alt=\"User avatar\" src=\"" + imageUrl + "\" />";

                // Generate link tag if it is enabled
                string userName = HTMLHelper.HTMLEncode(ValidationHelper.GetString(GetData(data, "PostUserName"), ""));
                string realUserName = HTMLHelper.HTMLEncode(ValidationHelper.GetString(GetData(data, "UserName"), ""));

                if ((RedirectToUserProfile) && (!String.IsNullOrEmpty(realUserName)))
                {
                    string memberProfile = GetMemberProfileUrl(realUserName);
                    if (!String.IsNullOrEmpty(memberProfile))
                    {
                        imageUrl = string.Format("<a href=\"{0}\" title=\"{2}\" class=\"PostUser\">{1}</a>", DocumentURLProvider.GetUrl(memberProfile), imageUrl, userName);
                    }
                }

                // Add to the request cache
                RequestStockHelper.Add("forumAvatar_" + userId, imageUrl);
            }

            if (imageSelected)
            {
                return imageUrl;
            }

            return String.Empty;
        }

        #endregion


        #region "Search methods"

        /// <summary>
        /// Retusrn search dataset.
        /// </summary>
        public DataSet GetSearchDataSet()
        {
            string where = "";

            // Search where condition
            string tmpWhere = GetWhereCondition();
            // If search where condition doesn't exist return nothing
            if (String.IsNullOrEmpty(tmpWhere))
            {
                return null;
            }

            where += "(GroupSiteID = " + SiteID;

            // Community group search
            if (CommunityGroupID > 0)
            {
                where += " AND GroupGroupID = " + CommunityGroupID;
            }
            else
            {
                where += " AND GroupGroupID IS NULL";
            }

            // Search in forums
            if (!String.IsNullOrEmpty(SearchInForums))
            {
                string[] forums = SearchInForums.Split(';');
                SearchInForums = "";
                foreach (string temp in forums)
                {
                    int forumId = ValidationHelper.GetInteger(temp, 0);
                    if (forumId > 0)
                    {
                        SearchInForums += forumId + ",";
                    }
                }

                SearchInForums = SearchInForums.TrimEnd(',');

                if (SearchInForums != String.Empty)
                {
                    where += " AND PostForumID IN (" + SearchInForums + ")";
                }
            }

            // Search in threads
            if (!String.IsNullOrEmpty(SearchInThreads))
            {
                string[] threads = SearchInThreads.Split(';');
                string threadWhere = String.Empty;
                foreach (string temp in threads)
                {
                    int postId = ValidationHelper.GetInteger(temp, 0);
                    if (postId > 0)
                    {
                        ForumPostInfo fpi = ForumPostInfoProvider.GetForumPostInfo(postId);
                        if ((fpi != null) && (fpi.PostLevel == 0))
                        {
                            if (!String.IsNullOrEmpty(threadWhere))
                            {
                                threadWhere += " OR ";
                            }
                            threadWhere += " (PostIDPath LIKE '" + fpi.PostIDPath + "' + '%')";
                        }
                    }
                }

                if (!String.IsNullOrEmpty(threadWhere))
                {
                    where += " AND (" + threadWhere + ") ";
                }
            }

            // Search in groups
            if (!String.IsNullOrEmpty(SearchInGroups))
            {
                string searchin = SearchInGroups.Trim().Replace("'", "''").Replace(";;", ";");
                // Remove semicolon at start
                if (searchin.StartsWithCSafe(";"))
                {
                    searchin = searchin.Remove(0, 1);
                }
                // Remove semicolon at the end;
                if (searchin.EndsWithCSafe(";"))
                {
                    searchin = searchin.Substring(0, searchin.Length - 1);
                }

                // Check whether exist at least one group and create where condition
                if (!String.IsNullOrEmpty(searchin))
                {
                    searchin = "('" + searchin.Replace(";", "','") + "')";
                    where += " AND GroupName IN " + searchin;
                }
            }


            where += ")";

            // Custom where condition
            if (!String.IsNullOrEmpty(WhereCondition))
            {
                where = "(" + where + ") AND (" + WhereCondition + ")";
            }

            // Security
            where = ForumInfoProvider.CombineSecurityWhereCondition(where, CommunityGroupID);


            if (!String.IsNullOrEmpty(tmpWhere))
            {
                where = where + " AND (" + tmpWhere + ")";
            }

            // only approved
            where = where + " AND (PostApproved = 1)";

            // Do not search in adhoc group
            where = where + " AND (GroupName != 'AdHocForumGroup')";

            return ForumPostInfoProvider.Search(where, GetOrderBy(), 0, null);
        }


        /// <summary>
        /// Gets the ORDER BY statement reflecting information obtained from the query string.
        /// </summary>    
        private string GetOrderBy()
        {
            // Get info from the query string
            string searchorderby = QueryHelper.GetString("searchorderby", "");
            string searchorder = QueryHelper.GetString("searchorder", "");

            string orderBy;

            // Get ORDER BY statement

            switch (searchorderby.Trim().ToLowerCSafe())
            {
                case "subject":
                    orderBy = "PostSubject";
                    break;

                case "author":
                    orderBy = "PostUserName";
                    break;

                default:
                    // Default order by is post-time
                    orderBy = "PostTime";
                    break;
            }

            // If sorting direction was specified and even the column for sorting was specified
            if ((orderBy != "") && (searchorder != ""))
            {
                if (searchorder.Trim().ToLowerCSafe() == "ascending")
                {
                    orderBy += SqlHelper.ORDERBY_ASC;
                }
                else
                {
                    orderBy += SqlHelper.ORDERBY_DESC;
                }
            }

            return orderBy;
        }


        /// <summary>
        /// Gets the WHERE condition reflecting the information obtained from the query string.
        /// </summary>    
        private string GetWhereCondition()
        {
            // Get information from the query string
            string searchtext = QueryHelper.GetString("searchtext", "");
            string searchusername = QueryHelper.GetString("searchusername", "");
            string searchin = QueryHelper.GetString("searchin", "");

            // Generate WHERE condition
            string where = "";

            // If the exact text to search was specified
            if (searchtext != "")
            {
                string[] columns;
                switch (searchin.Trim().ToLowerCSafe())
                {
                    case "subject":
                        columns = new[] { "PostSubject" };
                        where += GenerateWhereCondition(searchtext, columns);
                        break;

                    case "text":
                        columns = new[] { "PostText" };
                        where += GenerateWhereCondition(searchtext, columns);
                        break;

                    default:
                        // Search everything by default - like 'subjecttext' is query parameter    
                        columns = new[] { "PostText", "PostSubject" };
                        where += GenerateWhereCondition(searchtext, columns);
                        break;
                }
            }

            // If the post user was specified
            if (searchusername != "")
            {
                if (where != "")
                {
                    where = ((where != "(") ? "(" + where + ") AND " : "(");
                }
                where += "PostUserName LIKE N'%" + SqlHelper.GetSafeQueryString(searchusername, false) + "%'";
            }

            // Returns the empty string if there was no relevant information supplied
            return where;
        }


        /// <summary>
        /// Ensures exact phrase.
        /// </summary>
        private string ExactPhrase(Match m)
        {
            string result = m.Value.Replace("\"", "");
            result = result.Replace(" ", "##_##");
            result = result.Replace("+", "##-##");
            return result;
        }


        /// <summary>
        /// Generates search WHERE condition for the specified columns.
        /// </summary>
        /// <param name="searchText">Text to search</param>
        /// <param name="columns">Name of the columns where the text should be searched</param>    
        private string GenerateWhereCondition(string searchText, IEnumerable<string> columns)
        {
            string where = "";

            // Get groups ("sometext") from within the text
            searchText = TextGroupsRegExp.Replace(SqlHelper.GetSafeQueryString(searchText, false), ExactPhrase);

            // Ensure spaces between words and special chararcters ('+')
            searchText = searchText.TrimStart('+').TrimStart(' ');
            searchText = searchText.TrimEnd('+').TrimEnd(' ');
            searchText = searchText.Replace("  ", " ");
            searchText = searchText.Replace(" +", "+").Replace("+ ", "+");

            string[] orRes = searchText.Split(' ');

            foreach (string column in columns)
            {
                foreach (string ors in orRes)
                {
                    if (!String.IsNullOrEmpty(ors))
                    {
                        string roundWhere = "";
                        string[] andRes = ors.Split('+');

                        foreach (string ands in andRes)
                        {
                            if (!String.IsNullOrEmpty(ands))
                            {
                                roundWhere += "(";

                                if (column.ToLowerCSafe() == "postsubject")
                                {
                                    roundWhere += column + " LIKE N'%" + HTMLHelper.HTMLEncode(ands.Replace("##_##", " ").Replace("##-##", "+")) + "%'";
                                }
                                else
                                {
                                    roundWhere += column + " LIKE N'%" + ands.Replace("##_##", " ").Replace("##-##", "+") + "%'";
                                }

                                roundWhere += ") AND ";
                            }
                        }

                        // Remove and at the end
                        if (roundWhere.EndsWithCSafe(" AND "))
                        {
                            roundWhere = roundWhere.Substring(0, roundWhere.Length - 5);
                        }

                        if (!String.IsNullOrEmpty(roundWhere))
                        {
                            roundWhere += " OR ";
                            where += roundWhere;
                        }
                    }
                }
            }
            // Remove and at the end
            if (where.EndsWithCSafe(" OR "))
            {
                where = where.Substring(0, where.Length - 4);
            }

            return where;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets member profile URL based on settings key. Replaces the wildcards
        /// for user name and culture code
        /// </summary>
        /// <param name="userName">User name for wildcard substitution.</param>
        /// <returns>Member profile URL with resolved wildcards.</returns>
        private string GetMemberProfileUrl(string userName)
        {
            string memberProfile = SettingsKeyInfoProvider.GetValue(SiteContext.CurrentSiteName + ".CMSMemberProfilePath");
            if (!String.IsNullOrEmpty(memberProfile))
            {
                // Resolve well-known wildcards for UserName and CultureCode
                memberProfile = memberProfile.ToLowerCSafe().Replace("{username}", userName);
                var currentCulture = LocalizationContext.CurrentCulture;
                memberProfile = memberProfile.Replace("{culturecode}", (currentCulture != null) ? currentCulture.CultureCode : String.Empty);
            }

            return memberProfile;
        }


        /// <summary>
        /// Returns true if user can vote to the selected post, if yes, save info about it.
        /// </summary>
        /// <param name="postId">PostId</param>
        private bool CanVote(int postId)
        {
            // Get list of answers from cookie
            string answers = ValidationHelper.GetString(CookieHelper.GetValue(CookieName.ForumPostAnswer), "");
            string readyAnswers = "";

            if (answers != string.Empty)
            {
                string[] splitted = answers.Split(',');
                // Go through all answers
                foreach (string split in splitted)
                {
                    if (!String.IsNullOrEmpty(split))
                    {
                        int loadedId = ValidationHelper.GetInteger(split, 0);
                        if ((loadedId > 0) && (loadedId == postId))
                        {
                            // If postID is in cookie return false
                            return false;
                        }

                        readyAnswers += split + ",";
                    }
                }
            }

            CookieHelper.SetValue(CookieName.ForumPostAnswer, readyAnswers + postId, DateTime.Now.AddMonths(1));
            return true;
        }


        /// <summary>
        /// Returns root post info, root post is selected based on path.
        /// </summary>
        /// <param name="path">Post id path</param>
        private ForumPostInfo RootPostInfo(string path)
        {
            // get postid from path
            int postId = ForumPostInfoProvider.GetPostRootFromIDPath(path);
            // Try to get post ifo from request
            ForumPostInfo fpi = RequestStockHelper.GetItem("ForumViewer_RootPost_" + postId) as ForumPostInfo;
            if (fpi == null)
            {
                // Load forum post
                fpi = ForumPostInfoProvider.GetForumPostInfo(postId);
                if (fpi != null)
                {
                    RequestStockHelper.Add("ForumViewer_RootPost_" + postId, fpi);
                }
            }

            return fpi;
        }


        /// <summary>
        /// Returns current thred info based on current thread or post id path.
        /// </summary>
        /// <param name="data">Container.DataItem</param>
        private ForumPostInfo CurrentThreadInfo(object data)
        {
            // Try to get forum post info from current thread
            ForumPostInfo fpi = ForumContext.CurrentThread;
            if (fpi == null)
            {
                // Try to get post id from path and get post info
                string path = ValidationHelper.GetString(GetData(data, "PostIDPath"), "");
                if (!String.IsNullOrEmpty(path))
                {
                    fpi = RootPostInfo(path);
                }
            }

            return fpi;
        }


        /// <summary>
        /// Returns value that indicates whether current thread is locked.
        /// </summary>
        /// <param name="data">Container.DataItem</param>
        private bool ThreadIsLocked(object data)
        {
            ForumPostInfo fpi = CurrentThreadInfo(data);
            if (fpi != null)
            {
                return fpi.PostIsLocked;
            }

            return ValidationHelper.GetBoolean(GetData(data, "PostIsLocked"), true);
        }


        /// <summary>
        /// Returns true if action is available.
        /// </summary>
        /// <param name="data">Container.DataItem</param>
        /// <param name="type">Action type</param>
        public bool IsAvailable(object data, ForumActionType type)
        {
            ForumInfo fi = null;

            // Try to get forum id from post
            int forumID = ValidationHelper.GetInteger(GetData(data, "PostForumID"), 0);

            // Try to get forum id from forum
            if (forumID == 0)
            {
                forumID = ValidationHelper.GetInteger(GetData(data, "ForumID"), 0);
            }

            if (forumID > 0)
            {
                fi = ForumInfoProvider.GetForumInfo(forumID);
            }

            if ((fi == null) && (ForumContext.CurrentForum != null))
            {
                forumID = ForumContext.CurrentForum.ForumID;
                fi = ForumContext.CurrentForum;
            }

            if (fi == null)
            {
                return false;
            }

            // Check whether current user is moderator
            bool isModerator = ForumContext.UserIsModerator(forumID, CommunityGroupID);
            bool unathorizedView = RedirectUnauthorized && MembershipContext.AuthenticatedUser.IsPublic();

            switch (type)
            {
                // Approve
                case ForumActionType.Appprove:
                case ForumActionType.ApproveAll:
                    if ((EnableOnSiteManagement) && (isModerator) && (!ValidationHelper.GetBoolean(GetData(data, "PostApproved"), false)))
                    {
                        return true;
                    }
                    break;

                // Reject
                case ForumActionType.Reject:
                case ForumActionType.RejectAll:
                    if ((EnableOnSiteManagement) && (isModerator) && (ValidationHelper.GetBoolean(GetData(data, "PostApproved"), false)))
                    {
                        return true;
                    }

                    break;

                // Delete
                case ForumActionType.Delete:
                    // Allow delete to moderators and if allowed in forum also allow deleting to author of the post (if not public user)
                    CurrentUserInfo uiDelete = MembershipContext.AuthenticatedUser;
                    int deletePostUserId = ValidationHelper.GetInteger(GetData(data, "PostUserID"), 0);
                    return ((EnableOnSiteManagement) && (isModerator || ((fi.ForumAuthorDelete) && (uiDelete != null) && (!uiDelete.IsPublic()) && (uiDelete.UserID == deletePostUserId))));

                // Lock thread
                case ForumActionType.LockThread:
                    if ((EnableOnSiteManagement) && (isModerator) && (!ValidationHelper.GetBoolean(GetData(data, "PostIsLocked"), false)))
                    {
                        return true;
                    }
                    break;

                // Unlock thread
                case ForumActionType.UnlockThread:
                    if ((EnableOnSiteManagement) && (isModerator) && (ValidationHelper.GetBoolean(GetData(data, "PostIsLocked"), false)))
                    {
                        return true;
                    }
                    break;

                // Stick thread
                case ForumActionType.StickThread:
                    if ((EnableOnSiteManagement) && (isModerator) && (ValidationHelper.GetInteger(GetData(data, "PostStickOrder"), 0) == 0))
                    {
                        return true;
                    }
                    break;

                // Move or stick thread
                case ForumActionType.UnstickThread:
                case ForumActionType.MoveStickyThreadDown:
                case ForumActionType.MoveStickyThreadUp:
                    if ((EnableOnSiteManagement) && (isModerator) && (ValidationHelper.GetInteger(GetData(data, "PostStickOrder"), 0) > 0))
                    {
                        return true;
                    }
                    break;

                // Lock forum
                case ForumActionType.LockForum:
                    if ((EnableOnSiteManagement) && ((MembershipContext.AuthenticatedUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin)) || (ForumContext.IsCommunityGroupAdmin(ValidationHelper.GetInteger(GetData(data, "ForumID"), 0)))) && (!ValidationHelper.GetBoolean(GetData(data, "ForumIsLocked"), false)))
                    {
                        return true;
                    }
                    break;

                // Unlock forum
                case ForumActionType.UnlockForum:
                    if ((EnableOnSiteManagement) && ((MembershipContext.AuthenticatedUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin)) || ((ForumContext.IsCommunityGroupAdmin(ValidationHelper.GetInteger(GetData(data, "ForumID"), 0))))) && (ValidationHelper.GetBoolean(GetData(data, "ForumIsLocked"), false)))
                    {
                        return true;
                    }
                    break;

                case ForumActionType.Edit:
                    // Allow edit to moderators and if allowed in forum also allow editing to author of the post (if not public user)
                    CurrentUserInfo uiEdit = MembershipContext.AuthenticatedUser;
                    int editPostUserId = ValidationHelper.GetInteger(GetData(data, "PostUserID"), 0);
                    return EnableOnSiteManagement && (isModerator || ((fi.ForumAuthorEdit) && (uiEdit != null) && (!uiEdit.IsPublic()) && (uiEdit.UserID == editPostUserId)));

                // Attachment
                case ForumActionType.Attachment:
                    return (AuthenticationHelper.IsAuthenticated() // Only authenticated
                        // If attach files is enabled moderator can add/edit attachments    
                            && ((MembershipContext.AuthenticatedUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin) || (fi.AllowAttachFiles != SecurityAccessEnum.Nobody && isModerator))
                        // Check permissions  
                                || (ForumInfoProvider.IsAuthorizedPerForum(fi.ForumID, fi.ForumGroupID, "AttachFiles", fi.AllowAttachFiles, MembershipContext.AuthenticatedUser)
                                    // If exists current post, check whether user is author and whether can edit attachments
                                    && ((ValidationHelper.GetInteger(GetData(data, "PostID"), 0) == 0) || (ValidationHelper.GetInteger(GetData(data, "PostUserID"), 0) == MembershipContext.AuthenticatedUser.UserID)))));

                case ForumActionType.Forum:
                    // Check only if HideForumForUnauthorized is set to true
                    return (!HideForumForUnauthorized) || (isModerator || (ForumInfoProvider.IsAuthorizedPerForum(fi.ForumID, fi.ForumGroupID, "AccessToForum", fi.AllowAccess, MembershipContext.AuthenticatedUser)));

                case ForumActionType.Thread:
                    return true;

                case ForumActionType.IsNotAnswer:
                case ForumActionType.IsAnswer:
                    return (isModerator || ForumInfoProvider.IsAuthorizedPerForum(fi.ForumID, fi.ForumGroupID, "MarkAsAnswer", fi.AllowMarkAsAnswer, MembershipContext.AuthenticatedUser)) && ((fi.ForumType == 2) || ((CurrentThreadInfo(data) != null) && (CurrentThreadInfo(data).PostType == 1))) && !fi.ForumIsLocked && !ThreadIsLocked(data) && (ValidationHelper.GetInteger(GetData(data, "PostLevel"), 0) > 0);

                case ForumActionType.NewThread:
                    return !fi.ForumIsLocked && (isModerator || ForumInfoProvider.IsAuthorizedPerForum(fi.ForumID, fi.ForumGroupID, "Post", fi.AllowPost, MembershipContext.AuthenticatedUser) || (unathorizedView && ((fi.AllowPost != SecurityAccessEnum.Nobody))));

                case ForumActionType.Quote:
                    return !ThreadIsLocked(data) && !fi.ForumIsLocked && fi.ForumEnableQuote && CheckRelativeLevel(data) && (isModerator || ForumInfoProvider.IsAuthorizedPerForum(fi.ForumID, fi.ForumGroupID, "Reply", fi.AllowReply, MembershipContext.AuthenticatedUser) || (unathorizedView && (fi.AllowReply != SecurityAccessEnum.Nobody)));

                case ForumActionType.Reply:
                    return !ThreadIsLocked(data) && !fi.ForumIsLocked && CheckRelativeLevel(data) && (isModerator || ForumInfoProvider.IsAuthorizedPerForum(fi.ForumID, fi.ForumGroupID, "Reply", fi.AllowReply, MembershipContext.AuthenticatedUser) || (unathorizedView && (fi.AllowReply != SecurityAccessEnum.Nobody)));

                case ForumActionType.SubscribeToForum:
                    // Hide subscribe to forum if forum is adhoc and doesn't exist yet
                    if ((ForumContext.CurrentForum != null) && (ForumContext.CurrentForum.ForumID == 0))
                    {
                        return false;
                    }
                    return EnableSubscription && (!fi.ForumIsLocked) && (isModerator || ForumInfoProvider.IsAuthorizedPerForum(fi.ForumID, fi.ForumGroupID, "Subscribe", fi.AllowSubscribe, MembershipContext.AuthenticatedUser) || (unathorizedView && (fi.AllowSubscribe != SecurityAccessEnum.Nobody)));

                case ForumActionType.SubscribeToPost:
                    return EnableSubscription && !ThreadIsLocked(data) && !fi.ForumIsLocked && CheckRelativeLevel(data) && (isModerator || ForumInfoProvider.IsAuthorizedPerForum(fi.ForumID, fi.ForumGroupID, "Subscribe", fi.AllowSubscribe, MembershipContext.AuthenticatedUser) || (unathorizedView && (fi.AllowSubscribe != SecurityAccessEnum.Nobody)));

                case ForumActionType.MoveToTheOtherForum:
                    return (EnableOnSiteManagement) && isModerator;

                case ForumActionType.SplitThread:
                    return ((EnableOnSiteManagement) && isModerator);

                case ForumActionType.AddForumToFavorites:
                case ForumActionType.AddPostToFavorites:
                    // Hide add to favorites to forum if forum is adhoc and doesn't exist yet
                    if ((ForumContext.CurrentForum != null) && (ForumContext.CurrentForum.ForumID == 0))
                    {
                        return false;
                    }
                    return (EnableFavorites && MembershipContext.AuthenticatedUser != null && !MembershipContext.AuthenticatedUser.IsPublic());
            }

            // Action is not availbale by default 
            return false;
        }


        /// <summary>
        /// Tries to find parent forum viewer and copy values.
        /// </summary>
        /// <param name="control">Current forum viewer control</param>
        public static void CopyValuesFromParent(Control control)
        {
            if ((control != null) && (control.Parent != null) && ((control as ForumViewer) != null))
            {
                ForumViewer fv = null;
                Control currentParent = control.Parent;

                // Find not null parent
                while ((fv == null) && (currentParent != null))
                {
                    fv = currentParent as ForumViewer;
                    currentParent = currentParent.Parent;
                }

                if (fv != null)
                {
                    fv.CopyValues(control as ForumViewer);
                }
            }
        }


        /// <summary>
        /// Returns url with dependence on parameters.
        /// </summary>
        /// <param name="parameter">Querystring parameter</param>
        /// <param name="value">Querystring value</param>
        /// <param name="postback">Indicates whether link should be postback</param>
        /// <param name="data">Container.DataItem</param>
        /// <param name="encodeUrl">Indicates whether Url is encoded.</param>
        public string URLCreator(string parameter, string value, bool postback, object data, bool encodeUrl)
        {
            return URLCreator(parameter, value, postback, data, ForumActionType.Unknown, encodeUrl);
        }


        /// <summary>
        /// Returns url with dependence on parameters.
        /// </summary>
        /// <param name="parameter">Querystring parameter</param>
        /// <param name="value">Querystring value</param>
        /// <param name="postback">Indicates whether link should be postback</param>
        /// <param name="data">Container.DataItem</param>
        /// <param name="urlTarget">Indicates destinate part of url</param>
        /// <param name="encodeUrl">Indicates whether Url is encoded.</param>
        public string URLCreator(string parameter, string value, bool postback, object data, ForumActionType urlTarget = ForumActionType.Unknown, bool encodeUrl = false)
        {
            if (!postback)
            {
                // Current url 
                string url = UrlResolver.ResolveUrl(RequestContext.CurrentURL);

                // Backward compatibility
                if (QueryHelper.Contains("thread"))
                {
                    url = URLHelper.UpdateParameterInUrl(url, "threadid", ForumPostInfoProvider.GetPostRootFromIDPath(ValidationHelper.GetString(GetData(data, "PostIdPath"), "")).ToString());
                    url = URLHelper.RemoveParameterFromUrl(url, "thread");
                }

                // Indicates whether current parameter shiuld be used in url
                bool addParameter = true;

                // Check whether friendly urls are required
                if (UseFriendlyURL)
                {
                    // Set base url
                    url = FriendlyBaseURL;

                    if ((urlTarget == ForumActionType.Unknown) || (urlTarget != ForumActionType.ForumGroup))
                    {
                        // Set default 'whatever' part of url
                        string last = "forum";

                        string siteName = SiteContext.CurrentSiteName;

                        // Forum part of url, only if group is defined
                        if (ForumContext.CurrentGroup != null)
                        {
                            if (parameter.ToLowerCSafe() == "forumid")
                            {
                                // Forum prefix + forum id
                                url += "/f" + ValidationHelper.GetInteger(GetData(data, "ForumID"), 0);
                                // 'Whatever' part of url - forum name
                                last = URLHelper.GetSafeUrlPart(TextHelper.LimitLength(ResHelper.LocalizeString(ValidationHelper.GetString(GetData(data, "ForumDisplayName"), "forum")), 50), siteName);
                                // Don't add current parameter to the final url
                                addParameter = false;
                            }
                            else if (ForumContext.CurrentForum != null)
                            {
                                // Forum prefix + forum id
                                url += "/f" + ForumContext.CurrentForum.ForumID;
                                // 'Whatever' part of url - forum name
                                last = URLHelper.GetSafeUrlPart(TextHelper.LimitLength(ResHelper.LocalizeString(ForumContext.CurrentForum.ForumDisplayName), 50), siteName);
                            }
                        }
                        else
                        {
                            last = String.Empty;
                        }

                        // Check whether forum page link is required
                        if (parameter.ToLowerCSafe() == "fpage")
                        {
                            url += "/fp" + value;
                            addParameter = false;
                        }

                            // Check whether current query string contains forum page parameter
                        else if (QueryHelper.Contains("fpage"))
                        {
                            // Add forum page of url
                            url += "/fp" + QueryHelper.GetInteger("fpage", 0);
                        }

                        if ((urlTarget == ForumActionType.Unknown) || (urlTarget != ForumActionType.Forum))
                        {
                            // Thread part of url
                            if (parameter.ToLowerCSafe() == "threadid")
                            {
                                // Thread prefix + thread id
                                url += "/t" + ValidationHelper.GetInteger(GetData(data, "PostID"), 0);
                                // 'Whatever' part of url - Post subject
                                last = URLHelper.GetSafeUrlPart(TextHelper.LimitLength(ValidationHelper.GetString(GetData(data, "PostSubject"), "thread"), 50), siteName);
                                // Don't add current parameter to the final url
                                addParameter = false;
                            }
                            else if (ForumContext.CurrentThread != null)
                            {
                                // Thread prefix + thread id
                                url += "/t" + ForumContext.CurrentThread.PostId;
                                // 'Whatever' part of url - Post subject
                                last = URLHelper.GetSafeUrlPart(TextHelper.LimitLength(ForumContext.CurrentThread.PostSubject, 50), siteName);
                            }

                            // Check whether thread page link is required
                            if (parameter.ToLowerCSafe() == "tpage")
                            {
                                url += "/tp" + value;
                                addParameter = false;
                            }
                            // Check whether current query string contains thread page parameter
                            else if (QueryHelper.Contains("tpage"))
                            {
                                // Add thread page part of url
                                url += "/tp" + QueryHelper.GetInteger("tpage", 0);
                            }
                        }

                        if (last != String.Empty)
                        {
                            // Complete url with last part of url and extension
                            url += "/" + last + FriendlyURLExtension;
                        }
                        else
                        {
                            // Add only extension if last part is missig (due to friendly URL in single forum)
                            url += FriendlyURLExtension;
                        }
                    }
                    else
                    {
                        url += FriendlyURLExtension;
                        addParameter = false;
                    }
                }


                // Check whethe parameter should be added
                if (addParameter)
                {
                    if ((parameter.ToLowerCSafe() == "forumid") && (ForumContext.CurrentGroup == null))
                    {
                        // Do not add forum id parameter if current control starts in forum state
                    }
                    else
                    {
                        url = URLHelper.UpdateParameterInUrl(url, parameter, value);
                    }
                }

                if (encodeUrl)
                {
                    url = HTMLHelper.EncodeForHtmlAttribute(url);
                }

                return url;
            }

            // Generate postback url
            return "javascript:" + ControlsHelper.GetPostBackEventReference(this, parameter + value);
        }


        /// <summary>
        /// Returns true if under current post (represented by data object) can be addedd new one
        /// or can user subscribe
        /// </summary>
        /// <param name="data">Container.DataItem</param>
        private bool CheckRelativeLevel(object data)
        {
            if (MaxRelativeLevel > -1)
            {
                if (ValidationHelper.GetInteger(GetData(data, "PostLevel"), -1) >= MaxRelativeLevel)
                {
                    return false;
                }
            }

            // Check system max relative level
            if (ValidationHelper.GetInteger(GetData(data, "PostLevel"), -1) >= ForumPostInfoProvider.MaxPostLevel)
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Returns action URL.
        /// </summary>
        /// <param name="data">Container.DataItem</param>
        /// <param name="type">Forum action type</param>
        public string GetURL(object data, ForumActionType type)
        {
            // Check whether current action is available for current user
            if (!IsAvailable(data, type))
            {
                return "";
            }

            string url = "";

            switch (type)
            {
                case ForumActionType.Appprove:
                    url = URLCreator("Approve$", ValidationHelper.GetString(GetData(data, "PostID"), ""), true, null);
                    break;

                case ForumActionType.ApproveAll:
                    url = URLCreator("ApproveAll$", ValidationHelper.GetString(GetData(data, "PostID"), ""), true, null);
                    break;

                case ForumActionType.Reject:
                    url = URLCreator("Reject$", ValidationHelper.GetString(GetData(data, "PostID"), ""), true, null);
                    break;

                case ForumActionType.RejectAll:
                    url = URLCreator("RejectAll$", ValidationHelper.GetString(GetData(data, "PostID"), ""), true, null);
                    break;

                case ForumActionType.Delete:
                    url = URLCreator("Delete$", ValidationHelper.GetString(GetData(data, "PostID"), ""), true, null);
                    break;

                case ForumActionType.LockThread:
                    url = URLCreator("LockThread$", ValidationHelper.GetString(GetData(data, "PostID"), ""), true, null);
                    break;

                case ForumActionType.LockForum:
                    url = URLCreator("LockForum$", ValidationHelper.GetString(GetData(data, "ForumID"), ""), true, null);
                    break;

                case ForumActionType.UnlockThread:
                    url = URLCreator("UnlockThread$", ValidationHelper.GetString(GetData(data, "PostID"), ""), true, null);
                    break;

                case ForumActionType.UnlockForum:
                    url = URLCreator("UnlockForum$", ValidationHelper.GetString(GetData(data, "ForumID"), ""), true, null);
                    break;

                case ForumActionType.StickThread:
                    url = URLCreator("StickThread$", ValidationHelper.GetString(GetData(data, "PostID"), ""), true, null);
                    break;

                case ForumActionType.UnstickThread:
                    url = URLCreator("UnstickThread$", ValidationHelper.GetString(GetData(data, "PostID"), ""), true, null);
                    break;

                case ForumActionType.MoveStickyThreadUp:
                    url = URLCreator("MoveStickyThreadUp$", ValidationHelper.GetString(GetData(data, "PostID"), ""), true, null);
                    break;

                case ForumActionType.MoveStickyThreadDown:
                    url = URLCreator("MoveStickyThreadDown$", ValidationHelper.GetString(GetData(data, "PostID"), ""), true, null);
                    break;

                case ForumActionType.IsAnswer:
                    if (ValidationHelper.GetInteger(GetData(data, "PostLevel"), 0) > 0)
                    {
                        url = URLCreator("IsAnswer$", ValidationHelper.GetString(GetData(data, "PostID"), ""), true, null);
                    }
                    break;

                case ForumActionType.IsNotAnswer:
                    if (ValidationHelper.GetInteger(GetData(data, "PostLevel"), 0) > 0)
                    {
                        url = URLCreator("IsNotAnswer$", ValidationHelper.GetString(GetData(data, "PostID"), ""), true, null);
                    }
                    break;

                case ForumActionType.NewThread:
                    url = URLCreator("ReplyTo", "0", false, null);
                    url = URLHelper.RemoveParameterFromUrl(url, "threadid");
                    url = URLHelper.RemoveParameterFromUrl(url, "thread");
                    break;

                case ForumActionType.Quote:
                    url = URLCreator("ReplyTo", ValidationHelper.GetString(GetData(data, "PostID"), ""), false, null);
                    url = URLHelper.UpdateParameterInUrl(url, "mode", "quote");
                    break;

                case ForumActionType.Reply:
                    url = URLCreator("ReplyTo", ValidationHelper.GetString(GetData(data, "PostID"), ""), false, null);
                    break;

                case ForumActionType.SubscribeToForum:
                    url = URLCreator("SubscribeTo", "0", false, null);
                    url = URLHelper.RemoveParameterFromUrl(url, "threadid");
                    url = URLHelper.RemoveParameterFromUrl(url, "thread");
                    break;

                case ForumActionType.SubscribeToPost:
                    url = URLCreator("SubscribeTo", ValidationHelper.GetString(GetData(data, "PostID"), ""), false, null);
                    break;

                case ForumActionType.Edit:
                    url = URLCreator("PostID", ValidationHelper.GetString(GetData(data, "PostID"), ""), false, null);
                    if (ForumContext.CurrentThread == null)
                    {
                        url = URLHelper.UpdateParameterInUrl(url, "threadid", ForumPostInfoProvider.GetPostRootFromIDPath(ValidationHelper.GetString(GetData(data, "PostIDPath"), "")).ToString());
                    }
                    url = URLHelper.UpdateParameterInUrl(url, "mode", "edit");
                    break;

                case ForumActionType.Attachment:
                    url = URLCreator("PostID", ValidationHelper.GetString(GetData(data, "PostID"), ""), false, null);
                    url = URLHelper.RemoveParameterFromUrl(url, "replyto");
                    if (ForumContext.CurrentThread == null)
                    {
                        int threadId = ForumPostInfoProvider.GetPostRootFromIDPath(Convert.ToString(GetData(data, "PostIDPath")));
                        url = URLHelper.UpdateParameterInUrl(url, "threadid", threadId.ToString());
                    }
                    url = URLHelper.UpdateParameterInUrl(url, "mode", "attachment");
                    break;

                case ForumActionType.Forum:
                    var forumId = ValidationHelper.GetInteger(GetData(data, "ForumID"), 0);
                    url = URLCreator("ForumID", ValidationHelper.GetString(forumId, ""), false, data);

                    var forumGroupId = GetForumGroupId(forumId);
                    if (forumGroupId > 0)
                    {
                        url = URLHelper.AddParameterToUrl(url, "CommunityGroupID", ValidationHelper.GetString(forumGroupId, ""));
                    }

                    url = URLHelper.RemoveParameterFromUrl(url, "threadid");
                    url = URLHelper.RemoveParameterFromUrl(url, "thread");
                    url = URLHelper.RemoveParameterFromUrl(url, "postid");
                    url = URLHelper.RemoveParameterFromUrl(url, "replyto");
                    url = URLHelper.RemoveParameterFromUrl(url, "mode");
                    url = URLHelper.RemoveParameterFromUrl(url, "moveto");
                    break;

                case ForumActionType.Thread:
                    url = URLCreator("ThreadID", ValidationHelper.GetString(GetData(data, "PostID"), ""), false, data);
                    url = URLHelper.RemoveParameterFromUrl(url, "postid");
                    url = URLHelper.RemoveParameterFromUrl(url, "replyto");
                    url = URLHelper.RemoveParameterFromUrl(url, "mode");
                    url = URLHelper.RemoveParameterFromUrl(url, "thread");
                    url = URLHelper.RemoveParameterFromUrl(url, "moveto");
                    break;

                case ForumActionType.MoveToTheOtherForum:
                    if (ValidationHelper.GetInteger(GetData(data, "PostLevel"), 0) == 0)
                    {
                        url = URLCreator("MoveTo", ValidationHelper.GetString(GetData(data, "PostID"), ""), false, data);
                        url = URLHelper.UpdateParameterInUrl(url, "mode", "topicmove");
                    }
                    break;

                case ForumActionType.SplitThread:
                    if (ValidationHelper.GetInteger(GetData(data, "PostLevel"), 0) > 0)
                    {
                        url = URLCreator("SplitThread$", ValidationHelper.GetString(GetData(data, "PostID"), ""), true, null);
                    }
                    break;

                case ForumActionType.AddPostToFavorites:
                    url = URLCreator("AddPostToFavorites$", ValidationHelper.GetString(GetData(data, "PostID"), ""), true, null);
                    break;

                case ForumActionType.AddForumToFavorites:
                    url = URLCreator("AddForumToFavorites$", ValidationHelper.GetString(GetData(data, "ForumID"), ""), true, null);
                    break;
            }

            return url;
        }


        private static int GetForumGroupId(int forumId)
        {
            if (forumId <= 0)
            {
                return 0;
            }

            var forum = ForumInfoProvider.GetForumInfo(forumId);
            if (forum == null)
            {
                return 0;
            }

            var forumGroup = ForumGroupInfoProvider.GetForumGroupInfo(forum.ForumGroupID);

            return forumGroup?.GroupGroupID ?? 0;
        }


        /// <summary>
        /// Returns link with dependence on parameters.
        /// </summary>
        /// <param name="data">Container.DataItem</param>
        /// <param name="text">Link text</param>
        /// <param name="cssClassName">CSS class name</param>
        /// <param name="type">Forum action type</param>
        public string GetLink(object data, object text, string cssClassName, ForumActionType type)
        {
            string url = GetURL(data, type);
            if (url != "")
            {
                if (type == ForumActionType.Delete)
                {
                    //Register delete confirmation script
                    ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "ForumDeletePost",
                                                           ScriptHelper.GetScript(" function ForumDeletePost(url) {\n" +
                                                                                  "if (confirm(" + ScriptHelper.GetString(ResHelper.GetString("ForumPost_View.DeleteConfirmation")) + ")) { \n eval(url); \n}\n}"
                                                               ));

                    url = "javascript:ForumDeletePost(" + HTMLHelper.HTMLEncode(ScriptHelper.GetString(url)) + ");";
                }
                else if (type == ForumActionType.SplitThread)
                {
                    //Register split confirmation script
                    ScriptHelper.RegisterClientScriptBlock(this, typeof(string), "ForumSplitConfirm",
                                                           ScriptHelper.GetScript(" function ForumSplitConfirm(url) {\n" +
                                                                                  "if (confirm(" + ScriptHelper.GetString(ResHelper.GetString("ForumPost_View.SplitConfirmation")) + ")) { \n eval(url); \n}\n}"
                                                               ));

                    url = "javascript:ForumSplitConfirm(" + HTMLHelper.HTMLEncode(ScriptHelper.GetString(url)) + ");";
                }
                else
                {
                    url = HTMLHelper.HTMLEncode(url);
                }

                return "<a " + CSSClassNameTransformer(cssClassName) + "href=\"" + url + "\">" + HTMLHelper.HTMLEncode(text as string) + "</a>";
            }

            return "";
        }


        /// <summary>
        /// Resolve post text with dependence on current forum and ensures line endings of the post text.
        /// </summary>
        /// <param name="text">Post text</param>
        public string ResolvePostText(object text)
        {
            if (text != null)
            {
                // Resolve macros
                if (ForumContext.CurrentForumDiscussionMacroHelper != null)
                {
                    return ForumContext.CurrentForumDiscussionMacroHelper.ResolveMacros(text.ToString());
                }
            }

            return String.Empty;
        }


        /// <summary>
        /// Resolve post text with dependence on current forum.
        /// </summary>
        /// <param name="text">Post text</param>
        /// <param name="data">Container dataitem</param>
        public string ResolvePostText(object text, object data)
        {
            // Try to get forum id from post
            int forumID = ValidationHelper.GetInteger(GetData(data, "PostForumID"), 0);
            // Try to get forum id from forum
            if (forumID == 0)
            {
                forumID = ValidationHelper.GetInteger(GetData(data, "ForumID"), 0);
            }
            // If forum id is not specified return false
            if ((forumID == 0) && (ForumContext.CurrentForum != null))
            {
                forumID = ForumContext.CurrentForum.ForumID;
            }

            ForumInfo fi = ForumInfoProvider.GetForumInfo(forumID);
            if (fi == null)
            {
                return HTMLHelper.HTMLEncode(ValidationHelper.GetString(text, ""));
            }

            string postText = ValidationHelper.GetString(text, "");

            DiscussionMacroResolver dmh = new DiscussionMacroResolver();
            dmh.EnableBold = fi.ForumEnableFontBold;
            dmh.EnableItalics = fi.ForumEnableFontItalics;
            dmh.EnableStrikeThrough = fi.ForumEnableFontStrike;
            dmh.EnableUnderline = fi.ForumEnableFontUnderline;
            dmh.EnableCode = fi.ForumEnableCodeSnippet;
            dmh.EnableColor = fi.ForumEnableFontColor;
            dmh.EnableImage = fi.ForumEnableImage || fi.ForumEnableAdvancedImage;
            dmh.EnableQuote = fi.ForumEnableQuote;
            dmh.EnableURL = fi.ForumEnableURL || fi.ForumEnableAdvancedURL;
            dmh.MaxImageSideSize = fi.ForumImageMaxSideSize;
            dmh.QuotePostText = ResHelper.GetString("DiscussionMacroResolver.QuotePostText");
            dmh.UseNoFollowForLinks = HTMLHelper.UseNoFollowForUsersLinks(SiteContext.CurrentSiteName);

            if (fi.ForumHTMLEditor)
            {
                dmh.EncodeText = false;
                dmh.ConvertLineBreaksToHTML = false;
            }
            else
            {
                dmh.EncodeText = true;
                dmh.ConvertLineBreaksToHTML = true;
            }

            // Resolve macros
            return dmh.ResolveMacros(postText);
        }


        /// <summary>
        /// Returnes Signature area if signature is enabled and is not empty.
        /// </summary>
        /// <param name="data">Data of the forum post</param>
        /// <param name="contentBefore">Content before signature</param>
        /// <param name="contentAfter">Content after signature</param>
        public string GetSignatureArea(object data, string contentBefore, string contentAfter)
        {
            if (EnableSignature)
            {
                // Add signature to post text
                string signature = Convert.ToString(GetData(data, "PostUserSignature"));
                if (signature != "")
                {
                    return contentBefore + HTMLHelper.HTMLEncodeLineBreaks(signature) + contentAfter;
                }
            }

            return "";
        }


        /// <summary>
        /// Returns url without action parameters.
        /// </summary>
        public string ClearURL()
        {
            string url = URLHelper.RemoveParameterFromUrl(RequestContext.CurrentURL, "replyTo");
            url = URLHelper.RemoveParameterFromUrl(url, "mode");
            url = URLHelper.RemoveParameterFromUrl(url, "PostID");
            url = URLHelper.RemoveParameterFromUrl(url, "SubscribeTo");
            url = URLHelper.RemoveParameterFromUrl(url, "moderated");
            return url;
        }


        /// <summary>
        /// Returns html code if current user is moderator.
        /// </summary>
        /// <param name="forumID">Forum identifier</param>
        /// <param name="htmlCode">HTML code which should be returned</param>
        /// <param name="checkForumLock">Indicates whether rights to the forum locking should be checked</param>
        public string AdministratorCode(object forumID, string htmlCode, bool checkForumLock)
        {
            if (EnableOnSiteManagement && ForumContext.UserIsModerator(ValidationHelper.GetInteger(forumID, 0), CommunityGroupID))
            {
                // Check whether user is authorize to lock/unlock forum
                if (checkForumLock)
                {
                    if ((!MembershipContext.AuthenticatedUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin)) && (!ForumContext.IsCommunityGroupAdmin(ValidationHelper.GetInteger(forumID, 0))))
                    {
                        return String.Empty;
                    }
                }

                if (ForumContext.CurrentForum == null)
                {
                    // Global administrator or moderator check
                    if (MembershipContext.AuthenticatedUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin) || (MembershipContext.AuthenticatedUser.IsAuthorizedPerResource("cms.forums", "modify")))
                    {
                        return htmlCode;
                    }

                    // Community group check
                    if ((CommunityGroupID > 0) && (ForumContext.IsCommunityGroupAdmin(ValidationHelper.GetInteger(forumID, 0))))
                    {
                        return htmlCode;
                    }
                }
                else
                {
                    return htmlCode;
                }
            }

            return String.Empty;
        }


        /// <summary>
        /// Returns html code if current user is moderator.
        /// </summary>
        /// <param name="forumID">Forum identifier</param>
        /// <param name="htmlCode">HTML code which should be returned</param>
        public string AdministratorCode(object forumID, string htmlCode)
        {
            return AdministratorCode(forumID, htmlCode, false);
        }


        /// <summary>
        /// Returns string with information about unapproved posts.
        /// </summary>
        /// <param name="data">Data item</param>
        /// <param name="formatString">Sets the formating string whih should be used to diplay message. Use macro {0} for number of unapproved posts</param>
        /// <param name="hideForEmpty">Indicates whether text should be empty if current forum does not contain unapproved posts</param>
        public string UnaprovedPosts(object data, string formatString, bool hideForEmpty)
        {
            if (!String.IsNullOrEmpty(formatString))
            {
                int forumId = ValidationHelper.GetInteger(GetData(data, "PostForumID"), 0);

                // If user is moderator
                if ((forumId > 0) && ForumContext.UserIsModerator(forumId, CommunityGroupID))
                {
                    int approved = ValidationHelper.GetInteger(GetData(data, "PostThreadPosts"), 0);
                    int all = ValidationHelper.GetInteger(GetData(data, "PostThreadPostsAbsolute"), 0);

                    if ((all - approved) > 0)
                    {
                        return String.Format(formatString, (all - approved));
                    }
                }
            }

            return String.Empty;
        }


        /// <summary>
        /// Copy current properties to the child control.
        /// </summary>
        /// <param name="frmv">Forum viewer control</param>
        public void CopyValues(ForumViewer frmv)
        {
            if (frmv != null)
            {
                frmv.IsAdHocForum = IsAdHocForum;
                frmv.GroupID = GroupID;
                frmv.ForumID = ForumID;
                frmv.ThreadID = ThreadID;
                frmv.ViewMode = ViewMode;
                frmv.ForumLayout = ForumLayout;
                frmv.UseFriendlyURL = UseFriendlyURL;
                frmv.FriendlyURLExtension = FriendlyURLExtension;
                frmv.FriendlyBaseURL = FriendlyBaseURL;
                frmv.EnableAvatars = EnableAvatars;
                frmv.EnablePostsPaging = EnablePostsPaging;
                frmv.EnableThreadPaging = EnableThreadPaging;
                frmv.DisplayAttachmentImage = DisplayAttachmentImage;
                frmv.AttachmentImageMaxSideSize = AttachmentImageMaxSideSize;
                frmv.AvatarMaxSideSize = AvatarMaxSideSize;
                frmv.ThreadPageSize = ThreadPageSize;
                frmv.PostsPageSize = PostsPageSize;
                frmv.LogonPageURL = LogonPageURL;
                frmv.AccessDeniedPageURL = AccessDeniedPageURL;
                frmv.RedirectUnauthorized = RedirectUnauthorized;
                frmv.HideForumForUnauthorized = HideForumForUnauthorized;
                frmv.ShowMode = ShowMode;
                frmv.ExpandTree = ExpandTree;
                frmv.EnableFavorites = EnableFavorites;
                frmv.EnableSignature = EnableSignature;
                frmv.EnableFavorites = EnableFavorites;
                frmv.SearchInGroups = SearchInGroups;
                frmv.SearchNoResults = SearchNoResults;
                frmv.MaxRelativeLevel = MaxRelativeLevel;
                frmv.DisplayBadgeInfo = DisplayBadgeInfo;
                frmv.RedirectToUserProfile = RedirectToUserProfile;
                frmv.UseRedirectAfterAction = UseRedirectAfterAction;
                frmv.UnsubscriptionURL = UnsubscriptionURL;
                frmv.SearchInForums = SearchInForums;
                frmv.CommunityGroupID = CommunityGroupID;
                frmv.WhereCondition = WhereCondition;
                frmv.BaseURL = BaseURL;
                frmv.GenerateActionScripts = GenerateActionScripts;
                frmv.EnableOnSiteManagement = EnableOnSiteManagement;
                frmv.EnableSubscription = EnableSubscription;
                frmv.AbuseReportAccess = AbuseReportAccess;
                frmv.AbuseReportRoles = AbuseReportRoles;
                frmv.SearchInThreads = SearchInThreads;
                frmv.SiteName = SiteName;
                frmv.SiteID = SiteID;
                frmv.LogActivity = LogActivity;
            }
        }


        /// <summary>
        /// Reloads the data of the forum control.
        /// </summary>
        public virtual void ReloadData()
        {
            // Nothing in base class
        }


        /// <summary>
        /// Provides a DateTime got via GetData in correct time-zone.
        /// </summary>
        /// <param name="data">Data to fetch the DateTime from. (Is passed to <see cref="GetData"/>)</param>
        /// <param name="key">Key to be passed to <see cref="GetData"/>.</param>
        /// <returns>DateTime in correct time-zone.</returns>
        /// <remarks>If DateTime got via GetData is null, "N/A" is returned.</remarks>
        private String GetTimezonedDateTime(object data, string key)
        {
            DateTime dateTime = ValidationHelper.GetDateTime(GetData(data, key), DateTimeHelper.ZERO_TIME);
            if (dateTime != DateTimeHelper.ZERO_TIME)
            {
                return TimeZoneHelper.ConvertToUserTimeZone(dateTime, false, MembershipContext.AuthenticatedUser, SiteInfoProvider.GetSiteInfo(SiteID));
            }

            return ResHelper.GetString("general.na");
        }

        #endregion


        #region "IPostBackEventHandler Members"

        /// <summary>
        /// Ensures post back event.
        /// </summary>
        /// <param name="eventArgument">Action argument</param>
        public void RaisePostBackEvent(string eventArgument)
        {
            string[] info = eventArgument.Split('$');
            string reloadMessage = "";
            if (info.Length == 2)
            {
                int id = ValidationHelper.GetInteger(info[1], 0);
                if (id > 0)
                {
                    ForumPostInfo fpi;
                    ForumInfo fri;
                    switch (info[0])
                    {
                        case "Delete":
                            fpi = ForumPostInfoProvider.GetForumPostInfo(id);
                            if ((fpi != null) && (IsAvailable(fpi, ForumActionType.Delete)))
                            {
                                // Delete post                            
                                ForumPostInfoProvider.DeleteForumPostInfo(fpi);
                            }
                            break;

                        case "Approve":
                            fpi = ForumPostInfoProvider.GetForumPostInfo(id);
                            if ((fpi != null) && (IsAvailable(fpi, ForumActionType.Appprove)))
                            {
                                // Approve post
                                fpi.PostApproved = true;
                                ForumPostInfoProvider.SetForumPostInfo(fpi);
                            }
                            break;

                        case "ApproveAll":
                            fpi = ForumPostInfoProvider.GetForumPostInfo(id);
                            if ((fpi != null) && (IsAvailable(fpi, ForumActionType.ApproveAll)))
                            {
                                // Approve current post
                                fpi.PostApproved = true;
                                ForumPostInfoProvider.SetForumPostInfo(fpi);

                                // Get all approved sub posts
                                DataSet ds = ForumPostInfoProvider.SelectForumPosts(fpi.PostForumID, fpi.PostIDPath + "/%", "(PostID<>" + fpi.PostId + " AND (PostApproved = 0 OR PostApproved IS NULL))", null, -1, false);
                                if (!DataHelper.DataSourceIsEmpty(ds))
                                {
                                    foreach (DataRow dr in ds.Tables[0].Rows)
                                    {
                                        // Approve all child posts
                                        ForumPostInfo fp = new ForumPostInfo(dr);
                                        fp.PostApproved = true;
                                        ForumPostInfoProvider.SetForumPostInfo(fp);
                                    }
                                }
                            }
                            break;

                        case "Reject":
                            fpi = ForumPostInfoProvider.GetForumPostInfo(id);
                            if ((fpi != null) && (IsAvailable(fpi, ForumActionType.Reject)))
                            {
                                // Reject post
                                fpi.PostApproved = false;
                                ForumPostInfoProvider.SetForumPostInfo(fpi);
                            }
                            break;

                        case "RejectAll":
                            fpi = ForumPostInfoProvider.GetForumPostInfo(id);
                            if ((fpi != null) && (IsAvailable(fpi, ForumActionType.RejectAll)))
                            {
                                // Reject current post
                                fpi.PostApproved = false;
                                ForumPostInfoProvider.SetForumPostInfo(fpi);

                                // Get all approved sub posts
                                DataSet ds = ForumPostInfoProvider.SelectForumPosts(fpi.PostForumID, fpi.PostIDPath + "/%", "(PostID<>" + fpi.PostId + " AND PostApproved = 1)", null, -1, false);
                                if (!DataHelper.DataSourceIsEmpty(ds))
                                {
                                    foreach (DataRow dr in ds.Tables[0].Rows)
                                    {
                                        // Reject all child posts
                                        ForumPostInfo fp = new ForumPostInfo(dr);
                                        fp.PostApproved = false;
                                        ForumPostInfoProvider.SetForumPostInfo(fp);
                                    }
                                }
                            }
                            break;

                        case "LockThread":
                            fpi = ForumPostInfoProvider.GetForumPostInfo(id);
                            if ((fpi != null) && (IsAvailable(fpi, ForumActionType.LockThread)))
                            {
                                // Lock thread
                                fpi.PostIsLocked = true;
                                ForumPostInfoProvider.SetForumPostInfo(fpi);
                            }
                            break;

                        case "UnlockThread":
                            fpi = ForumPostInfoProvider.GetForumPostInfo(id);
                            if ((fpi != null) && (IsAvailable(fpi, ForumActionType.UnlockThread)))
                            {
                                // Unlock thread
                                fpi.PostIsLocked = false;
                                ForumPostInfoProvider.SetForumPostInfo(fpi);
                            }
                            break;

                        case "LockForum":
                            fri = ForumInfoProvider.GetForumInfo(id);
                            if ((fri != null) && (IsAvailable(fri, ForumActionType.LockForum)))
                            {
                                // Lock forum
                                fri.ForumIsLocked = true;
                                ForumInfoProvider.SetForumInfo(fri);
                            }
                            break;

                        case "UnlockForum":
                            fri = ForumInfoProvider.GetForumInfo(id);
                            if ((fri != null) && (IsAvailable(fri, ForumActionType.UnlockForum)))
                            {
                                // Unlock forum
                                fri.ForumIsLocked = false;
                                ForumInfoProvider.SetForumInfo(fri);
                            }
                            break;

                        case "StickThread":
                            fpi = ForumPostInfoProvider.GetForumPostInfo(id);
                            if ((fpi != null) && (IsAvailable(fpi, ForumActionType.StickThread)))
                            {
                                // Stick thread
                                ForumPostInfoProvider.StickThread(fpi);
                            }
                            break;

                        case "UnstickThread":
                            fpi = ForumPostInfoProvider.GetForumPostInfo(id);
                            if ((fpi != null) && (IsAvailable(fpi, ForumActionType.UnstickThread)))
                            {
                                // Unstick thread
                                ForumPostInfoProvider.UnstickThread(fpi);
                            }
                            break;

                        case "MoveStickyThreadDown":
                            fpi = ForumPostInfoProvider.GetForumPostInfo(id);
                            if ((fpi != null) && (IsAvailable(fpi, ForumActionType.MoveStickyThreadDown)))
                            {
                                // Move thread down
                                ForumPostInfoProvider.MoveStickyThreadDown(fpi);
                            }
                            break;

                        case "MoveStickyThreadUp":
                            fpi = ForumPostInfoProvider.GetForumPostInfo(id);
                            if ((fpi != null) && (IsAvailable(fpi, ForumActionType.MoveStickyThreadUp)))
                            {
                                // Move thread up
                                ForumPostInfoProvider.MoveStickyThreadUp(fpi);
                            }
                            break;

                        case "IsAnswer":
                            fpi = ForumPostInfoProvider.GetForumPostInfo(id);
                            if ((fpi != null) && (IsAvailable(fpi, ForumActionType.IsAnswer)))
                            {
                                if (CanVote(fpi.PostId))
                                {
                                    // Vote as answer
                                    fpi.PostIsAnswer++;
                                    ForumPostInfoProvider.SetForumPostInfo(fpi);
                                    reloadMessage = ResHelper.GetString("Forums.AnswerThanks");
                                }
                                else
                                {
                                    reloadMessage = ResHelper.GetString("Forums.AnswerDenied");
                                }
                            }
                            break;

                        case "IsNotAnswer":
                            fpi = ForumPostInfoProvider.GetForumPostInfo(id);
                            if ((fpi != null) && (IsAvailable(fpi, ForumActionType.IsNotAnswer)))
                            {
                                if (CanVote(fpi.PostId))
                                {
                                    // Vote as not answer
                                    fpi.PostIsNotAnswer++;
                                    ForumPostInfoProvider.SetForumPostInfo(fpi);
                                    reloadMessage = ResHelper.GetString("Forums.AnswerThanks");
                                }
                                else
                                {
                                    reloadMessage = ResHelper.GetString("Forums.AnswerDenied");
                                }
                            }
                            break;

                        case "SplitThread":
                            fpi = ForumPostInfoProvider.GetForumPostInfo(id);
                            if ((fpi != null) && (IsAvailable(fpi, ForumActionType.SplitThread)))
                            {
                                // Split thread
                                ForumPostInfoProvider.SplitThread(fpi);
                            }
                            break;

                        case "AddPostToFavorites":
                            if (MembershipContext.AuthenticatedUser != null)
                            {
                                fpi = ForumPostInfoProvider.GetForumPostInfo(id);
                                if (fpi != null)
                                {
                                    // Add post to favorites
                                    if (ForumUserFavoritesInfoProvider.AddUserFavoritePost(id, MembershipContext.AuthenticatedUser.UserID, TextHelper.LimitLength(fpi.PostSubject, 50, "..."), SiteID))
                                    {
                                        reloadMessage = ResHelper.GetString("Forums.SuccessPostFavorite");
                                    }
                                    else
                                    {
                                        reloadMessage = ResHelper.GetString("Forums.UnSuccessPostFavorite");
                                    }
                                }
                            }
                            break;

                        case "AddForumToFavorites":
                            if (MembershipContext.AuthenticatedUser != null)
                            {
                                fri = ForumInfoProvider.GetForumInfo(id);
                                if (fri != null)
                                {
                                    // Add forum to favorites
                                    if (ForumUserFavoritesInfoProvider.AddUserFavoriteForum(id, MembershipContext.AuthenticatedUser.UserID, TextHelper.LimitLength(fri.ForumDisplayName, 50, "..."), SiteID))
                                    {
                                        reloadMessage = ResHelper.GetString("Forums.SuccessForumFavorite");
                                    }
                                    else
                                    {
                                        reloadMessage = ResHelper.GetString("Forums.UnSuccessForumFavorite");
                                    }
                                }
                            }
                            break;
                    }
                }
            }

            // Redirect after action if is enabled
            if (UseRedirectAfterAction)
            {
                URLHelper.Redirect(RequestContext.CurrentURL);
            }
            else
            {
                // Reload the data
                ReloadData();
                ControlsHelper.UpdateCurrentPanel(this);

                if (!String.IsNullOrEmpty(reloadMessage))
                {
                    ScriptHelper.RegisterStartupScript(this, typeof(string), "ForumViewerReloadMessage", ScriptHelper.GetScript("alert('" + reloadMessage.Replace("'", "\\\'") + "')"));
                }
            }
        }

        #endregion
    }
}
