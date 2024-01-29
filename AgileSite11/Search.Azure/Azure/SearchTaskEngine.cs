using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Search.Internal;

using Microsoft.Azure.Search.Models;
using Index = Microsoft.Azure.Search.Models.Index;

namespace CMS.Search.Azure
{
    /// <summary>
    /// Search task engine.
    /// </summary>
    internal class SearchTaskEngine : ISearchTaskEngine
    {
        private FieldPropertiesEqualityComparer mFieldComparer;


        /// <summary>
        /// Comparer to compare two Azure index <see cref="Field"/>s.
        /// </summary>
        private FieldPropertiesEqualityComparer FieldComparer
        {
            get
            {
                return mFieldComparer ?? (mFieldComparer = new FieldPropertiesEqualityComparer());
            }
        }


        /// <summary>
        /// Processes <paramref name="task"/> according to it's <see cref="SearchTaskAzureInfo.SearchTaskAzureType"/>.
        /// </summary>
        /// <param name="task">Search task to process.</param>
        public void ProcessAzureSearchTask(SearchTaskAzureInfo task)
        {
            switch (task.SearchTaskAzureType)
            {
                case SearchTaskTypeEnum.Rebuild:
                    ExecuteRebuildTask(task);
                    break;

                case SearchTaskTypeEnum.Update:
                    ExecuteUpdateTask(task);
                    break;

                case SearchTaskTypeEnum.Delete:
                    ExecuteDeleteTask(task);
                    break;

                case SearchTaskTypeEnum.Process:
                    ExecuteProcessTask(task);
                    break;
            }
        }


        /// <summary>
        /// Executes process task for Azure index by modifying some of it's content.
        /// </summary>
        private void ExecuteProcessTask(SearchTaskAzureInfo task)
        {
            if (task.SearchTaskAzureObjectType == PredefinedObjectType.CATEGORY)
            {
                ProcessDocumentCategoryChange(task);

                return;
            }


            var indexes = SearchIndexInfoProvider.GetIndexIDs(SearchIndexInfoProvider.GeneralIndexTypeList, SearchIndexInfo.AZURE_SEARCH_PROVIDER)
                ?.Select(SearchIndexInfoProvider.GetSearchIndexInfo)
                .Where(index => index?.IndexSettings?.Items != null);

            var searchableProvider = SearchablesRetrievers.Get(task.SearchTaskAzureObjectType);

            foreach (var indexInfo in indexes ?? Enumerable.Empty<SearchIndexInfo>())
            {
                if (!searchableProvider.IsObjectTypeIndexed(indexInfo, task.SearchTaskAzureObjectType))
                {
                    continue;
                }

                var searchServiceManager = CreateSearchServiceManager(SearchService.FromAdminApiKey(indexInfo.IndexSearchServiceName, indexInfo.IndexAdminKey));
                if (!searchServiceManager.IndexExists(indexInfo.IndexName))
                {
                    continue;
                }

                var existingIndex = searchServiceManager.GetIndex(indexInfo.IndexName);
                var searchable = searchableProvider.GetSearchableObject(indexInfo, task.SearchTaskAzureObjectType, task.SearchTaskAzureAdditionalData, task.SearchTaskAzureMetadata);

                UpdateIndex(indexInfo, new[] { searchable }, searchableObj =>
                {
                    if (searchableObj == null)
                    {
                        return IndexAction.Delete(NamingHelper.GetValidFieldName(SearchFieldsConstants.ID), task.SearchTaskAzureAdditionalData);
                    }

                    var document = searchableObj.GetSearchDocument(indexInfo);
                    var azureDoc = DocumentCreator.Instance.CreateDocument(document, searchableObj, indexInfo);

                    return IndexAction.MergeOrUpload(azureDoc);
                }, existingIndex);
            }
        }


