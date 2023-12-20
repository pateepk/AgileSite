using System;
using System.Data;

using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Search;

namespace CMS.MessageBoards
{
    /// <summary>
    /// Provides handlers for message boards
    /// </summary>
    internal static class MessageBoardHandlers
    {
        /// <summary>
        /// Initializes the message board handlers
        /// </summary>
        public static void Init()
        {
            DocumentEvents.GetContent.Execute += GetContent_Execute;
            DocumentEvents.Update.Before += Update_Before;
            DocumentEvents.ResetRating.Execute += ResetRating_Execute;
        }

        
        /// <summary>
        /// Resets rating for all messages of given document
        /// </summary>
        private static void ResetRating_Execute(object sender, DocumentRatingEventArgs e)
        {
            // Prepare where condition
            var query = BoardInfoProvider.GetMessageBoards()
                                      .Column("BoardID")
                                      .WhereEquals("BoardDocumentID", e.Node.DocumentID);

            var where = new WhereCondition()
                .WhereIn("MessageBoardID", query);

            BoardMessageInfoProvider.UpdateData("[MessageRatingValue] = NULL", where);
        }


        /// <summary>
        /// Ensures site change of the board when site of the document changes
        /// </summary>
        private static void Update_Before(object sender, DocumentEventArgs e)
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

                // Update message boards
                var parameters = new QueryDataParameters();
                parameters.Add("@SiteID", siteId);

                // Prepare where condition
                var query = DocumentHelper.GetDocuments()
                                          .All()
                                          .LatestVersion(false)
                                          .Column("DocumentID")
                                          .Path(document.NodeAliasPath, PathTypeEnum.Section)
                                          .OnSite(siteId);
                var where = new WhereCondition()
                    .WhereIn("BoardDocumentID", query);

                BoardInfoProvider.UpdateData("BoardSiteID = @SiteID", where.ToString(true), parameters);
            });
        }


        /// <summary>
        /// Includes message board content to the document search content
        /// </summary>
        private static void GetContent_Execute(object sender, DocumentSearchEventArgs e)
        {
            if (e.IsCrawler || !e.Settings.IncludeMessageCommunication)
            {
                return;
            }

            // Get the messages
            DataSet messages = BoardMessageInfoProvider.GetDocumentMessages(e.Node.DocumentID);
            if (DataHelper.DataSourceIsEmpty(messages))
            {
                return;
            }

            e.Content += SearchHelper.AddObjectDataToDocument(e.IndexInfo, e.SearchDocument, BoardMessageInfo.OBJECT_TYPE, messages);
            messages.Dispose();
        }
    }
}