using System;

namespace CMS.Search
{
    /// <summary>
    /// Provides statistics of Lucene indexes.
    /// </summary>
    public class LuceneIndexStatisticsProvider : IIndexStatisticsProvider
    {
        /// <summary>
        /// Gets statistics of a Lucene index.
        /// </summary>
        /// <param name="indexInfo">Index to retrieve statistics for.</param>
        /// <returns>Statistics of given index.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="indexInfo"/> is null.</exception>
        /// <seealso cref="SearchIndexInfo.LUCENE_SEARCH_PROVIDER"/>
        public IIndexStatistics GetStatistics(SearchIndexInfo indexInfo)
        {
            if (indexInfo == null)
            {
                throw new ArgumentNullException(nameof(indexInfo));
            }

            return new LuceneIndexStatistics(indexInfo);
        }
    }
}
