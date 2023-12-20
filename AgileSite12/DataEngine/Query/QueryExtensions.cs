using System.Collections.Generic;

namespace CMS.DataEngine.Query
{
    /// <summary>
    /// Extensions to easily convert values to queries
    /// </summary>
    public static class QueryExtensions
    {
        /// <summary>
        /// Converts string to a query column which can be used in a query
        /// </summary>
        /// <param name="columnName">Column name</param>
        public static QueryColumn AsColumn(this string columnName)
        {
            return new QueryColumn(columnName, true);
        }


        /// <summary>
        /// Converts object to a query value which can be used as a query parameter
        /// </summary>
        /// <param name="value">Value</param>
        /// <param name="expand">If true, the value expands as constant</param>
        public static QueryValueExpression AsValue<T>(this T value, bool expand = false)
        {
            return new QueryValueExpression(value, null, expand);
        }


        /// <summary>
        /// Converts string to a query expression which can be used in a query
        /// </summary>
        /// <param name="expression">Expression</param>
        public static QueryExpression AsExpression(this string expression)
        {
            return new QueryExpression(expression);
        }


        /// <summary>
        /// Converts value to a literal value which can be used in a query
        /// </summary>
        /// <param name="value">Value to convert</param>
        public static QueryExpression AsLiteral<T>(this T value)
        {
            return DataTypeManager.GetSqlValue(value).AsExpression();
        }


        /// <summary>
        /// Gets the final executing query for a given query
        /// </summary>
        internal static IDataQuery GetFinalExecutingQuery(this IDataQuery dataQuery)
        {
            var query = dataQuery.GetExecutingQuery();
            if (query == dataQuery)
            {
                return dataQuery;
            }

            return query.GetFinalExecutingQuery();
        }


        /// <summary>
        /// Sets values in update query to the given set of values
        /// </summary>
        /// <param name="query">Data query</param>
        /// <param name="values">Values for the data. Dictionary of [columnName] => [value]</param>
        internal static void SetUpdateQueryValues(this IDataQuery query, IEnumerable<KeyValuePair<string, object>> values)
        {
            var updateExpression = new UpdateQueryExpression(values);

            var valuesExpression = query.IncludeDataParameters(updateExpression.Parameters, updateExpression.GetExpression());

            query.EnsureParameters().AddMacro(QueryMacros.VALUES, valuesExpression);
        }


        /// <summary>
        /// Gets the number of items which returns given object query.
        /// </summary>
        /// <typeparam name="TInfo">Info object type</typeparam>
        /// <param name="objectQuery">Object query</param>
        /// <returns>Number of items.</returns>
        public static int GetCount<TInfo>(this ObjectQuery<TInfo> objectQuery)
            where TInfo : BaseInfo
        {
            var query = objectQuery.Column(new AggregatedColumn(AggregationType.Count, null).As("Count"));

            if (!string.IsNullOrEmpty(query.OrderByColumns))
            {
                query.OrderByColumns = null;
            }

            return query.GetScalarResult<int>();
        }


        /// <summary>
        /// Ensures <see cref="QueryDataParameters.FillDataSet"/> property for given <typeparamref name="TInfo"/>.
        /// </summary>
        /// <param name="parameters"><see cref="QueryDataParameters"/> object</param>
        /// <typeparam name="TInfo">Info object type</typeparam>
        public static void EnsureDataSet<TInfo>(this QueryDataParameters parameters)
            where TInfo : BaseInfo
        {
            if (parameters.FillDataSet is InfoDataSet<TInfo>)
            {
                return;
            }

            parameters.FillDataSet = new InfoDataSet<TInfo>();
        }
    }
}
