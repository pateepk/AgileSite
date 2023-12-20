using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Search;
using CMS.Base;

namespace CMS.CustomTables
{
    /// <summary>
    /// Search indexer for custom table search index
    /// </summary>
    public class CustomTableSearchIndexer : SearchIndexer
    {
        /// <summary>
        /// Indexes the custom table
        /// </summary>
        /// <param name="sti">Search task</param>
        protected override void ExecuteProcessTask(SearchTaskInfo sti)
        {
            // Get custom table index ids
            List<int> indexIds = SearchIndexInfoProvider.GetIndexIDs(new List<string> { sti.SearchTaskObjectType }, SearchIndexInfo.LUCENE_SEARCH_PROVIDER);
            if (indexIds != null)
            {
                // Loop thru all user indexes
                foreach (int indexId in indexIds)
                {
                    // Get index info
                    SearchIndexInfo indexInfo = SearchIndexInfoProvider.GetSearchIndexInfo(indexId);
                    if (indexInfo?.IndexSettings?.Items?.Count > 0)
                    {
                        // Get item keys
                        Guid[] keys = null;
                        lock (indexInfo)
                        {
                            keys = new Guid[indexInfo.IndexSettings.Items.Count];
                            indexInfo.IndexSettings.Items.Keys.CopyTo(keys, 0);
                        }

                        string[] value = sti.SearchTaskValue.Split(';');

                        int itemId = ValidationHelper.GetInteger(value[0], 0);
                        string currentItemClassName = ValidationHelper.GetString(value[1], String.Empty);
                        WhereCondition whereCondition = null;
                        bool indexFound = false;
                        bool update = false;

                        // Loop thru all index settings
                        foreach (Guid key in keys)
                        {
                            SearchIndexSettingsInfo sis = indexInfo.IndexSettings.Items[key];
                            if (sis != null && String.Equals(ValidationHelper.GetString(sis.ClassNames, String.Empty), currentItemClassName, StringComparison.InvariantCultureIgnoreCase))
                            {
                                whereCondition = new WhereCondition(sis.WhereCondition);
                                indexFound = true;
                                break;
                            }
                        }

                        if (indexFound)
                        {
                            whereCondition = whereCondition.WhereEquals(sti.SearchTaskField, itemId);

                            var items = CustomTableHelper.GetSearchDocuments(indexInfo, currentItemClassName, whereCondition.ToString(true), 0);
                            if ((items != null) && (items.Count > 0))
                            {
                                // Update
                                SearchHelper.Update(LuceneSearchDocumentHelper.ToLuceneSearchDocument(items[0]), indexInfo);
                                update = true;
                            }
                        }

                        if (!update)
                        {
                            // Delete
                            SearchHelper.Delete(SearchFieldsConstants.ID, sti.SearchTaskValue, indexInfo);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Rebuilds the custom table index
        /// </summary>
        /// <remarks>This method needs to be run in a thread safe way such as smart search task queue.</remarks>
        /// <param name="indexInfo">Search index</param>
        public override void Rebuild(SearchIndexInfo indexInfo)
        {
            // Check whether exist index settings
            if (indexInfo?.IndexSettings?.Items?.Count > 0)
            {
                // Get item keys
                Guid[] keys = null;
                lock (indexInfo)
                {
                    keys = new Guid[indexInfo.IndexSettings.Items.Count];
                    indexInfo.IndexSettings.Items.Keys.CopyTo(keys, 0);
                }

                // Collection of query names and where conditions
                var queries = new List<string[]>();

                // Loop thru all index settings
                foreach (Guid key in keys)
                {
                    // Get settings info
                    SearchIndexSettingsInfo sis = indexInfo.IndexSettings.Items[key];
                    string[] queryWhere = { sis.ClassNames, sis.WhereCondition };

                    if (SearchHelper.IsClassSearchEnabled(sis.ClassNames))
                    {
                        queries.Add(queryWhere);
                    }
                }

                #region "Indexing"

                // Get current index writer
                var iw = indexInfo.Provider.GetWriter(true);

                try
                {
                    if (iw != null)
                    {
                        foreach (string[] query in queries)
                        {
                            // Starts first iteration
                            bool start = true;
                            // Last item id
                            int lastId = 0;
                            // Batch number
                            int batchNumber = 0;
                            // Number of processed items
                            int itemsProcessed = 0;

                            // Documents collection
                            List<SearchDocument> documents = null;

                            // Get forum iDocuments
                            while (((documents != null) && (documents.Count == indexInfo.IndexBatchSize)) || start)
                            {
                                start = false;
                                batchNumber++;
                                Logger.LogCustomTableBatchStart(batchNumber, lastId, query[0]);

                                // Get data
                                documents = CustomTableHelper.GetSearchDocuments(indexInfo, query[0], query[1], lastId);

                                if ((documents != null) && (documents.Count > 0))
                                {
                                    string[] key = ValidationHelper.GetString(documents[documents.Count - 1].GetValue(SearchFieldsConstants.ID), String.Empty).Split(';');

                                    // Get last ID  (document id)
                                    lastId = ValidationHelper.GetInteger(key[0], 1);

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
                    // Close index
                    iw?.Close();
                }

                #endregion

                SearchHelper.FinishRebuild(indexInfo);
            }
        }


        /// <summary>
        /// Loads the results to the given result collection
        /// </summary>
        /// <param name="results">Collection of results to load</param>
        /// <param name="result">Dictionary of the loaded results indexed by their key</param>
        public override void LoadResults(IEnumerable<ILuceneSearchDocument> results, SafeDictionary<string, DataRow> result)
        {
            var customTables = new SafeDictionary<string, string>();

            // Load custom table items
            foreach (var filteredDocument in results)
            {
                string[] value = ValidationHelper.GetString(filteredDocument.Get(SearchFieldsConstants.ID), String.Empty).Split(';');
                customTables[value[1]] = Convert.ToString(customTables[value[1]]) + "," + value[0];
            }


            // Check whether custom tables are in result set
            if (customTables.Count > 0)
            {
                // Loop thru all classes
                foreach (DictionaryEntry entry in customTables)
                {
                    string className = entry.Key.ToString();
                    var ti = CustomTableItemProvider.GetTypeInfo(className);

                    // Generate where condition
                    string where = $"{ti.IDColumn} IN ({entry.Value.ToString().TrimStart(',')})";

                    // Load data
                    DataSet tableDs = CustomTableItemProvider.GetItems(className, where);

                    // Check whether result contains at least one record
                    if (!DataHelper.DataSourceIsEmpty(tableDs))
                    {
                        // Add data rows to the current search result storage
                        foreach (DataRow dr in tableDs.Tables[0].Rows)
                        {
                            result[dr[ti.IDColumn] + ";" + className.ToLowerInvariant() + "_" + CustomTableInfo.OBJECT_TYPE_CUSTOMTABLE] = dr;
                        }
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
            return DataClassInfoProvider.GetDataClassInfo(Convert.ToString(obj.Get("_customtablename")));
        }


        /// <summary>
        /// Gets the collection of search fields. When no SearchFields colection is provided, new is created.
        /// </summary>
        /// <param name="index">Search index</param>
        /// <param name="searchFields">Search fields collection</param>
        public override ISearchFields GetSearchFields(SearchIndexInfo index, ISearchFields searchFields = null)
        {
            searchFields = searchFields ?? new SearchFields(false);

            foreach (var setting in index.IndexSettings.Items.Values)
            {
                try
                {
                    CustomTableItem.New(setting.ClassNames).GetSearchFields(index, searchFields);
                }
                catch
                {
                    // Creating custom table item can throw exception when data class is incorrect or no longer exists
                }
            }

            return searchFields;
        }
    }
}
