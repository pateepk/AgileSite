using System;
using System.Collections.Concurrent;

namespace CMS.Search.Internal
{
    /// <summary>
    /// Manages searchables retrievers used for specific object type.
    /// </summary>
    public static class SearchablesRetrievers
    {
        private static readonly ConcurrentDictionary<string, SearchablesRetriever> mSearchablesRetrievers = new ConcurrentDictionary<string, SearchablesRetriever>(StringComparer.OrdinalIgnoreCase);
                 
        private static readonly Lazy<SearchablesRetriever> mDefaultRetriever = new Lazy<SearchablesRetriever>();


        /// <summary>
        /// Registers given searchables retriever for the specific object type.
        /// </summary>
        /// <param name="objectType">Object type.</param>
        public static void Register<TRetriever>(string objectType)
            where TRetriever : SearchablesRetriever, new()
        {
            mSearchablesRetrievers[objectType] = new TRetriever();
        }


        /// <summary>
        /// Gets searchables retriever related to given <paramref name="objectType"/>.
        /// </summary>
        /// <param name="objectType">Object type.</param>
        public static SearchablesRetriever Get(string objectType)
        {
            if (objectType == null || !mSearchablesRetrievers.ContainsKey(objectType))
            {
                return mDefaultRetriever.Value;
            }

            // Currently is impossible to remove helpers from dictionary, so this method should be thread safe
            return mSearchablesRetrievers[objectType];
        }
    }
}
