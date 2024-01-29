using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.Search
{
    /// <summary>
    /// Denotes class providing index statistics.
    /// </summary>
    public interface IIndexStatisticsProvider
    {
        /// <summary>
        /// Gets statistics of an index.
        /// </summary>
        /// <param name="indexInfo">Index to retrieve statistics for.</param>
        /// <returns>Statistics of given index, or null when statistics are not available.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="indexInfo"/> is null.</exception>
        IIndexStatistics GetStatistics(SearchIndexInfo indexInfo);
    }
}
