using System;
using System.Linq;

using CMS.DataEngine;
using CMS.SiteProvider;

namespace CMS.Search
{
    /// <summary>
    /// Class providing management of the Search index - Site binding.
    /// </summary>
    public class SearchIndexSiteInfoProvider : AbstractInfoProvider<SearchIndexSiteInfo, SearchIndexSiteInfoProvider>
    {
        #region "Public static methods"

        /// <summary>
        /// Returns all search index sites bindings.
        /// </summary>
        public static ObjectQuery<SearchIndexSiteInfo> GetSearchIndexSites()
        {
            return  ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns all sites for specified search indexes.
        /// </summary>
        /// <param name="searchIndexIds">Search index IDs</param>
        public static ObjectQuery<SiteInfo> GetIndexSites(params int[] searchIndexIds)
        {
            return ProviderObject.GetIndexSitesInternal(searchIndexIds);
        }


        /// <summary>
        /// Returns all indexes for specified sites.
        /// </summary>
        /// <param name="siteIds">Site IDs</param>
        public static ObjectQuery<SearchIndexInfo> GetSiteIndexes(params int[] siteIds)
        {
            return ProviderObject.GetSiteIndexesInternal(siteIds);
        }


        /// <summary>
        /// Returns search index site bindings for specified sites.
        /// </summary>
        /// <param name="siteIds">Site IDs</param>        
        public static ObjectQuery<SearchIndexSiteInfo> GetSiteIndexBindings(params int[] siteIds)
        {
            return ProviderObject.GetIndexSiteInfosFromSitesInternal(siteIds);
        }


        /// <summary>
        /// Returns search index site bindings for specified indexes.
        /// </summary>
        /// <param name="searchIndexIds">Search index IDs</param>        
        public static ObjectQuery<SearchIndexSiteInfo> GetIndexSiteBindings(params int[] searchIndexIds)
        {
            return ProviderObject.GetIndexSiteInfosFromIndexesInternal(searchIndexIds);
        }


        /// <summary>
        /// Returns search index site binding structure for the specified index and site.
        /// </summary>
        /// <param name="indexId">Search index ID</param>
        /// <param name="siteId">Site ID</param>
        public static SearchIndexSiteInfo GetSearchIndexSiteInfo(int indexId, int siteId)
        {
            return ProviderObject.GetSearchIndexSiteInfoInternal(indexId, siteId);
        }


        /// <summary>
        /// Sets specified searchIndexSite.
        /// </summary>
        /// <param name="infoObj">SearchIndexSite to set</param>
        public static void SetSearchIndexSiteInfo(SearchIndexSiteInfo infoObj)
        {
            ProviderObject.SetSearchIndexSiteInfoInternal(infoObj);
        }


        /// <summary>
        /// Deletes specified search index site binding.
        /// </summary>
        /// <param name="infoObj">SearchIndexSite object</param>
        public static void DeleteSearchIndexSiteInfo(SearchIndexSiteInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified search index site binding.
        /// </summary>
        /// <param name="indexId">Search index ID</param>
        /// <param name="siteId">Site ID</param>
        public static void DeleteSearchIndexSiteInfo(int indexId, int siteId)
        {
            SearchIndexSiteInfo infoObj = GetSearchIndexSiteInfo(indexId, siteId);
            DeleteSearchIndexSiteInfo(infoObj);
        }


        /// <summary>
        /// Creates search index site binding. 
        /// </summary>
        /// <param name="indexId">Search index ID</param>
        /// <param name="siteId">Site ID</param>   
        public static void AddSearchIndexToSite(int indexId, int siteId)
        {
            ProviderObject.AddSearchIndexToSiteInternal(indexId, siteId);
        }


        /// <summary>
        /// Returns <c>>true</c> if given search index has any site assigned, <c>false</c> otherwise.
        /// </summary>
        /// <param name="indexId">Smart search index id</param>
        public static bool SearchIndexHasAnySite(int indexId)
        {
            return GetIndexSites(indexId).Any();
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns object query with all sites for specified search indexes.
        /// </summary>
        /// <param name="searchIndexIds">Search index IDs</param>
        protected virtual ObjectQuery<SiteInfo> GetIndexSitesInternal(params int[] searchIndexIds)
        {
            var getSiteIDs = SearchIndexSiteInfoProvider.GetSearchIndexSites()
                .Column("IndexSiteID")
                .WhereIn("IndexId", searchIndexIds);

            return SiteInfoProvider.GetSites()
                .WhereIn("SiteID", getSiteIDs);
        }


        /// <summary>
        /// Returns object query with all search indexes for specified sites.
        /// </summary>
        /// <param name="siteIds">Site IDs</param>
        protected virtual ObjectQuery<SearchIndexInfo> GetSiteIndexesInternal(params int[] siteIds)
        {
            var getIndexIDs = SearchIndexSiteInfoProvider.GetSearchIndexSites()
                .Column("IndexID")
                .WhereIn("IndexSiteId", siteIds);

            return SearchIndexInfoProvider.GetSearchIndexes()
                .WhereIn("IndexId", getIndexIDs);
        }


        /// <summary>
        /// Returns object query with search index site bindings for the specified sites.
        /// </summary>
        /// <param name="siteIds">Site IDs</param>
        /// <returns>Dataset with results</returns>
        protected virtual ObjectQuery<SearchIndexSiteInfo> GetIndexSiteInfosFromSitesInternal(params int[] siteIds)
        {
            return GetSearchIndexSites().WhereIn("IndexSiteID", siteIds);
        }


        /// <summary>
        /// Returns object query with search index site bindings for the specified indexes.
        /// </summary>
        /// <param name="searchIndexIds">Search index IDs</param>
        /// <returns>Dataset with results</returns>
        protected virtual ObjectQuery<SearchIndexSiteInfo> GetIndexSiteInfosFromIndexesInternal(params int[] searchIndexIds)
        {
            return GetSearchIndexSites().WhereIn("IndexID", searchIndexIds);
        }


        /// <summary>
        /// Returns the SearchIndexSiteInfo structure for the specified index and site.
        /// Null if binding doesn't exist.
        /// </summary>
        /// <param name="indexId">Search index ID</param>
        /// <param name="siteId">Site ID</param>
        protected virtual SearchIndexSiteInfo GetSearchIndexSiteInfoInternal(int indexId, int siteId)
        {
            var indexSitesQuery = GetSearchIndexSites()
                .Where("IndexID", QueryOperator.Equals, indexId)
                .Where("IndexSiteID", QueryOperator.Equals, siteId)
                .TopN(1);

            if (indexSitesQuery.Count > 0)
            {
                return indexSitesQuery.First();
            }

            return null;
        }


        /// <summary>
        /// Sets specified search index site binding.
        /// </summary>
        /// <param name="searchIndexSite">SearchIndexSiteInfo object</param>
        protected virtual void SetSearchIndexSiteInfoInternal(SearchIndexSiteInfo searchIndexSite)
        {
            if (searchIndexSite == null)
            {
                throw new ArgumentNullException("searchIndexSite");
            }

            // Check IDs
            if ((searchIndexSite.IndexSiteID <= 0) || (searchIndexSite.IndexID <= 0))
            {
                throw new InvalidOperationException("One or more object identifiers are not set.");
            }

            // Get existing
            SearchIndexSiteInfo existing = GetSearchIndexSiteInfo(searchIndexSite.IndexID, searchIndexSite.IndexSiteID);
            if (existing != null)
            {
                // Do nothing, item does not carry any new data
            }
            else
            {
                // Set the object
                searchIndexSite.Generalized.InsertData();

                // Remove from searchindex cache
                SearchIndexInfoProvider.RemoveFromIndexSiteCache(searchIndexSite.IndexSiteID.ToString(), searchIndexSite.Generalized.LogWebFarmTasks);
            }
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(SearchIndexSiteInfo info)
        {
            if (info != null)
            {
                // Delete binding
                base.DeleteInfo(info);

                // Remove from searchindex cache
                SearchIndexInfoProvider.RemoveFromIndexSiteCache(info.IndexSiteID.ToString(), info.Generalized.LogWebFarmTasks);
            }
        }


        /// <summary>
        /// Creates search index site binding. 
        /// </summary>
        /// <param name="indexId">Search index ID</param>
        /// <param name="siteId">Site ID</param>   
        protected virtual void AddSearchIndexToSiteInternal(int indexId, int siteId)
        {
            // Get the objects
            SiteInfo site = SiteInfoProvider.GetSiteInfo(siteId);
            SearchIndexInfo index = SearchIndexInfoProvider.GetSearchIndexInfo(indexId);

            if ((site != null) && (index != null))
            {
                // Create new binding
                SearchIndexSiteInfo infoObj = new SearchIndexSiteInfo();
                infoObj.IndexSiteID = siteId;
                infoObj.IndexID = indexId;

                // Save to the database
                SetSearchIndexSiteInfo(infoObj);
            }
        }

        #endregion
    }
}