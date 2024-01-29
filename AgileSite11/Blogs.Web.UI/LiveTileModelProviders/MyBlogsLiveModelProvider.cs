using System;

using CMS.ApplicationDashboard.Web.UI;
using CMS.Blogs.Web.UI;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;

[assembly: RegisterLiveTileModelProvider(ModuleName.BLOGS, "MyBlogs", typeof(MyBlogsLiveModelProvider))]

namespace CMS.Blogs.Web.UI
{
    /// <summary>
    /// Provides live tile model for the my blogs dashboard tile.
    /// </summary>
    internal class MyBlogsLiveModelProvider : ILiveTileModelProvider
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
                int waitingCommentsCount = GetNumberOfWaitingComments(liveTileContext.SiteInfo.SiteID, liveTileContext.UserInfo.UserID);
                if (waitingCommentsCount == 0)
                {
                    return null;
                }

                return new LiveTileModel
                {
                    Value = waitingCommentsCount,
                    Description = ResHelper.GetString("myblogs.livetiledescription")
                };
            }, new CacheSettings(2, "MyBlogsLiveModelProvider", liveTileContext.SiteInfo.SiteID, liveTileContext.UserInfo.UserID));
        }


        /// <summary>
        /// Gets number of total comments waiting for the approval.
        /// </summary>
        /// <param name="siteId">Site the comments belongs to</param>
        /// <param name="userId">The user providing the approval</param>
        /// <returns>Total number of waiting comments</returns>
        private int GetNumberOfWaitingComments(int siteId, int userId)
        {
            var comments = BlogCommentInfoProvider.GetBlogComments()
                                .Column("CommentID")
                                .WhereEquals("CommentApproved", 0)
                                .WhereIn("CommentPostDocumentID", new IDQuery<TreeNode>("DocumentID")
                                                                    .WhereEquals("NodeOwner", userId)
                                                                    .WhereEquals("ClassName", "cms.blogpost")
                                                                    .OnSite(siteId));
            return comments.Count;
        }
    }
}