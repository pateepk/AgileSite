using System;
using System.Data;

using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.DataEngine.Query;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.PortalEngine;
using CMS.Base;
using CMS.SiteProvider;

namespace CMS.Blogs
{
    /// <summary>
    /// Class providing blog helper methods.
    /// </summary>
    public static class BlogHelper
    {
        internal const string NODE_ALIAS_PATH_COLUMN_NAME = "NodeAliasPath";


        /// <summary>
        /// Object type for Blog page type
        /// </summary>
        public const string BLOG_OBJECT_TYPE = "cms.document.cms.blog";


        /// <summary>
        /// Class name for Blog page type
        /// </summary>
        public const string BLOG_CLASS_NAME = "cms.blog";


        /// <summary>
        /// Returns parent blog of the specified document.
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <param name="selectOnlyPublished">Select only published</param>
        public static TreeNode GetParentBlog(int documentId, bool selectOnlyPublished)
        {
            // Get document
            var tree = new TreeProvider(MembershipContext.AuthenticatedUser);
            var blog = PortalContext.ViewMode.IsLiveSite() ?
                tree.SelectSingleDocument(documentId) : DocumentHelper.GetDocument(documentId, tree);
            if (blog == null)
            {
                return null;
            }

            return GetParentBlog(blog.NodeAliasPath, blog.NodeSiteName, selectOnlyPublished);
        }


        /// <summary>
        /// Returns parent blog of the specified document.
        /// </summary>
        /// <param name="aliasPath">Alias path</param>
        /// <param name="siteName">Site name</param>
        /// <param name="selectOnlyPublished">Select only published</param>
        public static TreeNode GetParentBlog(string aliasPath, string siteName, bool selectOnlyPublished)
        {
            // Check site
            SiteInfo si = SiteInfoProvider.GetSiteInfo(siteName);
            if (si == null)
            {
                throw new Exception("[BlogHelper.GetParentBlog]: Parent blog site doesn't exist.");
            }

            // Check license
            if (!LicenseHelper.LicenseVersionCheck(si.DomainName, FeatureEnum.Blogs, ObjectActionEnum.Edit))
            {
                LicenseHelper.GetAllAvailableKeys(FeatureEnum.Blogs);
            }

            // Get parent blog data
            var tree = new TreeProvider(MembershipContext.AuthenticatedUser);
            string where = TreePathUtils.GetNodesOnPathWhereCondition(aliasPath, false, false).ToString(true);

            DataSet data;
            if (PortalContext.ViewMode.IsLiveSite())
            {
                data = tree.SelectNodes(siteName, "/%", TreeProvider.ALL_CULTURES, true, BLOG_CLASS_NAME, where, "NodeLevel DESC", -1, selectOnlyPublished, 1);
            }
            else
            {
                data = DocumentHelper.GetDocuments(siteName, "/%", TreeProvider.ALL_CULTURES, true, BLOG_CLASS_NAME, where, "NodeLevel DESC", -1, selectOnlyPublished, 1, tree);
            }

            // No parent blog
            if (DataHelper.DataSourceIsEmpty(data))
            {
                return null;
            }

            return TreeNode.New(BLOG_CLASS_NAME, data.Tables[0].Rows[0]);
        }


        /// <summary>
        /// Returns DataSet with all posts from all blogs.
        /// </summary>
        /// <param name="siteName">Nodes site name</param>
        /// <param name="aliasPath">Path. It may contain % and _ as wild card characters for any number of unknown characters or one unknown character respectively (for MS SQL)</param>
        /// <param name="cultureCode">Nodes culture code</param>
        /// <param name="combineWithDefaultCulture">Specifies if return the default culture document when specified culture not found</param>        
        /// <param name="where">Where condition to use for parent blogs selection</param>
        /// <param name="orderBy">Order by clause to use for the data selection</param>        
        /// <param name="selectOnlyPublished">Select only published nodes</param>
        public static DataSet GetBlogPosts(string siteName, string aliasPath, string cultureCode, bool combineWithDefaultCulture, string where, string orderBy, bool selectOnlyPublished)
        {
            // Check site
            SiteInfo si = SiteInfoProvider.GetSiteInfo(siteName);
            if (si == null)
            {
                throw new Exception("[BlogHelper.GetBlogPosts]: Blog post site doesn't exist.");
            }

            CheckLicense(si);

            var tree = new TreeProvider(MembershipContext.AuthenticatedUser);

            var data = GetBlogDocument(siteName, aliasPath, cultureCode, combineWithDefaultCulture, @where, selectOnlyPublished, tree);

            // No blog found
            if (DataHelper.DataSourceIsEmpty(data))
            {
                return null;
            }

            var wherePosts = GetBlogsWhereCondition(data).ToString(true);

            if (PortalContext.ViewMode.IsLiveSite())
            {
                return tree.SelectNodes(siteName, "/%", cultureCode, combineWithDefaultCulture, "cms.blogpost", wherePosts, orderBy, -1, selectOnlyPublished);
            }

            return DocumentHelper.GetDocuments(siteName, "/%", cultureCode, combineWithDefaultCulture, "cms.blogpost", wherePosts, orderBy, -1, selectOnlyPublished, tree);
        }


