using System;
using System.Linq;

using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.Localization;

namespace CMS.Search
{
    /// <summary>
    /// Class providing SearchIndexCultureInfo management.
    /// </summary>
    public class SearchIndexCultureInfoProvider : AbstractInfoProvider<SearchIndexCultureInfo, SearchIndexCultureInfoProvider>
    {
        #region "Public static methods"

        /// <summary>
        /// Returns all bindings between cultures and search indexes.
        /// </summary>
        public static ObjectQuery<SearchIndexCultureInfo> GetSearchIndexCultures()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns all cultures for specified search index.
        /// </summary>
        /// <param name="searchIndexId">Search index id</param>
        public static InfoDataSet<CultureInfo> GetSearchIndexCultures(int searchIndexId)
        {
            string where = "IndexID = " + searchIndexId;
            return ProviderObject.GetSearchIndexCulturesInternal(null, where, null, 0);
        }


        /// <summary>
        /// Returns all cultures for specified search index.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="topN">TopN items</param>
        /// <param name="columns">List of columns to get</param>
        public static InfoDataSet<CultureInfo> GetSearchIndexCultures(string where, string orderBy, int topN, string columns)
        {
            return ProviderObject.GetSearchIndexCulturesInternal(columns, where, orderBy, topN);
        }


        /// <summary>
        /// Returns the SearchIndexCultureInfo structure for the specified searchIndexCulture id.
        /// </summary>
        /// <param name="indexId">SearchIndex id</param>
        /// <param name="cultureId">Culture id</param>
        public static SearchIndexCultureInfo GetSearchIndexCultureInfo(int indexId, int cultureId)
        {
            return ProviderObject.GetSearchIndexCultureInfoInternal(indexId, cultureId);
        }


        /// <summary>
        /// Sets specified searchIndexCulture.
        /// Doesn't update existing mItems.
        /// </summary>
        /// <param name="infoObj">SearchIndexCultureInfo to set</param>
        public static void SetSearchIndexCultureInfo(SearchIndexCultureInfo infoObj)
        {
            ProviderObject.SetSearchIndexCultureInfoInternal(infoObj);
        }


        /// <summary>
        /// Deletes specified searchIndexCulture.
        /// </summary>
        /// <param name="infoObj">SearchIndexCultureInfo object</param>
        public static void DeleteSearchIndexCultureInfo(SearchIndexCultureInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified searchIndexCulture.
        /// </summary>
        /// <param name="indexId">SearchIndex id</param>
        /// <param name="cultureId">Culture id</param>
        public static void DeleteSearchIndexCultureInfo(int indexId, int cultureId)
        {
            SearchIndexCultureInfo infoObj = GetSearchIndexCultureInfo(indexId, cultureId);
            DeleteSearchIndexCultureInfo(infoObj);
        }


        /// <summary>
        /// Adds specified sarchIndex to culture.
        /// </summary>
        /// <param name="indexId">SearchIndex id</param>
        /// <param name="cultureId">Culture id</param>   
        public static void AddSearchIndexCulture(int indexId, int cultureId)
        {
            // Get the objects
            CultureInfo culture = CultureInfoProvider.GetCultureInfo(cultureId);
            SearchIndexInfo index = SearchIndexInfoProvider.GetSearchIndexInfo(indexId);

            if ((culture != null) && (index != null))
            {
                // Create new binding
                SearchIndexCultureInfo infoObj = new SearchIndexCultureInfo();
                infoObj.IndexCultureID = cultureId;
                infoObj.IndexID = indexId;

                // Save to the database
                SetSearchIndexCultureInfo(infoObj);
            }
        }


        /// <summary>
        /// Returns <c>true</c> if given index has assigned at least one culture, <c>false</c> otherwise.
        /// </summary>
        /// <param name="indexId">Smart search index id</param>
        public static bool SearchIndexHasAnyCulture(int indexId)
        {
            return GetSearchIndexCultures(indexId).Any();
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns dataset with all cultures for specified search index.
        /// </summary>
        /// <param name="columns">Columns</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="topN">TOP N Rows</param>
        protected virtual InfoDataSet<CultureInfo> GetSearchIndexCulturesInternal(string columns, string where, string orderBy, int topN)
        {
            var parameters = new QueryDataParameters();
            parameters.EnsureDataSet<CultureInfo>();

            return ConnectionHelper.ExecuteQuery("CMS.SearchIndexCulture.selectcultures", parameters, where, orderBy, topN, columns).As<CultureInfo>();
        }


        /// <summary>
        /// Returns the SearchIndexCultureInfo structure for the specified searchIndex.
        /// </summary>
        /// <param name="indexId">SearchIndex id</param>
        /// <param name="cultureId">Culture id</param>
        protected virtual SearchIndexCultureInfo GetSearchIndexCultureInfoInternal(int indexId, int cultureId)
        {
            var where = new WhereCondition().WhereEquals("IndexID", indexId).WhereEquals("IndexCultureID", cultureId);
            return GetSearchIndexCultures().Where(where).TopN(1).BinaryData(true).FirstObject;
        }


        /// <summary>
        /// Sets specified searchIndexCulture.
        /// Doesn't update existing item.
        /// </summary>
        /// <param name="searchIndexCulture">SearchIndexCultureInfo to set</param>
        protected virtual void SetSearchIndexCultureInfoInternal(SearchIndexCultureInfo searchIndexCulture)
        {
            if (searchIndexCulture != null)
            {
                // Check IDs
                if ((searchIndexCulture.IndexCultureID <= 0) || (searchIndexCulture.IndexID <= 0))
                {
                    throw new Exception("[SearchIndexCultureInfoProvider.SetSearchIndexCultureInfo]: Object IDs not set.");
                }

                // Get existing
                SearchIndexCultureInfo existing = GetSearchIndexCultureInfo(searchIndexCulture.IndexID, searchIndexCulture.IndexCultureID);
                if (existing != null)
                {
                    // Do nothing, item does not carry any data
                }
                else
                {
                    // Remove from searchindex cache
                    CultureInfo ci = CultureInfoProvider.GetCultureInfo(searchIndexCulture.IndexCultureID);
                    if (ci != null)
                    {
                        SearchIndexInfoProvider.RemoveFromIndexCultureCache(searchIndexCulture.IndexID + "_" + ci.CultureCode, searchIndexCulture.Generalized.LogWebFarmTasks);
                    }

                    searchIndexCulture.Generalized.InsertData();
                }
            }
            else
            {
                throw new Exception("[SearchIndexCultureInfoProvider.SetSearchIndexCultureInfo]: No SearchIndexCultureInfo object set.");
            }
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(SearchIndexCultureInfo info)
        {
            if (info != null)
            {
                // Remove from searchindex cache
                CultureInfo ci = CultureInfoProvider.GetCultureInfo(info.IndexCultureID);
                if (ci != null)
                {
                    SearchIndexInfoProvider.RemoveFromIndexCultureCache(info.IndexID + "_" + ci.CultureCode, info.Generalized.LogWebFarmTasks);
                }

                base.DeleteInfo(info);
            }
        }

        #endregion
    }
}