        /// <summary>
        /// Re-indexes all documents containing updated category to reflect its name change.
        /// </summary>
        private void ProcessDocumentCategoryChange(SearchTaskAzureInfo task)
        {
            List<int> indexIds = SearchIndexInfoProvider.GetIndexIDs(new List<string> { PredefinedObjectType.DOCUMENT }, SearchIndexInfo.AZURE_SEARCH_PROVIDER);
            if (indexIds == null)
            {
                return;
            }

            var updatedDocumentIds = new HashSet<int>();
            var indexes = indexIds.Select(SearchIndexInfoProvider.GetSearchIndexInfo).Where(index => index != null);
            foreach (var index in indexes)
            {
                var searchServiceManager = CreateSearchServiceManager(SearchService.FromAdminApiKey(index.IndexSearchServiceName, index.IndexAdminKey));
                var documentKeys = SearchTaskEngineUtils.GetAllDocumentIdsByCategoryId(index.IndexName, task.SearchTaskAzureAdditionalData, searchServiceManager);
                var documentIds = documentKeys.Select(key => Int32.Parse(key.Split(new char[] { '-' }, 2).First())).Distinct().Where(id => !updatedDocumentIds.Contains(id)).ToList();

                UpdateAffectedDocuments(documentIds, task.SearchTaskAzureCreated);
                updatedDocumentIds.AddRange(documentIds);
            }
        }


        /// <summary>
        /// Updates content of document indexes based on enumeration of changed documents.
        /// </summary>
        private void UpdateAffectedDocuments(IEnumerable<int> documentIds, DateTime taskCreated)
        {
            foreach (var documentId in documentIds)
            {
                foreach (var indexWithSearchables in GetIndexesWithSearchableObjectsForDocumentUpdate(PredefinedObjectType.DOCUMENT, documentId))
                {
                    var index = indexWithSearchables.Key;
                    var searchables = indexWithSearchables.Value;

                    ExecuteUpdateOnIndex(index, searchables, taskCreated);
                }
            }
        }


        /// <summary>
        /// Executes delete task for Azure index by removing some of it's content.
        /// </summary>
        private void ExecuteDeleteTask(SearchTaskAzureInfo task)
        {
            var searchableProvider = SearchablesRetrievers.Get(task.SearchTaskAzureObjectType);
            var dsIndexes = searchableProvider.GetRelevantIndexes(task.SearchTaskAzureObjectType, SearchIndexInfo.AZURE_SEARCH_PROVIDER);

            foreach (DataRow indexRow in dsIndexes.Tables[0].Rows)
            {
                int searchIndexId = ValidationHelper.GetInteger(indexRow["IndexID"], 0);
                SearchIndexInfo index = SearchIndexInfoProvider.GetSearchIndexInfo(searchIndexId);

                if (index != null && SearchTaskEngineUtils.TaskWasCreatedAfterIndexRebuild(task.SearchTaskAzureCreated, index))
                {
                    var searchServiceManager = CreateSearchServiceManager(SearchService.FromAdminApiKey(index.IndexSearchServiceName, index.IndexAdminKey));
                    if (!searchServiceManager.IndexExists(index.IndexName))
                    {
                        continue;
                    }

                    ExecuteDeleteTaskForIndex(task, index, searchServiceManager);
                }
            }
        }


        /// <summary>
        /// Executes delete task for given Azure <paramref name="index"/> by removing some of it's content.
        /// </summary>
        /// <param name="task">Delete search task to execute.</param>
        /// <param name="index">Index to perform delete operation on.</param>
        /// <param name="searchServiceManager">Search service manager.</param>
        private static void ExecuteDeleteTaskForIndex(SearchTaskAzureInfo task, SearchIndexInfo index, SearchServiceManager searchServiceManager)
        {
            if (task.SearchTaskAzureMetadata == SearchFieldsConstants.PARTIAL_REBUILD)
            {
                DeleteDocumentsFromSiteInSubTree(task, index, searchServiceManager);
            }
            else if (task.SearchTaskAzureMetadata == SearchFieldsConstants.SITE)
            {
                DeleteDocumentsFromSite(task, index, searchServiceManager);
            }
            else
            {
                DeleteOneDocument(task, index, searchServiceManager);
            }
        }


