using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Localization;

namespace CMS.Search.Internal
{
    /// <summary>
    /// Helps with retrieving <see cref="ISearchable"/> objects and <see cref="SearchIndexInfo"/>s
    /// related to search tasks.
    /// </summary>
    /// <seealso cref="SearchablesRetrievers.Register{TRetriever}(string)"/>
    public class SearchablesRetriever
    {
        private readonly IndexLogger mLogger = new IndexLogger();


        /// <summary>
        /// Logs progress of a search task.
        /// </summary>
        protected IndexLogger Logger => mLogger;


        /// <summary>
        /// Gets relevant indexes for given <paramref name="objectType"/>, <paramref name="siteName"/> and <paramref name="searchProvider"/>.
        /// </summary>
        /// <param name="objectType">Defines object type that index should cover.</param>
        /// <param name="siteName">Only indexes on given site are checked. If not specified then indexes through all sites are checked if they are relevant.</param>
        /// <param name="searchProvider">
        /// Defines search provider for which to return relevant indexes.
        /// If not defined then indexes for all search providers are returned.
        /// </param>
        public virtual IEnumerable<SearchIndexInfo> GetRelevantIndexes(string objectType, string siteName, string searchProvider)
        {
            throw new NotImplementedException("Getting all relevant indexes is not supported by this class.");
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
        public virtual List<SearchIndexInfo> GetRelevantIndexes(ISearchable searchObject, string searchProvider)
        {
            throw new NotImplementedException($"Getting all relevant indexes for given {nameof(searchObject)} and {nameof(searchProvider)} is not supported by this class.");
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
        public virtual DataSet GetRelevantIndexes(string taskType, string searchProvider)
        {
            var where = new WhereCondition().WhereEquals(nameof(SearchIndexInfo.IndexType), taskType);
            if (!String.IsNullOrEmpty(searchProvider))
            {
                where.And().WhereEquals(nameof(SearchIndexInfo.IndexProvider), searchProvider);
            }

            return SearchIndexInfoProvider.GetSearchIndexes().Where(where.ToString(true)).Column("IndexID");
        }


        /// <summary>
        /// Indicates whether <paramref name="objectType"/> is to be included in <paramref name="index"/>.
        /// </summary>
        /// <param name="index">Index for which to perform the test.</param>
        /// <param name="objectType">Object type whose presence in <paramref name="index"/> to test.</param>
        /// <returns>True if <paramref name="objectType"/> is to be included in <paramref name="index"/> based on index settings, otherwise false.</returns>
        public bool IsObjectTypeIndexed(SearchIndexInfo index, string objectType)
        {
            foreach (var sisi in index?.IndexSettings?.Items?.Values)
            {
                if (String.Equals(sisi.ClassNames, objectType, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Returns <see cref="ISearchable"/> object for given <paramref name="index"/>
        /// that satisfies conditions given by <paramref name="objectType"/>, <paramref name="value"/> and <paramref name="field"/>.
        /// Returns null if no <see cref="ISearchable"/> object satisfies given conditions.
        /// </summary>
        /// <param name="index">Index for which to return <see cref="ISearchable"/> object.</param>
        /// <param name="objectType">Returned <see cref="ISearchable"/> object with given <paramref name="objectType"/>.</param>
        /// <param name="value">Returned <see cref="ISearchable"/> object with given <paramref name="value"/> in given <paramref name="field"/>.</param>
        /// <param name="field">Defines where to find <paramref name="value"/> and in what form to expect given data for a <see cref="ISearchable"/> object.</param>
        /// <seealso cref="IsObjectTypeIndexed"/>
        public ISearchable GetSearchableObject(SearchIndexInfo index, string objectType, string value, string field)
        {
            foreach (var sisi in index?.IndexSettings?.Items?.Values)
            {
                // Check whether the current index is specified for the current class
                if (!String.Equals(sisi.ClassNames, objectType, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                // Get the object ID (in case of BizFormItems, the SearchID is a compound value of ID and ObjectType)
                int itemId = ValidationHelper.GetInteger(GetObjectID(value), 0);

                // Generate a where condition
                string whereCondition = sisi.WhereCondition;
                whereCondition = SqlHelper.AddWhereCondition(new WhereCondition().WhereEquals(field, itemId).ToString(true), whereCondition);

                // Check whether exists an appropriate object for the current conditions
                DataSet objectDs = null;
                BaseInfo info = ModuleManager.GetReadOnlyObject(objectType);
                if (info != null)
                {
                    objectDs = info.Generalized.GetData(null, whereCondition, null, 1);
                }

                var searchable = objectDs?.Tables[0]?.Rows?.Count > 0 ? ModuleManager.GetObject(objectDs.Tables[0].Rows[0], objectType) as ISearchable : null;
                if (searchable == null)
                {
                    continue;
                }

                return searchable;
            }

            return null;
        }


        /// <summary>
        /// Returns collection of all <see cref="ISearchable"/> objects for given <paramref name="indexInfo"/>.
        /// </summary>
        /// <param name="indexInfo"><see cref="SearchIndexInfo"/> for which to return collection of <see cref="ISearchable"/>s.</param>
        /// <seealso cref="ISearchable.GetSearchDocument(ISearchIndexInfo)"/>
        public virtual IEnumerable<ISearchable> GetSearchableObjects(SearchIndexInfo indexInfo)
        {
            if (indexInfo.IndexSettings == null)
            {
                yield break;
            }

            foreach (SearchIndexSettingsInfo sisi in indexInfo.IndexSettings.Items.Values)
            {
                BaseInfo info = ModuleManager.GetObject(sisi.ClassNames);
                if (info == null)
                {
                    continue;
                }

                var ti = info.TypeInfo;
                if (!SearchHelper.IsClassSearchEnabled(ti.ObjectClassName))
                {
                    continue;
                }

                // Get primary key
                string primaryKey = ti.IDColumn;
                if (String.IsNullOrEmpty(primaryKey))
                {
                    continue;
                }

                string whereCondition = sisi.WhereCondition;
                string idColumn = Convert.ToString(primaryKey);
                int batchSize = indexInfo.IndexBatchSize;
                bool multiplePKs = idColumn.Contains(";");

                // Starts first iteration
                bool start = true;
                // Number of batch
                int batchNumber = 0;
                // Number of processed items
                int itemsProcessed = 0;
                // Last item id
                int lastId = 0;

                List<ISearchable> searchableObjects = null;

                while (((searchableObjects != null) && (searchableObjects.Count == indexInfo.IndexBatchSize)) || start)
                {
                    // Log start of batch
                    start = false;
                    batchNumber++;

                    if (indexInfo.IndexType == SearchHelper.ONLINEFORMINDEX)
                    {
                        LogFormBatchStart(batchNumber, lastId, info.Generalized.ObjectDisplayName);
                    }
                    else
                    {
                        Logger.LogBatchStart(batchNumber, lastId, ti.ObjectType);
                    }

                    searchableObjects = null;

                    DataSet ds;

                    if (!multiplePKs)
                    {
                        // Load top n rows for current object
                        // Select from last item
                        string scopeWhere = SqlHelper.AddWhereCondition(idColumn + " > " + lastId, whereCondition);
                        ds = info.Generalized.GetData(null, scopeWhere, idColumn, batchSize);
                    }
                    else
                    {
                        // If the table has multiple PKs, disable the batch size
                        ds = info.Generalized.GetData(null, whereCondition, idColumn, -1);
                    }

                    if (!DataHelper.DataSourceIsEmpty(ds))
                    {
                        searchableObjects = new List<ISearchable>();

                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            ISearchable obj = ModuleManager.GetObject(dr, ti.ObjectType) as ISearchable;
                            if (obj != null)
                            {
                                searchableObjects.Add(obj);
                            }
                        }
                    }

                    if ((searchableObjects != null) && (searchableObjects.Any()))
                    {
                        // Get the last object ID (in case of BizFormItems, the SearchID is a compound value of ID and ObjectType)
                        string itemId = searchableObjects.Last().GetSearchID();
                        itemId = GetObjectID(itemId);

                        // Get last ID  (document id)
                        lastId = ValidationHelper.GetInteger(itemId, 1);

                        foreach (var searchableObj in searchableObjects)
                        {
                            yield return searchableObj;
                        }

                        itemsProcessed += searchableObjects.Count;
                    }

                    // Log end of batch
                    Logger.LogBatchEnd(itemsProcessed);

                    // If the table has multiple PKs, disable the batch size (run the query just once)
                    if (multiplePKs)
                    {
                        break;
                    }
                }
            }
        }


        /// <summary>
        /// Returns object ID from potentially compound ID (Id;objecttype in case of BizForms).
        /// </summary>
        /// <param name="itemId">Object ID</param>
        /// <param name="objectType">Object type received from compound key</param>
        internal string GetObjectID(string itemId, out string objectType)
        {
            objectType = null;

            var result = itemId;

            string[] value = itemId.Split(';');
            if (value.Length != 2)
            {
                return result;
            }

            result = value[0];
            objectType = value[1];
            return result;
        }


        #region "Private methods"

        /// <summary>
        /// Returns object ID from potentially compound ID.
        /// </summary>
        /// <param name="itemId">Object ID</param>
        private string GetObjectID(string itemId)
        {
            string objType;

            return GetObjectID(itemId, out objType);
        }


        /// <summary>
        /// Logs start of batch processing. 
        /// </summary>
        /// <param name="batchNumber">Number of batch.</param>
        /// <param name="lastID">ID of last item that is not present in the batch.</param>
        /// <param name="formDisplayName">Form name</param>
        private void LogFormBatchStart(int batchNumber, int lastID, string formDisplayName)
        {
            if (batchNumber == 1)
            {
                Logger.LogMessage(LocalizationHelper.GetStringFormat("smartsearch.taskbatch.startform", formDisplayName));
            }
            else
            {
                Logger.LogBatchStart(batchNumber, lastID);
            }
        }

        #endregion   
    }
}
