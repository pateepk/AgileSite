using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Base class for the query consisting of multiple queries
    /// </summary>
    public abstract class MultiQueryBase<TQuery, TInnerQuery> :
        DataQueryBase<TQuery>,
        IMultiQuery<TQuery, TInnerQuery>
        where TQuery : MultiQueryBase<TQuery, TInnerQuery>, new()
        where TInnerQuery : AbstractQueryObject, IDataQuery<TInnerQuery>, new()
    {
        #region "Variables"

        private QueryColumnList mSelectResultColumns;
        private string mOrderByResultColumns;

        /// <summary>
        /// Defines if type columns should be retrieved. If null, they are included if particular types were explicitly requested.
        /// </summary>
        private bool? mUseTypeColumns;

        private StringSafeDictionary<TInnerQuery> mQueries = new StringSafeDictionary<TInnerQuery>();
        private List<TInnerQuery> mQueriesList = new List<TInnerQuery>();
        private bool mDefaultOrderByType;

        private TInnerQuery mDefaultQuery;

        /// <summary>
        /// Defines how default query can be used. If null, the default query is forced in case typed columns are explicitly not allowed.
        /// </summary>
        private UseDefaultQueryEnum? mUseDefaultQuery;

        private bool mUseGlobalWhereOnResult;

        private const string ORIGINAL_SOURCE = "##ORIGINALSOURCE##";

        #endregion


        #region "Properties"

        /// <summary>
        /// Inner queries. Can be used to search for specific query.
        /// </summary>
        public StringSafeDictionary<TInnerQuery> Queries
        {
            get
            {
                return mQueries;
            }
            protected set
            {
                mQueries = value;
                Changed();
            }
        }


        /// <summary>
        /// Inner queries list. Defines order of the queries.
        /// </summary>
        public List<TInnerQuery> QueriesList
        {
            get
            {
                return mQueriesList;
            }
            protected set
            {
                mQueriesList = value;
                Changed();
            }
        }


        /// <summary>
        /// Default query used by the process when type queries are not used.
        /// </summary>
        protected TInnerQuery DefaultQuery
        {
            get
            {
                return mDefaultQuery;
            }
            set
            {
                mDefaultQuery = value;
                Changed();
            }
        }


        /// <summary>
        /// Flag indicating if the default query should be forcibly used. Default false. This flag is automatically reverted to false in case initializer of particular type query is used through Type("sometype", q => q.Where(...)).
        /// </summary>
        protected UseDefaultQueryEnum UseDefaultQuery
        {
            get
            {
                if (mUseDefaultQuery == null)
                {
                    // If type columns are explicitly not wanted, the default query is used instead of the typed ones, otherwise only if no types are defined
                    return (mUseTypeColumns == false) ? UseDefaultQueryEnum.Force : UseDefaultQueryEnum.Allowed;
                }

                return mUseDefaultQuery.Value;
            }
            set
            {
                mUseDefaultQuery = value;
                Changed();
            }
        }


        /// <summary>
        /// If true (default), the query uses type columns for the output, otherwise it uses only global columns
        /// </summary>
        public bool UseTypeColumns
        {
            get
            {
                if (mUseTypeColumns == null)
                {
                    // If types are defined explicitly, the query returns their full data by default
                    return Queries.Count > 0;
                }

                return mUseTypeColumns.Value;
            }
            set
            {
                mUseTypeColumns = value;
                Changed();
            }
        }


        /// <summary>
        /// If true, the result is ordered by source type by default, then by source order. If false (default), the result is ordered by the source order, and items from different types may interleave.
        /// </summary>
        public bool DefaultOrderByType
        {
            get
            {
                return mDefaultOrderByType;
            }
            set
            {
                mDefaultOrderByType = value;
                Changed();
            }
        }


        /// <summary>
        /// If true, the global where condition from the parent query is used outside the inner queries on the whole result. If false (default), the global where condition is used inside individual inner queries.
        /// </summary>
        public bool UseGlobalWhereOnResult
        {
            get
            {
                return mUseGlobalWhereOnResult;
            }
            set
            {
                mUseGlobalWhereOnResult = value;
                Changed();
            }
        }


        /// <summary>
        /// List of columns to use for results, by default returns all columns defined in the inner queries. Example: "DocumentName, DocumentID"
        /// </summary>
        public QueryColumnList SelectResultColumnsList
        {
            get
            {
                return mSelectResultColumns ?? (mSelectResultColumns = new QueryColumnList(this));
            }
            set
            {
                mSelectResultColumns = value;
                Changed();
            }
        }


        /// <summary>
        /// List of columns for the result order by. If not specified, the result is ordered by sources and global order by.
        /// </summary>
        public string OrderByResultColumns
        {
            get
            {
                return mOrderByResultColumns;
            }
            set
            {
                mOrderByResultColumns = value;
                Changed();
            }
        }


        /// <summary>
        /// Returns true if the query returns single column
        /// </summary>
        public override bool ReturnsSingleColumn
        {
            get
            {
                return SelectResultColumnsList.IsSingleColumn || SelectColumnsList.IsSingleColumn;
            }
        }


        /// <summary>
        /// Returns the types of the inner queries
        /// </summary>
        public IEnumerable<string> SelectedTypes
        {
            get
            {
                // Try to get single query
                var single = GetSingleInnerQuery();
                if (single != null)
                {
                    return new[] { single.Name };
                }

                return QueriesList.Select(q => q.Name);
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        protected MultiQueryBase()
            : base(null, null)
        {
        }


        /// <summary>
        /// Ensures the queries for the given types
        /// </summary>
        /// <param name="types">Types to ensure</param>
        protected void EnsureQueries(params string[] types)
        {
            EnsureQueries((IEnumerable<string>)types);
        }


        /// <summary>
        /// Ensures the queries for the given types
        /// </summary>
        /// <param name="types">Types to ensure</param>
        protected void EnsureQueries(IEnumerable<string> types)
        {
            foreach (var type in types)
            {
                EnsureQuery(type);
            }
        }


        /// <summary>
        /// Ensures the query with the given type
        /// </summary>
        /// <param name="type">Query type</param>
        protected virtual TInnerQuery EnsureQuery(string type)
        {
            var query = Queries[type];
            if (query == null)
            {
                query = CreateQuery(type);

                Queries[type] = query;
                QueriesList.Add(query);
            }

            return query;
        }


        /// <summary>
        /// Resolves the given type into corresponding types
        /// </summary>
        /// <param name="type">Source type</param>
        protected virtual List<string> ResolveType(string type)
        {
            // No resolving by default
            return new List<string> { type };
        }


        /// <summary>
        /// Creates query for the given type
        /// </summary>
        /// <param name="type">Query type</param>
        protected abstract TInnerQuery CreateQuery(string type);


        /// <summary>
        /// Sets class name for current query
        /// </summary>
        /// <param name="value">New class name value</param>
        protected override void SetClassName(string value)
        {
            if (DefaultQuery != null)
            {
                DefaultQuery.ClassName = value;
            }

            base.SetClassName(value);
        }

        #endregion


        #region "Execution methods"

        /// <summary>
        /// Gets the query to execute against database
        /// </summary>
        /// <param name="settings">Parameters for the query</param>
        public override IDataQuery GetExecutingQuery(DataQuerySettings settings = null)
        {
            IDataQuery q = null;

            // When source is defined, single query cannot be used, because we need to apply the source
            var singleQueryAllowed = !IsQuerySourceUsed();
            if (singleQueryAllowed)
            {
                q = GetSingleQuery(settings);
            }

            // Use multi-query if single query was not allowed, or if there are multiple executing queries
            if (q == null)
            {
                q = GetMultiQuery(settings);
            }

            q.IsSubQuery = IsSubQuery;
            q.IsNested = IsNested;

            return q;
        }


        /// <summary>
        /// Returns true if the query has defined QuerySource
        /// </summary>
        private bool IsQuerySourceUsed()
        {
            return (QuerySource != null);
        }


        /// <summary>
        /// Gets the complete parameters for the query execution
        /// </summary>
        /// <param name="executingQuery">Executing query for which the parameters are retrieved</param>
        public override DataQuerySettings GetCompleteSettings(IDataQuery executingQuery = null)
        {
            var q = GetExecutingQuery();
            if (q == null)
            {
                // No executing query = no settings
                return null;
            }

            // Use executing query to include all parameters for the query
            return q.GetCompleteSettings(executingQuery);
        }


        /// <summary>
        /// Gets the global settings for the query execution
        /// </summary>
        private DataQuerySettings GetGlobalSettings()
        {
            return base.GetCompleteSettings();
        }


        /// <summary>
        /// Gets the list of queries for multi query
        /// </summary>
        protected List<TInnerQuery> GetQueriesForMultiQuery()
        {
            var queries = QueriesList;
            if ((queries.Count == 0) && (DefaultQuery != null))
            {
                // When default query is available, use list with the default query
                queries = new List<TInnerQuery> { DefaultQuery };
            }
            
            // Clone the queries
            queries = queries.Select(CloneInnerQuery).ToList();

            return queries;
        }
        

        /// <summary>
        /// Gets the default source
        /// </summary>
        protected override QuerySource GetDefaultSource()
        {
            return new QuerySourceTable(ORIGINAL_SOURCE, "SubData");
        }


        /// <summary>
        /// Gets a multi-query for execution
        /// </summary>
        /// <param name="settings">Parameters for the query</param>
        protected virtual IDataQuery GetMultiQuery(DataQuerySettings settings)
        {
            // No queries, no query text
            var queries = GetQueriesForMultiQuery();

            return BuildMultiQueryFrom(queries, settings);
        }


        /// <summary>
        /// Builds a multi-query from the given list of queries
        /// </summary>
        /// <param name="queries">List of queries</param>
        /// <param name="settings">Parameters for the query</param>
        protected IDataQuery BuildMultiQueryFrom(List<TInnerQuery> queries, DataQuerySettings settings)
        {
            if ((queries == null) || (queries.Count == 0))
            {
                return null;
            }

            bool isSingleQuery = (queries.Count == 1);

            // Prepare global settings
            DataQuerySettings globalSettings = settings;
            if (globalSettings == null)
            {
                globalSettings = GetGlobalSettings();
                if (IsPagedQuery && !ForceOrderBy)
                {
                    globalSettings.OrderByColumns = OrderByColumns;
                }
            }

            var globalColumns = globalSettings.SelectColumnsList;

            // Expand columns when inner queries are more than 1 to synchronize its columns to avoid duplicity and to be able to use UNION
            // or if single query has defined global columns then type's columns should be expanded so '*' does not automatically include all global columns
            bool expandColumns = !isSingleQuery || globalColumns.AnyColumnsDefined;

            // Clone queries to allow their modification 
            queries.ForEach(q => PrepareInnerMultiQuery(q, expandColumns));

            // Ensure column lists
            if (!isSingleQuery)
            {
                SynchronizeColumnLists(queries);
            }

            int index = 0;
            var globalOrderBy = globalSettings.OrderByColumns;

            // Force resolve available general columns if they should be expanded or type columns are not requested
            var forceResolve = expandColumns || !UseTypeColumns;

            ResolveColumns(globalColumns, IncludeBinaryData, forceResolve);

            string extraColsFormat = "";

            // Apply the global order by only if DISTINCT or GROUP BY expressions are not used due to their limitations. (Original column is kept in the order by expression, because 
            // the columns aliases would need to be included in the select statement as well.)
            var applyOrderBy = (globalSettings.OrderByColumns != SqlHelper.NO_COLUMNS) && !globalSettings.SelectDistinct && !globalSettings.HasGroupBy;
            if (applyOrderBy)
            {
                // Ensure order by columns to be propagated to outer query, unless the global columns are locked
                globalColumns.EnsureOrderByColumns(ref globalOrderBy);

                // Ensure inner paging columns, but only in case result should be ordered
                if (OrderByResultColumns != SqlHelper.NO_COLUMNS)
                {
                    var extraCols = new QueryColumnList(SqlHelper.NO_COLUMNS);

                    extraCols.AddUnique(new RowNumberColumn("{0}").As(SystemColumns.SOURCE_ROW_NUMBER));
                    extraCols.AddUnique(new QueryColumn("{1}").As(SystemColumns.SOURCE_NUMBER));
                    extraCols.AddUnique(new QueryColumn("'{2}'").As(SystemColumns.SOURCE_TYPE));

                    extraColsFormat = extraCols.Columns;

                    var defaultOrder = String.Format(DefaultOrderByType ? "{1}, {0}" : "{0}, {1}", SystemColumns.SOURCE_ROW_NUMBER, SystemColumns.SOURCE_NUMBER);

                    globalOrderBy = SqlHelper.AddOrderBy(globalOrderBy, defaultOrder);
                }
            }

            // Prepare parameters
            var parameters = NewDataParameters();

            var globalWhere = globalSettings.WhereCondition;
            globalWhere = parameters.IncludeDataParameters(globalSettings.Parameters, globalWhere);

            var queryTexts = new List<string>();

            foreach (var query in queries)
            {
                // Add global columns and synchronized list of local columns
                // Prefer query columns before the global ones (in case the inner query has a column expression X AS Y and there is a global column Y)
                query.SelectColumnsList.AddRangeUnique(globalColumns);

                // Append row number and source number
                var innerSettings = query.GetCompleteSettings();

                string orderBy = innerSettings.OrderByColumns;
                if (String.IsNullOrEmpty(orderBy))
                {
                    orderBy = query.DefaultOrderByColumns;
                    if (String.IsNullOrEmpty(orderBy))
                    {
                        throw new NotSupportedException("[MultiQueryBase.GenerateQueryText]: The inner query for type '" + query.Name + "' cannot be generated without the orderBy set.");
                    }
                }

                string extraCols = String.Format(extraColsFormat, orderBy, index, query.Name);

                query.SelectColumnsList.AddRangeUnique(SqlHelper.ParseColumnList(extraCols));

                // Add global where condition
                query.WhereCondition = SqlHelper.AddWhereCondition(UseGlobalWhereOnResult ? null : globalWhere, query.WhereCondition);

                // Include parameters and handle name duplicities
                var innerParameters = query.GetCompleteQueryParameters();

                // Reset order by columns (handled above by row number), unless necessary by inner query
                if ((innerParameters.Macros.TopN <= 0) && (innerParameters.MaxRecords <= 0))
                {
                    innerParameters.Macros.OrderBy = SqlHelper.NO_COLUMNS;
                }

                var innerQueryText = innerParameters.GetFullQueryText(parameters);

                queryTexts.Add(innerQueryText);

                index++;
            }

            // Merge queries with UNION
            string queryText = SqlHelper.UnionQueries(queryTexts.ToArray(), true);

            // Resolve macros in original query
            queryText = parameters.ResolveMacros(queryText);

            // Execute the query via DataQuery
            var source = GetSource();

            if (source.SourceExpression.Contains(ORIGINAL_SOURCE))
            {
                queryText = source.IncludeDataParameters(parameters, queryText);
                source.SourceExpression = source.SourceExpression.Replace(ORIGINAL_SOURCE, "(" + Environment.NewLine + queryText + Environment.NewLine + ")");
            }

            // Create nested query
            var result = new DataQuery().From(source);

            // Base parameters
            if (UseGlobalWhereOnResult)
            {
                result.WhereCondition = globalWhere;
            }
            result.OrderByColumns = OrderByResultColumns ?? globalOrderBy;

            result.TopNRecords = TopNRecords;
            result.Offset = Offset;
            result.MaxRecords = MaxRecords;
            result.SelectColumnsList = SelectResultColumnsList;
            result.SelectDistinct = SelectDistinct;

            // Group by and distinct
            if (!String.IsNullOrEmpty(GroupByColumns))
            {
                result.GroupByColumns = GroupByColumns;
                result.HavingCondition = HavingCondition;
            }

            result.IsSubQuery = IsSubQuery;

            // Add total column counted from sub-totals if paging is used
            if (result.IsPagedQuery && !result.IsSubQuery && AllowTopNDistribution())
            {
                EnsureTotalFromSubTotals(result, isSingleQuery);
            }

            return result;
        }


        /// <summary>
        /// Synchronizes the column lists in the given list of queries
        /// </summary>
        /// <param name="queries">Queries</param>
        private void SynchronizeColumnLists(IEnumerable<TInnerQuery> queries)
        {
            var columnLists = queries.Select(q => q.SelectColumnsList).ToList();

            SqlHelper.EnsureMissingColumns(columnLists);
        }


        /// <summary>
        /// Ensures that the total number of records is generated from sub-totals if paging is enabled
        /// </summary>
        /// <param name="query">Result query</param>
        /// <param name="singleQuery">If true, the query is a single query</param>
        private void EnsureTotalFromSubTotals(IDataQuery query, bool singleQuery)
        {
            // Add total macro to provide expression for getting total records
            if (singleQuery)
            {
                query.TotalExpression = SystemColumns.SUB_TOTAL_RECORDS;
            }
            // For a multiquery with defined top N use default total expression
            else if (query.TopNRecords <= 0)
            {
                query.TotalExpression = SystemColumns.AGGREGATED_TOTAL;
            }
        }


        /// <summary>
        /// Attempts to get a single query if multi-query is defined only by a single query
        /// </summary>
        protected virtual TInnerQuery GetSingleInnerQuery()
        {
            TInnerQuery query = null;

            // Use default query if required
            if (UseDefaultQuery == UseDefaultQueryEnum.Force)
            {
                query = DefaultQuery;
            }

            // No query, try to use default query is available
            if ((query == null) && (QueriesList.Count == 0))
            {
                query = DefaultQuery;
            }

            // For single query, use it directly
            if ((query == null) && (QueriesList.Count == 1))
            {
                query = QueriesList[0];
            }

            return query;
        }


        /// <summary>
        /// Attempts to get a single query for the whole result based on the current state of the query object
        /// </summary>
        /// <param name="settings">Parameters for the query</param>
        protected virtual IDataQuery GetSingleQuery(DataQuerySettings settings)
        {
            TInnerQuery query = null;
            string typeWhere = null;

            // Use default query if required
            if (UseDefaultQuery == UseDefaultQueryEnum.Force)
            {
                query = DefaultQuery;

                if (query != null)
                {
                    // Types are ordered to make the resulting query deterministic
                    var types = Queries.TypedKeys.OrderBy(x => x);

                    typeWhere = GetTypesWhereCondition(types);
                }
            }

            // No query, try to use default query is available
            if ((query == null) && (QueriesList.Count == 0))
            {
                query = DefaultQuery;
            }

            var typedQuery = false;

            // For single query, use it directly
            if ((query == null) && (QueriesList.Count == 1))
            {
                query = QueriesList[0];
                typedQuery = true;
            }

            if (query == null)
            {
                return query;
            }

            query = CloneInnerQuery(query);

            // Prepare the query
            ApplyProperties(query);
                
            if (typedQuery)
            {
                var columnsDefined = SelectColumnsList.AnyColumnsDefined;

                if (UseTypeColumns && columnsDefined)
                {
                    // If type columns should be in result and any selected columns are defined expand typed query's columns to avoid using '*' as all columns (do not include global columns)
                    query.ExpandColumns();
                }
                else if (!UseTypeColumns && !columnsDefined)
                {
                    // If type columns should not be in result load explicitly only global columns to avoid resolving '*' as also type's columns (do not include type's columns)
                    query.SelectColumnsList.Load(GetAvailableColumns().ToArray());
                }
            }

            // Apply global parameters
            ApplyGlobalParameters(query, settings, true);

            if (typeWhere != null)
            {
                query.WhereCondition = SqlHelper.AddWhereCondition(query.WhereCondition, typeWhere);
            }

            var selectResultColumns = SelectResultColumnsList;
            var orderByResultColumns = OrderByResultColumns;

            SqlHelper.HandleEmptyColumns(ref orderByResultColumns);

            IDataQuery resultQuery = query;

            // If either result columns are specified and different, or specific order by for result is set, wrap into a nested query
            if ((selectResultColumns.AnyColumnsDefined && (selectResultColumns != SelectColumnsList)) || !String.IsNullOrEmpty(orderByResultColumns))
            {
                var innerSettings = query.GetCompleteSettings();

                // Move group by / having to outer scope
                query.HavingCondition = null;
                query.GroupByColumns = null;

                resultQuery = query.AsNested<DataQuery>();

                // Apply parameters to outer query
                if (selectResultColumns.AnyColumnsDefined)
                {
                    resultQuery.SelectColumnsList = selectResultColumns;
                }
                if (!String.IsNullOrEmpty(orderByResultColumns))
                {
                    resultQuery.OrderByColumns = orderByResultColumns;
                }

                resultQuery.GroupByColumns = innerSettings.GroupByColumns;
                resultQuery.HavingCondition = innerSettings.HavingCondition;
            }

            // Add total column counted from sub-totals if paging is used
            if (resultQuery.IsPagedQuery && !resultQuery.IsSubQuery && AllowTopNDistribution())
            {
                EnsureTotalFromSubTotals(resultQuery, true);
            }

            return resultQuery;
        }


        /// <summary>
        /// Makes a clone of the inner query
        /// </summary>
        /// <param name="query">Query to clone</param>
        private static TInnerQuery CloneInnerQuery(TInnerQuery query)
        {
            query = (TInnerQuery)query.CloneObject();
            return query;
        }


        /// <summary>
        /// Creates new data parameters for query execution
        /// </summary>
        private QueryDataParameters NewDataParameters()
        {
            // Prepare parameters
            var parameters = new QueryDataParameters();
            parameters.FillDataSet = NewDataSet();

            return parameters;
        }


        /// <summary>
        /// Returns the where condition which filters the default query data for specific types
        /// </summary>
        /// <param name="types">List of types for which create the where condition</param>
        protected virtual string GetTypesWhereCondition(IEnumerable<string> types)
        {
            throw new NotSupportedException("[MultiQueryBase.GetTypesWhereCondition]: This method must be overridden in order to use default query filtered by specific types.");
        }


        /// <summary>
        /// Creates a new DataSet for the query results
        /// </summary>
        protected virtual DataSet NewDataSet()
        {
            return null;
        }
        

        /// <summary>
        /// Prepares the inner query for execution within multi query
        /// </summary>
        /// <param name="query">Query to prepare</param>
        /// <param name="expandColumns">If true, query columns will be expanded</param>
        private void PrepareInnerMultiQuery(TInnerQuery query, bool expandColumns)
        {
            ApplyProperties(query, true);

            if (!UseTypeColumns && !query.SelectColumnsList.AnyColumnsDefined)
            {
                // Do not include any column by default if type columns should not be part of the result or no columns are defined (default selection will be used)
                query.SelectColumnsList.Load(SqlHelper.NO_COLUMNS);
            }
            else if (expandColumns)
            {
                // Expand all columns
                query.ExpandColumns();
            }
        }

        
        /// <summary>
        /// Applies global parameters to a single inner query
        /// </summary>
        /// <param name="query">Inner query</param>
        /// <param name="settings">Parameters for the query</param>
        /// <param name="includeParameters">If true, the parameters are also included into the query</param>
        private void ApplyGlobalParameters(TInnerQuery query, DataQuerySettings settings, bool includeParameters)
        {
            // Get global parameters
            settings = settings ?? GetGlobalSettings();

            var where = settings.WhereCondition;
            if (includeParameters)
            {
                where = query.IncludeDataParameters(Parameters, where);
            }

            // Prefer query columns before the global ones (in case the inner query has a column expression X AS Y and there is a global column Y)
            AddColumnsToQuery(query, settings.SelectColumnsList);

            ApplyOrderBy(settings, query);

            query.WhereCondition = SqlHelper.AddWhereCondition(where, query.WhereCondition);

            // Top N and paging
            query.MaxRecords = settings.MaxRecords;
            query.Offset = settings.Offset;

            ApplyTopN(settings.TopNRecords, query);

            query.SelectDistinct = SelectDistinct;

            // Group by and having
            if (!String.IsNullOrEmpty(GroupByColumns))
            {
                query.GroupByColumns = GroupByColumns;
                query.HavingCondition = HavingCondition;
            }
        }


        /// <summary>
        /// Add columns to the query.
        /// </summary>
        /// <param name="query">Query where columns will be added</param>
        /// <param name="columns">Columns to add</param>
        private static void AddColumnsToQuery(TInnerQuery query, QueryColumnList columns)
        {
            query.SelectColumnsList.AddRangeUnique(columns, false);

            // Relates to CM-4947, when user wants to define another columns with aliases but all columns selector is already present
            if (query.SelectColumnsList.ReturnsAllColumns)
            {
                // Input from webpart is { *, Column AS X } and user wants the X to be in resulting set and he is aware that X shouldn't be contained in *
                query.SelectColumnsList.AddRangeUniqueColumnsWithAliases(columns);
            }
        }


        /// <summary>
        /// Gets the list of all available columns for this query
        /// </summary>
        protected override List<string> GetAvailableColumns()
        {
            // No available columns by default
            return new List<string>();
        }


        /// <summary>
        /// Modifies the query to be able to be used as a sub-query, e.g. for usage in WHERE A IN ([query]). Ensures single column result, and removes order by from the result.
        /// </summary>
        public override IDataQuery AsSubQuery()
        {
            var result = base.AsSubQuery();

            // Ensure the result columns to be the same as select columns for multi query
            var mc = result as IMultiQuery;
            if (mc != null)
            {
                mc.OrderByResultColumns = SqlHelper.NO_COLUMNS;
            }

            return result;
        }


        /// <summary>
        /// Creates a single column query from the given query
        /// </summary>
        /// <param name="defaultColumn">Specific column to use in case query doesn't return single column yet</param>
        /// <param name="forceColumn">If true, the given column is forced to the output</param>
        public override IDataQuery AsSingleColumn(string defaultColumn = null, bool forceColumn = false)
        {
            var result = base.AsSingleColumn(defaultColumn, forceColumn);

            // Ensure the result columns to be the same as select columns for multi query
            var mc = result as IMultiQuery;
            if (mc != null)
            {
                mc.UseTypeColumns = false;
                mc.SelectResultColumnsList = mc.SelectColumnsList;
            }

            return result;
        }


        /// <summary>
        /// Gets the default single column for the query
        /// </summary>
        protected override string GetDefaultSingleColumn()
        {
            var defaultColumn = base.GetDefaultSingleColumn();

            // Fallback to global select columns for the default single column in case it would be set
            if (string.IsNullOrEmpty(defaultColumn) && SelectColumnsList.IsSingleColumn)
            {
                defaultColumn = SelectColumnsList.Columns;
            }

            return defaultColumn;
        }


        /// <summary>
        /// Copies the properties to the target query.
        /// </summary>
        /// <param name="target">Target query</param>
        public override void CopyPropertiesTo(IQueryObject target)
        {
            var t = target as MultiQueryBase<TQuery, TInnerQuery>;
            if (t != null)
            {
                t.DefaultQuery = DefaultQuery;
                t.UseDefaultQuery = UseDefaultQuery;
                t.SelectResultColumnsList = SelectResultColumnsList;
                t.mOrderByResultColumns = mOrderByResultColumns;
                t.mUseTypeColumns = mUseTypeColumns;
                t.mQueriesList.AddRange(mQueriesList);
                t.DefaultOrderByType = DefaultOrderByType;

                foreach (string key in mQueries.Keys)
                {
                    t.mQueries.Add(key, mQueries[key]);
                }
            }

            base.CopyPropertiesTo(target);
        }

        #endregion


        #region "Setup methods"

        /// <summary>
        /// Sets the given source as the source of the data query
        /// </summary>
        /// <param name="source">Data query source</param>
        public override TQuery WithSource(DataQuerySource source)
        {
            throw new NotSupportedException("[MultiQueryBase.WithSource]: The data source cannot be used in the multi object query.");
        }


        /// <summary>
        /// Includes given type with optional parameters.
        /// When additional parameters are specified, the query always executes using sub-queries for individual types.
        /// If only type is specified, the query allows usage of the default query and additional data from sub-queries may not be included.
        /// </summary>
        /// <param name="type">Type to include</param>
        /// <param name="parameters">Action to setup the inner type parameters</param>
        public TQuery Type(string type, Action<TInnerQuery> parameters = null)
        {
            var result = GetTypedQuery();

            var query = result.EnsureQuery(type);
            if (parameters != null)
            {
                parameters(query);

                // Do not allow default query anymore
                result.UseDefaultQuery = UseDefaultQueryEnum.NotAllowed;
            }

            return result;
        }


        /// <summary>
        /// Includes the given types to the resulting query
        /// </summary>
        /// <param name="types">Types to include</param>
        public TQuery Types(params string[] types)
        {
            var result = GetTypedQuery();

            foreach (var type in types)
            {
                if (!string.IsNullOrEmpty(type))
                {
                    // Resolve the type to ensure wildcards
                    var resolvedTypes = result.ResolveType(type);

                    result.EnsureQueries(resolvedTypes);
                }
            }

            return result;
        }


        /// <summary>
        /// Sets the column to select
        /// </summary>
        /// <param name="column">Column to set to be selected</param>
        public TQuery ResultColumn(string column)
        {
            return ResultColumns(column);
        }


        /// <summary>
        /// Sets the column to select
        /// </summary>
        /// <param name="column">Column to set to be selected</param>
        public TQuery ResultColumn(IQueryColumn column)
        {
            return ResultColumns(column);
        }


        /// <summary>
        /// Sets the columns to select
        /// </summary>
        /// <param name="columns">Columns to set to be selected</param>
        public TQuery ResultColumns(IEnumerable<string> columns)
        {
            var result = GetTypedQuery();

            result.SelectResultColumnsList.Load(columns.ToArray());

            return result;
        }


        /// <summary>
        /// Sets the columns to select
        /// </summary>
        /// <param name="columns">Columns to set to be selected</param>
        public TQuery ResultColumns(params string[] columns)
        {
            // Process single string input as list of columns separated by commas e.g "ColA, ColB, ColC"
            if ((columns != null) && (columns.Length == 1))
            {
                var result = GetTypedQuery();
                result.SelectResultColumnsList.Load(columns[0]);

                return result;
            }

            return ResultColumns((IEnumerable<string>)columns);
        }


        /// <summary>
        /// Sets the columns to select
        /// </summary>
        /// <param name="columns">Columns to set to be selected</param>
        public TQuery ResultColumns(params IQueryColumn[] columns)
        {
            var result = GetTypedQuery();

            result.SelectResultColumnsList.Load(columns);

            return result;
        }


        /// <summary>
        /// Adds the additional column to the query
        /// </summary>
        /// <param name="column">Column to add</param>
        public TQuery AddResultColumn(IQueryColumn column)
        {
            return AddResultColumns(column);
        }


        /// <summary>
        /// Adds the additional columns to the query
        /// </summary>
        /// <param name="columns">Columns to add</param>
        public TQuery AddResultColumns(params IQueryColumn[] columns)
        {
            var result = GetTypedQuery();

            result.SelectResultColumnsList.AddRangeUnique(columns);

            return result;
        }


        /// <summary>
        /// Adds the additional column to the query
        /// </summary>
        /// <param name="column">Column to add</param>
        public TQuery AddResultColumn(string column)
        {
            return AddResultColumns(column);
        }


        /// <summary>
        /// Adds the additional columns to the query
        /// </summary>
        /// <param name="columns">Columns to add</param>
        public TQuery AddResultColumns(IEnumerable<string> columns)
        {
            var result = GetTypedQuery();

            result.SelectResultColumnsList.AddRangeUnique(columns.ToArray());

            return result;
        }


        /// <summary>
        /// Adds the additional columns to the query
        /// </summary>
        /// <param name="columns">Columns to add</param>
        public TQuery AddResultColumns(params string[] columns)
        {
            return AddResultColumns((IEnumerable<string>)columns);
        }


        /// <summary>
        /// Adds the columns to the order by query
        /// </summary>
        /// <param name="columns">Columns to add to order by</param>
        public TQuery ResultOrderBy(params string[] columns)
        {
            return ResultOrderBy(OrderDirection.Default, columns);
        }


        /// <summary>
        /// Adds the columns to the order by query to order by the given columns in descending order
        /// </summary>
        /// <param name="columns">Columns to add to order by</param>
        public TQuery ResultOrderByDescending(params string[] columns)
        {
            return ResultOrderBy(OrderDirection.Descending, columns);
        }


        /// <summary>
        /// Adds the columns to the order by query to order by the given columns in ascending order
        /// </summary>
        /// <param name="columns">Columns to add to order by</param>
        public TQuery ResultOrderByAscending(params string[] columns)
        {
            return ResultOrderBy(OrderDirection.Ascending, columns);
        }


        /// <summary>
        /// Adds the columns to the order by query
        /// </summary>
        /// <param name="dir">Order direction</param>
        /// <param name="columns">Columns to add to order by</param>
        public TQuery ResultOrderBy(OrderDirection dir, params string[] columns)
        {
            var result = GetTypedQuery();

            string orderBy = result.OrderByResultColumns;

            // Add all column lists
            foreach (var list in columns)
            {
                orderBy = SqlHelper.AddOrderBy(orderBy, list, dir);
            }

            result.OrderByResultColumns = orderBy;

            return result;
        }


        /// <summary>
        /// Sets the query to return no columns at all
        /// </summary>
        public TQuery NoDefaultColumns()
        {
            return NoColumns();
        }

        #endregion
    }
}