        /// <summary>
        /// Deletes documents from Azure on a site under given node alias path (specified in additional task data).
        /// </summary>
        /// <param name="task">Delete search task to execute.</param>
        /// <param name="index">Index to perform delete operation on.</param>
        /// <param name="searchServiceManager">Search service manager.</param>
        private static void DeleteDocumentsFromSiteInSubTree(SearchTaskAzureInfo task, SearchIndexInfo index, SearchServiceManager searchServiceManager)
        {
            var siteNode = new SiteNodeIdentifier(task.SearchTaskAzureAdditionalData);
            var docIds = SearchTaskEngineUtils.GetAllDocumentIdsInSubTree(index.IndexCodeName, siteNode.SiteName, siteNode.NodeAliasPath, searchServiceManager);

            if (docIds.Count > 0)
            {
                searchServiceManager.DeleteDocuments(index.IndexCodeName, SearchFieldsConstants.ID, docIds);
            }
        }


        /// <summary>
        /// Deletes documents from Azure belonging to given site (specified in additional task data).
        /// </summary>
        /// <param name="task">Delete search task to execute.</param>
        /// <param name="index">Index to perform delete operation on.</param>
        /// <param name="searchServiceManager">Search service manager.</param>
        private static void DeleteDocumentsFromSite(SearchTaskAzureInfo task, SearchIndexInfo index, SearchServiceManager searchServiceManager)
        {
            var docIds = SearchTaskEngineUtils.GetAllDocumentIdsBySite(index.IndexCodeName, task.SearchTaskAzureAdditionalData, searchServiceManager);

            if (docIds.Count > 0)
            {
                searchServiceManager.DeleteDocuments(index.IndexCodeName, SearchFieldsConstants.ID, docIds);
            }
        }


        /// <summary>
        /// Deletes document from Azure by ID (specified in additional task data).
        /// </summary>
        /// <param name="task">Delete search task to execute.</param>
        /// <param name="index">Index to perform delete operation on.</param>
        /// <param name="searchServiceManager">Search service manager.</param>
        private static void DeleteOneDocument(SearchTaskAzureInfo task, SearchIndexInfo index, SearchServiceManager searchServiceManager)
        {
            searchServiceManager.DeleteDocuments(index.IndexCodeName, SearchFieldsConstants.ID, new[] { task.SearchTaskAzureAdditionalData });
        }


        /// <summary>
        /// Returns service new manager instance.
        /// </summary>
        internal virtual SearchServiceManager CreateSearchServiceManager(SearchService searchService)
        {
            return new SearchServiceManager(searchService, new ExponentialRetryStrategy());
        }


        /// <summary>
        /// Executes update task for all relevant Azure indexes by modifying their content.
        /// </summary>
        private void ExecuteUpdateTask(SearchTaskAzureInfo task)
        {
            foreach (var indexWithSearchables in GetIndexesWithSearchableObjects(task))
            {
                var index = indexWithSearchables.Key;
                var searchables = indexWithSearchables.Value;

                ExecuteUpdateOnIndex(index, searchables, task.SearchTaskAzureCreated);
            }
        }


