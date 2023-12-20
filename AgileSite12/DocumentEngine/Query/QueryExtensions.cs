using CMS.DataEngine;

namespace CMS.DocumentEngine.Query
{
    /// <summary>
    /// Extensions for data queries.
    /// </summary>
    internal static class QueryExtensions
    {
        /// <summary>
        /// Expands the query to its full source set by reseting properties that transform the original set of data to a limited set.
        /// This includes mainly aggregations and restrictions of result count.
        /// </summary>
        /// <param name="query">Query.</param>
        public static void ExpandToFullSourceSet(this IDataQuery query)
        {
            query.TopNRecords = 0;
            query.Offset = 0;
            query.MaxRecords = 0;
            query.SelectDistinct = false;
            query.GroupByColumns = null;
            query.HavingCondition = null;
        }
    }
}
