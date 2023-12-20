using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine.Search;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Search;
using CMS.SiteProvider;
using CMS.DocumentEngine.Internal;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Indexer for documents
    /// </summary>
    public class DocumentSearchIndexer : SearchIndexer
    {
        #region "Variables"

        /// <summary>
        /// Document category IDs field name
        /// </summary>
        public const string FIELD_DOCUMENTCATEGORYIDS = "DocumentCategoryIDs";
        private readonly DocumentSearchablesRetriever mDocumentSearchablesRetriever;

        #endregion


        /// <summary>
        /// Helps with retrieving <see cref="ISearchable"/> objects and <see cref="SearchIndexInfo"/>s
        /// related to document search tasks.
        /// </summary>
        private DocumentSearchablesRetriever DocumentSearchablesRetriever => mDocumentSearchablesRetriever;


        /// <summary>
        /// Document search indexer.
        /// </summary>
        /// <param name="documentSearchablesRetriever">Responsible for retrieving objects related to document indexing.</param>
        protected DocumentSearchIndexer(DocumentSearchablesRetriever documentSearchablesRetriever)
            : base(documentSearchablesRetriever)
        {
            mDocumentSearchablesRetriever = documentSearchablesRetriever;
        }


        /// <summary>
        /// Document search indexer.
        /// </summary>
        public DocumentSearchIndexer()
            : this(new DocumentSearchablesRetriever())
        { }


        #region "Methods"


        /// <summary>
        /// Rebuilds the document index
        /// </summary>
        /// <param name="indexInfo">Search index</param>
        public override void Rebuild(SearchIndexInfo indexInfo)
        {
            // Check whether exist index settings
            if ((indexInfo.IndexSettings == null) || (indexInfo.IndexSettings.Items == null) || (indexInfo.IndexSettings.Items.Count <= 0))
            {
                return;
            }

            List<int> sites;
            List<string> cultures;

            SafeDictionary<string, string> allowedNodes;
            SafeDictionary<string, string> excludedNodes;

            if (DocumentSearchablesRetriever.PrepareBuildingValues(indexInfo, out sites, out cultures, out allowedNodes, out excludedNodes, null))
            {
                // Use partial rebuild for current index
                Rebuild(indexInfo, DocumentSearchablesRetriever.GetSearchableObjectsInternal(indexInfo, DocumentSearchablesRetriever.ToSiteInfoEnumerable(sites), cultures, allowedNodes, excludedNodes), true);

                SearchHelper.FinishRebuild(indexInfo);
            }
        }


        /// <summary>
        /// Ensures full or partial index rebuild.
        /// </summary>
        /// <remarks>This method needs to be run in a thread safe way such as smart search task queue.</remarks>
        /// <param name="indexInfo">Search index</param>
        /// <param name="searchableObjects">Collection of searchable objects to be indexed</param>
        /// <param name="fullRebuild">Indicates whether current rebuild is full or partial</param>
        private void Rebuild(SearchIndexInfo indexInfo, IEnumerable<ISearchable> searchableObjects, bool fullRebuild)
        {
            // Get current index writer
            var iw = indexInfo.Provider.GetWriter(fullRebuild);

            try
            {
                if (iw == null)
                {
                    return;
                }

                bool first = true;
                string searchId = String.Empty;
                foreach (var searchableObj in searchableObjects)
                {
                    if (first)
                    {
                        searchId = ValidationHelper.GetString(searchableObj.GetValue(SearchFieldsConstants.ID), String.Empty);
                        first = false;
                    }

                    var luceneDocument = LuceneSearchDocumentHelper.ToLuceneSearchDocument(searchableObj.GetSearchDocument(indexInfo));

                    // Use update for partial rebuild
                    if (!fullRebuild)
                    {
                        // Partial rebuild updates always only one document so last id is same as current document id
                        iw.UpdateDocument(luceneDocument, searchId);
                    }
                    // Use insert for full rebuild
                    else
                    {
                        iw.AddDocument(luceneDocument);
                    }
                }

                iw.Flush();

                // Optimize only for full index
                if (fullRebuild)
                {
                    // Optimize index
                    SearchIndexInfoProvider.SetIndexStatus(indexInfo, IndexStatusEnum.OPTIMIZING);
                    iw.Optimize();
                }
            }
            catch (Exception e)
            {
                if (fullRebuild)
                {
                    LogContext.LogEventToCurrent(EventType.WARNING, "Smart search", "INDEXERROR", "DocumentSearchIndexer.Rebuild() causes exception and search index status is set to ERROR.\nIndex name: " + indexInfo.IndexName + "\n\nException: " + e.Message + "\n\nStack trace: " + e.StackTrace, null, 0, null, 0, null, null, 0, null, null, null, DateTime.Now);

                    // Set statuses to error
                    SearchIndexInfoProvider.SetIndexStatus(indexInfo, IndexStatusEnum.ERROR);
                }

                throw;
            }
            finally
            {
                if (iw != null)
                {
                    // Close index
                    iw.Close();
                }
            }
        }

        #endregion


        /// <summary>
        /// Rebuilds part of the index
        /// </summary>
        /// <param name="taskInfo">Search task</param>
        public override void PartialRebuild(SearchTaskInfo taskInfo)
        {
            if (String.IsNullOrEmpty(taskInfo.SearchTaskValue))
            {
                return;
            }

            string[] strArray = taskInfo.SearchTaskValue.Split(';');

            var siteName = strArray[0];
            var nodeAliasPath = strArray[1];

            var indexes = SearchablesRetriever.GetRelevantIndexes(taskInfo.SearchTaskObjectType, siteName, SearchIndexInfo.LUCENE_SEARCH_PROVIDER);

            foreach (var index in indexes)
            {
                var searchables = DocumentSearchablesRetriever.GetSearchableObjects(index, nodeAliasPath, siteName);

                // Use partial rebuild for current index
                Rebuild(index, searchables, false);
                SearchIndexInfoProvider.SetIndexFilesLastUpdateTime(index, DateTime.Now);

                SearchHelper.InvalidateSearcher(index.IndexGUID);
            }
        }


        /// <summary>
        /// Executes the search index update task
        /// </summary>
        /// <param name="sti">Search task</param>
        protected override void ExecuteUpdateTask(SearchTaskInfo sti)
        {
            // Indicates whether regular update should be skipped
            bool skipRegularUpdate = false;

            if (sti.SearchTaskField == SearchFieldsConstants.PARTIAL_REBUILD)
            {
                SearchHelper.PartialRebuild(sti);

                skipRegularUpdate = true;
            }

            // Get document id
            string[] id = sti.SearchTaskValue.Split(';');
            var selectId = ValidationHelper.GetInteger(id[0], 0);

            if (!skipRegularUpdate)
            {
                DocumentUpdate(selectId, sti);
            }
        }


        /// <summary>
        /// Selects the search document based on the given ID
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <returns>Enumeration of searchable objects or empty enumeration.</returns>
        public override IEnumerable<ISearchable> SelectSearchDocument(int documentId)
        {
            var tree = new TreeProvider();

            // Get class name
            var className = TreePathUtils.GetClassNameByDocumentID(documentId);

            // Load all documents (regular document + all linked docs)
            var data = tree.SelectNodes(TreeProvider.ALL_SITES, TreeProvider.ALL_DOCUMENTS, TreeProvider.ALL_CULTURES, false, className, "DocumentID = " + documentId, null, -1, false);

            // Loop thru all documents and create iDocument collection
            if (DataHelper.DataSourceIsEmpty(data))
            {
                return Enumerable.Empty<ISearchable>();
            }

            var items = new List<ISearchable>();
            foreach (DataRow row in data.Tables[0].Rows)
            {
                items.Add(TreeNode.New(row["classname"] as string, row));
            }
            data.Dispose();

            return items;
        }


        /// <summary>
        /// Deletes specific item from the index
        /// </summary>
        /// <param name="taskInfo">Search task</param>
        /// <param name="indexInfo">Search index</param>
        protected override void DeleteItem(SearchTaskInfo taskInfo, SearchIndexInfo indexInfo)
        {
            bool deleteEnabled = true;

            #region "Special case for linked documents, documents are directly deleted"

            // If current object is document and delete is provided by id field
            if (taskInfo.SearchTaskField == SearchFieldsConstants.ID)
            {
                string[] id = taskInfo.SearchTaskValue.Split(';');

                // Get document id
                int docId = ValidationHelper.GetInteger(id[0], 0);
                int nodeId = 0;

                if (id.Length > 1)
                {
                    nodeId = ValidationHelper.GetInteger(id[1], 0);
                }

                // Retrieve document trough process module command
                var searchableDocuments = SelectSearchDocument(docId).ToList();

                // Check whether current document is linked document
                if (searchableDocuments.Any())
                {
                    // Indicates whether current id should be used for delete
                    var singleDelete = true;

                    // Loop thru all loaded iDocuments
                    foreach (var document in searchableDocuments)
                    {
                        // Try find document nodeid => Document is in database => Archived or non-published
                        if (nodeId == ValidationHelper.GetInteger(document.GetValue("NodeID"), -1))
                        {
                            singleDelete = false;
                            break;
                        }
                    }

                    // Clear documents collection if required document isn't in DB
                    if (singleDelete)
                    {
                        searchableDocuments.Clear();
                    }
                }

                if (searchableDocuments.Any())
                {
                    // Loop thru all loaded iDocuments
                    foreach (var document in searchableDocuments)
                    {
                        SearchHelper.Delete(taskInfo.SearchTaskField, document.GetSearchID(), indexInfo);
                    }
                    deleteEnabled = false;
                }
            }

            #endregion

            #region "Special case for moved documents, documents are directly deleted"

            // If current object is document and delete is provided by id field
            if (taskInfo.SearchTaskField == SearchFieldsConstants.PARTIAL_REBUILD)
            {
                // Get site name and node alias path
                string[] values = taskInfo.SearchTaskValue.Split(';');

                // Get site info
                SiteInfo si = SiteInfoProvider.GetSiteInfo(values[0]);

                // Check whether
                if ((si != null) && (SearchIndexInfoProvider.IndexIsInSite(indexInfo, si.SiteID)))
                {
                    var searcher = indexInfo.Provider.GetSearcher(true);
                    if ((searcher?.NumberOfDocuments() > 0))
                    {
                        // Create alias path filter
                        string path = values[1].TrimEnd('%');
                        var apf = SearchManager.CreateFilter("NodeAliasPath", path, (value, match) => value.StartsWith(match, StringComparison.InvariantCultureIgnoreCase));

                        // Create query for document type and specific site only
                        string query = SearchSyntaxHelper.GetFieldCondition(SearchFieldsConstants.TYPE, TreeNode.OBJECT_TYPE, TreeNode.OBJECT_TYPE);
                        query += SearchSyntaxHelper.GetFieldCondition("nodesiteid", si.SiteID);

                        // Search for all documents
                        var hits = searcher.Search(query, indexInfo.Provider.GetAnalyzer(true), apf);

                        if ((hits != null) && (hits.Length() > 0))
                        {
                            List<string> ids = new List<string>();

                            // Loop thru all documents
                            for (int i = 0; i < hits.Length(); i++)
                            {
                                // Get id value
                                ids.Add(hits.Doc(i).Get(SearchFieldsConstants.ID));
                            }

                            // Delete documents from index
                            SearchHelper.Delete(SearchFieldsConstants.ID, ids, indexInfo);
                        }
                    }
                }

                deleteEnabled = false;
            }

            #endregion

            if (deleteEnabled)
            {
                SearchHelper.Delete(taskInfo.SearchTaskField, taskInfo.SearchTaskValue, indexInfo);
            }
        }


        /// <summary>
        /// Checks the permissions for the given result document
        /// </summary>
        /// <param name="settings">Check permission settings</param>
        /// <param name="currentDoc">Current result document</param>
        /// <param name="index">Current document index</param>
        public override bool CheckResultPermissions(SearchResults settings, ILuceneSearchDocument currentDoc, int index)
        {
            int filterResult = TreeSecurityProvider.FilterSearchResults(settings, currentDoc, index);
            var allow = filterResult > 0;

            // Check whether current document should be added to the filtered collections
            var documents = settings.Documents;
            if (allow && (documents != null))
            {
                documents.Add(currentDoc.Get(SearchFieldsConstants.ID));
            }

            return allow;
        }


        /// <summary>
        /// Loads the results to the given result collection
        /// </summary>
        /// <param name="results">Collection of results to load</param>
        /// <param name="result">Dictionary of the loaded results indexed by their key</param>
        public override void LoadResults(IEnumerable<ILuceneSearchDocument> results, SafeDictionary<string, DataRow> result)
        {
            var searchData = GetSearchData(results);
            var classGroups = searchData.GroupBy(d => d.ClassName, StringComparer.InvariantCultureIgnoreCase);

            foreach (var classGroup in classGroups)
            {
                var className = classGroup.Key;
                var where = GetWhereCondition(classGroup.ToList());
                LoadData(result, className, where);
            }
        }


        private static IEnumerable<DocumentSearchData> GetSearchData(IEnumerable<ILuceneSearchDocument> results)
        {
            return results.Select(doc => new DocumentSearchData
            {
                ClassName = doc.Get("classname"),
                NodeID = TryGetId(doc, "nodeid"),
                DocumentID = TryGetId(doc, "documentid"),
                IsLink = TryGetId(doc, "nodelinkednodeid") > 0
            });
        }


        private static int TryGetId(ILuceneSearchDocument filteredDocument, string fieldName)
        {
            int id;
            int.TryParse(filteredDocument.Get(fieldName), out id);

            return id;
        }


        private static void LoadData(SafeDictionary<string, DataRow> result, string className, WhereCondition where)
        {
            var tree = new TreeProvider();
            var data = tree.SelectNodes("##ALL##", TreeProvider.ALL_DOCUMENTS, "##ALL##", false, className, where.ToString(true));
            if (DataHelper.DataSourceIsEmpty(data))
            {
                return;
            }

            foreach (DataRow row in data.Tables[0].Rows)
            {
                var key = string.Format("{0};{1}_{2}", row["DocumentID"], row["NodeID"], TreeNode.OBJECT_TYPE);
                result[key] = row;
            }
        }


        private static WhereCondition GetWhereCondition(IList<DocumentSearchData> data)
        {
            var where = new WhereCondition();
            var documentIds = data.Where(d => !d.IsLink)
                                  .Select(d => d.DocumentID)
                                  .ToList();

            if (documentIds.Any())
            {
                where.Where(standardWhere => standardWhere.WhereIn("DocumentID", documentIds)
                                                          .WhereNull("NodeLinkedNodeID"));
            }

            var links = data.Where(d => d.IsLink)
                            .ToList();

            foreach (var link in links)
            {
                var linkClosure = link;
                where.Or().Where(linkWhere => linkWhere.WhereEquals("DocumentID", linkClosure.DocumentID)
                                                       .WhereEquals("NodeID", linkClosure.NodeID));
            }

            return where;
        }


        /// <summary>
        /// Returns the data class info for the given search document
        /// </summary>
        /// <param name="obj">Document object</param>
        public override DataClassInfo GetDataClassInfo(ILuceneSearchDocument obj)
        {
            return DataClassInfoProvider.GetDataClassInfo(Convert.ToString(obj.Get("classname")));
        }


        /// <summary>
        /// Fills the data to the search result data row
        /// </summary>
        /// <param name="resultItem">Result item</param>
        /// <param name="resultDr">Result data row</param>
        /// <param name="doc">Found search document</param>
        public override bool FillSearchResult(SearchResultItem resultItem, DataRow resultDr, ILuceneSearchDocument doc)
        {
            var result = base.FillSearchResult(resultItem, resultDr, doc);
            if (result)
            {
                resultItem.DocumentExtensions = DataHelper.GetStringValue(resultDr, "DocumentExtensions");

                ValidatePreviewImage(resultItem, resultDr);

                var dci = GetDataClassInfo(doc);
                if (String.Equals(dci.ClassSearchContentColumn, "documentcontent", StringComparison.InvariantCultureIgnoreCase))
                {
                    var content = DataHelper.GetDataRowValue(resultDr, dci.ClassSearchContentColumn) as string;

                    if (!string.IsNullOrEmpty(content))
                    {
                        var editableContentProvider = new SearchEditableContentProvider(content);
                        resultItem.Content = editableContentProvider.GetSearchContent();
                    }
                }

                return true;
            }

            return false;
        }


        private static void ValidatePreviewImage(SearchResultItem resultItem, DataRow resultDr)
        {
            var siteId = ValidationHelper.GetInteger(resultDr["NodeSiteID"], 0);
            var attachmentGuid = ValidationHelper.GetGuid(resultItem.Image, Guid.Empty);

            if (attachmentGuid == Guid.Empty || siteId <= 0)
            {
                return;
            }

            var attachmentExtension = GetPreviewAttachmentExtension(siteId, attachmentGuid);

            if (string.IsNullOrEmpty(attachmentExtension))
            {
                return;
            }

            if (!ImageHelper.IsImage(attachmentExtension))
            {
                resultItem.Image = null;
            }
        }


        private static string GetPreviewAttachmentExtension(int siteId, Guid attachmentGuid)
        {
            return AttachmentInfoProvider.GetAttachments()
                .OnSite(siteId)
                .WithGuid(attachmentGuid)
                .BinaryData(false)
                .Column("AttachmentExtension")
                .TopN(1)
                .GetScalarResult<string>();
        }


        /// <summary>
        /// Checks if given <paramref name="className"/> is related to search index settings from <paramref name="indexSettings"/>.
        /// </summary>
        /// <param name="className">Class name</param>
        /// <param name="indexSettings">Search index settings</param>
        public override bool IsClassNameRelevantToIndex(string className, SearchIndexSettings indexSettings)
        {
            // Check main class names which are relevant to all page's indexes
            if (new[] { "cms.tree", TreeNode.OBJECT_TYPE }.Contains(className, StringComparer.InvariantCultureIgnoreCase))
            {
                return true;
            }

            // Check search settings
            foreach (var classNames in indexSettings.Items.Values
                                        .Where(item => item.Type == SearchIndexSettingsInfo.TYPE_ALLOWED)
                                        .Select(item => item.ClassNames))
            {
                if (!String.IsNullOrEmpty(classNames))
                {
                    // Find given class in search settings
                    return classNames.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Any(pageType => pageType.Equals(className, StringComparison.InvariantCultureIgnoreCase));
                }

                // Empty classNames field means all page types are related to given index
                return true;
            }

            return false;
        }


        /// <summary>
        /// Returns URL to given search result item.
        /// </summary>
        /// <param name="resultItem">Search result item for which to return an URL.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="resultItem"/> is null.</exception>
        public override string GetSearchImageUrl(SearchResultItem resultItem)
        {
            if (resultItem == null)
            {
                throw new ArgumentNullException(nameof(resultItem));
            }

            // Get base image URL
            var result = base.GetSearchImageUrl(resultItem);
            if (IsAbsoluteUrlOrVirtualPath(result))
            {
                return AdministrationUrlHelper.ResolveImageUrl(result);
            }

            // No image, no result
            if (String.IsNullOrEmpty(resultItem.Image))
            {
                return null;
            }

            // Attachment guid
            var guid = ValidationHelper.GetGuid(resultItem.Image, Guid.Empty);
            if (guid != Guid.Empty)
            {
                result = AttachmentURLProvider.GetAttachmentUrl(guid, "preview_image", null, 0, "getimage");
            }
            // Node alias path
            else if (resultItem.Image.StartsWith("/", StringComparison.Ordinal))
            {
                string siteName = ValidationHelper.GetString(resultItem.GetSearchValue("SiteName"), String.Empty);

                result = DocumentURLProvider.GetUrl(resultItem.Image, null, siteName);
            }

            return result;
        }


        /// <summary>
        /// Gets the collection of search fields. When no SearchFields colection is provided, new is created.
        /// </summary>
        /// <param name="index">Search index</param>
        /// <param name="searchFields">Search fields collection</param>
        public override ISearchFields GetSearchFields(SearchIndexInfo index, ISearchFields searchFields = null)
        {
            return TreeNode.New().GetSearchFields(index, searchFields);
        }


        /// <summary>
        /// Determines whether the given string is an absolute URL or virtual path.
        /// </summary>
        /// <param name="value">Text to evaluate</param>
        private static bool IsAbsoluteUrlOrVirtualPath(string value)
        {
            if (String.IsNullOrEmpty(value))
            {
                return false;
            }

            Uri result;
            return value.StartsWith("~/", StringComparison.Ordinal) || Uri.TryCreate(value, UriKind.Absolute, out result);
        }


        /// <summary>
        /// Container for document search data.
        /// </summary>
        private class DocumentSearchData
        {
            public string ClassName
            {
                get;
                set;
            }


            public int NodeID
            {
                get;
                set;
            }


            public int DocumentID
            {
                get;
                set;
            }


            public bool IsLink
            {
                get;
                set;
            }
        }
    }
}