        /// <summary>
        /// Performs documents update for an Azure index.
        /// </summary>
        private void ExecuteUpdateOnIndex(SearchIndexInfo indexInfo, IEnumerable<ISearchable> searchables, DateTime taskCreated)
        {
            if (!SearchTaskEngineUtils.TaskWasCreatedAfterIndexRebuild(taskCreated, indexInfo))
            {
                return;
            }

            var searchServiceManager = CreateSearchServiceManager(SearchService.FromAdminApiKey(indexInfo.IndexSearchServiceName, indexInfo.IndexAdminKey));
            if (!searchServiceManager.IndexExists(indexInfo.IndexName))
            {
                return;
            }

            var azureIndex = searchServiceManager.GetIndex(indexInfo.IndexCodeName);

            UpdateIndex(indexInfo, searchables, searchableObj =>
            {
                var document = searchableObj.GetSearchDocument(indexInfo);

                var isExcluded = document.Contains(SearchDocument.DOCUMENT_EXCLUDED_FROM_SEARCH_FIELD) && ValidationHelper.GetBoolean(document.GetValue(SearchDocument.DOCUMENT_EXCLUDED_FROM_SEARCH_FIELD), false);
                if (isExcluded)
                {
                    string idFieldName = NamingHelper.GetValidFieldName(SearchFieldsConstants.ID);
                    string idValue = NamingHelper.GetValidDocumentKey(searchableObj.GetSearchID());

                    return IndexAction.Delete(idFieldName, idValue);
                }
                else
                {
                    var azureDoc = DocumentCreator.Instance.CreateDocument(document, searchableObj, indexInfo);

                    return IndexAction.MergeOrUpload(azureDoc);
                }
            }, azureIndex);
        }


        /// <summary>
        /// Returns collection of Azure indexes with all the related <see cref="ISearchable"/> objects which got modified for given <paramref name="task"/>.
        /// </summary>
        private static Dictionary<SearchIndexInfo, IEnumerable<ISearchable>> GetIndexesWithSearchableObjects(SearchTaskAzureInfo task)
        {
            if (task.SearchTaskAzureMetadata.Equals(SearchFieldsConstants.PARTIAL_REBUILD, StringComparison.OrdinalIgnoreCase))
            {
                var provider = SearchablesRetrievers.Get(task.SearchTaskAzureObjectType);

                return GetIndexesWithSearchableObjectsForPartialRebuild(task.SearchTaskAzureObjectType, task.SearchTaskAzureAdditionalData, provider);
            }
            else
            {
                return GetIndexesWithSearchableObjectsForDocumentUpdate(task.SearchTaskAzureObjectType, ValidationHelper.GetInteger(task.SearchTaskAzureInitiatorObjectID, 0));
            }
        }


        /// <summary>
        /// Returns collection of Azure indexes with all the related <see cref="ISearchable"/> objects for given <paramref name="objectType"/> and <paramref name="documentId"/>
        /// that are necessary for document update.
        /// </summary>
        private static Dictionary<SearchIndexInfo, IEnumerable<ISearchable>> GetIndexesWithSearchableObjectsForDocumentUpdate(string objectType, int documentId)
        {
            var list = new List<KeyValuePair<SearchIndexInfo, ISearchable>>();
            var searchables = SearchIndexers.GetIndexer(objectType).SelectSearchDocument(documentId);

            foreach (var searchable in searchables)
            {
                list.AddRange(SearchIndexInfoProvider.GetRelevantIndexes(searchable, SearchIndexInfo.AZURE_SEARCH_PROVIDER)
                                                     .Select(index => new KeyValuePair<SearchIndexInfo, ISearchable>(index, searchable)));
            }

            return list.GroupBy(kvp => kvp.Key, kvp => kvp.Value)
                       .ToDictionary(group => group.Key, group => group.AsEnumerable());
        }


        /// <summary>
        /// Returns collection of Azure indexes with all the related <see cref="ISearchable"/> objects for given <paramref name="objectType"/> 
        /// and <paramref name="siteNodeIdentifier"/> (site name and node alias path) that are necessary for partial rebuild.
        /// </summary>
        private static Dictionary<SearchIndexInfo, IEnumerable<ISearchable>> GetIndexesWithSearchableObjectsForPartialRebuild(string objectType, string siteNodeIdentifier, SearchablesRetriever searchableRetriever)
        {
            var siteNode = new SiteNodeIdentifier(siteNodeIdentifier);

            return searchableRetriever.GetRelevantIndexes(objectType, siteNode.SiteName, SearchIndexInfo.AZURE_SEARCH_PROVIDER)
                                 .ToDictionary(index => index, searchableRetriever.GetSearchableObjects);
        }


