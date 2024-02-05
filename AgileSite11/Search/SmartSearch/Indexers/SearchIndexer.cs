using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;
using CMS.EventLog;
using CMS.Localization;
using CMS.Search.Internal;

namespace CMS.Search
{
    /// <summary>
    /// Base class for search indexer
    /// </summary>
    public class SearchIndexer
    {
        private readonly Lazy<IndexLogger> mLogger = new Lazy<IndexLogger>();


        /// <summary>
        /// Helps with retrieving<see cref="ISearchable"/> objects and <see cref="SearchIndexInfo"/> s
        /// related to search tasks.
        /// </summary>
        protected SearchablesRetriever SearchablesRetriever
        {
            get;
            set;
        }


        /// <summary>
        /// Logs actions related to indexes and search tasks.
        /// </summary>
        protected IndexLogger Logger => mLogger.Value;


        /// <summary>
        /// General search indexer.
        /// </summary>
        /// <param name="searchablesRetriever">Helps retrieving objects related to indexing.</param>
        protected SearchIndexer(SearchablesRetriever searchablesRetriever)
        {
            SearchablesRetriever = searchablesRetriever;
        }


        /// <summary>
        /// Search indexer. 
        /// </summary>
        public SearchIndexer()
            : this(new SearchablesRetriever())
        { }


        #region "Task execution methods"

        /// <summary>
        /// Executes the search index task
        /// </summary>
        /// <param name="sti">Search task</param>
        public virtual void ExecuteTask(SearchTaskInfo sti)
        {
            Logger.LogTaskStart(sti);

            try
            {
                switch (sti.SearchTaskType)
                {
                    case SearchTaskTypeEnum.Rebuild:
                        ExecuteRebuildTask(sti);
                        break;

                    case SearchTaskTypeEnum.Optimize:
                        ExecuteOptimizeTask(sti);
                        break;

                    case SearchTaskTypeEnum.Delete:
                        ExecuteDeleteTask(sti);
                        break;

                    case SearchTaskTypeEnum.Update:
                        ExecuteUpdateTask(sti);
                        break;

                    case SearchTaskTypeEnum.Process:
                        ExecuteProcessTask(sti);
                        break;
                }
            }
            catch
            {
                Logger.LogMessage(LocalizationHelper.GetString("smartsearch.taskfinishederror"));
                throw;
            }

            Logger.LogMessage(LocalizationHelper.GetString("smartsearch.taskfinishedok"));
        }


        /// <summary>
        /// Executes the search index update task
        /// </summary>
        /// <param name="sti">Search task</param>
        protected virtual void ExecuteUpdateTask(SearchTaskInfo sti)
        {
            var selectId = ValidationHelper.GetInteger(sti.SearchTaskValue, 0);

            DocumentUpdate(selectId, sti);
        }


        /// <summary>
        /// Update specified document 
        /// </summary>
        /// <param name="documentId">Document id</param>
        /// <param name="taskInfo">Search task</param>
        protected void DocumentUpdate(int documentId, SearchTaskInfo taskInfo)
        {
            var documents = SelectSearchDocument(documentId);

            foreach (ISearchable document in documents)
            {
                // Get relevant Lucene indexes for document
                var indexes = SearchIndexInfoProvider.GetRelevantIndexes(document, SearchIndexInfo.LUCENE_SEARCH_PROVIDER);

                if (indexes == null)
                {
                    continue;
                }

                // Loop trough all relevant indexes
                foreach (SearchIndexInfo index in indexes)
                {
                    // Don't process if task was created before index rebuild time
                    if (index.ActualRebuildTime > taskInfo.SearchTaskCreated)
                    {
                        continue;
                    }

                    // Get search document
                    var searchDocument = document.GetSearchDocument(index);

                    // Get document property "DocumentSearchExcluded"
                    bool isExcluded = searchDocument.Contains(SearchDocument.DOCUMENT_EXCLUDED_FROM_SEARCH_FIELD)
                        && ValidationHelper.GetBoolean(searchDocument.GetValue(SearchDocument.DOCUMENT_EXCLUDED_FROM_SEARCH_FIELD), false);

                    if (isExcluded)
                    {
                        // Document is excluded from search => delete
                        SearchHelper.Delete(taskInfo.SearchTaskField, (string)searchDocument.GetValue(SearchFieldsConstants.ID), index);
                    }
                    else
                    {
                        var luceneDocument = LuceneSearchDocumentHelper.ToLuceneSearchDocument(searchDocument);
                        SearchHelper.Update(luceneDocument, index);
                    }
                }
            }
        }


