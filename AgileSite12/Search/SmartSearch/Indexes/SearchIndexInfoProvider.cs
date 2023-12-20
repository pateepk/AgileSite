using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;
using CMS.IO;
using CMS.SiteProvider;
using CMS.Search.Internal;

namespace CMS.Search
{
    using IndexCultureTable = SafeDictionary<string, bool?>;
    using SiteIndexesTable = SafeDictionary<int, List<int>>;
    using FlatIndexesTable = SafeDictionary<string, Dictionary<string, List<int>>>;

    using ClassList = List<string>;

    /// <summary>
    /// Class providing SearchIndexInfo management.
    /// </summary>
    public class SearchIndexInfoProvider : AbstractInfoProvider<SearchIndexInfo, SearchIndexInfoProvider>
    {
        #region "Constants"

        internal const string CLEAR_INDEXES_TASK = "CLEARSEARCHINDEXES";
        internal const string CLEAR_INDEX_TASK = "CLEARSEARCHINDEX";
        internal const string SEARCH_INDEX_SITE_CACHE_TASK = "SEARCHINDEXSITECACHE";
        internal const string SEARCH_INDEX_CULTURE_CACHE_TASK = "SEARCHINDEXCULTURECACHE";
        internal const string INVALIDATE_INDEX_IDS_TASK = "INVALIDATEINDEXIDS";

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public SearchIndexInfoProvider()
            : base(SearchIndexInfo.TYPEINFO, new HashtableSettings
            {
                ID = true,
                Name = true,
                Load = LoadHashtableEnum.All
            })
        {
        }

        #endregion


        #region "Variables & caches"

        // List of class names which have a general search index set.
        private static readonly CMSStatic<ClassList> mIndexedClasses = new CMSStatic<ClassList>();

        // Cache containing information whether search index belongs to culture, key is indexid_culturecode. [indexId + "_" + cultureCode] -> [Bool]
        private static readonly CMSStatic<IndexCultureTable> mCachedIndexCulture = new CMSStatic<IndexCultureTable>(() => new IndexCultureTable());

        // Cache containing information about indexes assigned to sites. Key - siteId, Value - List(indexIds). [siteId -> List[int]]
        private static readonly CMSStatic<SiteIndexesTable> mSiteIndexes = new CMSStatic<SiteIndexesTable>(() => new SiteIndexesTable());

        // Collection of ids of indexes.
        private static readonly CMSStatic<FlatIndexesTable> mFlatIndexes = new CMSStatic<FlatIndexesTable>(() => new FlatIndexesTable());

        // Collection of index types which are handled using general indexer.
        private static readonly List<string> mGeneralIndexTypeList = new List<string> { SearchHelper.GENERALINDEX };

        // Flag indicating whether smart search indexing is enabled, by default loaded from settings.
        private static readonly CMSLazy<bool> mSearchEnabled = new CMSLazy<bool>(() => SettingsKeyInfoProvider.GetBoolValue("CMSSearchIndexingEnabled"));

        // Table lock for loading.
        private static readonly object tableLock = new object();

        // Lock for manipulation with ObjectType - IndexIDs cache
        private static readonly object flatIndexesLock = new object();

        #endregion


        #region "Public properties"

        /// <summary>
        /// List of class names which have a general search index set.
        /// </summary>
        private static ClassList IndexedClasses
        {
            get
            {
                return mIndexedClasses;
            }
            set
            {
                mIndexedClasses.Value = value;
            }
        }


        /// <summary>
        /// Collection of index types which are handled using general indexer.
        /// </summary>
        public static List<string> GeneralIndexTypeList
        {
            get
            {
                return mGeneralIndexTypeList;
            }
        }


        /// <summary>
        /// Cache containing information whether search index belongs to culture, key is indexid_culturecode. [indexId + "_" + cultureCode] -> [Bool]
        /// </summary>
        internal static IndexCultureTable CachedIndexCulture
        {
            get
            {
                return mCachedIndexCulture;
            }
        }


        /// <summary>
        /// Cache containing information about indexes assigned to sites. Key - siteId, Value - List(indexIds). [siteId -> List[int]]
        /// </summary>
        internal static SiteIndexesTable SiteIndexes
        {
            get
            {
                return mSiteIndexes;
            }
        }


