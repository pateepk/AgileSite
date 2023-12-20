using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

using CMS.Base;
using CMS.DataEngine.Query;
using CMS.Helpers;

using QueryType = CMS.DataEngine.QueryName;

namespace CMS.DataEngine
{
    /// <summary>
    /// Queries particular database data or defines parameters for data selection
    /// </summary>
    public abstract class DataQueryBase<TQuery> : DataQuerySettingsBase<TQuery>, IDataQuery<TQuery>
        where TQuery : DataQueryBase<TQuery>, new()
    {
        #region "Variables"

        // DataSet with the result
        private DataSet mResult;

        // If true, the query was executed
        private bool mDataLoaded;

        // If true, the query has been generated
        private bool mQueryGenerated;

        // Total number of records available for the given query
        private int mTotalRecords = -1;

        // If true, the query includes binary data columns
        private bool mIncludeBinaryData = true;

        // Object for locking this instance context
        private readonly object lockObject = new object();

        private string mQueryText;
        private string mClassName;
        private string mQueryName;
        private string mDefaultOrderByColumns;
        private DataQuerySource mDataSource;
        private string mCustomQueryText;
        private bool? mReturnsSingleColumn;
        private string mConnectionStringName;
        private bool? mAllowMaterialization;

        #endregion


        #region "Properties"

        /// <summary>
        /// Object name, empty by default
        /// </summary>
        public override string Name
        {
            get
            {
                return FullQueryName;
            }
        }


        /// <summary>
        /// Query text
        /// </summary>
        public string QueryText
        {
            get
            {
                if (!mQueryGenerated)
                {
                    lock (lockObject)
                    {
                        if (!mQueryGenerated)
                        {
                            // Execute the query
                            mQueryText = GetFullQueryText(false, false);

                            mQueryGenerated = true;
                        }
                    }
                }

                return mQueryText;
            }
        }


        /// <summary>
        /// Custom query text
        /// </summary>
        public string CustomQueryText
        {
            get
            {
                return mCustomQueryText;
            }
            set
            {
                mCustomQueryText = value;
                Changed();
            }
        }


        /// <summary>
        /// DataSet with the result
        /// </summary>
        public DataSet Result
        {
            get
            {
                if (!mDataLoaded)
                {
                    lock (lockObject)
                    {
                        if (!mDataLoaded)
                        {
                            // Execute the query
                            Execute();
                        }
                    }
                }

                return mResult;
            }
            protected set
            {
                mResult = value;
                mDataLoaded = (value != null);
            }
        }


        /// <summary>
        /// Class name
        /// </summary>
        public string ClassName
        {
            get
            {
                return mClassName ?? (mClassName = GetClassName());
            }
            set
            {
                SetClassName(value);
                DataSourceChanged();
            }
        }


        /// <summary>
        /// Query name
        /// </summary>
        public string QueryName
        {
            get
            {
                return mQueryName;
            }
            set
            {
                mQueryName = value;
                DataSourceChanged();
            }
        }


        /// <summary>
        /// Represents a full query name of the query
        /// </summary>
        public virtual string FullQueryName
        {
            get
            {
                // Ensure default query name
                var queryName = QueryName;
                var className = ClassName;
                if (String.IsNullOrEmpty(queryName))
                {
                    // Fallback to the SELECTALL query if is explicitly defined because of backward compatibility, if not exists in database use GENERALSELECT
                    queryName = QueryInfoProvider.QueryIsExplicitlyDefined(className, QueryType.SELECTALL) ? QueryType.SELECTALL : QueryType.GENERALSELECT;
                }

                return $"{className}.{queryName}";
            }
        }


        /// <summary>
        /// Gets the number of total records when paging is used. Gets updated once the query executes
        /// </summary>
        public int TotalRecords
        {
            get
            {
                // Check if some results are to be returned
                if (ReturnsNoResults)
                {
                    return 0;
                }

                if (mTotalRecords < 0)
                {
                    lock (lockObject)
                    {
                        if (mTotalRecords < 0)
                        {
                            LoadTotalRecords();
                        }
                    }
                }

                return mTotalRecords;
            }
            protected set
            {
                mTotalRecords = value;
            }
        }


        /// <summary>
        /// Number of total items in the collection
        /// </summary>
        public int Count
        {
            get
            {
                if (!IsPagedQuery)
                {
                    // Queries without paging return all items at once, hence total records is the same as count
                    return TotalRecords;
                }

                // Page will start at offset and return all remaining records capped by MaxRecords
                return Math.Min(Math.Max(TotalRecords - Offset, 0), MaxRecords);
            }
        }


        /// <summary>
        /// If true, the query includes the object binary data. Default is true
        /// </summary>
        public bool IncludeBinaryData
        {
            get
            {
                return mIncludeBinaryData;
            }
            set
            {
                mIncludeBinaryData = value;
                Changed();
            }
        }


        /// <summary>
        /// Data source that provides the query data. If not set, the query queries the database directly
        /// </summary>
        public DataQuerySource DataSource
        {
            get
            {
                return mDataSource;
            }
            set
            {
                mDataSource = value;

                DefaultQuerySource = "[ExternalSource]";
                DataSourceChanged();
            }
        }


        /// <summary>
        /// Default order by columns used in case if needed, and order by is not specified
        /// </summary>
        public string DefaultOrderByColumns
        {
            get
            {
                return mDefaultOrderByColumns ?? (mDefaultOrderByColumns = GetDefaultOrderBy());
            }
            set
            {
                mDefaultOrderByColumns = value;

                Changed();
            }
        }


        /// <summary>
        /// Returns true if the next page is available.
        /// </summary>
        public bool NextPageAvailable
        {
            get
            {
                if (!IsPagedQuery)
                {
                    return false;
                }

                // Next page is available if current page ends lower than total count
                return (TotalRecords > Offset + MaxRecords);
            }
        }


        /// <summary>
        /// Returns true if the query supports data reader
        /// </summary>
        public bool SupportsReader
        {
            get
            {
                // External data source doesn't support reader
                if (HasDataSource)
                {
                    return false;
                }

                return true;
            }
        }


        /// <summary>
        /// Returns true if the query returns single column
        /// </summary>
        public virtual bool ReturnsSingleColumn
        {
            get
            {
                if (mReturnsSingleColumn == null)
                {
                    return SelectColumnsList.IsSingleColumn;
                }

                return mReturnsSingleColumn.Value;
            }
            set
            {
                mReturnsSingleColumn = value;
            }
        }


        /// <summary>
        /// Connection string name
        /// </summary>
        public string ConnectionStringName
        {
            get
            {
                return mConnectionStringName ?? (mConnectionStringName = GetConnectionStringName());
            }
            set
            {
                mConnectionStringName = value;
                ConnectionStringForced = (value != null);

                DataSourceChanged();
            }
        }


        /// <summary>
        /// If true, the connection string was set explicitly
        /// </summary>
        protected bool ConnectionStringForced
        {
            get;
            private set;
        }


        /// <summary>
        /// If true, the query allows materialization
        /// </summary>
        public bool AllowMaterialization
        {
            get
            {
                if (mAllowMaterialization == null)
                {
                    return !SystemContext.DevelopmentMode;
                }

                return mAllowMaterialization.Value;
            }
            set
            {
                mAllowMaterialization = value;
            }
        }


        /// <summary>
        /// Returns true if the query has specific data source
        /// </summary>
        public bool HasDataSource
        {
            get
            {
                return (DataSource != null);
            }
        }


        /// <summary>
        /// If true, this query is combined from several queries. When additional parameters are applied to it, it will be wrapped into a nested query.
        /// </summary>
        public bool IsCombinedQuery
        {
            get;
            set;
        }

        #endregion


        #region "Mirrored properties"

        /// <summary>
        /// Collection of the result tables
        /// </summary>
        public DataTableCollection Tables
        {
            get
            {
                return Result.Tables;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates a query based on the given query name
        /// </summary>
        /// <param name="className">Class name</param>
        /// <param name="queryName">Query name</param>
        protected DataQueryBase(string className, string queryName)
        {
            ClassName = className;
            QueryName = queryName;
        }


        /// <summary>
        /// Creates a query based on the given query name
        /// </summary>
        /// <param name="queryName">Full query name</param>
        protected DataQueryBase(string queryName)
        {
            if (queryName != null)
            {
                string className;

                ObjectHelper.ParseFullName(queryName, out className, out queryName);

                ClassName = className;
                QueryName = queryName;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Marks the object as changed
        /// </summary>
        public override void Changed()
        {
            base.Changed();

            Reset();

            mQueryGenerated = false;
            mQueryText = null;
        }


        /// <summary>
        /// Flushes the results but leaves the generated query text unchanged.
        /// After the reset, query can be executed again to obtain new data.
        /// </summary>
        public override void Reset()
        {
            base.Reset();

            mDataLoaded = false;
            mTotalRecords = -1;
        }


        /// <summary>
        /// Marks the object as changed when data source changes
        /// </summary>
        protected virtual void DataSourceChanged()
        {
            DataSourceName = null;

            Changed();
        }


        /// <summary>
        /// Gets the connection string name of the query
        /// </summary>
        private string GetConnectionStringName()
        {
            string result = null;

            // Get the name from the attached data source or connection string name
            if (HasDataSource)
            {
                result = DataSource.ConnectionStringName;
            }
            else if (!String.IsNullOrEmpty(ClassName) && ConnectionHelper.IsConnectionStringInitialized)
            {
                // Get connection string from the query as a source name
                var qi = QueryInfoProvider.GetQueryInfo(FullQueryName, false);
                if (qi != null)
                {
                    var connStringName = qi.QueryConnectionString;

                    if (!String.IsNullOrEmpty(connStringName))
                    {
                        result = connStringName;
                    }
                }
            }

            // Use default connection string name if not determined by the query
            return result ?? ConnectionHelper.DEFAULT_CONNECTIONSTRING_NAME;
        }


        /// <summary>
        /// Gets the complete parameters for the query execution. The parameters are always a new instance of DataQuerySettings which can be further modified without any impact to the query itself.
        /// </summary>
        /// <param name="executingQuery">Executing query for which the parameters are retrieved</param>
        public virtual DataQuerySettings GetCompleteSettings(IDataQuery executingQuery = null)
        {
            var settings = new DataQuerySettings();

            lock (lockObject)
            {
                CopyPropertiesTo(settings);

                EnsureColumns(settings);
                EnsureDefaultOrderBy(settings);
                
                bool hasDataSource = HasDataSource;

                if (executingQuery != null)
                {
                    hasDataSource = executingQuery.HasDataSource;
                }

                if (!hasDataSource)
                {
                    // Ensure default source only in case external data source is not defined
                    EnsureDefaultSource(settings);
                }
            }

            return settings;
        }


        /// <summary>
        /// Ensures columns within the given parameters
        /// </summary>
        /// <param name="settings">Query parameters</param>
        private void EnsureColumns(DataQuerySettings settings)
        {
            if (!IncludeBinaryData)
            {
                ResolveColumns(settings.SelectColumnsList, false);
            }
        }


        /// <summary>
        /// Gets the binary columns for this query
        /// </summary>
        protected List<string> GetBinaryColumns()
        {
            // Resolve columns if binary data not included
            List<string> binaryColumns = null;

            var csi = GetClassStructureInfo();
            if ((csi != null) && csi.HasBinaryColumns)
            {
                binaryColumns = csi.BinaryColumns;
            }

            return binaryColumns;
        }


        /// <summary>
        /// Resolves the columns in the given list
        /// </summary>
        /// <remarks>
        /// Merges <paramref name="columns"/> with columns from <see cref="ClassStructureInfo"/> of type <typeparamref name="TQuery"/>.
        /// </remarks>
        /// <param name="columns">List of columns to resolve</param>
        /// <param name="includeBinaryData">Include binary columns</param>
        /// <param name="forceResolve">If true, the resolving is forced (resolves even when not necessary)</param>
        protected void ResolveColumns(QueryColumnList columns, bool includeBinaryData, bool forceResolve = false)
        {
            // Only resolve if columns would contain all columns (*), otherwise the columns would not be resolved anyway
            if (columns.ReturnsAllColumns && (!includeBinaryData || forceResolve))
            {
                var allColumns = GetAvailableColumns();

                // Don't resolve if all columns collection is not available
                if ((allColumns != null) && (allColumns.Count > 0))
                {
                    IEnumerable<string> allCols = allColumns;
                    IEnumerable<string> binaryColumns = null;

                    // Remove binary columns if necessary
                    if (!includeBinaryData)
                    {
                        binaryColumns = GetBinaryColumns();

                        if (binaryColumns != null)
                        {
                            allCols = allCols.Except(binaryColumns);
                        }
                    }

                    // Resolve the columns
                    if (forceResolve || (binaryColumns != null))
                    {
                        // Add columns to the list, ignore the same columns
                        columns.AddRange(allCols.Select(colName => colName.AsColumn()));

                        columns.RemoveAll(new QueryColumn(SqlHelper.COLUMNS_ALL));
                    }
                }
                else if (!columns.AnyColumnsDefined)
                {
                    columns.Load(SqlHelper.NO_COLUMNS);
                }
            }
        }


        /// <summary>
        /// Gets the list of all available columns for this query
        /// </summary>
        protected virtual List<string> GetAvailableColumns()
        {
            var csi = GetClassStructureInfo();

            // No available columns if structure info not found
            if (csi == null)
            {
                return null;
            }

            return csi.ColumnNames;
        }


        /// <summary>
        /// Ensures the default order by within the given query parameters
        /// </summary>
        /// <param name="settings">Query parameters</param>
        private void EnsureDefaultOrderBy(DataQuerySettings settings)
        {
            // Ensure some order by in case of paging
            if ((IsPagedQuery || ForceOrderBy) && String.IsNullOrEmpty(settings.OrderByColumns))
            {
                settings.OrderBy(DefaultOrderByColumns);
            }
        }


        /// <summary>
        /// Ensures default source within the given query parameters
        /// </summary>
        /// <param name="settings">Query parameters</param>
        private void EnsureDefaultSource(DataQuerySettings settings)
        {
            // Ensure default source within parameters
            if ((settings.QuerySource == null) && (settings.DefaultQuerySource == null))
            {
                // Get the default source for this query
                var src = GetDefaultSource();

                settings.DefaultQuerySource = src;
            }
        }


        /// <summary>
        /// Gets the default source for this query
        /// </summary>
        protected override QuerySource GetDefaultSource()
        {
            string src = null;

            // Get class structure info
            var csi = GetClassStructureInfo();
            if (csi != null)
            {
                // Get the table name
                src = csi.TableName;
            }

            return new QuerySource(src);
        }


        /// <summary>
        /// Gets the class name for current query
        /// </summary>
        protected virtual string GetClassName()
        {
            return null;
        }


        /// <summary>
        /// Gets the class name for current query
        /// </summary>
        protected virtual void SetClassName(string value)
        {
            mClassName = value;
        }


        /// <summary>
        /// Gets the default order by columns
        /// </summary>
        protected virtual string GetDefaultOrderBy()
        {
            return null;
        }


        /// <summary>
        /// Gets the ID column for this query
        /// </summary>
        protected virtual string GetIDColumn()
        {
            string idColumn = null;

            // Get class structure info
            var csi = GetClassStructureInfo();
            if (csi != null)
            {
                // Get the ID column
                idColumn = csi.IDColumn;
            }

            return idColumn;
        }


        /// <summary>
        /// Copies the properties to the target query.
        /// </summary>
        /// <param name="target">Target query</param>
        public override void CopyPropertiesTo(IQueryObject target)
        {
            var t = target as IDataQuery;
            if (t != null)
            {
                if (!String.IsNullOrEmpty(QueryName))
                {
                    t.QueryName = QueryName;
                }

                if (!String.IsNullOrEmpty(ClassName))
                {
                    t.ClassName = ClassName;
                }

                if (DataSource != null)
                {
                    t.DataSource = DataSource;
                }

                if (!String.IsNullOrEmpty(CustomQueryText))
                {
                    t.CustomQueryText = CustomQueryText;
                }

                t.IncludeBinaryData = IncludeBinaryData;

                if (ConnectionStringForced)
                {
                    t.ConnectionStringName = ConnectionStringName;
                }

                if (mAllowMaterialization != null)
                {
                    t.AllowMaterialization = mAllowMaterialization.Value;
                }
            }

            base.CopyPropertiesTo(target);
        }


        /// <summary>
        /// Applies this query parameters to the target object
        /// </summary>
        /// <param name="target">Target object defining parameters</param>
        public override void ApplyParametersTo(IQueryObject target)
        {
            var t = target as IDataQuery;
            if (t != null)
            {
                t.IsSubQuery = IsSubQuery;
            }

            base.ApplyParametersTo(target);
        }


        /// <summary>
        /// Returns the string representation of the expression, with possibility of expanding parameters
        /// </summary>
        /// <param name="expand">If true, the result is expanded with parameters so it can act as standalone value.</param>
        public override string ToString(bool expand)
        {
            return GetFullQueryText(expand, false);
        }


        /// <summary>
        /// Gets the full query text including resolved parameters
        /// </summary>
        /// <param name="expand">If true, the parameters are expanded</param>
        /// <param name="includeParameters">If true, parameter declarations are included if parameters are not expanded</param>
        /// <param name="settings">Query settings</param>
        public string GetFullQueryText(bool expand = false, bool includeParameters = true, DataQuerySettings settings = null)
        {
            var q = GetExecutingQuery();
            if (q == null)
            {
                // No executing query = no query text
                return "";
            }

            if (q == this)
            {
                // Get the query text from this query
                return GetFullQueryTextInternal(expand, includeParameters, settings);
            }

            // Get the query text from executing query
            return q.GetFullQueryText(expand, includeParameters, settings);
        }


        /// <summary>
        /// Gets the full query text including resolved parameters
        /// </summary>
        /// <param name="expand">If true, the parameters are expanded with their values, otherwise the parameter names are kept in the query.</param>
        /// <param name="includeParameters">If true, parameter declarations are included if parameters are not expanded</param>
        /// <param name="settings">Query settings</param>
        private string GetFullQueryTextInternal(bool expand = false, bool includeParameters = true, DataQuerySettings settings = null)
        {
            settings = settings ?? GetCompleteSettings();

            // Get query parameters and finalize them so that they represent final query for execution
            var query = GetCompleteQueryParametersInternal(settings);

            return query.GetFullQueryText(expand, includeParameters);
        }


        /// <summary>
        /// Loads the number of total records count from the query
        /// </summary>
        private void LoadTotalRecords()
        {
            if (IsPagedQuery)
            {
                LoadTotalRecordsPaged();
            }
            else
            {
                LoadTotalRecordsNonPaged();
            }
        }


        /// <summary>
        /// Loads the number of total records for an unpaged query
        /// </summary>
        protected void LoadTotalRecordsNonPaged()
        {
            // Get total records with COUNT(*) over the query
            var originalOrderBy = mOrderByColumns;

            try
            {
                // Inner query does not need order by
                mOrderByColumns = SqlHelper.NO_COLUMNS;

                // If the final query is different, get total records from it
                var q = this.GetFinalExecutingQuery();
                if (q != this)
                {
                    TotalRecords = q.TotalRecords;

                    return;
                }

                if (HasDataSource)
                {
                    // Get total records from data source (query with external data source does not support AsNested)
                    var p = PrepareDataQuerySourceParameters();

                    TotalRecords = DataSource.GetCount(p);

                    return;
                }

                var countQuery =
                     AsNestedInternal<DataQuery>()
                         .Column(new CountColumn().As("Count"));

                TotalRecords = countQuery.GetScalarResult(0);
            }
            finally
            {
                mOrderByColumns = originalOrderBy;
            }
        }


        /// <summary>
        /// Loads the number of total records for a paged query
        /// </summary>
        protected void LoadTotalRecordsPaged()
        {
            var originalMaxRecords = mMaxRecords;

            try
            {
                // Disable paging
                mMaxRecords = 0;

                // Load count as non-paged
                LoadTotalRecordsNonPaged();
            }
            finally
            {
                mMaxRecords = originalMaxRecords;
            }
        }


        /// <summary>
        /// Converts the query to the query column using this query as nested
        /// </summary>
        /// <param name="columnName">Column name</param>
        public virtual IQueryColumn AsColumn(string columnName)
        {
            var result = new NestedSelectQueryColumn(this).As(columnName);

            return result;
        }


        /// <summary>
        /// Modifies the query to be able to be used as a sub-query, e.g. for usage in WHERE A IN ([query]). Ensures single column result, and removes order by from the result.
        /// </summary>
        public virtual IDataQuery AsSubQuery()
        {
            // Ensure single column
            var result = AsSingleColumn();

            // Remove order by unless TOPN is also specified or paging is used
            if ((result.TopNRecords <= 0) && (MaxRecords <= 0))
            {
                result.OrderByColumns = SqlHelper.NO_COLUMNS;
            }

            result.IsSubQuery = true;

            return result;
        }


        /// <summary>
        /// Gets a query expression representing this object as a value
        /// </summary>
        public virtual string GetExpression()
        {
            return QueryText;
        }


        /// <summary>
        /// Gets a query expression representing this object as a value
        /// </summary>
        public virtual IQueryExpression AsValue()
        {
            var val = SqlHelper.GetValueExpression(QueryText);

            var expr = new QueryExpression();

            expr.Expression = expr.IncludeDataParameters(Parameters, val);

            return expr;
        }


        /// <summary>
        /// Returns true, if the query returns any results
        /// </summary>
        public bool HasResults()
        {
            // Check if some results are to be returned
            if (ReturnsNoResults)
            {
                return false;
            }

            // Get the count
            return Count > 0;
        }


        /// <summary>
        /// Creates a single column query from the given query
        /// </summary>
        /// <param name="defaultColumn">Specific column to use in case query doesn't return single column yet</param>
        /// <param name="forceColumn">If true, the given column is forced to the output</param>
        public virtual IDataQuery AsSingleColumn(string defaultColumn = null, bool forceColumn = false)
        {
            var result = GetTypedQuery();

            // If filter columns are defined, must be wrapped into a nested query
            if (!FilterColumns.NoColumns)
            {
                return result.AsNested<DataQuery>().AsSingleColumn();
            }

            if (!ReturnsSingleColumn || forceColumn)
            {
                if (String.IsNullOrEmpty(defaultColumn))
                {
                    // ID as the default for single column
                    defaultColumn = GetDefaultSingleColumn();

                    if (String.IsNullOrEmpty(defaultColumn))
                    {
                        throw new NotSupportedException("[DataQueryBase.AsSingleColumn]: Unable to convert the query to a single column query, cannot find the ID column. Please specify the return column of the query explicitly by calling method Column(...)");
                    }
                }

                result.SelectColumnsList.Load(defaultColumn);
            }

            return result;
        }


        /// <summary>
        /// Gets the default single column for the query
        /// </summary>
        protected virtual string GetDefaultSingleColumn()
        {
            return GetIDColumn();
        }


        /// <summary>
        /// Creates an ID query from the given query
        /// </summary>
        public virtual IDataQuery AsIDQuery()
        {
            return AsSingleColumn(GetIDColumn(), true);
        }


        /// <summary>
        /// Creates a nested query from the given query
        /// </summary>
        /// <param name="settings">Settings</param>
        public TResult AsNested<TResult>(NestedQuerySettings settings = null)
            where TResult : IDataQuery<TResult>, new()
        {
            var q = GetExecutingQuery();
            if (q == this)
            {
                // Wrap current query
                return AsNestedInternal<TResult>(null, settings);
            }

            // Wrap executing query
            return q.AsNested<TResult>(settings);
        }


        /// <summary>
        /// Gets the query to execute against database
        /// </summary>
        /// <param name="settings">Parameters for the query</param>
        public virtual IDataQuery GetExecutingQuery(DataQuerySettings settings = null)
        {
            return this;
        }



        /// <summary>
        /// Applies main query properties to the given query to ensure synchronized state before execution
        /// </summary>
        /// <param name="query">Query to prepare</param>
        /// <param name="multiQuery">If true, the query is an inner query within multi-query</param>
        protected virtual void ApplyProperties(IDataQuery query, bool multiQuery = false)
        {
            // Apply data source
            if (DataSource != null)
            {
                query.DataSource = DataSource;
            }

            // Propagate query name
            if (!String.IsNullOrEmpty(QueryName))
            {
                query.QueryName = QueryName;
            }

            // Apply the connection string to the target query in case it is forced
            if (ConnectionStringForced)
            {
                query.ConnectionStringName = ConnectionStringName;
            }

            query.IsSubQuery = IsSubQuery;

            // Apply top N, paging and order by to make inner queries in multi-query more effective
            if (AllowTopNDistribution() && (ApplyTopN(TopNRecords, query) || ApplyPagingTopN(this, query)))
            {
                ApplyOrderBy(this, query);
            }
        }


        /// <summary>
        /// Returns true if distribution of TOP N to inner queries is allowed. Defaults to false.
        /// Distribution of TOP N to inner queries helps performance mainly in case the inner query is complex and includes nesting levels, for simple queries it is better not to allow the distribution.
        /// </summary>
        protected virtual bool AllowTopNDistribution()
        {
            // Top N distribution is not allowed by default because it may harm performance of simple queries
            return false;
        }


        /// <summary>
        /// Applies TOP N settings to the given query if it is more restrictive than the current query settings.
        /// </summary>
        /// <param name="topN">Top N to apply</param>
        /// <param name="query">Target query</param>
        /// <returns>Returns true if TOP N was applied to the inner query</returns>
        protected bool ApplyTopN(int topN, IDataQuery query)
        {
            // Apply the TOP N (which may be more restrictive) to the sub-query
            if ((topN > 0) && ((query.TopNRecords <= 0) || (query.TopNRecords > topN)))
            {
                query.TopNRecords = topN;
                return true;
            }

            return false;
        }


        /// <summary>
        /// Applies top N based on paging settings to the inner query to select only necessary data
        /// </summary>
        /// <param name="settings">Query settings</param>
        /// <param name="query">Target query</param>
        protected bool ApplyPagingTopN(IDataQuerySettings settings, IDataQuery query)
        {
            if (settings.MaxRecords > 0)
            {
                // Get top N so that it covers all records up to the last requested item
                var topN = settings.Offset + settings.MaxRecords;

                ApplyTopN(topN, query);

                if (!query.IsSubQuery)
                {
                    var cols = query.SelectColumnsList;

                    // Add column to count sub-total for the total number of records
                    cols.AddUnique(new CountColumn { Over = "" }.As(SystemColumns.SUB_TOTAL_RECORDS));

                    query.TotalExpression = SystemColumns.SUB_TOTAL_RECORDS;
                }

                return true;
            }

            return false;
        }


        /// <summary>
        /// Applies order by from the given settings to the inner query
        /// </summary>
        /// <param name="settings">Source settings</param>
        /// <param name="query">Target query</param>
        protected static void ApplyOrderBy(IDataQuerySettings settings, IDataQuery query)
        {
            query.OrderByColumns = SqlHelper.AddOrderBy(settings.OrderByColumns, query.OrderByColumns);
        }


        /// <summary>
        /// Creates a nested query from the given query
        /// </summary>
        /// <param name="settings">Parameters for the query</param>
        /// <param name="nestedSettings">Nested settings</param>
        private TResult AsNestedInternal<TResult>(DataQuerySettings settings = null, NestedQuerySettings nestedSettings = null)
            where TResult : IDataQuery<TResult>, new()
        {
            TResult q;

            lock (lockObject)
            {
                if (HasDataSource)
                {
                    // Use memory data source pointing to the current query result when nested
                    var src = new MemoryDataQuerySource(() => Result);

                    q = new TResult().WithSource(src);

                    return q;
                }

                settings = settings ?? GetCompleteSettings();
                nestedSettings = nestedSettings ?? new NestedQuerySettings();

                // Remove order by (not valid in the inner query
                string orderBy = settings.OrderByColumns;

                var innerColumns = settings.SelectColumnsList;

                if (settings.SelectDistinct)
                {
                    var orderByColumns = new OrderByColumnList(orderBy);
                    orderByColumns.FilterOrderByColumns(innerColumns);
                    orderBy = orderByColumns.ToString();
                }
                else
                {
                    // Only ensure order by columns if the current columns are not locked
                    if (nestedSettings.EnsureOrderByColumns)
                    {
                        innerColumns.EnsureOrderByColumns(ref orderBy);
                    }
                }

                int offset = settings.Offset;
                int maxRecords = settings.MaxRecords;
                string totalExpression = settings.TotalExpression;

                // Remove order by unless top N is specified. In that case order by must stay to properly select top items
                if (settings.TopNRecords <= 0)
                {
                    settings.OrderByColumns = null;
                }

                // Remove paging parameters from the inner query
                if (nestedSettings.MovePagingToOuterQuery)
                {
                    settings.Offset = 0;
                    settings.MaxRecords = 0;
                    settings.TotalExpression = null;
                }

                settings.IsNested = true;

                var innerQuery = GenerateQueryText(settings);

                // Create new general query to wrap result
                q = new TResult();
                q.CustomQueryText = SqlHelper.GENERAL_SELECT;

                // Define the nested query as new query source
                innerQuery = q.IncludeDataParameters(settings.Parameters, innerQuery);

                var innerSource = new QuerySourceTable("(" + Environment.NewLine + innerQuery + Environment.NewLine + ")", "SubData");

                q.DefaultQuerySource = new QuerySource(innerSource);

                if ((settings.Parameters != null) && nestedSettings.UseSameDataSet)
                {
                    q.EnsureParameters().FillDataSet = settings.Parameters.FillDataSet;
                }

                // Setup paging parameters to the outer query
                if (nestedSettings.MovePagingToOuterQuery)
                {
                    if (maxRecords > 0)
                    {
                        q.Offset = offset;
                        q.MaxRecords = maxRecords;
                    }

                    if (!String.IsNullOrEmpty(totalExpression))
                    {
                        q.TotalExpression = totalExpression;
                    }
                }

                q.OrderByColumns = orderBy;

                q.IsSubQuery = IsSubQuery;
                q.ConnectionStringName = ConnectionStringName;

                if (ReturnsSingleColumn)
                {
                    q.ReturnsSingleColumn = true;
                }

                // Copy select columns to outer query to narrow down any filter columns
                if (settings.FilterColumns.AnyColumnsDefined)
                {
                    q.SelectColumnsList = SelectColumnsList.AsAliases();
                }
            }

            return q;
        }


        /// <summary>
        /// Makes a materialized list from the given query
        /// </summary>
        /// <param name="columnName">Column name to output</param>
        /// <param name="distinct">If true, only distinct values are selected</param>
        public IDataQuery AsMaterializedList(string columnName = null, bool distinct = false)
        {
            // Get the list of IDs
            var q = AsSingleColumn(columnName, (columnName != null));

            if (distinct)
            {
                q.SelectDistinct = true;
            }

            var ids = q.GetListResult<int>();

            return DataQuery.FromList(ids);
        }


        /// <summary>
        /// Changes the type of the query to another type
        /// </summary>
        public TNewType As<TNewType>()
            where TNewType : IDataQuery, new()
        {
            var q = new TNewType();

            CopyPropertiesTo(q);

            return q;
        }


        /// <summary>
        /// Transforms the current result
        /// </summary>
        /// <param name="func">Select function</param>
        public IEnumerable<T> Select<T>(Func<DataRow, T> func)
        {
            var ds = Result;

            if (ds != null)
            {
                // Process all tables
                foreach (DataTable dt in ds.Tables)
                {
                    // Process all rows
                    foreach (DataRow dr in dt.Rows)
                    {
                        yield return func(dr);
                    }
                }
            }
        }


        /// <summary>
        /// Gets the scalar
        /// </summary>
        /// <param name="defaultValue">Default value if result not found or not capable to convert to output type</param>
        public T GetScalarResult<T>(T defaultValue = default(T))
        {
            var ds = Result;

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                var dt = ds.Tables[0];
                if ((dt != null) && (dt.Columns.Count > 0))
                {
                    var dr = dt.Rows[0];
                    if (dr != null)
                    {
                        return ValidationHelper.GetValue(dr[0], defaultValue);
                    }
                }
            }

            return defaultValue;
        }


        /// <summary>
        /// Gets the result as a list of values from the first column that the query returns. Excludes null values from the result.
        /// </summary>
        public IList<T> GetListResult<T>()
        {
            var result = new List<T>();

            var ds = Result;
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                var dt = ds.Tables[0];

                // Check proper column type
                var type = typeof(T);
                if (dt.Columns[0].DataType != type)
                {
                    throw new NotSupportedException("[DataQueryBase.GetListResult]: The result column type must be '" + type.FullName + "' in order to get a typed list of results.");
                }

                // Process all rows
                foreach (DataRow dr in dt.Rows)
                {
                    // Process the item
                    var objVal = DataHelper.GetNull(dr[0]);
                    if (objVal != null)
                    {
                        var value = (T)objVal;

                        result.Add(value);
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Executes the given action for each page (DataSet) in the results. If the query is not set up as a paged query before calling this 
        /// method and <paramref name="pageSize"/> is not set, executes the action once with the current results.
        /// </summary>
        /// <param name="pageAction">Action which will be executed for each page</param>
        /// <param name="pageSize">Page size. 0 means no processing page-by-page. By default uses current paging settings. Page size of the query is set to the original value after this method finishes.</param>
        public void ForEachPage(Action<TQuery> pageAction, int pageSize = -1)
        {
            if (pageAction == null)
            {
                return;
            }

            lock (lockObject)
            {
                var originalOffset = Offset;
                var originalMaxRecords = MaxRecords;

                try
                {
                    // Set up batch size
                    if (pageSize >= 0)
                    {
                        MaxRecords = pageSize;
                    }

                    var q = (TQuery)this;

                    // For non-paged, execute on complete result
                    if (!IsPagedQuery)
                    {
                        pageAction(q);
                        return;
                    }

                    Offset = 0;

                    do
                    {
                        // Get the data
                        pageAction(q);

                        // Check if the next page is available
                        if (NextPageAvailable)
                        {
                            q = q.NextPage();
                        }
                        else
                        {
                            // End iteration
                            break;
                        }
                    } while (true);
                }
                catch (ActionCancelledException)
                {
                    // Enumeration was cancelled
                }
                finally
                {
                    // Revert to original state
                    Offset = originalOffset;
                    MaxRecords = originalMaxRecords;
                }
            }
        }


        /// <summary>
        /// Executes the given action for each item (DataRow) in the result. Processes the items in batches of the given size.
        /// </summary>
        /// <param name="rowAction">Row action</param>
        /// <param name="batchSize">Batch size. 0 means no batch processing. By default uses current paging settings.</param>
        public void ForEachRow(Action<DataRow> rowAction, int batchSize = -1)
        {
            if (rowAction == null)
            {
                return;
            }

            try
            {
                // Process all pages
                ForEachPage(q => DataHelper.ForEachRow(q, rowAction), batchSize);
            }
            catch (ActionCancelledException)
            {
                // Enumeration was cancelled
            }
        }


        /// <summary>
        /// Gets a source for this query
        /// </summary>
        public QuerySource GetSource()
        {
            return QuerySource ?? DefaultQuerySource ?? GetDefaultSource();
        }


        /// <summary>
        /// Applies the given settings to the query
        /// </summary>
        /// <param name="parameters">Parameters to apply</param>
        public IDataQuery ApplySettings(AbstractQueryObject parameters)
        {
            parameters?.ApplyParametersTo(this);

            return this;
        }


        /// <summary>
        /// Applies the given settings to the query
        /// </summary>
        /// <param name="parameters">Parameters to apply</param>
        public IDataQuery ApplySettings(Action<DataQuerySettings> parameters)
        {
            if (parameters != null)
            {
                // Create and apply new settings
                var s = new DataQuerySettings();

                parameters(s);

                ApplySettings(s);
            }

            return this;
        }


        /// <summary>
        /// Gets the class structure info for this query
        /// </summary>
        protected virtual ClassStructureInfo GetClassStructureInfo()
        {
            return ClassStructureInfo.GetClassInfo(ClassName);
        }


        /// <summary>
        /// Creates a new empty query
        /// </summary>
        protected virtual TQuery NewEmptyQuery()
        {
            return new TQuery();
        }

        #endregion


        #region "Execution methods"

        /// <summary>
        /// Executes the current query and returns it's results as a DataSet
        /// </summary>
        public DataSet Execute()
        {
            // Execute the query
            mResult = GetData();

            mDataLoaded = true;

            return mResult;
        }


        /// <summary>
        /// Executes the current query and returns it's results as a data reader
        /// </summary>
        /// <param name="commandBehavior">Command behavior</param>
        /// <param name="newConnection">If true, the reader should be executed using its own dedicated connection</param>
        public DbDataReader ExecuteReader(CommandBehavior commandBehavior = CommandBehavior.Default, bool newConnection = false)
        {
            return GetReaderFromDB(commandBehavior, newConnection);
        }


        /// <summary>
        /// Executes the query. Sets the total records number during execution.
        /// </summary>
        protected virtual DataSet GetData()
        {
            // Check if some results are to be returned
            if (ReturnsNoResults)
            {
                TotalRecords = 0;

                return null;
            }

            DataSet result;

            lock (lockObject)
            {
                // Use data source if specified
                result = HasDataSource ? GetDataFromDataSource() : GetDataFromDB();
            }

            return result;
        }


        /// <summary>
        /// Executes the current over data source and returns it's results as a DataSet
        /// </summary>
        protected virtual DataSet GetDataFromDataSource()
        {
            var p = PrepareDataQuerySourceParameters();

            DataSet result = DataSource.GetData(p);

            // Update the total number of records
            TotalRecords = p.TotalRecords;

            return result.Copy();
        }


        /// <summary>
        /// Prepares the data source parameters
        /// </summary>
        private DataQuerySourceParameters PrepareDataQuerySourceParameters()
        {
            var settings = GetCompleteSettings();
            
            // Execute with data source
            var p = new DataQuerySourceParameters(this, settings, Offset, MaxRecords);

            return p;
        }


        /// <summary>
        /// Executes the query
        /// </summary>
        protected virtual DataSet GetDataFromDB()
        {
            var q = GetExecutingQuery();
            if (q == null)
            {
                // No executing query = no data
                return null;
            }

            if (q == this)
            {
                // Get the data from this query
                return GetDataFromDBInternal();
            }

            // Get data from the executing query
            int totalRecords = 0;
            var results = GetResults(q, ref totalRecords);

            // Store total records
            TotalRecords = totalRecords;

            return results;
        }


        /// <summary>
        /// Executes the current query and returns it's results as a DataSet
        /// </summary>
        private DataSet GetDataFromDBInternal()
        {
            int totalRecords = 0;
            DataSet result;

            lock (lockObject)
            {
                // Execute the query
                var conn = GetConnection();

                QueryParameters query = GetCompleteQueryParameters();

                // Execute query
                result = conn.ExecuteQuery(query, ref totalRecords);

                // Update the total number of records
                TotalRecords = totalRecords;
            }

            return result;
        }


        /// <summary>
        /// Gets results from executing query
        /// </summary>
        /// <param name="query">Executing query</param>
        /// <param name="totalRecords">Returns the total records number</param>
        protected virtual DataSet GetResults(IDataQuery query, ref int totalRecords)
        {
            // Run the query
            var results = query.Result;

            // Store total records
            totalRecords = query.TotalRecords;

            return results;
        }


        /// <summary>
        /// Executes the query
        /// </summary>
        /// <param name="commandBehavior">Command behavior</param>
        /// <param name="newConnection">If true, the reader should be executed using its own dedicated connection</param>
        protected DbDataReader GetReaderFromDB(CommandBehavior commandBehavior, bool newConnection)
        {
            var q = GetExecutingQuery();
            if (q == null)
            {
                // No executing query = no reader
                return null;
            }

            if (q == this)
            {
                // Get the reader from this query
                return GetReaderFromDBInternal(commandBehavior, newConnection);
            }

            // Get the reader from executing query
            return q.ExecuteReader(commandBehavior, newConnection);
        }


        /// <summary>
        /// Executes the query
        /// </summary>
        /// <param name="commandBehavior">Command behavior</param>
        /// <param name="newConnection">If true, the reader should be executed using its own dedicated connection</param>
        private DbDataReader GetReaderFromDBInternal(CommandBehavior commandBehavior, bool newConnection)
        {
            DbDataReader result;

            lock (lockObject)
            {
                var settings = GetCompleteSettings();

                // Execute the query
                var conn = GetConnection();
                var query = GetQueryParameters(settings);

                query.UseNewConnection = newConnection;

                // Execute the reader
                result = conn.ExecuteReader(query, commandBehavior);
            }

            return result;
        }


        /// <summary>
        /// Gets the executing query parameters
        /// </summary>
        /// <param name="settings">Query settings</param>
        private QueryParameters GetQueryParameters(DataQuerySettings settings)
        {
            QueryParameters query;

            settings = settings ?? GetCompleteSettings();

            var parameters = settings.Parameters;
            var expressions = settings.GetExpressions(parameters);

            if (!String.IsNullOrEmpty(CustomQueryText))
            {
                // Execute custom query text
                var queryText = GetBaseQueryText();

                query = new QueryParameters(queryText, parameters, QueryTypeEnum.SQLQuery)
                {
                    Macros = expressions
                };

                // Populate query name in case of general select
                var name = (CustomQueryText == SqlHelper.GENERAL_SELECT) ? QueryType.GENERALSELECT : "custom";

                query.Name = ClassName + "." + name;
            }
            else
            {
                // Get query
                var qi = QueryInfoProvider.GetQueryInfo(FullQueryName, true);

                // Prepare the query
                query = new QueryParameters(qi, parameters, expressions);
            }

            // Add before / after content unless the query is sub query or nested query
            if ((parameters != null) && !settings.IsSubQuery && !settings.IsNested)
            {
                query.Text = parameters.AddBeforeAfter(query.Text);
            }

            query.IsSubQuery = settings.IsSubQuery;
            query.IsNested = settings.IsNested;

            query.MaxRecords = settings.MaxRecords;
            query.Offset = settings.Offset;

            // Set the query connection string name if explicitly set
            if (ConnectionStringForced)
            {
                query.ConnectionStringName = ConnectionStringName;
            }

            return query;
        }


        /// <summary>
        /// Gets the executing connection for the query
        /// </summary>
        private GeneralConnection GetConnection()
        {
            return ConnectionHelper.GetConnection();
        }


        /// <summary>
        /// Returns query parameter container filled with the complete settings of current query.
        /// </summary>
        /// <remarks>
        /// Wraps distinct paged query as nested so the row number column required for paging doesn't thwart the distinct selection.
        /// </remarks>
        /// <param name="settings">Query settings to use</param>
        public QueryParameters GetCompleteQueryParameters(DataQuerySettings settings = null)
        {
            var q = GetExecutingQuery();
            if (q == null)
            {
                // No executing query = no query parameters
                return null;
            }

            if (q == this)
            {
                // Get the query parameters from this query
                return GetCompleteQueryParametersInternal(settings);
            }

            // Get the parameters from executing query
            return q.GetCompleteQueryParameters(settings);
        }


        /// <summary>
        /// Returns query parameter container filled with the complete settings of current query.
        /// </summary>
        /// <remarks>
        /// Wraps distinct paged query as nested so the row number column required for paging doesn't thwart the distinct selection.
        /// </remarks>
        /// <param name="settings">Query settings to use</param>
        private QueryParameters GetCompleteQueryParametersInternal(DataQuerySettings settings = null)
        {
            var q = this;

            // Ensure settings
            settings = settings ?? q.GetCompleteSettings();

            if (settings.IsPagedQuery)
            {
                if (settings.SelectDistinct)
                {
                    // For distinct paged query wrap as nested query so that distinct is evaluated correctly
                    q = q.AsNestedInternal<TQuery>(settings);

                    // When query is wrapped, incoming settings cannot be used, because they are meant for the original query
                    settings = q.GetCompleteSettings();
                }
                else
                {
                    // Paged query uses nested WITH -> we need to replace possible order column alias for real column name
                    TranslateOrderByColumn(settings);
                }
            }

            var query = q.GetQueryParameters(settings);

            return query;
        }


        /// <summary>
        /// Translates order column alias for real column name in given DataQuerySettings
        /// Use only for paged queries
        /// </summary>
        private void TranslateOrderByColumn(IDataQuerySettings settings)
        {
            if (!IsPagedQuery)
            {
                throw new InvalidOperationException("Alias name could be translated into original column name only when query is paged one");
            }

            var orderColumns = new OrderByColumnList(settings.OrderByColumns);
            orderColumns.TranslateAliases(SelectColumnsList);
            settings.OrderByColumns = orderColumns.Columns;
        }


        /// <summary>
        /// Generates the query text
        /// </summary>
        /// <param name="settings">Query parameters</param>
        private string GenerateQueryText(DataQuerySettings settings)
        {
            string queryText;

            lock (lockObject)
            {
                // Prepare complete parameters
                if (settings == null)
                {
                    settings = GetCompleteSettings();
                }

                // Get base query text
                queryText = GetBaseQueryText();

                var parameters = settings.Parameters;
                var expressions = settings.GetExpressions(parameters);

                var isInnerQuery = settings.IsSubQuery || settings.IsNested;

                if (settings.IsPagedQuery)
                {
                    // Check if OrderBy is set
                    if (String.IsNullOrEmpty(expressions.OrderBy))
                    {
                        throw new NotSupportedException("[DataQuery.GenerateQueryText]: The paged query cannot be generated without the orderBy set.");
                    }

                    // Prepare the query
                    queryText = SqlHelper.PreparePagedQuery(queryText, expressions, settings.Offset, settings.MaxRecords, !settings.IsSubQuery, isInnerQuery);
                }

                // Prepare query and resolve macros
                if (parameters != null)
                {
                    queryText = isInnerQuery ? parameters.ResolveMacros(queryText) : parameters.GetCompleteQueryText(queryText);
                }

                queryText = expressions.ResolveMacros(queryText);
            }

            return queryText;
        }


        /// <summary>
        /// Gets the base query texts for this query
        /// </summary>
        private string GetBaseQueryText()
        {
            string queryText;

            if (HasDataSource)
            {
                // Fake query for external data source
                return SqlHelper.GENERAL_SELECT;
            }

            if (!String.IsNullOrEmpty(CustomQueryText))
            {
                // Custom query text
                queryText = CustomQueryText;
            }
            else
            {
                // Specific query
                var query = FullQueryName;

                // Get query
                var qi = QueryInfoProvider.GetQueryInfo(query, false);
                if (qi == null)
                {
                    throw new Exception("[DataQuery.GetBaseQueryText]: Query '" + query + "' not found.");
                }

                var type = qi.QueryType;

                SqlHelper.CheckQueryTypeForTextGeneration(type, query);

                queryText = qi.QueryText;
            }

            // Wrap combined query into another select if some parameters are set
            if (IsCombinedQuery && AnySettingsDefined())
            {
                queryText = SqlHelper.GetNestedQuery(queryText, "Data");
            }

            return queryText;
        }


        /// <summary>
        /// Gets data source identifier that represents the location from which the data are obtained. 
        /// </summary>
        /// <remarks>
        /// Source identifiers are used to determine whether sub queries should be materialized or inserted directly into parent query. 
        /// </remarks>
        protected override string GetDataSourceName()
        {
            // Get the name from the attached data source or connection string name
            if (HasDataSource)
            {
                return DataSource.DataSourceName;
            }

            // Specific connection string
            var connStringName = ConnectionStringName;
            if (ConnectionHelper.GetConnectionString(connStringName, true) != null)
            {
                return DataQuerySource.DATABASE_PREFIX + connStringName;
            }

            // Default source
            return base.GetDataSourceName();
        }


        /// <summary>
        /// Returns true if the given query is an external source
        /// </summary>
        /// <param name="query">Nested query</param>
        public override bool HasCompatibleSource(IDataQuery query)
        {
            var querySource = query.DataSourceName;

            // Materialized query is compatible with any other query
            if (querySource == DataQuerySource.MATERIALIZED)
            {
                return true;
            }

            return (querySource == DataSourceName);
        }

        #endregion


        #region "Setup methods"

        /// <summary>
        /// Applies the given parameters to the query
        /// </summary>
        /// <param name="parameters">Parameters to use</param>
        public TQuery WithSettings(Action<DataQuerySettings> parameters)
        {
            var result = GetTypedQuery();

            result.ApplySettings(parameters);

            return result;
        }


        /// <summary>
        /// Applies the given parameters to the query
        /// </summary>
        /// <param name="parameters">Parameters to use</param>
        public TQuery WithSettings(AbstractQueryObject parameters)
        {
            var result = GetTypedQuery();

            result.ApplySettings(parameters);

            return result;
        }


        /// <summary>
        /// Sets whether the binary data should be included to the result
        /// </summary>
        /// <param name="binary">Include binary data?</param>
        public TQuery BinaryData(bool binary)
        {
            var result = GetTypedQuery();
            result.IncludeBinaryData = binary;

            return result;
        }


        /// <summary>
        /// Sets the given DataSet as the source of the data query
        /// </summary>
        /// <param name="data">Source data</param>
        public TQuery WithSource(DataSet data)
        {
            return WithSource(new MemoryDataQuerySource(data));
        }


        /// <summary>
        /// Sets the given source as the source of the data query
        /// </summary>
        /// <param name="source">Data query source</param>
        public virtual TQuery WithSource(DataQuerySource source)
        {
            var result = GetTypedQuery();
            result.DataSource = source;

            return result;
        }


        /// <summary>
        /// Defines the source of the data (table, view or a nested query)
        /// </summary>
        /// <param name="source">Source</param>
        public TQuery From(QuerySource source)
        {
            var result = GetTypedQuery();

            result.QuerySource = source;

            return result;
        }


        /// <summary>
        /// Defines the source of the data (table, view or a nested query)
        /// </summary>
        /// <param name="sourceParameters">Source parameters</param>
        public TQuery Source(Action<QuerySource> sourceParameters)
        {
            var result = GetTypedQuery();

            // Create source instance from current source
            var source = GetSource();

            sourceParameters?.Invoke(source);

            result.QuerySource = source;

            // Include parameters from source condition
            result.IncludeDataParameters(source.Parameters, source);

            return result;
        }


        /// <summary>
        /// Sets the default order by for the query
        /// </summary>
        public TQuery OrderByDefault()
        {
            return OrderBy(DefaultOrderByColumns);
        }


        /// <summary>
        /// Expands the columns within this query
        /// </summary>
        public virtual TQuery ExpandColumns()
        {
            var result = GetTypedQuery();

            ResolveColumns(result.SelectColumnsList, IncludeBinaryData, true);

            return result;
        }


        /// <summary>
        /// Adds condition to all items that contain given text in any of the string columns. Performs a SQL substring search on the data.
        /// </summary>
        /// <param name="text">Text to search</param>
        public TQuery WhereAnyColumnContains(string text)
        {
            return WhereAnyColumn(QueryOperator.Like, GetContainsPattern(text));
        }


        /// <summary>
        /// Matches the given condition on any column with the same type as the given value type.
        /// </summary>
        /// <param name="op">Operator</param>
        /// <param name="value">Value</param>
        public TQuery WhereAnyColumn(QueryOperator op, object value)
        {
            var where = new WhereCondition();

            // Get class structure info
            var csi = GetClassStructureInfo();
            if (csi == null)
            {
                where.NoResults();
            }
            else
            {
                // Get the columns of the correct type, for NULL, get all columns
                var cols = (value == null) ? csi.ColumnNames : csi.GetColumns(value.GetType());
                if (cols.Count == 0)
                {
                    where.NoResults();
                }
                else
                {
                    // Add search condition
                    var searchWhere = new WhereCondition();

                    foreach (var colName in cols)
                    {
                        where.Or().Where(colName, op, value);
                    }

                    where.And(searchWhere);
                }
            }

            return Where(where);
        }


        /// <summary>
        /// Gets the union of this data query with another query
        /// </summary>
        /// <param name="query">Query to union with</param>
        /// <param name="unionAll">If true, the union does not eliminate the duplicities (produces UNION ALL)</param>
        public TQuery Union(TQuery query, bool unionAll = false)
        {
            var op = unionAll ? SqlOperator.UNION_ALL : SqlOperator.UNION;

            return CombineWith(query, op);
        }


        /// <summary>
        /// Combines the query with the given query
        /// </summary>
        /// <param name="query">Query to combine with</param>
        /// <param name="op">Operator</param>
        private TQuery CombineWith(TQuery query, string op)
        {
            var result = NewEmptyQuery();

            DataQuery.SetupCombinedQuery(result, new[] { this, query }, new[] { op }, ConnectionStringName);

            return result;
        }


        /// <summary>
        /// Gets the union of this data query with another query without eliminating the duplicities
        /// </summary>
        /// <param name="query">Query to union with</param>
        public TQuery UnionAll(TQuery query)
        {
            return Union(query, true);
        }


        /// <summary>
        /// Gets the intersection of this data query with another query
        /// </summary>
        /// <param name="query">Query to intersect with</param>
        public TQuery Intersect(TQuery query)
        {
            return CombineWith(query, SqlOperator.INTERSECT);
        }


        /// <summary>
        /// Gets the intersection of this data query with another query
        /// </summary>
        /// <param name="query">Query to intersect with</param>
        public TQuery Except(TQuery query)
        {
            return CombineWith(query, SqlOperator.EXCEPT);
        }

        #endregion


        #region "Operators"

        /// <summary>
        /// Implicit operator for conversion from DataQuery class to DataSet
        /// </summary>
        /// <param name="query">Query object</param>
        public static implicit operator DataSet(DataQueryBase<TQuery> query)
        {
            return query?.Result;
        }

        #endregion
    }
}