        /// <summary>
        /// Executes rebuild task for Azure index by creating new index on Azure and uploading all <see cref="Document"/>s to the index.
        /// </summary>
        private void ExecuteRebuildTask(SearchTaskAzureInfo task)
        {
            SearchIndexInfo indexInfo = SearchIndexInfoProvider.GetSearchIndexInfo(task.SearchTaskAzureInitiatorObjectID);

            if ((indexInfo != null) && (indexInfo.ActualRebuildTime <= task.SearchTaskAzureCreated))
            {
                SearchIndexInfoProvider.SetIndexStatus(indexInfo, IndexStatusEnum.REBUILDING);
                Rebuild(indexInfo);

                if (SearchIndexInfoProvider.GetIndexStatus(indexInfo) != IndexStatusEnum.ERROR)
                {
                    SearchIndexInfoProvider.SetIndexStatus(indexInfo, IndexStatusEnum.READY);
                    SearchIndexInfoProvider.SetIndexFilesLastUpdateTime(indexInfo, DateTime.Now);
                }
            }
        }


        /// <summary>
        /// Rebuilds <paramref name="indexInfo"/>.
        /// </summary>
        /// <param name="indexInfo">Index to rebuild.</param>
        private void Rebuild(SearchIndexInfo indexInfo)
        {
            var searchService = new SearchService { Name = indexInfo.IndexSearchServiceName, AdminApiKey = indexInfo.IndexAdminKey };
            var searchServiceManager = CreateSearchServiceManager(searchService);

            try
            {
                searchServiceManager.DeleteIndexIfExists(indexInfo.IndexName);
                UpdateIndex(indexInfo, SearchablesRetrievers.Get(indexInfo.IndexType).GetSearchableObjects(indexInfo), searchableObj =>
                {
                    var azureDoc = DocumentCreator.Instance.CreateDocument(searchableObj.GetSearchDocument(indexInfo), searchableObj, indexInfo);
                    return IndexAction.MergeOrUpload(azureDoc);
                });

                SearchHelper.FinishRebuild(indexInfo);
            }
            catch (Exception)
            {
                SearchIndexInfoProvider.SetIndexStatus(indexInfo, IndexStatusEnum.ERROR);

                throw;
            }
        }