        /// <summary>
        /// Returns hashtable of flat indexes.
        /// </summary>
        internal static FlatIndexesTable FlatIndexes
        {
            get
            {
                return mFlatIndexes;
            }
        }


        /// <summary>
        /// Gets or sets a property which indicates if Smart search is enabled.
        /// </summary>
        public static bool SearchEnabled
        {
            get
            {
                return mSearchEnabled.Value;
            }
            set
            {
                mSearchEnabled.Value = value;
            }
        }

        #endregion


        #region "Public static methods"


        /// <summary>
        /// Invalidates index ids collection for specified object type.
        /// </summary>
        /// <param name="type">Object type name</param>
        public static void InvalidateIndexIDs(string type)
        {
            InvalidateIndexIDs(type, true);
        }


        /// <summary>
        /// Invalidates index ids collection for specified object type.
        /// </summary>
        /// <param name="type">Object type name</param>
        /// <param name="logtask">Indicates whether web farm task should be logged</param>
        public static void InvalidateIndexIDs(string type, bool logtask)
        {
            ProviderObject.InvalidateIndexIDsInternal(type, logtask);
        }


        /// <summary>
        /// Returns list of index ids for specified object type(s) and <paramref name="searchProvider"/>.
        /// </summary>
        /// <param name="typeCodes">Object type names (separated with semicolon)</param>
        /// <param name="searchProvider">
        /// Defines search provider e.g. from <see cref="SearchIndexInfo.IndexProvider"/> for which to return relevant indexes.
        /// If not defined ids for all search providers are returned.
        /// </param>
        public static List<int> GetIndexIDs(List<string> typeCodes, string searchProvider = null)
        {
            if (typeCodes != null)
            {
                string searchProviderCacheKey = String.IsNullOrEmpty(searchProvider) ? "NoSearchProvider" : searchProvider;

                // Check if we have all the data loaded, if not, build the where condition to load the data
                WhereCondition where = null;
                bool load = false;
                lock (flatIndexesLock)
                {
                    foreach (var type in typeCodes)
                    {
                        var typeToLower = type.ToLowerInvariant();

                        // Skip if already loaded for given types and search provider
                        if (FlatIndexes.ContainsKey(typeToLower) && FlatIndexes[typeToLower].ContainsKey(searchProviderCacheKey))
                        {
                            continue;
                        }
                        else if (!FlatIndexes.ContainsKey(typeToLower))
                        {
                            FlatIndexes[typeToLower] = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);
                        }

                        // Make sure we init all the types and search providers with empty lists (needed if no indexes for given type and search provider are found, it remains empty collection, not null)
                        FlatIndexes[typeToLower][searchProviderCacheKey] = new List<int>();

                        // If the given object type was not initialized yet, include it in the result
                        if (where == null)
                        {
                            where = new WhereCondition();
                        }
                        where.Or(new WhereCondition("IndexType", QueryOperator.Equals, type));
                        load = true;
                    }

                    // If some types are not loaded yet, load them
                    if (load)
                    {
                        if (!String.IsNullOrEmpty(searchProvider))
                        {
                            where = where ?? new WhereCondition();

                            where = new WhereCondition(where, new WhereCondition().WhereEquals(nameof(SearchIndexInfo.IndexProvider), searchProvider));
                        }

                        var indexes = GetSearchIndexes().Where(where).Columns("IndexID", "IndexType");

                        // Check whether exists at least one index
                        if (!DataHelper.DataSourceIsEmpty(indexes))
                        {
                            // Loop thru all items and fill list of ids
                            foreach (DataRow indexDr in indexes.Tables[0].Rows)
                            {
                                var type = ValidationHelper.GetString(indexDr["IndexType"], "").ToLowerInvariant();
                                int id = ValidationHelper.GetInteger(indexDr["IndexID"], 0);

                                FlatIndexes[type][searchProviderCacheKey].Add(id);
                            }
                        }
                    }

                    // Build the final result from loaded cached data
                    var result = new List<int>();
                    foreach (var type in typeCodes)
                    {
                        // Get index ids from static collection
                        var results = FlatIndexes[type.ToLowerInvariant()][searchProviderCacheKey];

                        // If ids are not loaded, load it
                        if (results != null)
                        {
                            result.AddRange(results);
                        }
                    }

                    return result;
                }
            }

