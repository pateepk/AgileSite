using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.PortalEngine;
using CMS.Search;
using CMS.SiteProvider;
using CMS.Taxonomy;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Provides handlers for documents
    /// </summary>
    internal static class DocumentHandlers
    {
        /// <summary>
        /// Initializes the document handlers
        /// </summary>
        public static void Init()
        {
            // Handle page template update/delete for smart search tasks
            var pageInfoEvents = PageTemplateInfo.TYPEINFO.Events;
            pageInfoEvents.Update.After += LogSearchTasks;
            pageInfoEvents.Delete.After += LogSearchTasks;

            SettingsKeyInfoProvider.GetURL.Execute += ConvertAliasPathToDocumentUrl;
            SessionEvents.UpdateSessionData.Execute += UpdateSessionLocation;
            SearchEvents.Search.After += AddAttachmentResults;
            SiteInfo.TYPEINFO.Events.Delete.After += DeleteSearchIndex;
            TagGroupInfo.TYPEINFO.Events.Delete.Before += ClearTagsFromDocuments;

            // Ensure dropping and creating of the document view
            DataDefinitionItemEvents.ChangeItem.Before += RefreshDocumentView;
            DataDefinitionItemEvents.AddItem.Before += RefreshDocumentView;
            DataDefinitionItemEvents.RemoveItem.Before += RefreshDocumentView;

            DataDefinitionItemEvents.RemoveItem.Before += DeleteDocumentAttachments;
        }


        private static void RefreshDocumentView(object sender, DataDefinitionItemEventArgs e)
        {
            var classInfo = e.ClassInfo;
            if (classInfo == null)
            {
                return;
            }

            // Drop document view in case of modification to document tables
            switch (classInfo.ClassName.ToLowerInvariant())
            {
                case "cms.document":
                case "cms.tree":
                    var field = (FormFieldInfo)e.Item;
                    if ((field == null) || field.IsDummyField || field.External)
                    {
                        // No need to refresh any view if the field is not a database column
                        return;
                    }

                    var tm = new TableManager(null);

                    // Drop and recreate the view with new select statement
                    var schema = tm.DropView(SystemViewNames.View_CMS_Tree_Joined);

                    e.CallWhenFinished(() =>
                    {
                        string indexes;

                        // Generate new code for the view
                        string newSelect = SqlGenerator.GetSystemViewSqlQuery(SystemViewNames.View_CMS_Tree_Joined, out indexes);
                        bool indexed = !String.IsNullOrEmpty(indexes);

                        tm.CreateView(SystemViewNames.View_CMS_Tree_Joined, newSelect, indexed, schema);

                        // Execute the extra code for the view
                        if (indexed)
                        {
                            tm.ExecuteQuery(indexes, null, QueryTypeEnum.SQLQuery);
                        }
                    });

                    break;
            }
        }


        private static void DeleteDocumentAttachments(object sender, DataDefinitionItemEventArgs e)
        {
            var classInfo = e.ClassInfo;
            if ((classInfo == null) || !classInfo.ClassIsDocumentType)
            {
                return;
            }

            var field = (FormFieldInfo)e.Item;
            if (field == null)
            {
                return;
            }

            DocumentHelper.DeleteDocumentAttachments(classInfo.ClassName, field);
        }


        private static void ClearTagsFromDocuments(object sender, ObjectEventArgs e)
        {
            var tagGroup = (TagGroupInfo)e.Object;

            // Get document IDs which have tags from deleted group
            var documentIds = DocumentTagInfoProvider.GetDocumentTags()
                                .Distinct()
                                .Column("DocumentID")
                                .WhereIn("TagID", TagInfoProvider.GetTags()
                                                            .Column("TagID")
                                                            .WhereID("TagGroupID", tagGroup.TagGroupID))
                                .Select(t => t.DocumentID)
                                .ToList();

            if (documentIds.Count <= 0)
            {
                return;
            }

            // Clear document tags from documents via bulk update. This change is not logged via staging task because there is no support for it on the staging module side
            var where = new WhereCondition().WhereIn("DocumentID", documentIds);

            DocumentHelper.ChangeDocumentCultureDataField("DocumentTags", null, where);
        }


        private static void AddAttachmentResults(object sender, SearchEventArgs e)
        {
            var results = e.Results;
            var siteIds = results.SiteIDs;
            var parameters = e.Parameters;
            List<string> highlights;

            if (!parameters.SearchInAttachments || (siteIds.Count <= 0))
            {
                return;
            }

            var filteredResults = results.Results;
            var documents = results.Documents;

            // Attachment search
            TreeProvider tp = new TreeProvider();
            var attachmentsDs = tp.AttachmentSearch(parameters, siteIds, out highlights);
            e.Highlights = highlights;

            // Combine attachment results with regular results
            if (DataHelper.DataSourceIsEmpty(attachmentsDs))
            {
                return;
            }

            // Remove all attachments results, which are present in search results
            int i = 0;
            while (i < attachmentsDs.Tables[0].Rows.Count)
            {
                DataRow dr = attachmentsDs.Tables[0].Rows[i];
                if (documents.Contains(ValidationHelper.GetString(dr["DocumentID"] + ";" + dr["NodeID"], String.Empty)))
                {
                    attachmentsDs.Tables[0].Rows.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }

            // Combine results in filtered results
            int lastIndex = 1;
            bool justAdd = filteredResults.Count < 2;

            foreach (DataRow dr in attachmentsDs.Tables[0].Rows)
            {
                // Create iDocument equal to current attachment row result
                var documentParameters = new SearchDocumentParameters
                {
                    Type = TreeNode.OBJECT_TYPE,
                    Id = Convert.ToString(dr["DocumentID"]) + ";" + Convert.ToString(dr["NodeID"]),
                    Created = DateTimeHelper.ZERO_TIME
                };

                var searchDocument = SearchHelper.CreateDocument(documentParameters);
                var luceneDocument = LuceneSearchDocumentHelper.ToLuceneSearchDocument(searchDocument);

                // Add required fields
                luceneDocument.AddGeneralField("documentid", Convert.ToString(dr["DocumentID"]), true, false);
                luceneDocument.AddGeneralField("nodeid", Convert.ToString(dr["NodeID"]), true, false);
                luceneDocument.AddGeneralField("nodelinkednodeid", Convert.ToString(dr["NodeLinkedNodeID"]), true, false);

                // Get document class name
                DataClassInfo classInfo = DataClassInfoProvider.GetDataClassInfo(ValidationHelper.GetInteger(dr["NodeClassID"], 0));
                if (classInfo != null)
                {
                    luceneDocument.AddGeneralField("classname", classInfo.ClassName, true, false);
                }

                // iDocument should be added directly to the collection
                if (justAdd)
                {
                    filteredResults.Add(luceneDocument);
                }
                // Interlaced results
                else
                {
                    filteredResults.Insert(lastIndex, luceneDocument);
                    lastIndex += 2;
                    justAdd = (filteredResults.Count < lastIndex);
                }
            }
        }


        private static void UpdateSessionLocation(object sender, SessionEventArgs e)
        {
            e.Session.SessionLocation = DocumentContext.OriginalAliasPath;
        }


        /// <summary>
        /// Handler to convert URL starting with slash (considered to be an alias path) to the document URL
        /// </summary>
        private static void ConvertAliasPathToDocumentUrl(object sender, URLEventArgs e)
        {
            var url = e.URL;
            if (IsAliasPath(url))
            {
                e.URL = DocumentURLProvider.GetUrl(e.URL);
            }
        }


        private static bool IsAliasPath(string url)
        {
            return url.StartsWith("/", StringComparison.Ordinal);
        }


        /// <summary>
        /// Creates smart search tasks for document with removed/changed page template
        /// </summary>
        private static void LogSearchTasks(object sender, ObjectEventArgs e)
        {
            var template = e.Object;
            if ((template == null) || !PageTemplateInfoProvider.CreateSearchTasks || !SearchIndexInfoProvider.SearchEnabled)
            {
                return;
            }

            string where = String.Format("DocumentPageTemplateID = {0} OR NodeTemplateID = {0}", ((PageTemplateInfo)template).PageTemplateId);

            // Prepare variables for searching for depending nodes
            TreeProvider tp = new TreeProvider();

            // Get all nodes
            DataSet ds = tp.SelectNodes(TreeProvider.ALL_SITES, TreeProvider.ALL_DOCUMENTS, TreeProvider.ALL_CULTURES, false, null, where, null, -1, false, 0, DocumentColumnLists.SELECTNODES_REQUIRED_COLUMNS + ", DocumentCheckedOutVersionHistoryID, DocumentPublishedVersionHistoryID");
            if (DataHelper.DataSourceIsEmpty(ds))
            {
                return;
            }

            // Get node
            var taskParameters = new List<SearchTaskCreationParameters>();

            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                var node = TreeNode.New(dr["ClassName"].ToString(), dr);
                if ((node != null) && node.PublishedVersionExists)
                {
                    var parameters = new SearchTaskCreationParameters
                    {
                        TaskType = SearchTaskTypeEnum.Update,
                        ObjectType = TreeNode.OBJECT_TYPE,
                        ObjectField = SearchFieldsConstants.ID,
                        TaskValue = node.GetSearchID(),
                        RelatedObjectID = node.DocumentID
                    };

                    taskParameters.Add(parameters);
                }
            }

            SearchTaskInfoProvider.CreateTasks(taskParameters, true);
        }


        private static void DeleteSearchIndex(object sender, ObjectEventArgs e)
        {
            if (!SearchIndexInfoProvider.SearchEnabled)
            {
                return;
            }

            var obj = e.Object;
            string siteName = ValidationHelper.GetString(obj.GetValue("SiteName"), "");
            int siteID = ValidationHelper.GetInteger(obj.GetValue("SiteID"), 0);

            SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Delete, TreeNode.OBJECT_TYPE, SearchFieldsConstants.SITE, siteName, siteID);
        }
    }
}