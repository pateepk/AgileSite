using System;
using System.Linq;
using System.Collections.Generic;

using Microsoft.Azure.Search;
using Microsoft.Azure.Search.Models;

using Index = Microsoft.Azure.Search.Models.Index;

namespace CMS.Search.Azure
{
    /// <summary>
    /// Performs create, update or delete operations on Azure indexes.
    /// </summary>
    public class SearchServiceManager
    {
        private readonly SearchService mSearchService;
        private readonly ISearchServiceRetryStrategy mRetryStrategy;


        private ISearchServiceRetryStrategy RetryStrategy => mRetryStrategy;


        /// <summary>
        /// An event raised upon <see cref="CreateOrUpdateIndex(Index)"/> execution.
        /// The event allows for modification of processed <see cref="CreateOrUpdateIndexEventArgs.Index"/>.
        /// </summary>
        /// <remarks>This event should be used for configuring <see cref="Suggester"/>s and <see cref="ScoringProfile"/>s.</remarks>
        public static readonly CreateOrUpdateIndexHandler CreatingOrUpdatingIndex = new CreateOrUpdateIndexHandler { Name = $"{nameof(SearchServiceManager)}.{nameof(CreatingOrUpdatingIndex)}" };


        /// <summary>
        /// Performs create, update or delete operations on Azure indexes.
        /// </summary>
        /// <param name="searchService">Search service in which to create the index.</param>
        /// <param name="retryStrategy">Retry strategy that should be performed for failed operations. If null, no attempts are performed for failed operation.</param>
        internal SearchServiceManager(SearchService searchService, ISearchServiceRetryStrategy retryStrategy)
        {
            if (searchService == null)
            {
                throw new ArgumentNullException(nameof(searchService), $"Parameter {nameof(searchService)} can not be null.");
            }

            mSearchService = searchService;
            mRetryStrategy = retryStrategy ?? new ExponentialRetryStrategy(0, 0);
        }


        /// <summary>
        /// Performs create, update or delete operations on Azure indexes.
        /// </summary>
        /// <param name="searchService">Search service in which to create the index.</param>
        public SearchServiceManager(SearchService searchService)
            : this(searchService, null) {}


        /// <summary>
        /// Searches for documents and enumerates all results while paging over the individual result sets.
        /// </summary>
        /// <param name="indexName">Name of index to search in.</param>
        /// <param name="searchText">Text to search for.</param>
        /// <param name="searchParameters"><para>Search parameters to be used.</para>
        /// <para>
        /// The <see cref="Microsoft.Azure.Search.Models.SearchParameters.Skip"/> property is modified while paging over the individual result sets
        /// and is restored to its original value after enumeration end.
        /// </para>
        /// <para>
        /// Use the <see cref="Microsoft.Azure.Search.Models.SearchParameters.Top"/> property to set the page size.
        /// </para>
        /// </param>
        /// <returns>Enumeration of all documents found.</returns>
        /// <remarks>
        /// <para>
        /// See the Azure Search documentation on how to search for documents.
        /// </para>
        /// <para>
        /// The <see cref="Microsoft.Azure.Search.Models.SearchParameters.Skip"/> property of given <paramref name="searchParameters"/> is modified while enumerating
        /// over the individual result sets and is restored to its original value after enumeration end.
        /// </para>
        /// </remarks>
        internal IEnumerable<Document> GetAllDocuments(string indexName, string searchText, Microsoft.Azure.Search.Models.SearchParameters searchParameters)
        {
            if (indexName == null)
            {
                throw new ArgumentNullException(nameof(indexName));
            }
            if (searchParameters == null)
            {
                throw new ArgumentNullException(nameof(searchParameters));
            }

            using (var searchServiceClient = CreateServiceClient(mSearchService))
            {
                foreach (var document in GetAllDocumentsCore(searchServiceClient, indexName, searchText, searchParameters))
                {
                    yield return document;
                }
            }
        }


        /// <summary>
        /// Searches for documents and enumerates all results while advancing the <see cref="Microsoft.Azure.Search.Models.SearchParameters.Skip"/>
        /// of passed <paramref name="searchParameters"/>. The property <see cref="Microsoft.Azure.Search.Models.SearchParameters.Skip"/> is restored to its original value
        /// after enumeration end.
        /// </summary>
        private IEnumerable<Document> GetAllDocumentsCore(SearchServiceClient searchServiceClient, string indexName, string searchText, Microsoft.Azure.Search.Models.SearchParameters searchParameters)
        {
            using (var searchIndexClient = searchServiceClient.Indexes.GetClient(NamingHelper.GetValidIndexName(indexName)))
            {
                foreach (var searchResult in GetAllSearchResults(searchIndexClient, searchText, searchParameters))
                {
                    yield return searchResult.Document;
                }
            }
        }


        /// <summary>
        /// Searches for documents and enumerates all search results while advancing the <see cref="Microsoft.Azure.Search.Models.SearchParameters.Skip"/>
        /// of passed <paramref name="searchParameters"/>. The property <see cref="Microsoft.Azure.Search.Models.SearchParameters.Skip"/> is restored to its original value
        /// after enumeration end.
        /// </summary>
        private IEnumerable<Microsoft.Azure.Search.Models.SearchResult> GetAllSearchResults(ISearchIndexClient searchIndexClient, string searchText, Microsoft.Azure.Search.Models.SearchParameters searchParameters)
        {
            int? originalSkip = searchParameters.Skip;

            try
            {
                int resultCountInBatch;
                bool implicitBatchSize = !searchParameters.Top.HasValue;
                do
                {
                    var searchResults = RetryStrategy.Perform(() => searchIndexClient.Documents.Search(searchText, searchParameters)).Results;
                    foreach (var searchResult in searchResults)
                    {
                        yield return searchResult;
                    }

                    resultCountInBatch = searchResults.Count;

                    searchParameters.Skip = (searchParameters.Skip ?? 0) + resultCountInBatch;
                }

                // Do not rely on implicit batch size 50, which can change.
                // Continue until resultCountInBatch is zero in case of implicit batching. Batch size 0 would cause an infinite loop
                while ((implicitBatchSize && resultCountInBatch > 0) || (resultCountInBatch == searchParameters.Top && searchParameters.Top > 0));
            }
            finally
            {
                searchParameters.Skip = originalSkip;
            }
        }


