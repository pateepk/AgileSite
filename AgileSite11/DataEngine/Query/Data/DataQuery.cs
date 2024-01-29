using System;
using System.Collections.Generic;
using System.Linq;

namespace CMS.DataEngine
{
    /// <summary>
    /// Queries particular database data or defines parameters for data selection
    /// </summary>
    public class DataQuery : DataQueryBase<DataQuery>
    {
        #region "Constructors"

        /// <summary>
        /// Default constructor
        /// </summary>
        public DataQuery()
            : base(null, null)
        {
        }


        /// <summary>
        /// Creates a query based on the given query name
        /// </summary>
        /// <param name="queryName">Full query name</param>
        public DataQuery(string queryName)
            : base(queryName)
        {
        }


        /// <summary>
        /// Creates a query based on the given query name
        /// </summary>
        /// <param name="className">Class name</param>
        /// <param name="queryName">Query name</param>
        public DataQuery(string className, string queryName)
            : base(className, queryName)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Combines several queries into a single result
        /// </summary>
        /// <param name="queries">Queries</param>
        /// <param name="operators">Operators between queries</param>
        /// <param name="connectionStringName">Specifies connection string against which the query will be executed. If connection string name is not specified, uses the default database.</param>
        public static DataQuery Combine(IEnumerable<IDataQuery> queries, IEnumerable<string> operators, string connectionStringName = null)
        {
            var result = new DataQuery();

            SetupCombinedQuery(result, queries, operators, connectionStringName);

            return result;
        }


        /// <summary>
        /// Combines several queries into a single result
        /// </summary>
        /// <param name="queries">Queries</param>
        /// <param name="operators">Operators between queries</param>
        /// <param name="connectionStringName">Specifies connection string against which the query will be executed. If connection string name is not specified, uses the default database.</param>
        public static TQuery Combine<TQuery>(IEnumerable<TQuery> queries, IEnumerable<string> operators, string connectionStringName = null)
            where TQuery : class, IDataQuery, new()
        {
            var result = new TQuery();

            SetupCombinedQuery(result, queries, operators, connectionStringName);

            return result;
        }


        /// <summary>
        /// Combines several queries into a single result
        /// </summary>
        /// <param name="result">Result query</param>
        /// <param name="queries">Queries</param>
        /// <param name="operators">Operators between queries</param>
        /// <param name="connectionStringName">Specifies connection string against which the query will be executed. If connection string name is not specified, uses the default database.</param>
        internal static void SetupCombinedQuery(IDataQuery result, IEnumerable<IDataQuery> queries, IEnumerable<string> operators, string connectionStringName = null)
        {
            var queriesList = queries.ToList();

            var multiQueries = queriesList.OfType<IMultiQuery>();
            if (multiQueries.Any())
            {
                throw new NotSupportedException("The operation is not supported for objects implementing IMultiQuery interface. Use proper method from IMultiQuery implementation instead.");
            }

            result.IsCombinedQuery = true;

            // Set the query connection string
            if (connectionStringName != null)
            {
                result.ConnectionStringName = connectionStringName;
            }

            result.CustomQueryText = SqlHelper.SELECT_NULL;

            // Prepare queries
            var texts = new List<string>();
            var singleColumn = true;

            result.EnsureParameters();

            foreach (var query in queriesList)
            {
                string queryText = null;

                if (query != null)
                {
                    var includeQuery = query;

                    // Materialize query if necessary
                    if (!result.HasCompatibleSource(query))
                    {
                        if (!includeQuery.ReturnsSingleColumn)
                        {
                            throw new NotSupportedException("Only single column queries can be materialized within a combined query.");
                        }

                        includeQuery = includeQuery.AsMaterializedList();
                    }

                    var includeParameters = includeQuery.GetCompleteQueryParameters();
                    
                    queryText = includeParameters.GetFullQueryText(result.Parameters);

                    // One query not single column = result not single column
                    if (!includeQuery.ReturnsSingleColumn)
                    {
                        singleColumn = false;
                    }
                }

                texts.Add(queryText);
            }

            result.CustomQueryText = SqlHelper.CombineQueries(texts, operators);

            // Set the single column flag if all queries had single column
            if (singleColumn)
            {
                result.ReturnsSingleColumn = true;
            }
        }


        /// <summary>
        /// Creates a materialized DataQuery from the given list of IDs
        /// </summary>
        /// <param name="ids">List of IDs</param>
        public static DataQuery FromList(IEnumerable<int> ids)
        {
            var result = new DataQuery();
            var parameters = result.EnsureParameters();

            // Build the source of data parsed by function
            var selectCondition = new SelectCondition(parameters);
            var distinctIds = ids.Distinct().ToList();

            var idIntTable = distinctIds.Count > 0 ? SqlHelper.BuildIntTable(distinctIds) : null;
            var param = selectCondition.PrepareParameter<int>(idIntTable); 

            result.ReturnsSingleColumn = true;
            result.DataSourceName = DataQuerySource.MATERIALIZED;

            return result.From(param);
        }
        
        #endregion
    }
}
