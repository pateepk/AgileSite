using System;
using System.Collections.Generic;

namespace CMS.DataEngine
{
    /// <summary>
    /// Multi query interface for a specific query
    /// </summary>
    public interface IMultiQuery<TQuery, TInnerQuery> :
        IDataQuery<TQuery>,
        IMultiQuery
    {
        /// <summary>
        /// Includes given type with optional parameters
        /// </summary>
        /// <param name="type">Type to include</param>
        /// <param name="parameters">Action to setup the inner type parameters</param>
        TQuery Type(string type, Action<TInnerQuery> parameters = null);
        
        /// <summary>
        /// Includes the given types to the resulting query
        /// </summary>
        /// <param name="types">Types to include</param>
        TQuery Types(params string[] types);
        
        /// <summary>
        /// Sets the column to select
        /// </summary>
        /// <param name="column">Column to set to be selected</param>
        TQuery ResultColumn(string column);
        
        /// <summary>
        /// Sets the column to select
        /// </summary>
        /// <param name="column">Column to set to be selected</param>
        TQuery ResultColumn(IQueryColumn column);
        
        /// <summary>
        /// Sets the columns to select
        /// </summary>
        /// <param name="columns">Columns to set to be selected</param>
        TQuery ResultColumns(IEnumerable<string> columns);
        
        /// <summary>
        /// Sets the columns to select
        /// </summary>
        /// <param name="columns">Columns to set to be selected</param>
        TQuery ResultColumns(params string[] columns);
        
        /// <summary>
        /// Sets the columns to select
        /// </summary>
        /// <param name="columns">Columns to set to be selected</param>
        TQuery ResultColumns(params IQueryColumn[] columns);
        
        /// <summary>
        /// Adds the additional column to the query
        /// </summary>
        /// <param name="column">Column to add</param>
        TQuery AddResultColumn(IQueryColumn column);
        
        /// <summary>
        /// Adds the additional columns to the query
        /// </summary>
        /// <param name="columns">Columns to add</param>
        TQuery AddResultColumns(params IQueryColumn[] columns);
        
        /// <summary>
        /// Adds the additional column to the query
        /// </summary>
        /// <param name="column">Column to add</param>
        TQuery AddResultColumn(string column);
        
        /// <summary>
        /// Adds the additional columns to the query
        /// </summary>
        /// <param name="columns">Columns to add</param>
        TQuery AddResultColumns(IEnumerable<string> columns);
        
        /// <summary>
        /// Adds the additional columns to the query
        /// </summary>
        /// <param name="columns">Columns to add</param>
        TQuery AddResultColumns(params string[] columns);


        /// <summary>
        /// Adds the columns to the order by query
        /// </summary>
        /// <param name="columns">Columns to add to order by</param>
        TQuery ResultOrderBy(params string[] columns);
        
        /// <summary>
        /// Adds the columns to the order by query to order by the given columns in descending order
        /// </summary>
        /// <param name="columns">Columns to add to order by</param>
        TQuery ResultOrderByDescending(params string[] columns);
        
        /// <summary>
        /// Adds the columns to the order by query to order by the given columns in ascending order
        /// </summary>
        /// <param name="columns">Columns to add to order by</param>
        TQuery ResultOrderByAscending(params string[] columns);
        
        /// <summary>
        /// Adds the columns to the order by query
        /// </summary>
        /// <param name="dir">Order direction</param>
        /// <param name="columns">Columns to add to order by</param>
        TQuery ResultOrderBy(OrderDirection dir, params string[] columns);
        

        /// <summary>
        /// Sets the query to return no columns at all
        /// </summary>
        TQuery NoDefaultColumns();
    }


    /// <summary>
    /// Multi query interface
    /// </summary>
    public interface IMultiQuery : IDataQuery
    {
        /// <summary>
        /// List of columns to use for results, by default returns all columns defined in the inner queries. Example: "DocumentName, DocumentID"
        /// </summary>
        QueryColumnList SelectResultColumnsList
        {
            get;
            set;
        }


        /// <summary>
        /// If true (default), the query uses type columns for the output, otherwise it uses only global columns
        /// </summary>
        bool UseTypeColumns
        {
            get;
            set;
        }


        /// <summary>
        /// List of columns for the result order by. If not specified, the result is ordered by sources and global order by.
        /// </summary>
        string OrderByResultColumns
        {
            get;
            set;
        }
    }
}