            return null;
        }


        /// <summary>
        /// Returns true if smart search is enabled and exists at least one index for specified object type.
        /// </summary>
        /// <param name="type">Object type name</param>
        public static bool SearchTypeEnabled(string type)
        {
            if (!SearchEnabled)
            {
                return false;
            }

            List<int> items = GetIndexIDs(new List<string> { type });
            return (items != null) && (items.Count > 0);
        }


        /// <summary>
        /// Returns all search indexes
        /// </summary>
        /// <returns></returns>
        public static ObjectQuery<SearchIndexInfo> GetSearchIndexes()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns all sites for specified search index.
        /// </summary>
        /// <param name="searchIndexId">Search index id</param>
        public static InfoDataSet<SiteInfo> GetSearchIndexSites(int searchIndexId)
        {
            return SearchIndexSiteInfoProvider.GetIndexSites(searchIndexId).TypedResult;
        }


        /// <summary>
        /// Returns the SearchIndexInfo structure for the specified searchIndex.
        /// </summary>
        /// <param name="searchIndexId">SearchIndex id</param>
        public static SearchIndexInfo GetSearchIndexInfo(int searchIndexId)
        {
            return ProviderObject.GetInfoById(searchIndexId);
        }


        /// <summary>
        /// Returns the SearchIndexInfo structure for the specified searchIndex.
        /// </summary>
        /// <param name="searchIndexName">SearchIndex name</param>
        public static SearchIndexInfo GetSearchIndexInfo(string searchIndexName)
        {
            return ProviderObject.GetInfoByCodeName(searchIndexName);
        }


        /// <summary>
        /// Returns the SearchIndexInfo structure for the specified searchIndex. If localized SearchIndex found, then
        /// it is returned insted of regular one.
        /// </summary>
        /// <param name="searchIndexName">SearchIndex name</param>
        /// <param name="culture">Culture code</param>
        public static SearchIndexInfo GetLocalizedSearchIndexInfo(string searchIndexName, string culture)
        {
            // Get original name
            SearchIndexInfo sii = ProviderObject.GetInfoByCodeName(searchIndexName);

            // Get localized version
            if (sii != null)
            {
                // Check whether current culture is defined
                if (!String.IsNullOrEmpty(culture))
                {
                    // Try load index with current culture postfix
                    SearchIndexInfo cultureIndex = ProviderObject.GetInfoByCodeName(sii.IndexName + "_" + culture.Replace("-", "_"));
                    if (cultureIndex != null)
                    {
                        sii = cultureIndex;
                    }
                }
            }
            return sii;
        }


        /// <summary>
        /// Sets (updates or inserts) specified searchIndex.
        /// </summary>
        /// <param name="searchIndex">SearchIndex to set</param>
        public static void SetSearchIndexInfo(SearchIndexInfo searchIndex)
        {
            ProviderObject.SetSearchIndexInfoInternal(searchIndex);
        }


        /// <summary>
        /// Deletes specified searchIndex.
        /// </summary>
        /// <param name="infoObj">SearchIndex object</param>
        public static void DeleteSearchIndexInfo(SearchIndexInfo infoObj)
        {
            ProviderObject.DeleteSearchIndexInfoInternal(infoObj);
        }


        /// <summary>
        /// Deletes specified searchIndex.
        /// </summary>
        /// <param name="searchIndexId">SearchIndex id</param>
        public static void DeleteSearchIndexInfo(int searchIndexId)
        {
            SearchIndexInfo infoObj = GetSearchIndexInfo(searchIndexId);
            DeleteSearchIndexInfo(infoObj);
        }


