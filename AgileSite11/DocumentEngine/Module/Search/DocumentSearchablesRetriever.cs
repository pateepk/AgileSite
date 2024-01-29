using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Localization;
using CMS.Search;
using CMS.Search.Internal;
using CMS.SiteProvider;

namespace CMS.DocumentEngine.Internal
{
    /// <summary>
    /// Helps with retrieving <see cref="ISearchable"/> objects and <see cref="SearchIndexInfo"/>s
    /// related to document search tasks.
    /// </summary>
    /// <seealso cref="SearchablesRetrievers.Register{TRetriever}(string)"/>
    public class DocumentSearchablesRetriever : SearchablesRetriever
    {
        /// <summary>
        /// Returns collection of all <see cref="ISearchable"/> objects for given <paramref name="indexInfo"/>.
        /// </summary>
        /// <param name="indexInfo"><see cref="SearchIndexInfo"/> for which to return collection of <see cref="ISearchable"/>s.</param>
        /// <seealso cref="ISearchable.GetSearchDocument(ISearchIndexInfo)"/>
        public override IEnumerable<ISearchable> GetSearchableObjects(SearchIndexInfo indexInfo)
        {
            List<int> sites;
            List<string> cultures;

            SafeDictionary<string, string> allowedNodes;
            SafeDictionary<string, string> excludedNodes;

            if (PrepareBuildingValues(indexInfo, out sites, out cultures, out allowedNodes, out excludedNodes, null))
            {
                return GetSearchableObjectsInternal(indexInfo, sites.Select(id => SiteInfoProvider.GetSiteInfo(id)).Where(s => s != null), cultures, allowedNodes, excludedNodes);
            }

            return Enumerable.Empty<ISearchable>();
        }


        /// <summary>
        /// Returns dataset of index IDs for specific type.
        /// </summary>
        /// <param name="taskType">Task type.</param>
        /// <param name="searchProvider">
        /// Defines search provider for which to return relevant indexes.
        /// If not defined then indexes for all search providers are returned.
        /// </param>
        /// <seealso cref="SearchIndexInfo.IndexProvider"/>
        public override DataSet GetRelevantIndexes(string taskType, string searchProvider)
        {
            if (!IsDocumentRelatedType(taskType))
            {
                return base.GetRelevantIndexes(taskType, searchProvider);
            }

            var where = GetDocumentRelatedCondition();

            if (!String.IsNullOrEmpty(searchProvider))
            {
                where = new WhereBuilder().AddWhereCondition(where, new WhereCondition().WhereEquals(nameof(SearchIndexInfo.IndexProvider), searchProvider).ToString(true));
            }

            return SearchIndexInfoProvider.GetSearchIndexes().Where(where).Column("IndexID");
        }


