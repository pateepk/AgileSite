using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace CMS.DataEngine
{
    /// <summary>
    /// Interface for the data query for a specific query type
    /// </summary>
    public interface IDataQuery<TQuery> : 
        IDataQuerySettings<TQuery>, 
        IDataQuery
    {
        /// <summary>
        /// Applies the given parameters to the query
        /// </summary>
        /// <param name="parameters">Parameters to use</param>
        TQuery WithSettings(Action<DataQuerySettings> parameters);
        
        /// <summary>
        /// Applies the given parameters to the query
        /// </summary>
        /// <param name="parameters">Parameters to use</param>
        TQuery WithSettings(AbstractQueryObject parameters);
        
        /// <summary>
        /// Sets whether the binary data should be included to the result
        /// </summary>
        /// <param name="binary">Include binary data?</param>
        TQuery BinaryData(bool binary);
        
        /// <summary>
        /// Sets the given DataSet as the source of the data query
        /// </summary>
        /// <param name="data">Source data</param>
        TQuery WithSource(DataSet data);
        
        /// <summary>
        /// Sets the given source as the source of the data query
        /// </summary>
        /// <param name="source">Data query source</param>
        TQuery WithSource(DataQuerySource source);
        
        /// <summary>
        /// Defines the source of the data (table, view or a nested query)
        /// </summary>
        /// <param name="source">Source</param>
        TQuery From(QuerySource source);
        
        /// <summary>
        /// Defines the source of the data (table, view or a nested query)
        /// </summary>
        /// <param name="sourceParameters">Source parameters</param>
        TQuery Source(Action<QuerySource> sourceParameters);
        
        /// <summary>
        /// Sets the default order by for the query
        /// </summary>
        TQuery OrderByDefault();
        
        /// <summary>
        /// Expands the columns within this query
        /// </summary>
        TQuery ExpandColumns();
        
        /// <summary>
        /// Adds condition to all items that contain given text in any of the string columns. Performs a SQL substring search on the data.
        /// </summary>
        /// <param name="text">Text to search</param>
        TQuery WhereAnyColumnContains(string text);
        
        /// <summary>
        /// Matches the given condition on any column with the same type as the given value type.
        /// </summary>
        /// <param name="op">Operator</param>
        /// <param name="value">Value</param>
        TQuery WhereAnyColumn(QueryOperator op, object value);

        /// <summary>
        /// Gets the union of this data query with another query
        /// </summary>
        /// <param name="query">Query to union with</param>
        /// <param name="unionAll">If true, the union does not eliminate the duplicities (produces UNION ALL)</param>
        TQuery Union(TQuery query, bool unionAll = false);

        /// <summary>
        /// Gets the union of this data query with another query without eliminating the duplicities
        /// </summary>
        /// <param name="query">Query to union with</param>
        TQuery UnionAll(TQuery query);

        /// <summary>
        /// Gets the intersection of this data query with another query
        /// </summary>
        /// <param name="query">Query to intersect with</param>
        TQuery Intersect(TQuery query);

        /// <summary>
        /// Gets the intersection of this data query with another query
        /// </summary>
        /// <param name="query">Query to intersect with</param>
        TQuery Except(TQuery query);
    }


    /// <summary>
    /// Data query interface
    /// </summary>
    public interface IDataQuery : IDataQuerySettings, IQueryObjectWithValue
    {
        #region "Properties"

        /// <summary>
        /// Query name
        /// </summary>
        string QueryName
        {
            get;
            set;
        }

        /// <summary>
        /// Class name
        /// </summary>
        string ClassName
        {
            get;
            set;
        }
        
        /// <summary>
        /// Custom query text
        /// </summary>
        string CustomQueryText
        {
            get;
            set;
        }
        
        /// <summary>
        /// If true, the query includes the object binary data
        /// </summary>
        bool IncludeBinaryData
        {
            get;
            set;
        }

        /// <summary>
        /// Query text
        /// </summary>
        string QueryText
        {
            get;
        }
        
        /// <summary>
        /// DataSet with the result
        /// </summary>
        DataSet Result
        {
            get;
        }

        /// <summary>
        /// Number of actual records retrieved from the database
        /// </summary>
        int Count
        {
            get;
        }

        /// <summary>
        /// Gets the number of total records when paging is used. Gets updated once the query executes
        /// </summary>
        int TotalRecords
        {
            get;
        }
        
        /// <summary>
        /// Default order by columns used in case if needed, and order by is not specified
        /// </summary>
        string DefaultOrderByColumns
        {
            get;
            set;
        }
        
        /// <summary>
        /// Collection of the result tables
        /// </summary>
        DataTableCollection Tables
        {
            get;
        }

        /// <summary>
        /// Returns true if the query supports data reader
        /// </summary>
        bool SupportsReader
        {
            get;
        }


        /// <summary>
        /// Data source that provides the query data. If not set, the query queries the database directly
        /// </summary>
        DataQuerySource DataSource
        {
            get;
            set;
        }


        /// <summary>
        /// Returns true if the query has specific data source
        /// </summary>
        bool HasDataSource
        {
            get;
        }


        /// <summary>
        /// If true, this query is combined from several queries. When additional parameters are applied to it, it will be wrapped into a nested query.
        /// </summary>
        bool IsCombinedQuery
        {
            get;
            set;
        }


        /// <summary>
        /// Returns true if the query returns single column
        /// </summary>
        bool ReturnsSingleColumn
        {
            get;
            set;
        }


        /// <summary>
        /// Query connection string name
        /// </summary>
        string ConnectionStringName
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the query allows materialization
        /// </summary>
        bool AllowMaterialization
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the full query text including resolved parameters
        /// </summary>
        /// <param name="expand">If true, the parameters are expanded</param>
        /// <param name="includeParameters">If true, parameter declarations are included if parameters are not expanded</param>
        /// <param name="settings">Query settings</param>
        string GetFullQueryText(bool expand = false, bool includeParameters = true, DataQuerySettings settings = null);
        
        /// <summary>
        /// Gets a source for this query
        /// </summary>
        QuerySource GetSource();
        
        /// <summary>
        /// Returns query parameter container filled with the complete settings of current query.
        /// </summary>
        /// <remarks>
        /// Wraps distinct paged query as nested so the row number column required for paging doesn't thwart the distinct selection.
        /// </remarks>
        /// <param name="settings">Parameters for the query</param>
        QueryParameters GetCompleteQueryParameters(DataQuerySettings settings = null);

        /// <summary>
        /// Gets the complete parameters for the query execution
        /// </summary>
        /// <param name="executingQuery">Executing query for which the parameters are retrieved</param>
        DataQuerySettings GetCompleteSettings(IDataQuery executingQuery = null);
        
        /// <summary>
        /// Applies the given settings to the query
        /// </summary>
        /// <param name="parameters">Parameters to apply</param>
        IDataQuery ApplySettings(Action<DataQuerySettings> parameters);
        
        /// <summary>
        /// Applies the given settings to the query
        /// </summary>
        /// <param name="parameters">Parameters to apply</param>
        IDataQuery ApplySettings(AbstractQueryObject parameters);
        

        /// <summary>
        /// Executes the current query and returns it's results as a DataSet
        /// </summary>
        DataSet Execute();

        /// <summary>
        /// Executes the current query and returns it's results as a data reader
        /// </summary>
        /// <param name="commandBehavior">Command behavior</param>
        /// <param name="newConnection">If true, the reader should be executed using its own dedicated connection</param>
        DbDataReader ExecuteReader(CommandBehavior commandBehavior = CommandBehavior.Default, bool newConnection = false);


        /// <summary>
        /// Gets the scalar
        /// </summary>
        /// <param name="defaultValue">Default value if result not found or not capable to convert to output type</param>
        T GetScalarResult<T>(T defaultValue = default(T));

        /// <summary>
        /// Gets the result as a list of values from the first column that the query returns. Excludes null values from the result.
        /// </summary>
        IList<T> GetListResult<T>();


        /// <summary>
        /// Transforms the current result
        /// </summary>
        /// <param name="func">Select function</param>
        IEnumerable<T> Select<T>(Func<DataRow, T> func);

        /// <summary>
        /// Executes the given action for each item (DataRow) in the result. Processes the items in batches of the given size.
        /// </summary>
        /// <param name="rowAction">Row action</param>
        /// <param name="batchSize">Batch size. 0 means no batch processing. By default uses current paging settings.</param>
        void ForEachRow(Action<DataRow> rowAction, int batchSize = -1);
        
        /// <summary>
        /// Creates a nested query from the given query
        /// </summary>
        /// <param name="settings">Settings</param>
        TResult AsNested<TResult>(NestedQuerySettings settings = null)
            where TResult : IDataQuery<TResult>, new();
        
        /// <summary>
        /// Creates an ID query from the given query
        /// </summary>
        IDataQuery AsIDQuery();

        /// <summary>
        /// Changes the type of the query to another type
        /// </summary>
        T As<T>()
            where T : IDataQuery, new();

        /// <summary>
        /// Creates a single column query from the given query
        /// </summary>
        /// <param name="defaultColumn">Specific column to use in case query doesn't return single column yet</param>
        /// <param name="forceColumn">If true, the given column is forced to the output</param>
        IDataQuery AsSingleColumn(string defaultColumn = null, bool forceColumn = false);
        
        /// <summary>
        /// Modifies the query to be able to be used as a sub-query, e.g. for usage in WHERE A IN ([query]). Ensures single column result, and removes order by from the result.
        /// </summary>
        IDataQuery AsSubQuery();

        /// <summary>
        /// Makes a materialized list from the given query
        /// </summary>
        /// <param name="columnName">Column name to output</param>
        /// <param name="distinct">If true, only distinct IDs are selected</param>
        IDataQuery AsMaterializedList(string columnName = null, bool distinct = false);

        /// <summary>
        /// Returns true if the given query is an external source
        /// </summary>
        /// <param name="query">Nested query</param>
        bool HasCompatibleSource(IDataQuery query);

        /// <summary>
        /// Gets the query to execute against database
        /// </summary>
        /// <param name="settings">Parameters for the query</param>
        IDataQuery GetExecutingQuery(DataQuerySettings settings = null);

        #endregion
    }
}
