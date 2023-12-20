using System;
using System.Collections.Generic;
using System.Data;

using CMS.Base;
using CMS.DataEngine;
using CMS.Forums.Internal;
using CMS.Helpers;
using CMS.Membership;
using CMS.Search;
using CMS.SiteProvider;

namespace CMS.Forums
{
    /// <summary>
    /// Search indexer for forums
    /// </summary>
    public class ForumSearchIndexer : SearchIndexer
    {
        /// <summary>
        /// Forum search indexer.
        /// </summary>
        public ForumSearchIndexer()
            : base(new ForumSearchablesRetriever())
        { }


        /// <summary>
        /// Rebuilds the forum index
        /// </summary>
        /// <remarks>This method needs to be run in a thread safe way such as smart search task queue.</remarks>
        /// <param name="indexInfo">Search index</param>
        public override void Rebuild(SearchIndexInfo indexInfo)
        {
            // Check whether exist index settings
            if (indexInfo?.IndexSettings?.Items?.Count > 0)
            {
                #region "Prepare indexing options"

                // Get item keys
                Guid[] keys = null;
                lock (indexInfo)
                {
                    keys = new Guid[indexInfo.IndexSettings.Items.Count];
                    indexInfo.IndexSettings.Items.Keys.CopyTo(keys, 0);
                }

                var allowedForums = new List<int>();
                var excludedForums = new List<int>();

                var siteIds = new List<int>();

                // Loop thru all index settings
                foreach (Guid key in keys)
                {
                    // Get settings info
                    SearchIndexSettingsInfo sis = indexInfo.IndexSettings.Items[key];

                    // Check all index sites
                    if (String.IsNullOrEmpty(sis.SiteName))
                    {
                        // Check all index sites
                        var siteBindings = SearchIndexSiteInfoProvider.GetIndexSiteBindings(indexInfo.IndexID).Column("IndexSiteID");

                        foreach (SearchIndexSiteInfo siteBinding in siteBindings)
                        {
                            siteIds.Add(siteBinding.IndexSiteID);
                        }

                        continue;
                    }

                    // Check whether site exists ang get site id
                    SiteInfo si = SiteInfoProvider.GetSiteInfo(sis.SiteName);
                    if (si == null)
                    {
                        continue;
                    }

                    // Check all forums value
                    if (String.IsNullOrEmpty(sis.ForumNames))
                    {
                        siteIds.Add(si.SiteID);
                        continue;
                    }

                    string[] forumNames = sis.ForumNames.Split(';');

                    // Process all forums
                    if (forumNames.Length > 0)
                    {
                        foreach (string name in forumNames)
                        {
                            // Get particular forum by its ID
                            var forumIds = ForumInfoProvider.GetForumIdsByForumName(name, si.SiteID);
                            if (forumIds != null)
                            {
                                foreach (int forumId in forumIds)
                                {
                                    // Register the forum
                                    if (sis.Type == SearchIndexSettingsInfo.TYPE_ALLOWED)
                                    {
                                        allowedForums.Add(forumId);
                                    }
                                    else if (sis.Type == SearchIndexSettingsInfo.TYPE_EXLUDED)
                                    {
                                        excludedForums.Add(forumId);
                                    }
                                }
                            }
                        }
                    }
                }

                #endregion

                #region "Indexing"

                if ((allowedForums.Count > 0) || (siteIds.Count > 0))
                {
                    // Get current index writer
                    var iw = indexInfo.Provider.GetWriter(true);

                    try
                    {
                        if (iw != null)
                        {
                            // Starts first iteration
                            bool start = true;
                            // Last item id
                            int lastId = 0;
                            // Number of batch
                            int batchNumber = 0;
                            // Number of processed items
                            int itemsProcessed = 0;

                            // Create where condition
                            using (var where = new WhereCondition())
                            {
                                #region "Where condition"

                                // All forums in site
                                if (siteIds.Count > 0)
                                {
                                    where.WhereIn("ForumSiteID", siteIds);
                                }

                                // Allowed forums
                                if (allowedForums.Count > 0)
                                {
                                    where.Or().WhereIn("PostForumID", allowedForums);
                                }

                                // Excluded forums
                                if (excludedForums.Count > 0)
                                {
                                    where.WhereNotIn("PostForumID", excludedForums);
                                }

                                where.WhereTrue("PostApproved");

                                #endregion

                                // Documents collection
                                List<SearchDocument> documents = null;

                                // Get forum iDocuments
                                while (((documents != null) && (documents.Count == indexInfo.IndexBatchSize)) || start)
                                {
                                    start = false;
                                    batchNumber++;
                                    Logger.LogBatchStart(batchNumber, lastId, ForumInfo.TYPEINFO.ObjectClassName);

                                    // Get list of documents
                                    documents = ForumInfoProvider.GetSearchDocuments(indexInfo, where, lastId);

                                    if ((documents != null) && (documents.Count > 0))
                                    {
                                        // Get last ID  (document id)
                                        lastId = ValidationHelper.GetInteger(documents[documents.Count - 1].GetValue(SearchFieldsConstants.ID), 1);

                                        // Loop thru all iDocuments
                                        foreach (var doc in documents)
                                        {
                                            var luceneDocument = LuceneSearchDocumentHelper.ToLuceneSearchDocument(doc);
                                            iw.AddDocument(luceneDocument);
                                        }

                                        iw.Flush();

                                        itemsProcessed += documents.Count;
                                    }

                                    Logger.LogBatchEnd(itemsProcessed);
                                }
                            }
                        }

                        // Optimize index
                        SearchIndexInfoProvider.SetIndexStatus(indexInfo, IndexStatusEnum.OPTIMIZING);
                        iw.Optimize();
                    }
                    catch (Exception)
                    {
                        // Set statuses to error
                        SearchIndexInfoProvider.SetIndexStatus(indexInfo, IndexStatusEnum.ERROR);
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

                SearchHelper.FinishRebuild(indexInfo);
            }
        }


        /// <summary>
        /// Selects the search document based on the given ID
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <returns>Enumeration of searchable objects or empty enumeration.</returns>
        public override IEnumerable<ISearchable> SelectSearchDocument(int documentId)
        {
            List<ISearchable> posts = new List<ISearchable>();

            ForumPostInfo fopi = ForumPostInfoProvider.GetForumPostInfo(documentId);
            if (fopi != null)
            {
                posts.Add(fopi);
            }

            return posts;
        }


        /// <summary>
        /// Checks the permissions for the given result document
        /// </summary>
        /// <param name="settings">Check permission settings</param>
        /// <param name="currentDoc">Current result document</param>
        /// <param name="index">Current document index</param>
        public override bool CheckResultPermissions(SearchResults settings, ILuceneSearchDocument currentDoc, int index)
        {
            // Call filter method
            int forumFilterResult = ForumPostInfoProvider.FilterSearchResults(currentDoc, (UserInfo)settings.User);

            // Add document to the final collection or increase number of denied documents
            var allow = (forumFilterResult <= 0);

            return allow;
        }


        /// <summary>
        /// Loads the results to the given result collection
        /// </summary>
        /// <param name="results">Collection of results to load</param>
        /// <param name="result">Dictionary of the loaded results indexed by their key</param>
        public override void LoadResults(IEnumerable<ILuceneSearchDocument> results, SafeDictionary<string, DataRow> result)
        {
            var forumPosts = new List<string>();

            // Load forum posts
            foreach (var filteredDocument in results)
            {
                forumPosts.Add(ValidationHelper.GetInteger(filteredDocument.Get(SearchFieldsConstants.ID), 0).ToString());
            }

            if (forumPosts.Count > 0)
            {
                string where = " PostApproved = 1 AND PostID IN (" + String.Join(",", forumPosts.ToArray()) + ")";

                var srchData = ForumPostInfoProvider.Search(where, null, 0, null);
                if (!DataHelper.DataSourceIsEmpty(srchData))
                {
                    // Loop thru all results and add all datarows to the result collection (ID_ObjectType)
                    foreach (DataRow dr in srchData.Tables[0].Rows)
                    {
                        result[dr["PostID"] + "_" + ForumInfo.OBJECT_TYPE] = dr;
                    }
                }
            }
        }


        /// <summary>
        /// Returns the data class info for the given search document
        /// </summary>
        /// <param name="obj">Document object</param>
        public override DataClassInfo GetDataClassInfo(ILuceneSearchDocument obj)
        {
            return DataClassInfoProvider.GetDataClassInfo("forums.forumpost");
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

            return AvatarInfoProvider.GetUserAvatarImageUrl(resultItem.GetSearchValue("AvatarID"), resultItem.GetSearchValue("PostUserID"), ValidationHelper.GetString(resultItem.GetSearchValue("PostUserMail"), String.Empty), 0, 0, 0);
        }


        /// <summary>
        /// Gets the collection of search fields. When no SearchFields colection is provided, new is created.
        /// </summary>
        /// <param name="index">Search index</param>
        /// <param name="searchFields">Search fields collection</param>
        public override ISearchFields GetSearchFields(SearchIndexInfo index, ISearchFields searchFields = null)
        {
            return ForumPostInfo.New().GetSearchFields(index, searchFields);
        }


        /// <summary>
        /// Checks if given <paramref name="className"/> is related to search index settings from <paramref name="indexSettings"/>.
        /// </summary>
        /// <param name="className">Class name</param>
        /// <param name="indexSettings">Search index settings</param>
        public override bool IsClassNameRelevantToIndex(string className, SearchIndexSettings indexSettings)
        {
            // Only forum post class is related to Forum indexes
            return String.Equals(className, ForumPostInfo.OBJECT_TYPE, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