        /// <summary>
        /// Determines whether the class has a general index.
        /// </summary>
        /// <param name="objectType">Object type</param>
        public static bool IsObjectTypeIndexed(string objectType)
        {
            bool isIndexed = true;
            lock (tableLock)
            {
                // Work with a local variable (thread safe)
                var classes = IndexedClasses;
                if (classes == null)
                {
                    classes = new List<string>();

                    // Get all indexIDs of the index type General and On-Line forms (because those run through general engine as well)
                    var items = GetIndexIDs(GeneralIndexTypeList);

                    foreach (int id in items)
                    {
                        // Store all class names (which have a general index) in a static list
                        var sii = GetSearchIndexInfo(id);
                        if ((sii != null) && (sii.IndexSettings != null))
                        {
                            var settingsItems = sii.IndexSettings.Items;

                            foreach (var sisi in settingsItems.Values)
                            {
                                string siClassName = sisi.ClassNames.ToLowerCSafe();
                                if (!classes.Contains(siClassName))
                                {
                                    classes.Add(siClassName);
                                }
                            }
                        }
                    }

                    IndexedClasses = classes;
                }

                if (!string.IsNullOrEmpty(objectType))
                {
                    isIndexed = classes.Contains(objectType.ToLowerCSafe());
                }
            }

            return isIndexed;
        }


        /// <summary>
        /// Rebuilds all indexes which belongs to the specified site.
        /// </summary>
        /// <param name="site">ID or name of the site the indexes of which should be rebuilt</param>
        /// <param name="runIndexer">Indicates whether the indexer starts immediately</param>
        public static void RebuildSiteIndexes(SiteInfoIdentifier site, bool runIndexer = true)
        {
            ProviderObject.RebuildSiteIndexesInternal(site, runIndexer);
        }


        /// <summary>
        /// Sets last update time of search index files that belongs to current application process.
        /// </summary>
        /// <param name="indexInfo">Search index to update</param>
        /// <param name="updateTime">Time when index was updated</param>
        public static void SetIndexFilesLastUpdateTime(SearchIndexInfo indexInfo, DateTime updateTime)
        {
            ProviderObject.SetIndexFilesLastUpdateTimeInternal(indexInfo, updateTime);
        }


        /// <summary>
        /// Sets status of <paramref name="indexInfo"/> to <paramref name="status"/>. The method sets <see cref="SearchIndexInfo.IndexStatus"/> or <see cref="SearchIndexInfo.IndexStatusLocal"/>
        /// based on the following criteria. For shared storage file system based indexes or Azure indexes, the first property is set. For local storage file system based indexes, the latter one is set.
        /// </summary>
        /// <param name="indexInfo">Search index info whose status is to be set.</param>
        /// <param name="status">Status to be set.</param>
        public static void SetIndexStatus(SearchIndexInfo indexInfo, IndexStatusEnum status)
        {
            ProviderObject.SetIndexStatusInternal(indexInfo, status);
        }


