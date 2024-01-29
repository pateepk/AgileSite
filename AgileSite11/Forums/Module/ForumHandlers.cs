using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.Activities;
using CMS.Base;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Search;
using CMS.DataEngine;

namespace CMS.Forums
{
    /// <summary>
    /// Provides handlers for forums
    /// </summary>
    internal static class ForumHandlers
    {
        /// <summary>
        /// Initializes the blog handlers
        /// </summary>
        public static void Init()
        {
            DocumentEvents.GetContent.Execute += GetContent_Execute;
            DocumentEvents.Update.Before += Update_Before;
        }


        static void Update_Before(object sender, DocumentEventArgs e)
        {
            var originalSiteId = e.Node.NodeSiteID;

            e.CallWhenFinished(() =>
            {
                var document = e.Node;
                var siteId = document.NodeSiteID;

                // Check if site changed
                if (originalSiteId == siteId)
                {
                    return;
                }

                // Update forums
                var parameters = new QueryDataParameters();
                parameters.Add("@SiteID", siteId);

                ForumGroupInfo groupInfo = ForumGroupInfoProvider.GetAdHocGroupInfo(siteId);

                // Prepare where conditions
                var documentIDs = DocumentHelper.GetDocuments()
                                          .All()
                                          .LatestVersion(false)
                                          .Column("DocumentID")
                                          .Path(document.NodeAliasPath, PathTypeEnum.Section)
                                          .OnSite(siteId);
                var where = new WhereCondition()
                    .WhereIn("ForumDocumentID", documentIDs);

                var forumIDs = ForumInfoProvider.GetForums().Where(where).Column("ForumID");

                // Forums will be selected based on thir IDs
                var forumsWhere = new WhereCondition().WhereIn("ForumID", forumIDs);
                // Posts associated with given forums
                var postsWhere = new WhereCondition().WhereIn("PostForumID", forumIDs);

                var postIDs = ForumPostInfoProvider.GetForumPosts().Where(postsWhere).Column("PostId");

                // Attachments asociated with posts
                var attachmentsWhere = new WhereCondition().WhereIn("AttachmentPostID", postIDs);

                // Favorites associated with posts or forums
                var favoritesWhere = new WhereCondition().WhereIn("ForumID", forumIDs).Or().WhereIn("PostID",postIDs);

                // Update forum post site IDs
                ForumPostInfoProvider.UpdateData("PostSiteID = @SiteID", postsWhere.ToString(true), parameters);

                // Update ForumUserFavorites' site IDs
                ForumUserFavoritesInfoProvider.UpdateData("SiteID = @SiteID", favoritesWhere.ToString(true), parameters);

                // Update forum attachments' site IDs
                ForumAttachmentInfoProvider.UpdateData("AttachmentSiteID = @SiteID", attachmentsWhere.ToString(true), parameters);

                // Update forum site IDs and ForumGroupIDs
                parameters.Add("@GroupID", groupInfo.GroupID);
                ForumInfoProvider.UpdateData("ForumSiteID = @SiteID, ForumGroupID = @GroupID", forumsWhere.ToString(true), parameters);
            });
        }

        
        /// <summary>
        /// Includes forums content to the document search content
        /// </summary>
        private static void GetContent_Execute(object sender, DocumentSearchEventArgs e)
        {
            if (!e.IsCrawler && e.Settings.IncludeForums)
            {
                // Get document forum posts
                DataSet forumPosts = ForumPostInfoProvider.GetDocumentPosts(e.Node.DocumentID);
                if (!DataHelper.DataSourceIsEmpty(forumPosts))
                {
                    e.Content += SearchHelper.AddObjectDataToDocument(e.IndexInfo, e.SearchDocument, "forums.forumpost", forumPosts);
                    forumPosts.Dispose();
                }
            }
        }
    }
}