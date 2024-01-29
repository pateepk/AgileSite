using System.Collections.Generic;
using System.Data;
using System.Linq;
using System;
using System.Text;

using CMS.Helpers;

namespace CMS.DataEngine.Query
{
    /// <summary>
    /// Represents a batch of <see cref="IDataQuery" /> queries which are executed within a single database call.
    /// Queries are grouped by their connection strings, and each such group is executed as a single batch.
    /// </summary>
    internal sealed class QueryBatch
    {
        private IEnumerable<IDataQuery> Queries;
        

        /// <summary>
        /// Constructor, creates the batch from given queries
        /// </summary>
        /// <param name="queries">List of queries to batch</param>
        public QueryBatch(IEnumerable<IDataQuery> queries)
        {
            Queries = queries;
        }


        /// <summary>
        /// Executes the batch
        /// </summary>
        public DataSet Execute()
        {
            DataSet result = null;
            var queries = GetGroupedQueries();

            // Execute all queries
            foreach (var query in queries)
            {
                if (result == null)
                {
                    result = new DataSet();
                }

                var queryResult = query.Execute();
                if ((queryResult != null) && (queryResult.Tables.Count > 0))
                {
                    DataHelper.TransferTables(result, queryResult);
                }
            }

            return result;
        }
        

        /// <summary>
        /// Gets the queries to execute the batch grouped by connection string
        /// </summary>
        private IEnumerable<IDataQuery> GetGroupedQueries()
        {
            // Group the queries by the connection string with which they should be executed
            var queryGroups =
                Queries
                    .GroupBy(
                        q => ConnectionHelper.EnsureExistingConnectionStringName(q.ConnectionStringName),
                        x => x
                    );

            foreach (var queryGroup in queryGroups)
            {
                yield return BuildBatchQuery(queryGroup, queryGroup.Key);
            }
        }


        /// <summary>
        /// Builds a batch query from the given list of queries
        /// </summary>
        /// <param name="queries">Queries to batch</param>
        /// <param name="connectionStringName">Connection string under which the resulting query should execute</param>
        private IDataQuery BuildBatchQuery(IEnumerable<IDataQuery> queries, string connectionStringName)
        {
            var sb = new StringBuilder();

            var result = new DataQuery()
            {
                ConnectionStringName = connectionStringName
            };

            foreach (var query in queries)
            {
                if (sb.Length > 0)
                {
                    sb.Append(Environment.NewLine);
                }

                var parameters = query.GetCompleteQueryParameters();
                var queryText = parameters.GetFullQueryText(result.EnsureParameters());

                sb.Append(queryText);
            }
            
            result.CustomQueryText = sb.ToString();

            return result;
        }


        /// <summary>
        /// Returns the full query text of the query for debug purposes
        /// </summary>
        public string GetFullQueryText()
        {
            var queries = GetGroupedQueries();

            return String.Join("\r\n\r\n", queries.Select(q => q.GetFullQueryText()));
        }
    }
}