        internal static WhereCondition GetBlogsWhereCondition(DataSet data)
        {
            var whereCondition = new WhereCondition();

            foreach (DataRow dr in data.Tables[0].Rows)
            {
                whereCondition.Or().WhereStartsWith(NODE_ALIAS_PATH_COLUMN_NAME, Convert.ToString(dr[NODE_ALIAS_PATH_COLUMN_NAME]) + @"/");
            }

            return whereCondition;
        }


        private static DataSet GetBlogDocument(string siteName, string aliasPath, string cultureCode, bool combineWithDefaultCulture, string @where, bool selectOnlyPublished, TreeProvider tree)
        {
            if (PortalContext.ViewMode.IsLiveSite())
            {
                return tree.SelectNodes(siteName, aliasPath, cultureCode, combineWithDefaultCulture, BLOG_CLASS_NAME, @where, "", -1, selectOnlyPublished);
            }

            return DocumentHelper.GetDocuments(siteName, aliasPath, cultureCode, combineWithDefaultCulture, BLOG_CLASS_NAME, @where, "", -1, selectOnlyPublished, tree);
        }


        private static void CheckLicense(SiteInfo si)
        {
            if (!LicenseHelper.LicenseVersionCheck(si.DomainName, FeatureEnum.Blogs, ObjectActionEnum.Edit))
            {
                LicenseHelper.GetAllAvailableKeys(FeatureEnum.Blogs);
            }
        }


        /// <summary>
        /// Returns parent blog of the specified comment.
        /// </summary>
        /// <param name="commentId">Comment id</param>
        /// <param name="selectOnlyPublished">Select only published</param>
        public static TreeNode GetCommentParentBlog(int commentId, bool selectOnlyPublished)
        {
            CheckLicense();

            // Get current blog comment info
            var comment = BlogCommentInfoProvider.GetBlogCommentInfo(commentId);
            if (comment == null)
            {
                return null;
            }

            // Get comment post node
            var tree = new TreeProvider(MembershipContext.AuthenticatedUser);
            TreeNode postNode = PortalContext.ViewMode.IsLiveSite()
                ? tree.SelectSingleDocument(comment.CommentPostDocumentID)
                : DocumentHelper.GetDocument(comment.CommentPostDocumentID, tree);

            return (postNode != null) ? GetParentBlog(postNode.DocumentID, selectOnlyPublished) : null;
        }


        private static void CheckLicense()
        {
            if (!LicenseHelper.LicenseVersionCheck(RequestContext.CurrentDomain, FeatureEnum.Blogs, ObjectActionEnum.Edit))
            {
                LicenseHelper.GetAllAvailableKeys(FeatureEnum.Blogs);
            }
        }


        /// <summary>
        /// Determines if user is moderator of the specified blog.
        /// </summary>
        /// <param name="userName">User name</param>
        /// <param name="blogNode">Blog node (tree node of type 'cms.blog')</param>        
        public static bool IsUserBlogModerator(string userName, TreeNode blogNode)
        {
            // No blog or user is not specified
            if ((blogNode == null) || string.IsNullOrEmpty(userName))
            {
                return false;
            }

            string moderators = ValidationHelper.GetString(blogNode.GetValue("BlogModerators"), "");
            if (moderators != "")
            {
                return (";" + moderators.Trim(';') + ";").ToLowerCSafe().Contains(";" + userName.ToLowerCSafe() + ";");
            }

            return false;
        }


        /// <summary>
        /// Determines if user is owner of the specified blog.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="blogNode">Blog node (tree node of type 'cms.blog')</param>        
        public static bool IsUserBlogOwner(int userId, TreeNode blogNode)
        {
            // No blog or user is not specified
            if ((userId <= 0) || (blogNode == null))
            {
                return false;
            }

            return (userId == ValidationHelper.GetInteger(blogNode.GetValue("NodeOwner"), 0));
        }


        /// <summary>
        /// Checks if the current user is allowed to manage comments of the blog placed on specified node.
        /// </summary>
        /// <param name="blogNode">Node the blog resides at</param>
        /// <param name="exceptionOnFailure">If true, PermissionCheckException is thrown whenever a permission check fails</param>
        public static bool IsUserAuthorizedToManageComments(TreeNode blogNode, bool exceptionOnFailure = false)
        {
            // Global admin is authorized
            var user = MembershipContext.AuthenticatedUser;
            if (user.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin))
            {
                return true;
            }