        /// <summary>
        /// Selects the search document based on the given ID
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <returns>Enumeration of searchable objects or empty enumeration.</returns>
        public virtual IEnumerable<ISearchable> SelectSearchDocument(int documentId)
        {
            return Enumerable.Empty<ISearchable>();
        }


        /// <summary>
        /// Executes the search index delete task
        /// </summary>
        /// <param name="sti">Search task</param>
        protected virtual void ExecuteDeleteTask(SearchTaskInfo sti)
        {
            // If fieldname is not specified use ID field
            if (String.IsNullOrEmpty(sti.SearchTaskField))
            {
                sti.SearchTaskField = SearchFieldsConstants.ID;
            }

            // Get all indexes for current type
            DataSet dsIndexes = SearchablesRetriever.GetRelevantIndexes(sti.SearchTaskObjectType, SearchIndexInfo.LUCENE_SEARCH_PROVIDER);
            if (DataHelper.DataSourceIsEmpty(dsIndexes))
            {
                return;
            }

            // Loop trhu all indexes
            foreach (DataRow indexRow in dsIndexes.Tables[0].Rows)
            {
                int searchIndexId = ValidationHelper.GetInteger(indexRow["IndexID"], 0);

                SearchIndexInfo index = SearchIndexInfoProvider.GetSearchIndexInfo(searchIndexId);

                // Don't process if task was created before index rebuild time
                if (index.ActualRebuildTime <= sti.SearchTaskCreated)
                {
                    DeleteItem(sti, index);
                }
            }
        }


        /// <summary>
        /// Deletes specific item from the index
        /// </summary>
        /// <param name="taskInfo">Search task</param>
        /// <param name="indexInfo">Search index</param>
        protected virtual void DeleteItem(SearchTaskInfo taskInfo, SearchIndexInfo indexInfo)
        {
            SearchHelper.Delete(taskInfo.SearchTaskField, taskInfo.SearchTaskValue, indexInfo);
        }


        /// <summary>
        /// Executes the search optimize task
        /// </summary>
        /// <param name="sti">Search task</param>
        protected virtual void ExecuteOptimizeTask(SearchTaskInfo sti)
        {
            // Check whether index name is defined
            var indexName = ValidationHelper.GetString(sti.SearchTaskValue, String.Empty);
            if (!String.IsNullOrEmpty(indexName) && (indexName.ToLowerCSafe() != "##all##"))
            {
                // Try get index info
                var sii = SearchIndexInfoProvider.GetSearchIndexInfo(indexName);

                // Check whether info exists and index wasn't rebuilded
                if ((sii != null) && (sii.ActualRebuildTime <= sti.SearchTaskCreated))
                {
                    SearchHelper.Optimize(sii);
                }
            }
            // Optimize all indexes
            else
            {
                // Load search indexes
                DataSet indexes = SearchIndexInfoProvider.GetSearchIndexes().Column("IndexID");
                // Check whether exists at least one index
                if (DataHelper.DataSourceIsEmpty(indexes))
                {
                    return;
                }

                // loop thru all indexes
                foreach (DataRow inxDr in indexes.Tables[0].Rows)
                {
                    SearchIndexInfo cSii = SearchIndexInfoProvider.GetSearchIndexInfo(ValidationHelper.GetInteger(inxDr["IndexID"], 0));
                    if (cSii != null)
                    {
                        SearchHelper.Optimize(cSii);
                    }
                }
            }
        }


        /// <summary>
        /// Executes the search index rebuild task
        /// </summary>
        /// <param name="taskInfo">Search task</param>
        protected virtual void ExecuteRebuildTask(SearchTaskInfo taskInfo)
        {
            SearchIndexInfo indexInfo = SearchIndexInfoProvider.GetSearchIndexInfo(taskInfo.SearchTaskValue);

            // Check whether info exists and index wasn't rebuilded
            if ((indexInfo != null) && (indexInfo.ActualRebuildTime <= taskInfo.SearchTaskCreated))
            {
                SearchHelper.Rebuild(indexInfo);
            }
        }


