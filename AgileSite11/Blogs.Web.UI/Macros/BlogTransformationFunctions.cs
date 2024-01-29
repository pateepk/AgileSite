using System;
using System.Linq;
using System.Web;

using CMS.Base.Web.UI;
using CMS.DocumentEngine.Web.UI;
using CMS.Helpers;
using CMS.PortalEngine;
using CMS.SiteProvider;
using CMS.Membership;
using CMS.DocumentEngine;
using CMS.MacroEngine;
using CMS.Taxonomy;

namespace CMS.Blogs.Web.UI
{
    /// <summary>
    /// Blog functions.
    /// </summary>
    public class BlogTransformationFunctions
    {
        /// <summary>
        /// Returns user name.
        /// </summary>
        /// <param name="userId">User id</param>
        public static string GetUserName(object userId)
        {
            int id = ValidationHelper.GetInteger(userId, 0);
            if (id <= 0)
            {
                return string.Empty;
            }

            // Most of the post will be from the same user, store fullname to the request to minimize the DB access
            string key = "BlogTransformationFunctions_UserName_" + id;
            if (RequestStockHelper.Contains(key))
            {
                return ValidationHelper.GetString(RequestStockHelper.GetItem(key), string.Empty);
            }

            // Get user info from database
            UserInfo user = UserInfoProvider.GetUsers().Where("UserID = " + id).Columns("UserName").TopN(1).FirstOrDefault();
            if (user == null)
            {
                return string.Empty;
            }

            string result = HTMLHelper.HTMLEncode(UserInfoProvider.TrimSitePrefix(user.UserName));
            RequestStockHelper.Add(key, result);

            return result;
        }


        /// <summary>
        /// Returns user full name.
        /// </summary>
        /// <param name="userId">User id</param>
        public static string GetUserFullName(object userId)
        {
            int id = ValidationHelper.GetInteger(userId, 0);
            if (id <= 0)
            {
                return string.Empty;
            }

            return TransformationHelper.HelperObject.GetUserFullName(id);
        }


        /// <summary>
        /// Returns number of comments of given blog.
        /// </summary>
        /// <param name="postId">Post document id</param>
        /// <param name="postAliasPath">Post alias path</param>
        public static int GetBlogCommentsCount(object postId, object postAliasPath)
        {
            var docId = ValidationHelper.GetInteger(postId, 0);
            var aliasPath = ValidationHelper.GetString(postAliasPath, string.Empty);
            var currentUser = MembershipContext.AuthenticatedUser;
            var siteName = SiteContext.CurrentSiteName;

            // There has to be the current site
            if (string.IsNullOrEmpty(siteName))
            {
                throw new Exception("[BlogTransformationFunctions.GetBlogCommentsCount]: There is no current site!");
            }

            bool isOwner = false;

            // Is user authorized to manage comments?
            bool selectOnlyPublished = PortalContext.ViewMode.IsLiveSite();
            var blog = BlogHelper.GetParentBlog(aliasPath, siteName, selectOnlyPublished);
            if (blog != null)
            {
                isOwner = (currentUser.UserID == ValidationHelper.GetInteger(blog.GetValue("NodeOwner"), 0));
            }

            bool isUserAuthorized = (currentUser.IsAuthorizedPerResource("cms.blog", "Manage") || isOwner || BlogHelper.IsUserBlogModerator(currentUser.UserName, blog));

            // Get post comments
            return BlogCommentInfoProvider.GetPostCommentsCount(docId, !isUserAuthorized, isUserAuthorized);
        }


        /// <summary>
        /// Gets a list of links of tags assigned for the specific document pointing to the page with URL specified
        /// </summary>
        /// <param name="documentGroupId">ID of the group document tags belong to</param>
        /// <param name="documentTags">String containing all the tags related to the document</param>
        /// <param name="documentListPage">URL of the page displaying other documents of the specified tag</param>
        public static string GetDocumentTags(object documentGroupId, object documentTags, string documentListPage)
        {
            return GetDocumentTags(documentGroupId, documentTags, null, documentListPage);
        }


        /// <summary>
        /// Gets a list of links of tags assigned for the specific document pointing to the page with URL specified.
        /// </summary>
        /// <param name="documentGroupId">ID of the group document tags belong to</param>
        /// <param name="documentTags">String containing all the tags related to the document</param>
        /// <param name="nodeAliasPath">Node alias path</param>
        /// <param name="documentListPage">Path or URL of the page displaying other documents of the specified tag</param>
        public static string GetDocumentTags(object documentGroupId, object documentTags, object nodeAliasPath, string documentListPage)
        {
            var groupId = ValidationHelper.GetInteger(documentGroupId, 0);
            var tags = ValidationHelper.GetString(documentTags, null);
            var path = ValidationHelper.GetString(nodeAliasPath, null);
            var listPage = ValidationHelper.GetString(documentListPage, null);
            return GetDocumentTags(groupId, tags, path, null, listPage);
        }


        /// <summary>
        /// Gets a list of links of tags assigned for the specific document pointing to the page with URL specified.
        /// </summary>
        /// <param name="groupId">ID of the group document tags belong to</param>
        /// <param name="tags">String containing all the tags related to the document</param>
        /// <param name="path">Node alias path</param>
        /// <param name="culture">Document culture</param>
        /// <param name="listPage">Path or URL of the page displaying other documents of the specified tag</param>
        public static string GetDocumentTags(int groupId, string tags, string path = null, string culture = null, string listPage = null)
        {
            // Get inherited value
            if (groupId == 0)
            {
                // Ensure context properties
                var currentPageInfo = DocumentContext.CurrentPageInfo;

                if (path == null)
                {
                    path = currentPageInfo.NodeAliasPath;
                }

                if (culture == null)
                {
                    culture = currentPageInfo.DocumentCulture;
                }

                groupId = PageInfoProvider.GetParentProperty<int>(currentPageInfo.NodeSiteID, path, "DocumentTagGroupID", culture);
            }

            // Get valid URL either from relative URL or path
            listPage = ValidationHelper.IsURL(listPage) ? UrlResolver.ResolveUrl(listPage) : MacroContext.CurrentResolver.ResolvePath(listPage);

            // Get tags markup
            return GetLinksOrTagList(groupId, tags, listPage);
        }


        /// <summary>
        /// Gets a list of links of given tags in a specified group pointing to the page with URL specified.
        /// </summary>
        /// <param name="groupId">ID of the group document tags belong to</param>
        /// <param name="tags">String containing all the tags related to the document</param>
        /// <param name="url">URL of the page displaying other documents of the specified tag</param>
        internal static string GetLinksOrTagList(int groupId, string tags, string url = null)
        {
            if (string.IsNullOrWhiteSpace(tags) || (groupId <= 0))
            {
                return string.Empty;
            }

            // Get sorted list of document tags
            var tagsSet = TagHelper.GetTags(tags);
            var result = tagsSet.Select(t =>
            {
                var encodedTag = HTMLHelper.HTMLEncode(t);

                // If list page was specified make a list of links, otherwise return just list of tags
                bool renderLink = !string.IsNullOrEmpty(url);
                if (renderLink)
                {
                    return "<a href=\"" + url + "?tagname=" + HttpUtility.UrlEncode(t) + "&amp;groupid=" + groupId + "\">" + encodedTag + "</a>";
                }

                return encodedTag;
            });

            var tagsList = result.ToList();
            tagsList.Sort();

            return tagsList.Join(", ");
        }
    }
}