        /// <summary>
        /// Gets the list of indexes relevant to the given object.
        /// </summary>
        /// <param name="searchObject">Search object.</param>
        /// <param name="searchProvider">
        /// Defines search provider for which to return relevant indexes.
        /// If not defined then indexes for all search providers are returned.
        /// </param>
        /// <seealso cref="SearchIndexInfo.IndexProvider"/>
        public override List<SearchIndexInfo> GetRelevantIndexes(ISearchable searchObject, string searchProvider)
        {
            List<SearchIndexInfo> relevantIndexes = new List<SearchIndexInfo>();

            // Get TreeNode values
            int classId = ValidationHelper.GetInteger(searchObject.GetValue("nodeclassid"), 0);
            string className = DataClassInfoProvider.GetClassName(classId);
            string path = ValidationHelper.GetString(searchObject.GetValue("nodealiaspath"), String.Empty);
            int siteId = ValidationHelper.GetInteger(searchObject.GetValue("nodesiteid"), 0);
            string cultureCode = ValidationHelper.GetString(searchObject.GetValue("documentculture"), String.Empty);

            // Get site indexes from cache or DB
            List<int> indexes = SearchIndexInfoProvider.GetSiteIndexes(siteId);

            // Prepare array - because of concurrent modification
            int[] indexIDs;
            lock (indexes)
            {
                indexIDs = new int[indexes.Count];
                indexes.CopyTo(indexIDs, 0);
            }

            // Loop trough all indexes
            foreach (int indexId in indexIDs)
            {
                // Get from cache or load from DB
                SearchIndexInfo sii = SearchIndexInfoProvider.GetSearchIndexInfo(indexId);

                // Skip if not found
                if (sii == null || (!String.IsNullOrEmpty(searchProvider) && !sii.IndexProvider.Equals(searchProvider, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                // Default set
                bool isRelevant = false;

                // Get index settings
                SearchIndexSettings indexSettings = sii.IndexSettings;

                // If index is for documents
                if (IsDocumentRelatedType(sii.IndexType))
                {
                    // Get settings mItems
                    Dictionary<Guid, SearchIndexSettingsInfo> settingsItems = indexSettings.Items;

                    // Prepare key array - because of concurrent modification
                    Guid[] itemsKeys;
                    lock (settingsItems)
                    {
                        itemsKeys = new Guid[settingsItems.Count];
                        settingsItems.Keys.CopyTo(itemsKeys, 0);
                    }

                    foreach (Guid itemKey in itemsKeys)
                    {
                        SearchIndexSettingsInfo item = settingsItems[itemKey];

                        if (item == null)
                        {
                            continue;
                        }

                        // Check if class name and path are match, and index is assigned to culture
                        if (item.MatchClassNames(className) && item.MatchPath(path) && SearchIndexInfoProvider.IndexIsInCulture(sii, cultureCode))
                        {
                            if (item.Type == SearchIndexSettingsInfo.TYPE_ALLOWED)
                            {
                                isRelevant = true;
                            }
                            else
                            {
                                // One-time is excluded, always excluded
                                isRelevant = false;
                                break;
                            }
                        }
                    }
                }

                if (isRelevant)
                {
                    relevantIndexes.Add(sii);
                }
            }

            return relevantIndexes;
        }


        /// <summary>
        /// Gets relevant indexes for given <paramref name="indexType"/>, <paramref name="siteName"/> and <paramref name="searchProvider"/>.
        /// </summary>
        /// <param name="indexType">Defines object type that index covers. Indexes with <see cref="SearchIndexInfo.IndexType"/> that equals to this parameter are returned.</param>
        /// <param name="siteName">Only indexes on given site are checked. If not specified then indexes through all sites are checked if they are relevant.</param>
        /// <param name="searchProvider">
        /// Defines search provider for which to return relevant indexes.
        /// If not defined then indexes for all search providers are returned.
        /// </param>
        /// <seealso cref="SearchIndexInfo.IndexProvider"/>
        public override IEnumerable<SearchIndexInfo> GetRelevantIndexes(string indexType, string siteName, string searchProvider)
        {
            string siteWhere = String.Empty;

            // Get the site info
            if (!String.IsNullOrEmpty(siteName))
            {
                // Add limitation for current site
                var si = SiteInfoProvider.GetSiteInfo(siteName);
                if (si != null)
                {
                    siteWhere = " AND IndexID IN (SELECT IndexID FROM CMS_SearchIndexSite WHERE IndexSiteID = " + si.SiteID + ")";
                }
            }

            string where = (IsDocumentRelatedType(indexType)) ? GetDocumentRelatedCondition() : "IndexType = N'" + SqlHelper.EscapeQuotes(indexType) + "'";

            if (!String.IsNullOrEmpty(searchProvider))
            {
                where = new WhereBuilder().AddWhereCondition(where, new WhereCondition(nameof(SearchIndexInfo.IndexProvider), QueryOperator.Equals, searchProvider).ToString(true));
            }

            // Get all indexes for current type
            return SearchIndexInfoProvider.GetSearchIndexes().Where(where + " " + siteWhere).Column("IndexID").TypedResult
                .Select(i => SearchIndexInfoProvider.GetSearchIndexInfo(ValidationHelper.GetInteger(i.IndexID, 0)))
                .Where(i => i != null);
        }


        /// <summary>
        /// Prepares sites, cultures, allowed and excluded collections for partial or full rebuild.
        /// </summary>
        /// <param name="indexInfo">Search index</param>
        /// <param name="sites">List of sites</param>
        /// <param name="cultures">List of cultures</param>
        /// <param name="allowedNodes">List of allowed classes</param>
        /// <param name="excludedNodes">List of excluded classes</param>
        /// <param name="nodeAliasPath">Node alias path. If is specified, all allowed nodes must be beneath this path</param>
        /// <returns>Returns true if values are ready</returns>
        internal bool PrepareBuildingValues(SearchIndexInfo indexInfo, out List<int> sites, out List<string> cultures, out SafeDictionary<string, string> allowedNodes, out SafeDictionary<string, string> excludedNodes, string nodeAliasPath)
        {
            sites = new List<int>();
            cultures = new List<string>();
            allowedNodes = new SafeDictionary<string, string>();
            excludedNodes = new SafeDictionary<string, string>();

            // Check whether exist index settings
            if ((indexInfo == null) || (indexInfo.IndexSettings == null) || (indexInfo.IndexSettings.Items == null) || (indexInfo.IndexSettings.Items.Count <= 0))
            {
                return true;
            }

            #region "Sites/Cultures"

            // Get index sites
            var indexSites = SearchIndexSiteInfoProvider.GetIndexSiteBindings(indexInfo.IndexID)
                                                          .Column("IndexSiteID")
                                                          .GetListResult<int>();
            sites.AddRange(indexSites);

            // Get index cultures
            var indexCulturesData = SearchIndexCultureInfoProvider.GetSearchIndexCultures("IndexID = " + indexInfo.IndexID, null, 0, "CultureCode");
            if (!DataHelper.DataSourceIsEmpty(indexCulturesData))
            {
                var indexCultures = indexCulturesData.Tables[0]
                                                     .AsEnumerable()
                                                     .Select(f => f.Field<string>("CultureCode"));
                cultures.AddRange(indexCultures);
            }

            // At least one culture and one site must exist
            if ((sites.Count == 0) || (cultures.Count == 0))
            {
                LogContext.LogEventToCurrent(EventType.WARNING, "Smart search", "INDEXERROR", "Site or culture missing for index " + indexInfo.IndexName, null, 0, null, 0, null, null, 0, null, null, null, DateTime.Now);

                // Set error status
                SearchIndexInfoProvider.SetIndexStatus(indexInfo, IndexStatusEnum.ERROR);

                return false;
            }

            #endregion

            #region "Class table generation"

            // Get item keys
            Guid[] keys;
            lock (indexInfo)
            {
                keys = new Guid[indexInfo.IndexSettings.Items.Count];
                indexInfo.IndexSettings.Items.Keys.CopyTo(keys, 0);
            }

            // Loop thru all index settings
            foreach (Guid key in keys)
            {
                // Get settings info
                var sis = indexInfo.IndexSettings.Items[key];

                // Get all classes
                string[] classes = null;

                // Check whether class names contain macro for all class names
                if (String.IsNullOrEmpty(sis.ClassNames))
                {
                    // Get all document types
                    DataSet docTypeClasses = ConnectionHelper.ExecuteQuery("cms.class.selectall", null, "(ClassIsDocumentType = 1)", null, 0, "ClassName");
                    if (!DataHelper.DataSourceIsEmpty(docTypeClasses))
                    {
                        // Prepare classes array
                        classes = new string[docTypeClasses.Tables[0].Rows.Count];
                        // Initialize counter
                        int counter = 0;
                        // Loop thru all classes
                        foreach (DataRow dr in docTypeClasses.Tables[0].Rows)
                        {
                            // Add current class to the class array
                            classes[counter] = Convert.ToString(dr["Classname"]);
                            counter++;
                        }
                    }
                }
                else
                {
                    // If class name field doesn't contain all macro, use simple split by semicolon
                    classes = sis.ClassNames.Split(';');
                }

                // Indicates whether node alias path is specified
                bool nodeAliasExists = !String.IsNullOrEmpty(nodeAliasPath);

                // Prepare node alias path
                if (nodeAliasExists)
                {
                    nodeAliasPath = nodeAliasPath.TrimEnd('%').TrimEnd('/') + "/";
                }

                if (classes == null)
                {
                    continue;
                }

                // Loop thru all classes
                foreach (string className in classes)
                {
                    // Check whether class is defined 
                    if (String.IsNullOrEmpty(className))
                    {
                        continue;
                    }

                    // Check whether data class exists
                    var dci = DataClassInfoProvider.GetDataClassInfo(className);
                    if (dci == null)
                    {
                        continue;
                    }

                    var classKey = className.ToLowerInvariant();
                    // Create collection with allowed end excluded classes
                    if (sis.Type == SearchIndexSettingsInfo.TYPE_ALLOWED)
                    {
                        // Get the path
                        string singlePath = null;
                        string path = ValidationHelper.GetString(sis.Path, String.Empty);
                        // Indicates whether current path should be added to the final collection
                        bool addToCollection = dci.ClassSearchEnabled;

                        // If node alias path if specified, check whether current path is beneath the node alias path
                        if (nodeAliasExists)
                        {
                            addToCollection = addToCollection && nodeAliasPath.StartsWith(path.TrimEnd('%'), StringComparison.InvariantCultureIgnoreCase);
                            singlePath = SqlHelper.EscapeLikeQueryPatterns(nodeAliasPath.TrimEnd('/'));
                            path = nodeAliasPath + "%";
                        }

                        // Add to the final collection
                        if (addToCollection)
                        {
                            if (!string.IsNullOrEmpty(singlePath))
                            {
                                allowedNodes[classKey] = allowedNodes[classKey] + ";" + SqlHelper.EscapeQuotes(singlePath);
                            }
                            allowedNodes[classKey] = allowedNodes[classKey] + ";" + SqlHelper.EscapeQuotes(path);
                        }
                    }
                    else
                    {
                        excludedNodes[classKey] = excludedNodes[classKey] + ";" + ValidationHelper.GetString(sis.Path, String.Empty).Replace("'", "''");
                    }
                }
            }

            #endregion

            return true;
        }


        /// <summary>
        /// Returns <see cref="ISearchable"/> objects for given <paramref name="nodeAliasPath"/>.
        /// </summary>
        /// <param name="index">Index for which to find <see cref="ISearchable"/> objects.</param>
        /// <param name="nodeAliasPath">Node alias path for which to find <see cref="ISearchable"/> objects.</param>
        /// <param name="siteName">Site for which to find <see cref="ISearchable"/> objects. By default are used sites from <paramref name="index"/>'s settings.</param>
        internal IEnumerable<ISearchable> GetSearchablesObjects(SearchIndexInfo index, string nodeAliasPath, string siteName = null)
        {
            List<int> sites;
            List<string> cultures;

            SafeDictionary<string, string> allowedNodes;
            SafeDictionary<string, string> excludedNodes;

            //Check whether exist index settings
            if (index?.IndexSettings?.Items?.Count > 0 &&
                PrepareBuildingValues(index, out sites, out cultures, out allowedNodes, out excludedNodes, nodeAliasPath))
            {
                // Override sites if site is specified
                if (siteName != null)
                {
                    var site = SiteInfoProvider.GetSiteInfo(siteName);
                    if (site != null)
                    {
                        sites.Clear();
                        return GetSearchableObjectsInternal(index, new SiteInfo[] { site }, cultures, allowedNodes, excludedNodes);
                    }
                }

                return GetSearchableObjectsInternal(index, ToSiteInfoEnumerable(sites), cultures, allowedNodes, excludedNodes);
            }

            return Enumerable.Empty<ISearchable>();
        }


        /// <summary>
        /// Gets enumeration of existing <see cref="SiteInfo"/> objects for enumeration of <see cref="SiteInfo.SiteID"/>.
        /// </summary>
        internal IEnumerable<SiteInfo> ToSiteInfoEnumerable(IEnumerable<int> siteIDs)
        {
            return siteIDs.Select(id => SiteInfoProvider.GetSiteInfo(id)).Where(s => s != null);
        }


        /// <summary>
        /// Returns collection of <see cref="ISearchable"/>s for given <paramref name="indexInfo"/> and cms.document parameters.
        /// </summary>
        internal IEnumerable<ISearchable> GetSearchableObjectsInternal(SearchIndexInfo indexInfo, IEnumerable<SiteInfo> sites, IEnumerable<string> cultures, SafeDictionary<string, string> allowedNodes, SafeDictionary<string, string> excludedNodes)
        {
            var documentsParameters = new GetSearchDocumentsParams(cultures, indexInfo.IndexBatchSize);

            foreach (var site in sites)
            {
                // Set current site name
                documentsParameters.SiteName = site.SiteName;

                foreach (string currentClass in allowedNodes.Keys)
                {
                    // Class name
                    documentsParameters.ClassNames = currentClass;
                    documentsParameters.Allowed = allowedNodes[currentClass];
                    documentsParameters.Excluded = excludedNodes[currentClass];

                    // Starts first iteration
                    bool start = true;
                    // Last item id
                    int lastId = 0;
                    // Batch number
                    int batchNumber = 0;
                    // Number of processed items
                    int itemsProcessed = 0;

                    // Documents collection
                    List<ISearchable> searchableObjects = null;

                    // Indicates whether linked nodes should be loaded
                    bool linkedNodes = false;

                    while (((searchableObjects != null) && (searchableObjects.Any())) || start)
                    {
                        // Log start of batch
                        start = false;
                        batchNumber++;
                        LogBatchStart(batchNumber, lastId, currentClass, site.DisplayName, linkedNodes);

                        // Set flag, whether only linked nodes should be loaded
                        documentsParameters.LinkedNodes = linkedNodes;

                        // Get list of documents
                        searchableObjects = GetSearchDocuments(indexInfo, documentsParameters, ref lastId);

                        if (searchableObjects != null && searchableObjects.Any())
                        {
                            var searchId = ValidationHelper.GetString(searchableObjects.Last().GetSearchID(), String.Empty);
                            var ids = searchId.Split(';');
                            var identifiers = new
                            {
                                DocumentID = ids[0],
                                NodeID = ids[1]
                            };

                            // Get relevant identifier
                            var identifier = linkedNodes ? identifiers.NodeID : identifiers.DocumentID;
                            lastId = ValidationHelper.GetInteger(identifier, 1);

                            foreach (var searchableObj in searchableObjects)
                            {
                                yield return searchableObj;
                            }

                            itemsProcessed += searchableObjects.Count;
                        }

                        // Log end of batch
                        Logger.LogBatchEnd(itemsProcessed);

                        if ((searchableObjects == null) || (!searchableObjects.Any()))
                        {
                            lastId = 1;
                            searchableObjects = null;

                            if (!linkedNodes)
                            {
                                linkedNodes = true;
                                start = true;
                                lastId = 0;
                                batchNumber = 0;
                                itemsProcessed = 0;
                            }
                        }
                    }
                }
            }
        }


        #region "GetSearchDocumentsParams"

        /// <summary>
        /// Get search document parameters
        /// </summary>
        private class GetSearchDocumentsParams
        {
            public bool LinkedNodes
            {
                get;
                set;
            }

            public string Excluded
            {
                get;
                set;
            }

            public string Allowed
            {
                get;
                set;
            }

            public List<string> CultureCodes
            {
                get;
                private set;
            }

            public string SiteName
            {
                get;
                set;
            }

            public string ClassNames
            {
                get;
                set;
            }

            public int BatchSize
            {
                get;
                private set;
            }


            public GetSearchDocumentsParams(IEnumerable<string> cultures, int batchSize)
            {
                CultureCodes = new List<string>(cultures);
                BatchSize = batchSize;
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Returns list of <see cref="ISearchable"/> objects (documents) for given index and document parameters
        /// </summary>
        /// <param name="indexInfo">Search index</param>
        /// <param name="documentsParameters">Search parameters</param>
        /// <param name="lastId">ID of last processed document</param>
        private static List<ISearchable> GetSearchDocuments(SearchIndexInfo indexInfo, GetSearchDocumentsParams documentsParameters, ref int lastId)
        {
            var orderBy = documentsParameters.LinkedNodes ? "NodeID" : "DocumentID";
            var where = GetDocumentsWhereCondition(documentsParameters, lastId);

            var tree = new TreeProvider();
            using (var data = tree.SelectNodes(documentsParameters.SiteName, null, TreeProvider.ALL_CULTURES, false, documentsParameters.ClassNames, where, orderBy, -1, false, documentsParameters.BatchSize))
            {
                if (DataHelper.DataSourceIsEmpty(data))
                {
                    return null;
                }

                int currentLastId = 0;

                // Process all documents
                var searchableObjects = new List<ISearchable>();
                foreach (DataRow dr in data.Tables[0].Rows)
                {
                    // Add the document to the list
                    string className = documentsParameters.ClassNames;
                    using (TreeNode tn = TreeNode.New(className, dr))
                    {
                        tn.TreeProvider = tree;

                        if (tn.PublishedVersionExists)
                        {
                            try
                            {
                                searchableObjects.Add(tn);
                            }
                            catch (Exception ex)
                            {
                                // Do not log WebException for crawler indexes
                                bool logException = (indexInfo.IndexType != SearchHelper.DOCUMENTS_CRAWLER_INDEX) || !(ex is System.Net.WebException);

                                if (logException)
                                {
                                    string errorDescription = String.Format("Indexing page '{0}' of index '{1}' ended with an exception. Page is skipped.\nOriginal exception:\n{2}", tn.GetDocumentName(), indexInfo.IndexName, EventLogProvider.GetExceptionLogMessage(ex));
                                    EventLogProvider.LogEvent(EventType.ERROR, "Smart search", "REBUILD", errorDescription);
                                }
                            }
                        }


                        // Get current last id for empty final collection
                        currentLastId = documentsParameters.LinkedNodes ? tn.NodeID : tn.DocumentID;
                    }
                }

                // Return the last ID to the caller
                lastId = currentLastId;

                return searchableObjects;
            }
        }


        private static string GetDocumentsWhereCondition(GetSearchDocumentsParams documentsParameters, int lastId)
        {
            string where = null;

            // Check whether load linked node or not
            if (!documentsParameters.LinkedNodes)
            {
                where += " (DocumentID > " + lastId + ") AND NodeLinkedNodeID IS NULL";
            }
            else
            {
                where += " (NodeID > " + lastId + ") AND NodeLinkedNodeID IS NOT NULL";
            }

            // Add the condition to filter out documents excluded from search
            where = SqlHelper.AddWhereCondition(where, "(DocumentSearchExcluded IS NULL) OR (DocumentSearchExcluded = 0)");

            // Excluded documents
            string excluded = documentsParameters.Excluded;
            if (!String.IsNullOrEmpty(excluded))
            {
                excluded = excluded.TrimStart(';').Replace("'", "''");
                string[] paths = excluded.Split(';');

                var excludedWhere = new StringBuilder();
                foreach (string path in paths)
                {
                    if (excludedWhere.Length > 0)
                    {
                        excludedWhere.Append(" AND ");
                    }
                    excludedWhere.Append("(NodeAliasPath NOT LIKE N'" + path + "') ");
                }

                where += " AND (" + excludedWhere + ")";
            }

            // Allowed documents
            string allowed = documentsParameters.Allowed;
            if (!String.IsNullOrEmpty(allowed))
            {
                allowed = allowed.TrimStart(';').Replace("'", "''");
                string[] paths = allowed.Split(';');

                string allowedWhere = String.Empty;
                foreach (string path in paths)
                {
                    if (!String.IsNullOrEmpty(allowedWhere))
                    {
                        allowedWhere += " OR ";
                    }
                    allowedWhere += "(NodeAliasPath LIKE N'" + path + "')";
                }

                where += " AND (" + allowedWhere + ")";
            }

            // Document culture
            where += " AND (DocumentCulture IN (" + string.Join(", ", documentsParameters.CultureCodes.Select(culture => "N'" + culture.Replace("'", "''") + "'")) + "))";

            // Document type search enabled
            where += " AND (NodeClassID IN (SELECT ClassID FROM CMS_Class WHERE ClassSearchEnabled = 1))";
            return where;
        }


        /// <summary>
        /// Returns true of task type is related to document object
        /// </summary>
        /// <param name="taskType">Task type</param>
        private bool IsDocumentRelatedType(string taskType)
        {
            return string.Equals(taskType, PredefinedObjectType.DOCUMENT, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(taskType, SearchHelper.DOCUMENTS_CRAWLER_INDEX, StringComparison.OrdinalIgnoreCase);
        }


        /// <summary>
        /// Returns the condition for all index types related to documents
        /// </summary>
        private string GetDocumentRelatedCondition()
        {
            return new WhereCondition()
                .WhereIn("IndexType", new[] { PredefinedObjectType.DOCUMENT, SearchHelper.DOCUMENTS_CRAWLER_INDEX })
                .ToString(true);
        }


        /// <summary>
        /// Logs the start of batch processing. 
        /// </summary>
        /// <param name="batchNumber">Number of batch</param>
        /// <param name="lastID">ID of last item that is not present in the batch</param>
        /// <param name="documentTypeCodeName">Code name of the document type</param>
        /// <param name="siteDisplayName">Display name of the site</param>
        /// <param name="linkedNodes">Indicates whether standard documents or linked documents are processed in the batch</param>
        private void LogBatchStart(int batchNumber, int lastID, string documentTypeCodeName, string siteDisplayName, bool linkedNodes)
        {
            if (batchNumber == 1)
            {
                var documentType = DataClassInfoProvider.GetDataClassInfo(documentTypeCodeName);
                var documentTypeDisplayName = documentType != null ? ResHelper.LocalizeString(documentType.ClassDisplayName) : documentTypeCodeName;
                var message = linkedNodes ? "smartsearch.taskbatch.startdocumentlinked" : "smartsearch.taskbatch.startdocument";
                Logger.LogMessage(LocalizationHelper.GetStringFormat(message, documentTypeDisplayName, siteDisplayName));
            }
            else
            {
                Logger.LogBatchStart(batchNumber, lastID);
            }
        }

        #endregion
    }
}