        /// <summary>
        /// Gets status of <paramref name="indexInfo"/>. The method gets <see cref="SearchIndexInfo.IndexStatus"/> or <see cref="SearchIndexInfo.IndexStatusLocal"/>
        /// based on the following criteria. For shared storage file system based indexes or Azure indexes, the first property is gotten. For local storage file system based indexes, the latter one is gotten.
        /// </summary>
        /// <param name="indexInfo">Search index info whose status to get.</param>
        /// <returns>Gets status of given search index info.</returns>
        public static IndexStatusEnum GetIndexStatus(SearchIndexInfo indexInfo)
        {
            return ProviderObject.GetIndexStatusInternal(indexInfo);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Sets (updates or inserts) specified searchIndex.
        /// </summary>
        /// <param name="searchIndex">SearchIndex to set</param>
        protected virtual void SetSearchIndexInfoInternal(SearchIndexInfo searchIndex)
        {
            if (searchIndex != null)
            {
                bool isNew = (searchIndex.IndexID == 0);

                var ti = searchIndex.TypeInfo;

                bool codeNameChanged = searchIndex.ItemChanged(ti.CodeNameColumn);
                if (!isNew && codeNameChanged && searchIndex.IsLuceneIndex())
                {
                    string originalCodeName = ValidationHelper.GetString(searchIndex.GetOriginalValue(ti.CodeNameColumn), "");

                    // Ensure removal of the searcher from cache (search points to the old directory, we need to redirect to the new directory)
                    SearchHelper.InvalidateSearcher(searchIndex.IndexGUID);

                    // Move the files if the codename has been changed
                    MoveIndexFiles(originalCodeName, searchIndex.IndexName);
                }

                SetInfo(searchIndex);

                // Clear the indexedClass list
                ClearIndexedClassesList();

                searchIndex.Provider.InvalidateAnalyzer();

                if (isNew)
                {
                    // Reload list of indexes
                    InvalidateIndexIDs(searchIndex.IndexType);
                }
            }
        }


        /// <summary>
        /// Deletes specified searchIndex.
        /// </summary>
        /// <param name="infoObj">SearchIndex object</param>
        protected virtual void DeleteSearchIndexInfoInternal(SearchIndexInfo infoObj)
        {
            // Reload list of user indexes
            if (infoObj != null)
            {
                InvalidateIndexIDs(infoObj.IndexType);

                // Invalidate deleted searcher
                SearchHelper.InvalidateSearcher(infoObj.IndexGUID, true);

                // Clear the indexedClass list
                ClearIndexedClassesList();

                DeleteInfo(infoObj);

                if (infoObj.IsLuceneIndex())
                {
                    RemoveIndexFiles(infoObj);
                }
            }
        }


        /// <summary>
        /// Deletes directory with index files.
        /// </summary>
        /// <param name="infoObj">SearchIndex object.</param>
        private static void RemoveIndexFiles(SearchIndexInfo infoObj)
        {
            try
            {
                var path = SearchIndexInfo.IndexPathPrefix + infoObj.IndexName;
                if (Directory.Exists(path))
                {
                    DirectoryHelper.DeleteDirectory(path, true);
                }
            }
            catch (Exception ex)
            {
                EventLog.EventLogProvider.LogException("Search", "DELETEINDEX", ex, additionalMessage: $"Index code name is {infoObj.IndexCodeName}.");
            }
        }


        /// <summary>
        /// Rebuilds all indexes which belongs to the specified site.
        /// </summary>
        /// <param name="site">ID or name of the site the indexes of which should be rebuilt</param>
        /// <param name="runIndexer">Indicates whether the indexer starts immediately. If not specified this behavior depends on current settings.</param>
        protected virtual void RebuildSiteIndexesInternal(SiteInfoIdentifier site, bool? runIndexer = null)
        {
            if (site != null)
            {
                // Rebuild search index
                if (SearchEnabled)
                {
                    // Get all indexes depending on given site
                    var indexes = SearchIndexSiteInfoProvider.GetSiteIndexes(site.ObjectID).Columns("CMS_SearchIndex.IndexID", "IndexName");
                    var items = indexes.Select(index => new SearchTaskCreationParameters
                    {
                        TaskType = SearchTaskTypeEnum.Rebuild,
                        TaskValue = index.IndexName,
                        RelatedObjectID = index.IndexID
                    }
                    ).ToList();

                    // Rebuild all indexes
                    if (items.Count > 0)
                    {
                        SearchTaskInfoProvider.CreateTasks(items, runIndexer);
                    }
                }
            }
        }


        /// <summary>
        /// Sets last update time of search index files that belongs to current application process.
        /// </summary>
        /// <param name="indexInfo">Search index to update</param>
        /// <param name="updateTime">Time when index was updated</param>
        protected virtual void SetIndexFilesLastUpdateTimeInternal(SearchIndexInfo indexInfo, DateTime updateTime)
        {
            indexInfo.IndexFilesLastUpdate = updateTime;

            // Clear cached data
            ClearIndexFilesCachedData(indexInfo.Generalized.ObjectCodeName);
        }


        /// <summary>
        /// Sets status of <paramref name="indexInfo"/> to <paramref name="status"/>. The method sets <see cref="SearchIndexInfo.IndexStatus"/> or <see cref="SearchIndexInfo.IndexStatusLocal"/>
        /// based on the following criteria. For shared storage file system based indexes or Azure indexes, the first property is set. For local storage file system based indexes, the latter one is set.
        /// </summary>
        /// <param name="indexInfo">Search index info whose status is to be set.</param>
        /// <param name="status">Status to be set.</param>
        protected virtual void SetIndexStatusInternal(SearchIndexInfo indexInfo, IndexStatusEnum status)
        {
            if (SearchHelper.IndexesInSharedStorage || indexInfo.IsAzureIndex())
            {
                indexInfo.IndexStatus = status;
                SetSearchIndexInfo(indexInfo);
            }
            else
            {
                indexInfo.IndexStatusLocal = status;
            }
        }


        /// <summary>
        /// Gets status of <paramref name="indexInfo"/>. The method gets <see cref="SearchIndexInfo.IndexStatus"/> or <see cref="SearchIndexInfo.IndexStatusLocal"/>
        /// based on the following criteria. For shared storage file system based indexes or Azure indexes, the first property is gotten. For local storage file system based indexes, the latter one is gotten.
        /// </summary>
        /// <param name="indexInfo">Search index info whose status to get.</param>
        /// <returns>Gets status of given search index info.</returns>
        protected virtual IndexStatusEnum GetIndexStatusInternal(SearchIndexInfo indexInfo)
        {
            if (SearchHelper.IndexesInSharedStorage || indexInfo.IsAzureIndex())
            {
                return indexInfo.IndexStatus;
            }
            else
            {
                return indexInfo.IndexStatusLocal;
            }
        }


        /// <summary>
        /// Removes key from cached site indexes.
        /// </summary>
        /// <param name="key">Cache key to be removed (site id)</param>
        /// <param name="logWebFarmTask">Indicates whether webfarm task should be created</param>
        protected virtual void RemoveFromIndexSiteCacheInternal(string key, bool logWebFarmTask)
        {
            if (key == null)
            {
                return;
            }

            SiteIndexes.Remove(ValidationHelper.GetInteger(key, 0));
            if (logWebFarmTask)
            {
                // Create webfarm task if needed
                CreateWebFarmTask(SEARCH_INDEX_SITE_CACHE_TASK, key);
            }
        }


        /// <summary>
        /// Removes key from cached index-culture relation.
        /// </summary>
        /// <param name="key">Cache key to be removed</param>
        /// <param name="logWebFarmTask">Indicates whether webfarm task should be created</param>
        protected virtual void RemoveFromIndexCultureCacheInternal(string key, bool logWebFarmTask)
        {
            if (key == null)
            {
                return;
            }

            CachedIndexCulture.Remove(key.ToLowerCSafe());
            if (logWebFarmTask)
            {
                // Create webfarm task if needed
                CreateWebFarmTask(SEARCH_INDEX_CULTURE_CACHE_TASK, key);
            }
        }


        /// <summary>
        /// Invalidates index ids collection for specified object type.
        /// </summary>
        /// <param name="type">Object type name</param>
        /// <param name="logtask">Indicates whether web farm task should be logged</param>
        protected virtual void InvalidateIndexIDsInternal(string type, bool logtask)
        {
            lock (flatIndexesLock)
            {
                FlatIndexes.Remove(type.ToLowerInvariant());
            }

            if (logtask)
            {
                CreateWebFarmTask(INVALIDATE_INDEX_IDS_TASK, type);
            }
        }

        #endregion


        #region "SearchIndex specific methods"

        /// <summary>
        /// Removes key from cached site indexes.
        /// </summary>
        /// <param name="key">Cache key to be removed (site id)</param>
        /// <param name="logWebFarmTask">Indicates whether webfarm task should be created</param>
        public static void RemoveFromIndexSiteCache(string key, bool logWebFarmTask)
        {
            ProviderObject.RemoveFromIndexSiteCacheInternal(key, logWebFarmTask);
        }


        /// <summary>
        /// Removes key from cached index-culture relation.
        /// </summary>
        /// <param name="key">Cache key to be removed</param>
        /// <param name="logWebFarmTask">Indicates whether webfarm task should be created</param>
        public static void RemoveFromIndexCultureCache(string key, bool logWebFarmTask)
        {
            ProviderObject.RemoveFromIndexCultureCacheInternal(key, logWebFarmTask);
        }


        /// <summary>
        /// If SearchIndex has specified site assigned returns true, otherwise false.
        /// The result is cached for next time usage.
        /// </summary>
        /// <param name="sii">SearchIndexInfo</param>
        /// <param name="siteId">Site id</param>
        public static bool IndexIsInSite(SearchIndexInfo sii, int siteId)
        {
            if (sii == null)
            {
                return false;
            }

            // Get site indexes
            List<int> siteIndexes = GetSiteIndexes(siteId);

            return siteIndexes.Contains(sii.IndexID);
        }


        /// <summary>
        /// Returns search indexes of specified site. Tries to load it from cache, if not found indexes are loaded from DB.
        /// </summary>
        /// <param name="siteId">Site id</param>
        /// <returns>List with indexes ids. Never null, if no indexes for site, empty list is returned.</returns>
        public static List<int> GetSiteIndexes(int siteId)
        {
            // Get site indexes from cache
            List<int> indexes = SiteIndexes[siteId];

            // If not loaded yet, load it to cache
            if (indexes == null)
            {
                // Get indexes assigned to site
                indexes = SiteIndexes[siteId] = SearchIndexSiteInfoProvider.GetSiteIndexBindings(siteId).Column("IndexID").Select(index => index.IndexID).ToList();
            }

            return indexes;
        }


        /// <summary>
        /// If SearchIndex has specified culture assigned returns true, otherwise false.
        /// The result is cached for next time usage.
        /// </summary>
        /// <param name="sii">SearchIndexInfo</param>
        /// <param name="cultureCode">Culture code</param>
        public static bool IndexIsInCulture(SearchIndexInfo sii, string cultureCode)
        {
            if ((sii == null) || String.IsNullOrEmpty(cultureCode))
            {
                return false;
            }

            bool? isInCulture = null;
            string key = sii.IndexID + "_" + cultureCode.ToLowerCSafe();

            // Try to get from cache
            object cacheValue = CachedIndexCulture[key];

            // Is in cache
            if (cacheValue != null)
            {
                isInCulture = ValidationHelper.GetBoolean(cacheValue, false);
            }

            // Not in cache - load from db and store result
            if (!isInCulture.HasValue)
            {
                // Get index cultures
                DataSet ds = SearchIndexCultureInfoProvider.GetSearchIndexCultures("IndexID = " + sii.IndexID, null, 0, "CultureCode");

                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    // Go trough all assigned cultures
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        string indexCultureCode = ValidationHelper.GetString(dr["CultureCode"], String.Empty);
                        CachedIndexCulture[sii.IndexID + "_" + indexCultureCode.ToLowerCSafe()] = true;

                        if (cultureCode.EqualsCSafe(indexCultureCode, true))
                        {
                            // Index is assigned to culture
                            isInCulture = true;
                        }
                    }
                }

                // Wasn't found for culture
                if (!isInCulture.HasValue)
                {
                    isInCulture = false;
                }

                // Store in cache
                CachedIndexCulture[key] = isInCulture.Value;
            }