        /// <summary>
        /// Updates <paramref name="indexInfo"/> content and definition if necessary.
        /// </summary>
        /// <remarks>Updating is done in batches defined by <see cref="SearchIndexInfo.IndexBatchSize"/>.</remarks>
        /// <param name="indexInfo">Defines <see cref="SearchIndexInfo"/> to be created or updated in Azure Search service.</param>
        /// <param name="searchables">Objects whose documents in the index should be modified. Modification is defined by <paramref name="transformSearchableToIndexAction"/>.</param>
        /// <param name="transformSearchableToIndexAction">Defines how <paramref name="indexInfo"/> should be modified by a <see cref="ISearchable"/> object from <paramref name="searchables"/>.</param>
        /// <param name="existingIndex"> 
        /// Defines Azure <see cref="Index"/> already existing in Azure Search service. Azure <see cref="Index"/> has to be defined if there 
        /// is already existing Azure <see cref="Index"/> otherwise <see cref="Microsoft.Rest.Azure.CloudException"/> may be thrown.
        /// Can be null or empty if Azure <see cref="Index"/> is created from scratch.
        /// </param>
        /// <exception cref="Microsoft.Rest.Azure.CloudException">Is thrown if fields are removed from existing Azure index.</exception>
        private void UpdateIndex(SearchIndexInfo indexInfo, IEnumerable<ISearchable> searchables, Func<ISearchable, IndexAction> transformSearchableToIndexAction, Index existingIndex = null)
        {
            var searchServiceManager = CreateSearchServiceManager(SearchService.FromAdminApiKey(indexInfo.IndexSearchServiceName, indexInfo.IndexAdminKey));
            if (existingIndex == null)
            {
                existingIndex = new Index { Name = NamingHelper.GetValidIndexName(indexInfo.IndexName) };
            }

            var indexActions = new List<IndexAction>();
            var azureIndexFields = new Dictionary<string, Field>((existingIndex.Fields ?? Enumerable.Empty<Field>()).ToDictionary(f => f.Name, f => f), StringComparer.OrdinalIgnoreCase);

            int iteration = 0;
            bool updateIndex = !existingIndex.Fields?.Any() ?? true;

            using (var searchablesEnumerator = searchables.GetEnumerator())
            {
                bool exists = searchablesEnumerator.MoveNext();
                while (exists)
                {
                    ++iteration;
                    var searchable = searchablesEnumerator.Current;

                    var indexAction = transformSearchableToIndexAction(searchable);
                    indexActions.Add(indexAction);

                    if (indexAction.ActionType != IndexActionType.Delete)
                    {
                        // Get only search fields needed by the search document
                        var fields = DocumentFieldCreator.Instance.CreateFields(searchable, indexInfo).Where(f => indexAction.Document.Keys.Contains(f.Name, StringComparer.OrdinalIgnoreCase));
                        updateIndex = MergeFields(fields, azureIndexFields) || updateIndex;
                    }

                    // Batch processing, if there is no searchable to process send the rest of the documents to azure service
                    if (!(exists = searchablesEnumerator.MoveNext()) || (iteration == Math.Min(indexInfo.IndexBatchSize, SearchEngineConfiguration.Instance.DocumentsBatchSize)))
                    {
                        if (updateIndex)
                        {
                            existingIndex.Fields = azureIndexFields.Values.ToList();
                            searchServiceManager.CreateOrUpdateIndex(existingIndex);
                        }

                        searchServiceManager.ApplyIndexActions(indexInfo.IndexName, indexActions);

                        iteration = 0;
                        indexActions = new List<IndexAction>();
                        updateIndex = false;
                    }
                }
            }
        }


        /// <summary>
        /// Merges the <paramref name="fields"/> to all already existing <paramref name="azureIndexFields"/>.
        /// </summary>
        /// <remarks>Checks for inconsistencies between properties of the fields with the same name.</remarks>
        /// <param name="fields">Fields that needs to be merged with <paramref name="azureIndexFields"/>.</param>
        /// <param name="azureIndexFields">Already prepared fields for the Azure index.</param>
        /// <exception cref="InvalidOperationException">
        /// Exception is thrown if <paramref name="fields"/> contains a field already existing in <paramref name="azureIndexFields"/>
        /// but properties of those two fields are not compatible.
        /// </exception>
        /// <returns>True if new field has been added into <paramref name="azureIndexFields"/>. False otherwise.</returns>
        private bool MergeFields(IEnumerable<Field> fields, Dictionary<string, Field> azureIndexFields)
        {
            bool update = false;
            foreach (var field in fields)
            {
                if (!azureIndexFields.ContainsKey(field.Name))
                {
                    azureIndexFields.Add(field.Name, field);
                    update = true;
                }
                else if (azureIndexFields.ContainsKey(field.Name) && !FieldComparer.Equals(azureIndexFields[field.Name], field))
                {
                    throw new InvalidOperationException($"Index can not contain two search fields with the same name '{field.Name}' but different properties.");
                }
            }

            return update;
        }


        /// <summary>
        /// Represents compound identifier for node on a site.
        /// </summary>
        private class SiteNodeIdentifier
        {
            public SiteNodeIdentifier(string identifier)
            {
                var strArray = identifier.Split(';');
                SiteName = strArray[0];
                NodeAliasPath = strArray[1];
            }


            public string SiteName { get; }


            public string NodeAliasPath { get; }
        }
    }
}
