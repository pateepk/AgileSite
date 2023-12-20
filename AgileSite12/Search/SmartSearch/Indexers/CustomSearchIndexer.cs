using System;
using System.Collections.Generic;

using CMS.DataEngine;

namespace CMS.Search
{
    /// <summary>
    /// Search indexer for custom search index
    /// </summary>
    public class CustomSearchIndexer : SearchIndexer
    {
        /// <summary>
        /// Rebuilds the custom index
        /// </summary>
        /// <param name="srchInfo">Search index</param>
        public override void Rebuild(SearchIndexInfo srchInfo)
        {
            // Get custom search index interface
            ICustomSearchIndex customIndex = SearchHelper.GetCustomSearchIndex(srchInfo);

            // If custom search index exists, call rebuild
            if (customIndex != null)
            {
                customIndex.Rebuild(srchInfo);

                SearchHelper.FinishRebuild(srchInfo);
            }
        }


        /// <summary>
        /// Gets the collection of search fields. When no SearchFields collection is provided, new is created.
        /// </summary>
        /// <param name="index">Search index</param>
        /// <param name="searchFields">Search fields collection</param>
        public override ISearchFields GetSearchFields(SearchIndexInfo index, ISearchFields searchFields = null)
        {
            return searchFields ?? new SearchFields(false);
        }
    }
}
