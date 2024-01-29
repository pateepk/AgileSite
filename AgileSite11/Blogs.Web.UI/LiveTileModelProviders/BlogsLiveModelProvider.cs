using System;
using System.Data;

using CMS.ApplicationDashboard.Web.UI;
using CMS.Base;
using CMS.Blogs.Web.UI;
using CMS.Core;
using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.SiteProvider;

[assembly: RegisterLiveTileModelProvider(ModuleName.BLOGS, "Blog", typeof(BlogsLiveModelProvider))]

namespace CMS.Blogs.Web.UI
{
    /// <summary>
    /// Provides live tile model for the blogs dashboard tile.
    /// </summary>
    internal class BlogsLiveModelProvider : ILiveTileModelProvider
    {
        /// <summary>
        /// Loads total number of comments waiting for the approval.
        /// </summary>
        /// <param name="liveTileContext">Context of the live tile. Contains information about the user and the site the model is requested for</param>
        /// <exception cref="ArgumentNullException"><paramref name="liveTileContext"/> is null</exception>
        /// <returns>Live tile model</returns>
        public LiveTileModel GetModel(LiveTileContext liveTileContext)
        {
            if (liveTileContext == null)
            {
                throw new ArgumentNullException("liveTileContext");
            }

            return CacheHelper.Cache(() =>
            {
                int waitingCommentsCount = GetNumberOfWaitingComments(liveTileContext.SiteInfo, liveTileContext.UserInfo);
                if (waitingCommentsCount == 0)
                {
                    return null;
                }

                return new LiveTileModel
                {
                    Value = waitingCommentsCount,
                    Description = ResHelper.GetString("blogs.livetiledescription")
                };
            }, new CacheSettings(2, "BlogsLiveModelProvider", liveTileContext.SiteInfo.SiteID, liveTileContext.UserInfo.UserID));
        }


        /// <summary>
        /// Gets number of total comments waiting for the approval.
        /// </summary>
        /// <param name="siteInfo">Site the comments belongs to</param>
        /// <param name="userInfo">The user providing the approval</param>
        /// <returns>Total number of waiting comments</returns>
        private int GetNumberOfWaitingComments(SiteInfo siteInfo, IUserInfo userInfo)
        {
            int siteId = siteInfo.SiteID;
            var documentIDs = new IDQuery<TreeNode>("DocumentID").WhereEquals("ClassName", "cms.blogpost")
                                                                 .OnSite(siteId);

            if (!userInfo.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin) && !userInfo.IsAuthorizedPerResource(ModuleName.BLOGS, "Manage", siteInfo.SiteName, false))
            {
                // Check if current user is owner or moderator of any blogs
                var blogs = DocumentHelper.GetDocuments(BlogHelper.BLOG_CLASS_NAME)
                        .All()
                        .PublishedVersion()
                        .Column("NodeAliasPath")
                        .OnSite(siteId)
                        .Where(new WhereCondition()
                            .WhereEquals("NodeOwner", userInfo.UserID)
                            .Or()
                            .WhereContains("';' + BlogModerators + ';'".AsExpression(), ";" + userInfo.UserName + ";"))
                            .Result;

                if (!DataHelper.DataSourceIsEmpty(blogs))
                {
                    // Go thru all blogs which may current user moderate
                    WhereCondition moderatorCondition = new WhereCondition();
                    foreach (DataRow row in blogs.Tables[0].Rows)
                    {
                        string path = DataHelper.GetStringValue(row, "NodeAliasPath", String.Empty);
                        moderatorCondition.Or().WhereStartsWith("NodeAliasPath", path.TrimEnd('/') + "/");
                    }

                    documentIDs.Where(moderatorCondition);
                }
                else
                {
                    // Current user is not a moderator neither blog owner
                    return 0;
                }
            }

            var comments = BlogCommentInfoProvider.GetBlogComments()
                                .Column("CommentID")
                                .WhereEquals("CommentApproved", 0)
                                .WhereIn("CommentPostDocumentID", documentIDs);

            return comments.Count;
        }
    }
}