            return isInCulture.Value;
        }


        /// <summary>
        /// Returns all indexes which are relevant to the specified ISearchable object and given <paramref name="searchProvider"/>.
        /// Check ISearchable object type and with dependence on this type check index settings.
        /// </summary>
        /// <param name="searchObject">Object implementing ISearchable interface.</param>
        /// <param name="searchProvider">
        /// Defines search provider for which to return relevant indexes.
        /// If not defined then indexes for all search providers are returned.
        /// </param>
        /// <seealso cref="SearchIndexInfo.IndexProvider"/>
        public static List<SearchIndexInfo> GetRelevantIndexes(ISearchable searchObject, string searchProvider)
        {
            if (searchObject == null)
            {
                return null;
            }

            var helper = SearchablesRetrievers.Get(searchObject.SearchType);
            if (helper != null)
            {
                return helper.GetRelevantIndexes(searchObject, searchProvider);
            }

            return null;
        }

        #endregion


        #region "Helper methods"

        /// <summary>
        /// Clears the indexedClass list (thread safe).
        /// </summary>
        private void ClearIndexedClassesList()
        {
            lock (tableLock)
            {
                IndexedClasses = null;
            }
        }

        #endregion


        #region "Web farm"

        /// <summary>
        /// Clear hashtables.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        protected override void ClearHashtables(bool logTasks)
        {
            base.ClearHashtables(logTasks);

            ClearInternal(logTasks);
        }

        private void ClearInternal(bool logWebFarmTask)
        {
            lock (tableLock)
            {
                CachedIndexCulture.Clear();
                SiteIndexes.Clear();
                IndexedClasses = null;

                lock (flatIndexesLock)
                {
                    FlatIndexes.Clear();
                }
            }

            // Create web farm task if needed
            if (logWebFarmTask)
            {
                // Clear search indexes
                CreateWebFarmTask(CLEAR_INDEXES_TASK, String.Empty);
            }
        }


        /// <summary>
        /// Runs the processing of specific web farm task for current provider
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="data">Custom data</param>
        /// <param name="binary">Binary data</param>
        public override void ProcessWebFarmTask(string actionName, string data, byte[] binary)
        {
            // Switch by action name
            switch (actionName)
            {
                // Invalidate flat index ids
                case INVALIDATE_INDEX_IDS_TASK:
                    InvalidateIndexIDs(data, false);
                    break;

                // Clear cached site index bindings
                case SEARCH_INDEX_SITE_CACHE_TASK:
                    RemoveFromIndexSiteCache(data, false);
                    break;

                // Clear cached culture index bindings
                case SEARCH_INDEX_CULTURE_CACHE_TASK:
                    RemoveFromIndexCultureCache(data, false);
                    break;

                // Clear search indexes
                case CLEAR_INDEXES_TASK:
                    ClearHashtables(false);
                    break;

                // Clear index from hash tables
                case CLEAR_INDEX_TASK:
                    var indexToFlush = GetInfoByCodeName(data);
                    if (indexToFlush != null)
                    {
                        DeleteObjectFromHashtables(indexToFlush);
                    }
                    break;

                // If action name is not handled throw an exception
                default:
                    throw new Exception("[" + TypeInfo.ObjectType + ".ProcessWebFarmTask] The action name '" + actionName + "' has no supporting code.");
            }
        }


        /// <summary>
        /// Clears meta data of index files on other instances of web farm servers if running on configuration where clear is necessary.
        /// Example of such meta data is last update time of index files.
        /// </summary>
        /// <param name="indexCodeName">Code name of index</param>
        private void ClearIndexFilesCachedData(string indexCodeName)
        {
            if (SearchHelper.IndexesInSharedStorage)
            {
                CreateWebFarmTask(CLEAR_INDEX_TASK, indexCodeName);
            }
        }

        #endregion


        #region "Files operations"

        /// <summary>
        /// Moves index files from the old directory to the new one (after index code rename).
        /// </summary>
        /// <param name="oldCodeName">Old code name</param>
        /// <param name="newCodeName">New code name</param>
        private void MoveIndexFiles(string oldCodeName, string newCodeName)
        {
            string oldPath = SearchIndexInfo.IndexPathPrefix + oldCodeName;
            string newPath = SearchIndexInfo.IndexPathPrefix + newCodeName;

            // If there are index files of the old index and directory for new index does not exist yet, move the files to the new destination
            if (!Directory.Exists(oldPath))
            {
                return;
            }

            string[] oldFiles = Directory.GetFiles(oldPath);
            if (!Directory.Exists(newPath) || (Directory.GetFiles(newPath).Length == 0))
            {
                try
                {
                    DirectoryHelper.EnsureDiskPath(newPath + "\\", SystemContext.WebApplicationPhysicalPath);
                    foreach (var oldFile in oldFiles)
                    {
                        string fileName = Path.GetFileName(oldFile);
                        File.Copy(oldFile, newPath + "\\" + fileName);
                    }

                    DirectoryHelper.DeleteDirectory(oldPath, true);
                }
                catch (Exception ex)
                {
                    // Log to event log
                    EventLog.EventLogProvider.LogException("Search", "RENAME", ex);
                }
            }
        }

        #endregion
    }
}