            // Determine whether user is authorized to manage comments
            bool isOwner = IsUserBlogOwner(user.UserID, blogNode);
            bool isModerator = IsUserBlogModerator(user.UserName, blogNode);

            return (isOwner || isModerator || user.IsAuthorizedPerResource("cms.blog", "Manage", SiteContext.CurrentSiteName, exceptionOnFailure));
        }


        /// <summary>
        /// Returns all blogs from specified site, optionally owned by specified user or moderated by specified user.
        /// </summary>
        /// <param name="siteName">Site name</param> 
        /// <param name="ownerId">Blog owner (user ID)</param>
        /// <param name="moderator">Blog moderator (user name)</param>
        /// <param name="columns">Columns to be selected</param>
        /// <param name="customWhere">Additional WHERE condition</param>
        public static DataSet GetBlogs(string siteName, int ownerId = 0, string moderator = null, string columns = null, string customWhere = null)
        {
            var where = GetBlogsWhere(ownerId, moderator, customWhere).ToString(true);
            var tree = new TreeProvider(MembershipContext.AuthenticatedUser);

            if (PortalContext.ViewMode.IsLiveSite())
            {
                return tree.SelectNodes(siteName, "/%", TreeProvider.ALL_CULTURES, false, BLOG_CLASS_NAME, where, "BlogName ASC", TreeProvider.ALL_LEVELS, false, -1, columns);
            }

            return DocumentHelper.GetDocuments(siteName, "/%", TreeProvider.ALL_CULTURES, false, BLOG_CLASS_NAME, where, "BlogName ASC", TreeProvider.ALL_LEVELS, false, -1, columns, tree);
        }


        /// <summary>
        /// Returns combined where condition using given ownerId, moderator name and custom where condition.
        /// </summary>
        /// <param name="ownerId">Blog owner (user ID)</param>
        /// <param name="moderator">Blog moderator (user name)</param>
        /// <param name="customWhere">Additional WHERE condition</param>
        public static WhereCondition GetBlogsWhere(int ownerId, string moderator, string customWhere)
        {
            var condition = new WhereCondition();

            // Ensure blog moderator
            if (!string.IsNullOrEmpty(moderator))
            {
                condition.WhereContains("';' + BlogModerators + ';'".AsExpression(), ";" + moderator.Trim() + ";");
            }

            // Ensure blog owner
            if (ownerId > 0)
            {
                condition.Or().WhereEquals("NodeOwner", ownerId);
            }

            // Custom where condition
            if (!String.IsNullOrEmpty(customWhere))
            {
                var custom = new WhereCondition();
                custom.Where(condition);
                custom.Where(customWhere);
                condition = custom;
            }

            return condition;
        }


        /// <summary>
        /// Get blog double opt-in interval from settings
        /// </summary>
        /// <param name="siteName">Site name of related settings</param>
        public static int GetBlogDoubleOptInInterval(string siteName)
        {
            return SettingsKeyInfoProvider.GetIntValue(siteName + ".CMSBlogOptInInterval");
        }


        /// <summary>
        /// Indicates if opt-in is enabled for blog
        /// </summary>
        /// <param name="blog">Blog document to check</param>
        public static bool BlogOptInEnabled(TreeNode blog)
        {
            // Document is blog
            if ((blog == null) || !blog.NodeClassName.EqualsCSafe(BLOG_CLASS_NAME, true))
            {
                return false;
            }

            int optIn = blog.GetValue("BlogEnableOptIn", -1);
            if (optIn < 0)
            {
                return SettingsKeyInfoProvider.GetBoolValue(blog.NodeSiteName + ".CMSBlogEnableOptIn");
            }

            if (optIn > 0)
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Creates macro resolver
        /// </summary>
        /// <param name="blog">Blog tree node</param>
        /// <param name="blogPost">Blog post tree node</param>
        public static MacroResolver CreateMacroResolver(TreeNode blog, TreeNode blogPost)
        {
            MacroResolver resolver = MacroContext.CurrentResolver.CreateChild();
            resolver.SetNamedSourceData("Blog", blog);
            resolver.SetNamedSourceData("BlogPost", blogPost);

            resolver.SetNamedSourceData("BlogPostTitle", ValidationHelper.GetString(blogPost.GetValue("BlogPostTitle"), String.Empty));
            resolver.SetNamedSourceData("BlogLink", URLHelper.GetAbsoluteUrl(DocumentURLProvider.GetUrl(blog)));
            resolver.SetNamedSourceData("BlogPostLink", URLHelper.GetAbsoluteUrl(DocumentURLProvider.GetUrl(blogPost)));

            return resolver;
        }
    }
}