        /// <summary>
        /// Gets index statistics from Azure Search service.
        /// </summary>
        /// <param name="indexName">Name of index to retrieve statistics for. The name must meet the service's requirements (e.g. lowercase string, no starting or trailing dash).</param>
        /// <returns>Statistics of index named <paramref name="indexName"/>.</returns>
        public IndexGetStatisticsResult GetStatistics(string indexName)
        {
            using (var searchServiceClient = CreateServiceClient(mSearchService))
            {
                return searchServiceClient.Indexes.GetStatistics(NamingHelper.GetValidIndexName(indexName));
            }
        }


        /// <summary>
        /// Indicates whether index exists in Azure Search service.
        /// </summary>
        /// <param name="indexName">Name of index whose existence is to be checked.</param>
        /// <returns>Returns true if index exists, false otherwise.</returns>
        internal bool IndexExists(string indexName)
        {
            using (var searchServiceClient = CreateServiceClient(mSearchService))
            {
                return searchServiceClient.Indexes.Exists(NamingHelper.GetValidIndexName(indexName));
            }
        }


        /// <summary>
        /// Gets index from Azure Search service.
        /// </summary>
        /// <param name="indexName">Name of index to be retrieved. The name must meet the service's requirements (e.g. lowercase string, no starting or trailing dash).</param>
        public Index GetIndex(string indexName)
        {
            using (var searchServiceClient = CreateServiceClient(mSearchService))
            {
                return searchServiceClient.Indexes.Get(NamingHelper.GetValidIndexName(indexName));
            }
        }


        /// <summary>
        /// Deletes an Azure Search index if it exists.
        /// </summary>
        /// <param name="indexName">Name of index to be deleted.</param>
        public void DeleteIndexIfExists(string indexName)
        {
            indexName = NamingHelper.GetValidIndexName(indexName);
            using (var searchServiceClient = CreateServiceClient(mSearchService))
            {
                if (searchServiceClient.Indexes.Exists(indexName))
                {
                    searchServiceClient.Indexes.Delete(indexName);
                }
            }
        }


        /// <summary>
        /// Creates or updates <paramref name="index"/> in Azure Search.
        /// </summary>
        /// <param name="index">
        /// Index to be created or updated. The <see cref="Index.Name"/> of the <paramref name="index"/> must meet the service's
        /// requirements (e.g. lowercase string, no starting or trailing dash).
        /// </param>
        public void CreateOrUpdateIndex(Index index)
        {
            CreateOrUpdateIndexEventArgs eventArgs = new CreateOrUpdateIndexEventArgs { Index = index, SearchService = mSearchService };

            try
            {
                using (var searchServiceClient = CreateServiceClient(mSearchService))
                {
                    using (CreatingOrUpdatingIndex.StartEvent(eventArgs))
                    {
                        searchServiceClient.Indexes.CreateOrUpdate(eventArgs.Index);
                    }
                }
            }
            catch (Exception ex)
            {
                string fieldNamesWithType = String.Join(", ", eventArgs.Index.Fields.Select(f => f.Name + " (" + f.Type + ")"));
                throw new InvalidOperationException($"Creating or updating index '{eventArgs.Index.Name}' failed with the following error: {ex.Message}{Environment.NewLine}" +
                    $"Index fields: {fieldNamesWithType}.", ex);
            }
        }


        /// <summary>
        /// Deletes documents from the index.
        /// </summary>
        /// <param name="indexName">Name of the Azure index.</param>
        /// <param name="keyName">Name of the field by which documents will be deleted.</param>
        /// <param name="keyValues">Defines for which values in the <paramref name="keyName"/> field, documents will be deleted.</param>
        public virtual void DeleteDocuments(string indexName, string keyName, IEnumerable<string> keyValues)
        {
            using (var searchServiceClient = CreateServiceClient(mSearchService))
            {
                var azureIndex = searchServiceClient.Indexes.GetClient(NamingHelper.GetValidIndexName(indexName));
                var batch = IndexBatch.Delete(NamingHelper.GetValidFieldName(keyName), keyValues.Select(k => NamingHelper.GetValidDocumentKey(k)));

                RetryStrategy.Perform(batch, (b) => azureIndex.Documents.Index(b));
            }
        }


        /// <summary>
        /// Applies collection of <paramref name="actions"/> on index with name <paramref name="indexName"/>.
        /// </summary>
        /// <param name="indexName">Name of the Azure index.</param>
        /// <param name="actions">Collection of actions to modify index.</param>
        internal void ApplyIndexActions(string indexName, ICollection<IndexAction> actions)
        {
            if (!actions.Any())
            {
                return;
            }

            using (var searchServiceClient = CreateServiceClient(mSearchService))
            {
                var azureIndex = searchServiceClient.Indexes.GetClient(NamingHelper.GetValidIndexName(indexName));
                var batch = IndexBatch.New(actions);

                RetryStrategy.Perform(batch, (b) => azureIndex.Documents.Index(b));
            }
        }


        private SearchServiceClient CreateServiceClient(SearchService searchService)
        {
            return new SearchServiceClient(searchService.Name, new SearchCredentials(searchService.AdminApiKey));
        }
    }
}
