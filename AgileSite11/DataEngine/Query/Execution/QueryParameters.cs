using System;

using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Query parameters container.
    /// </summary>
    public class QueryParameters
    {
        #region "Variables"

        private bool mMacrosResolved;

        private QueryTypeEnum mType = QueryTypeEnum.SQLQuery;
        private QueryExecutionTypeEnum mExecutionType = QueryExecutionTypeEnum.Unknown;

        #endregion


        #region "Properties"

        /// <summary>
        /// Query type.
        /// </summary>
        public QueryTypeEnum Type
        {
            get
            {
                return mType;
            }
            set
            {
                mType = value;
            }
        }


        /// <summary>
        /// Requires transaction.
        /// </summary>
        public bool RequiresTransaction
        {
            get;
            set;
        }


        /// <summary>
        /// Connection string name to use
        /// </summary>
        public string ConnectionStringName
        {
            get;
            set;
        }


        /// <summary>
        /// Execution type.
        /// </summary>
        public QueryExecutionTypeEnum ExecutionType
        {
            get
            {
                return mExecutionType;
            }
            set
            {
                mExecutionType = value;
            }
        }


        /// <summary>
        /// If true, a new connection instance is created for this query execution
        /// </summary>
        public bool UseNewConnection
        {
            get;
            set;
        }


        /// <summary>
        /// Maximum number of records to get. If maxRecords is zero or less, all records are returned (no paging is used)
        /// </summary>
        public int MaxRecords
        {
            get;
            set;
        }


        /// <summary>
        /// Index of first record to get
        /// </summary>
        public int Offset
        {
            get;
            set;
        }


        /// <summary>
        /// Query name
        /// </summary>
        public string Name
        {
            get;
            set;
        }


        /// <summary>
        /// True if the parameters are suitable for query execution
        /// </summary>
        private bool IsForExecution
        {
            get;
            set;
        }


        /// <summary>
        /// Query text.
        /// </summary>
        public string Text
        {
            get;
            set;
        }


        /// <summary>
        /// Query expressions
        /// </summary>
        public QueryMacros Macros
        {
            get;
            set;
        }
        

        /// <summary>
        /// Query parameters.
        /// </summary>
        public QueryDataParameters Params
        {
            get;
            set;
        }
        

        /// <summary>
        /// If true, the query is a sub-query used in another query.
        /// This brings certain constraints such as that it cannot use order by or CTE.
        /// </summary>
        public bool IsSubQuery
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates that this query is nested within another query as its source.
        /// This brings certain constraints such as that is cannot use CTE.
        /// </summary>
        public bool IsNested
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="queryText">Query text</param>
        /// <param name="queryParams">Query parameters</param>
        /// <param name="queryType">Query type</param>
        /// <param name="transaction">Requires transaction</param>
        public QueryParameters(string queryText, QueryDataParameters queryParams, QueryTypeEnum queryType, bool transaction = false)
        {
            Text = queryText;
            Params = queryParams;
            Type = queryType;
            RequiresTransaction = transaction;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="qi">Query info</param>
        /// <param name="parameters">Query parameters</param>
        /// <param name="macros">Query expressions</param>
        public QueryParameters(QueryInfo qi, QueryDataParameters parameters, QueryMacros macros)
        {
            Name = qi.QueryFullName;
            Text = qi.QueryText;
            Type = qi.QueryType;
            RequiresTransaction = qi.QueryRequiresTransaction;
            ConnectionStringName = qi.QueryConnectionString;

            Params = parameters;
            Macros = macros;
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="queryName">Query name in format application.class.queryname</param>
        /// <param name="queryText">Query text</param>
        /// <param name="parameters">Query parameters</param>
        /// <param name="queryType">Query type</param>
        /// <param name="requiresTransaction">Requires transaction</param>
        /// <param name="macros">Query expressions</param>
        /// <param name="connectionStringName">Connection string name</param>
        public QueryParameters(string queryName, string queryText, QueryDataParameters parameters, QueryTypeEnum queryType, bool requiresTransaction, QueryMacros macros, string connectionStringName)
        {
            Name = queryName;
            Text = queryText;
            Params = parameters;
            Type = queryType;
            RequiresTransaction = requiresTransaction;
            Macros = macros;
            ConnectionStringName = connectionStringName;
        }


        /// <summary>
        /// Resolves the query macros in <see cref="Text"/> property value.
        /// </summary>
        public void ResolveMacros()
        {
            ResolveMacrosInternal(true);
        }


        /// <summary>
        /// Resolves the query macros in <see cref="Text"/> property value.
        /// </summary>
        /// <param name="updateQueryText">If true, the query text is updated with resolved version</param>
        /// <returns>Resolved query text</returns>
        private string ResolveMacrosInternal(bool updateQueryText)
        {
            string queryText = Text;

            // Only resolve if macros were not yet resolved
            if (!mMacrosResolved)
            {
                if (Params != null)
                {
                    queryText = Params.ResolveMacros(queryText);
                }

                if (Macros != null)
                {
                    queryText = Macros.ResolveMacros(queryText);
                }

                if (updateQueryText)
                {
                    Text = queryText;

                    // Mark current query text as already resolved
                    mMacrosResolved = true;
                }
            }

            return queryText;   
        }


        /// <summary>
        /// Gets the query parameters for query execution
        /// </summary>
        /// <param name="getTotal">If true, the query is configured to get total number of records when paging is enabled</param>
        /// <param name="subQuery">If true, the query is used as a sub-query</param>
        internal QueryParameters GetParametersForExecution(bool getTotal = true, bool subQuery = false)
        {
            var query = Clone();

            // Finalize query with paging / sub query settings
            query.FinalizeForExecution(getTotal, subQuery);

            return query;
        }


        /// <summary>
        /// Clones the query parameters
        /// </summary>
        private QueryParameters Clone()
        {
            var query = new QueryParameters(Name, Text, Params, Type, RequiresTransaction, Macros, ConnectionStringName);

            query.mMacrosResolved = mMacrosResolved;
            query.MaxRecords = MaxRecords;
            query.Offset = Offset;

            query.IsForExecution = IsForExecution;
            query.UseNewConnection = UseNewConnection;
            query.IsNested = IsNested;
            query.IsSubQuery = IsSubQuery;
            query.ExecutionType = ExecutionType;

            return query;
        }


        /// <summary>
        /// Finalizes the given query parameters for execution within SQL connection
        /// </summary>
        /// <param name="getTotal">If true, the query is configured to get total number of records when paging is enabled</param>
        /// <param name="subQuery">If true, the query is used as a sub-query</param>
        internal void FinalizeForExecution(bool getTotal = true, bool subQuery = false)
        {
            var maxRecords = MaxRecords;
            if (maxRecords > 0)
            {
                // Transform parameters to a paging query if paging is configured
                SqlHelper.CheckPagedQuery(this);

                // Prepare the query
                var macros = Macros;

                var queryText = SqlHelper.PreparePagedQuery(Text, macros, Offset, maxRecords, getTotal, subQuery);

                // Prepare the parameters and always allow cache for paged results (always is select)
                Text = queryText;
                Offset = 0;
                MaxRecords = 0;
            }

            IsForExecution = true;
        }


        /// <summary>
        /// Gets the full query text including resolved parameters
        /// </summary>
        /// <param name="parameters">Query parameters. If provided, the data parameters are included into the parameters and query text is altered accordingly</param>
        internal virtual string GetFullQueryText(QueryDataParameters parameters)
        {
            var text = GetFullQueryText(false, false);

            // Include data parameters and transform query text if provided
            if (parameters != null)
            {
                text = parameters.IncludeDataParameters(Params, text);
            }

            return text;
        }


        /// <summary>
        /// Gets the full query text including resolved parameters
        /// </summary>
        /// <param name="expand">If true, the parameters are expanded with their values, otherwise the parameter names are kept in the query.</param>
        /// <param name="includeParameters">If true, parameter declarations are included if parameters are not expanded</param>
        internal virtual string GetFullQueryText(bool expand = false, bool includeParameters = true)
        {
            // Check the query type
            SqlHelper.CheckQueryTypeForTextGeneration(Type, Name);

            if (!IsForExecution)
            {
                var isSubQuery = IsSubQuery;
                var isInnerQuery = isSubQuery || IsNested;

                var query = GetParametersForExecution(!isSubQuery, isInnerQuery);

                return query.GetFullQueryText(expand, includeParameters);
            }
            
            // Generate the query text
            var text = ResolveMacrosInternal(false);

            var dataParams = Params;
            if (dataParams != null)
            {
                if (expand)
                {
                    // Expand the query text if necessary
                    text = dataParams.Expand(text);
                }
                else if (includeParameters)
                {
                    // Get the parameters
                    var parameters = dataParams.GetDeclaration();

                    if (!String.IsNullOrEmpty(parameters))
                    {
                        text = String.Format("{0}\r\n\r\n{1}", parameters, text);
                    }
                }
            }

            return text;
        }


        /// <summary>
        /// Returns the string representation of the query parameters
        /// </summary>
        public override string ToString()
        {
            return GetFullQueryText(false, false);
        }

        #endregion
    }
}