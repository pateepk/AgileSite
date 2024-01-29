using System.Collections.Generic;

using CMS.Base;

namespace CMS.Search
{
    /// <summary>
    /// Manages the search indexers used for specific object type
    /// </summary>
    public static class SearchIndexers
    {
        #region "Variables"

        /// <summary>
        /// Registered search indexer tasks by their type
        /// </summary>
        private static readonly StringSafeDictionary<SearchIndexer> mIndexers = new StringSafeDictionary<SearchIndexer>();

        #endregion


        #region "Properties"

        /// <summary>
        /// Default search indexer
        /// </summary>
        public static SearchIndexer DefaultIndexer
        {
            get;
            private set;
        }


        /// <summary>
        /// Returns the collection of the indexers
        /// </summary>
        public static IEnumerable<string> IndexerTypes
        {
            get
            {
                return mIndexers.TypedKeys;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Initializes the system indexers
        /// </summary>
        public static void Init()
        {
            RegisterDefaultIndexer<SearchIndexer>();
            
            RegisterIndexer<CustomSearchIndexer>(SearchHelper.CUSTOM_SEARCH_INDEX);
        }


        /// <summary>
        /// Registers the default indexer which handles the search for all object types which don't have specific indexer
        /// </summary>
        public static void RegisterDefaultIndexer<IndexerType>()
            where IndexerType : SearchIndexer, new()
        {
            DefaultIndexer = new IndexerType();
        }


        /// <summary>
        /// Registers the given indexer for the specific object type
        /// </summary>
        /// <param name="objectType">Object type</param>
        public static void RegisterIndexer<IndexerType>(string objectType)
            where IndexerType : SearchIndexer, new()
        {
            mIndexers[objectType] = new IndexerType();
        }


        /// <summary>
        /// Gets the indexer by its object type
        /// </summary>
        /// <param name="objectType">Object type</param>
        public static SearchIndexer GetIndexer(string objectType)
        {
            if (objectType == null)
            {
                return DefaultIndexer;
            }
            return mIndexers[objectType] ?? DefaultIndexer;
        }
        
        #endregion
    }
}