        /// <summary>
        /// Processes the search index task
        /// </summary>
        /// <param name="sti">Search task</param>
        protected virtual void ExecuteProcessTask(SearchTaskInfo sti)
        {
            // Get object index ids
            List<int> indexIds = SearchIndexInfoProvider.GetIndexIDs(SearchIndexInfoProvider.GeneralIndexTypeList, SearchIndexInfo.LUCENE_SEARCH_PROVIDER);
            if (indexIds == null)
            {
                return;
            }

            // Loop through all general indexes
            foreach (int indexId in indexIds)
            {
                // Get index info
                SearchIndexInfo indexInfo = SearchIndexInfoProvider.GetSearchIndexInfo(indexId);

                if (!SearchablesRetriever.IsObjectTypeIndexed(indexInfo, sti.SearchTaskObjectType))
                {
                    continue;
                }

                var searchable = SearchablesRetriever.GetSearchableObject(indexInfo, sti.SearchTaskObjectType, sti.SearchTaskValue, sti.SearchTaskField);
                if (searchable != null)
                {
                    var luceneDocument = LuceneSearchDocumentHelper.ToLuceneSearchDocument(searchable.GetSearchDocument(indexInfo));
                    SearchHelper.Update(luceneDocument, indexInfo);
                }
                else
                {
                    SearchHelper.Delete(SearchFieldsConstants.ID, sti.SearchTaskValue, indexInfo);
                }
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Checks if given <paramref name="className"/> is related to search index settings from <paramref name="indexSettings"/>.
        /// </summary>
        /// <param name="className">Class name</param>
        /// <param name="indexSettings">Search index settings</param>
        public virtual bool IsClassNameRelevantToIndex(string className, SearchIndexSettings indexSettings)
        {
            return indexSettings.Items.Values.Any(item => item.ClassNames.Equals(className, StringComparison.InvariantCultureIgnoreCase));
        }


        /// <summary>
        /// Rebuilds the index
        /// </summary>
        /// <remarks>This method needs to be run in a thread safe way such as smart search task queue.</remarks>
        /// <param name="indexInfo">Search index</param>
        public virtual void Rebuild(SearchIndexInfo indexInfo)
        {
            if (indexInfo.IndexSettings == null)
            {
                return;
            }

            // Get current index writer
            var iw = indexInfo.Provider.GetWriter(true);
            if (iw != null)
            {
                try
                {
                    foreach (var searchable in SearchablesRetriever.GetSearchableObjects(indexInfo))
                    {
                        iw.AddDocument(LuceneSearchDocumentHelper.ToLuceneSearchDocument(searchable.GetSearchDocument(indexInfo)));
                    }

                    iw.Flush();

                    // Optimize index
                    SearchIndexInfoProvider.SetIndexStatus(indexInfo, IndexStatusEnum.OPTIMIZING);
                    iw.Optimize();
                }
                catch (Exception e)
                {
                    LogContext.LogEventToCurrent(EventType.WARNING, "Smart search", "INDEXERROR", "SearchIndexer.Rebuild() causes exception and search index status is set to ERROR.\nIndex name: " + indexInfo.IndexName + "\n\nException: " + e.Message + "\n\nStack trace: " + e.StackTrace, null, 0, null, 0, null, null, 0, null, null, null, DateTime.Now);

                    // Set statuses to error
                    SearchIndexInfoProvider.SetIndexStatus(indexInfo, IndexStatusEnum.ERROR);
                    throw;
                }
                finally
                {
                    // Close index
                    iw.Close();
                }
            }

            SearchHelper.FinishRebuild(indexInfo);
        }


        /// <summary>
        /// Rebuilds part of the index
        /// </summary>
        /// <param name="taskInfo">Search task</param>
        public virtual void PartialRebuild(SearchTaskInfo taskInfo)
        {
            // No partial rebuild support by default
        }


        /// <summary>
        /// Checks the permissions for the given result document
        /// </summary>
        /// <param name="settings">Check permission settings</param>
        /// <param name="currentDoc">Current result document</param>
        /// <param name="index">Current document index</param>
        public virtual bool CheckResultPermissions(SearchResults settings, ILuceneSearchDocument currentDoc, int index)
        {
            return true;
        }


        /// <summary>
        /// Loads the results to the given result collection
        /// </summary>
        /// <param name="results">Collection of results to load</param>
        /// <param name="result">Dictionary of the loaded results indexed by their key</param>
        public virtual void LoadResults(IEnumerable<ILuceneSearchDocument> results, SafeDictionary<string, DataRow> result)
        {
            var generalObjects = new SafeDictionary<string, string>();

            // Load custom table items
            foreach (var filteredDocument in results)
            {
                string className = ValidationHelper.GetString(filteredDocument.Get(SearchFieldsConstants.TYPE), String.Empty);
                if (className.Equals(SearchHelper.CUSTOM_SEARCH_INDEX, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                string key = className + ";" + filteredDocument.Get(SearchFieldsConstants.IDCOLUMNNAME);
                string currentIds = Convert.ToString(generalObjects[key]);
                generalObjects[key] = currentIds + "," + ValidationHelper.GetString(filteredDocument.Get(SearchFieldsConstants.ID), String.Empty);
            }

            // Check whether exists at least one general object in current search results
            if (generalObjects.Count <= 0)
            {
                return;
            }

            // loop thru all general objects
            foreach (DictionaryEntry entry in generalObjects)
            {
                string[] classNameIdColumn = entry.Key.ToString().Split(';');
                string className = classNameIdColumn[0];
                string idColumn = classNameIdColumn[1];

                string objType = null;
                string[] items = entry.Value.ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < items.Length; i++)
                {
                    items[i] = SearchablesRetriever.GetObjectID(items[i], out objType);
                }

                string where = string.Join(",", items);

                // Try to load data
                DataSet generalDs = ConnectionHelper.ExecuteQuery(className + ".selectall", null, idColumn + " IN (" + @where + ")");
                if (!DataHelper.DataSourceIsEmpty(generalDs))
                {
                    // Loop thru all results
                    foreach (DataRow dr in generalDs.Tables[0].Rows)
                    {
                        result[dr[idColumn] + (string.IsNullOrEmpty(objType) ? "" : ";" + objType) + "_" + className] = dr;
                    }
                }
            }
        }


        /// <summary>
        /// Fills the data to the search result data row. Returns true, if the data was correctly filled
        /// </summary>
        /// <param name="dr">Result data row to be filled with post-processed results</param>
        /// <param name="resultDr">Source data row with raw search results</param>
        /// <param name="doc">Found search document</param>
        public virtual bool FillSearchResult(DataRow dr, DataRow resultDr, ILuceneSearchDocument doc)
        {
            // Get class info for current item
            var dci = GetDataClassInfo(doc);
            if ((dci == null) || (resultDr == null))
            {
                return false;
            }

            dr["title"] = DataHelper.GetDataRowValue(resultDr, dci.ClassSearchTitleColumn);
            dr["content"] = DataHelper.GetDataRowValue(resultDr, dci.ClassSearchContentColumn);
            dr["created"] = DataHelper.GetDataRowValue(resultDr, dci.ClassSearchCreationDateColumn);
            dr["image"] = DataHelper.GetDataRowValue(resultDr, dci.ClassSearchImageColumn);

            return true;
        }


        /// <summary>
        /// Returns the data class info for the given search document
        /// </summary>
        /// <param name="obj">Document object</param>
        public virtual DataClassInfo GetDataClassInfo(ILuceneSearchDocument obj)
        {
            return DataClassInfoProvider.GetDataClassInfo(Convert.ToString(obj.Get(SearchFieldsConstants.TYPE)));
        }


        /// <summary>
        /// Returns URL to current search result item.
        /// </summary>
        /// <param name="id">ID of the item</param>
        /// <param name="type">Type of the index</param>
        /// <param name="imageUrl">Image</param>
        public virtual string GetSearchImageUrl(string id, string type, string imageUrl)
        {
            if (!String.IsNullOrEmpty(type))
            {
                var info = GetObjectByClassName(type) as ISearchable;

                // Try to get object type-specific image
                if (info != null)
                {
                    imageUrl = info.GetSearchImageUrl(id, imageUrl);
                }
            }

            return imageUrl;
        }


        /// <summary>
        /// Returns column value for current search result item.
        /// </summary>
        /// <param name="id">ID of the item</param>
        /// <param name="columnName">Column name</param>
        public static object GetSearchValue(string id, string columnName)
        {
            return SearchContext.GetSearchValue(id, columnName);
        }


        /// <summary>
        /// Gets the collection of search fields. When no SearchFields collection is provided, new is created.
        /// </summary>
        /// <param name="index">Search index</param>
        /// <param name="searchFields">Search fields collection</param>
        public virtual ISearchFields GetSearchFields(SearchIndexInfo index, ISearchFields searchFields = null)
        {
            searchFields = searchFields ?? new SearchFields(false);

            // Get search fields for each object type
            foreach (var settings in index.IndexSettings.Items.Values)
            {
                var searchable = ModuleManager.GetObject(settings.ClassNames) as ISearchable;

                if (searchable != null)
                {
                    searchable.GetSearchFields(index, searchFields);
                }
            }

            return searchFields;
        }


        /// <summary>
        /// Gets a new default object representation of the given class name.
        /// </summary>
        /// <param name="className">Class name</param>
        private static BaseInfo GetObjectByClassName(string className)
        {
            var classInfo = DataClassInfoProvider.GetDataClassInfo(className);
            if (classInfo != null)
            {
                string objectType = !String.IsNullOrEmpty(classInfo.ClassDefaultObjectType) ? classInfo.ClassDefaultObjectType : classInfo.ClassName;
                return ModuleManager.GetObject(objectType);
            }

            return null;
        }

        #endregion
    }
}
