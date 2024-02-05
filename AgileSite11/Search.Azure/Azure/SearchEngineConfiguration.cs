using System;

using Microsoft.Rest.TransientFaultHandling;

namespace CMS.Search.Azure
{
    /// <summary>
    /// Class encapsulating configuration properties to customize indexing of documents to Azure Search service.
    /// </summary>
    public sealed class SearchEngineConfiguration
    {
        private static readonly Lazy<SearchEngineConfiguration> lazy = new Lazy<SearchEngineConfiguration>(() => new SearchEngineConfiguration());


        private SearchEngineConfiguration()
        {
        }


        /// <summary>
        /// Returns <see cref="SearchEngineConfiguration"/> instance.
        /// </summary>
        public static SearchEngineConfiguration Instance
        {
            get { return lazy.Value; }
        }


        /// <summary>
        /// Maximum number of search documents processed per batch in one request to the Azure Search service (index uploads, merges or deletes).
        /// </summary>
        public int DocumentsBatchSize { get; set; } = 1000;


        /// <summary>
        /// Document count per page when searching Azure Search indexes.
        /// </summary>
        internal int DocumentSearchPageSize { get; set; } = 1000;


        /// <summary>
        /// Maximum time interval (in seconds) between two attempts to perform Azure search operations.
        /// The interval starts from 0 and grows exponentially up to the maximum: 0, 1, 2, 4, 8, 16, ...
        /// </summary>
        /// <remarks>The default value is 8.</remarks>
        public int MaxBackoffTimeSeconds { get; set; } = 8;


        /// <summary>
        /// Maximum number of retry attempts that the system performs after an Azure Search operation fails.
        /// </summary>
        /// <remarks>Setting the retry count to zero disables the retry strategy. The default value is 5.</remarks>
        public int RetryCount { get; set; } = 5;


        /// <summary>
        /// Defines an object responsible for detecting specific transient conditions on Azure Search service operations.
        /// By default every exception is handled as transient and so the operation that caused exception will be retried.
        /// </summary>
        /// <remarks>May improve performance by skipping retry strategy of an operation for specific exceptions.</remarks>
        /// <seealso cref="MaxBackoffTimeSeconds"/>
        /// <seealso cref="RetryCount"/>
        /// <seealso cref="ITransientErrorDetectionStrategy"/>
        public ITransientErrorDetectionStrategy TransientErrorDetectionStrategy { get; set; }
    }
}
