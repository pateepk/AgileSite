using System;
using System.Collections.Generic;

namespace CMS.DataEngine
{
    /// <summary>
    /// Data query parameters interface for a specific query
    /// </summary>
    public interface IDataQuerySettings<TQuery> : 
        IWhereCondition<TQuery>, 
        IDataQuerySettings
    {
        /// <summary>
        /// Identity method to make the query expression more readable. Use before the OrderBy method. Doesn't provide any functionality.
        /// </summary>
        TQuery Then();
        
        /// <summary>
        /// Identity method to make the query expression more readable. Use before the Columns or Page method. Doesn't provide any functionality.
        /// </summary>
        TQuery Take();

        /// <summary>
        /// Sets the query to use distinct selection over the given columns
        /// </summary>
        /// <param name="distinct">If set to true, returns only distinct (different) values.</param>
        TQuery Distinct(bool distinct = true);
        
        /// <summary>
        /// Selects only first top N number of records
        /// </summary>
        TQuery TopN(int topN);
        
        /// <summary>
        /// Sets the column to select
        /// </summary>
        /// <param name="column">Column to set to be selected</param>
        TQuery Column(string column);

        /// <summary>
        /// Sets the column to select
        /// </summary>
        /// <param name="column">Column to set to be selected</param>
        TQuery Column(IQueryColumn column);

        /// <summary>
        /// Sets the columns to select
        /// </summary>
        /// <param name="columns">Columns to set to be selected</param>
        TQuery Columns(IEnumerable<string> columns);

        /// <summary>
        /// Sets the columns to select
        /// </summary>
        /// <param name="columns">Columns to set to be selected</param>
        TQuery Columns(params string[] columns);

        /// <summary>
        /// Sets the columns to select
        /// </summary>
        /// <param name="columns">Columns to set to be selected</param>
        TQuery Columns(params IQueryColumn[] columns);

        /// <summary>
        /// Adds the additional column to the query
        /// </summary>
        /// <param name="column">Column to add</param>
        TQuery AddColumn(IQueryColumn column);
        
        /// <summary>
        /// Adds the additional columns to the query
        /// </summary>
        /// <param name="columns">Columns to add</param>
        TQuery AddColumns(params IQueryColumn[] columns);
        
        /// <summary>
        /// Adds the additional column to the query
        /// </summary>
        /// <param name="column">Column to add</param>
        TQuery AddColumn(string column);
        
        /// <summary>
        /// Adds the additional columns to the query
        /// </summary>
        /// <param name="columns">Columns to add</param>
        TQuery AddColumns(IEnumerable<string> columns);
        
        /// <summary>
        /// Adds the additional columns to the query
        /// </summary>
        /// <param name="columns">Columns to add</param>
        TQuery AddColumns(params string[] columns);

        /// <summary>
        /// Replaces the selected column with a new name
        /// </summary>
        /// <param name="originalName">Original column name</param>
        /// <param name="newName">New column name</param>
        TQuery ReplaceColumn(string originalName, string newName);
        
        /// <summary>
        /// Specifies the page to select with given page index and page size. Page number is indexed from 0 (first page)
        /// </summary>
        TQuery Page(int pageIndex, int pageSize);

        /// <summary>
        /// Sets up the query as a paged query with the given page size. Resets the page index to first page. Use in combination with NextPageAvailable and NextPage to iterate over the data in batches.
        /// </summary>
        /// <remarks>
        /// Note that if you iterate the results after calling this method, the results will cover only a single page. To iterate through all items page-by-page, use methods ForEachRow or ForEachObject.
        /// </remarks>
        TQuery PagedBy(int pageSize);

        /// <summary>
        /// Sets up the query to become not paged query and output all results at once.
        /// </summary>
        TQuery NotPaged();

        /// <summary>
        /// Adjusts the query to a next page, using the current page size. Use in combination with PagedBy and NextPageAvailable to iterate over the data in batches.
        /// </summary>
        /// <remarks>
        /// Number of records in one page can be specified by setting MaxRecords property or calling PagedBy() method.
        /// </remarks>
        TQuery NextPage();


        /// <summary>
        /// Adds the columns to the order by query
        /// </summary>
        /// <param name="columns">Columns to add to order by</param>
        TQuery OrderBy(params string[] columns);
        
        /// <summary>
        /// Adds the columns to the order by query to order by the given columns in descending order
        /// </summary>
        /// <param name="columns">Columns to add to order by</param>
        TQuery OrderByDescending(params string[] columns);
        
        /// <summary>
        /// Adds the columns to the order by query to order by the given columns in ascending order
        /// </summary>
        /// <param name="columns">Columns to add to order by</param>
        TQuery OrderByAscending(params string[] columns);
        
        /// <summary>
        /// Adds the columns to the order by query
        /// </summary>
        /// <param name="dir">Order direction</param>
        /// <param name="columns">Columns to add to order by</param>
        TQuery OrderBy(OrderDirection dir, params string[] columns);

        
        /// <summary>
        /// Clears the current group by, reverting the source of data to the original. Note, that this method also resets the existing having condition which is closely bound to the group by.
        /// </summary>
        TQuery NewGroupBy(params string[] columns);
        
        /// <summary>
        /// Specifies the columns to group by
        /// </summary>
        /// <param name="columns">List of columns to group by</param>
        TQuery GroupBy(params string[] columns);

        /// <summary>
        /// Clears the current having condition
        /// </summary>
        TQuery NewHaving();
        
        /// <summary>
        /// Specifies the group by having condition
        /// </summary>
        /// <param name="having">Having condition</param>
        /// <param name="replace">If true, the having condition replaces the original having condition</param>
        TQuery Having(string having, bool replace = false);
        
        /// <summary>
        /// Adds the given having conditions to the query.
        /// </summary>
        /// <param name="conditions">Having where conditions</param>
        TQuery Having(params IWhereCondition[] conditions);
        
        /// <summary>
        /// Adds the given where condition to the query. Creates a new where condition object and runs the setup actions on it.
        /// </summary>
        /// <param name="condition">Nested where condition</param>
        TQuery Having(Action<WhereCondition> condition);
        
        /// <summary>
        /// Adds the comment to the given query
        /// </summary>
        /// <param name="comment">Comment to add</param>
        TQuery WithComment(string comment);
    }


    /// <summary>
    /// Data query parameters interface
    /// </summary>
    public interface IDataQuerySettings : IWhereCondition
    {
        #region "Properties"

        /// <summary>
        /// List of columns by which the result should be sorted, e.g. "NodeLevel, DocumentName DESC"
        /// </summary>
        string OrderByColumns
        {
            get;
            set;
        }


        /// <summary>
        /// List of columns to group by, by default doesn't group, e.g. "NodeLevel, NodeOwner"
        /// </summary>
        string GroupByColumns
        {
            get;
            set;
        }


        /// <summary>
        /// List of columns to return, by default returns all columns, e.g. "DocumentName, DocumentID"
        /// </summary>
        QueryColumnList SelectColumnsList
        {
            get;
            set;
        }


        /// <summary>
        /// If set, selects only first top N number of records
        /// </summary>
        int TopNRecords
        {
            get;
            set;
        }


        /// <summary>
        /// Index of the first record to return (use for paging together with MaxRecords)
        /// </summary>
        int Offset
        {
            get;
            set;
        }


        /// <summary>
        /// Maximum number of results to return (use for paging together with Offset)
        /// </summary>
        int MaxRecords
        {
            get;
            set;
        }


        /// <summary>
        /// Total items expression. When defined, used instead default total items for a paged query.
        /// </summary>
        string TotalExpression
        {
            get;
            set;
        }


        /// <summary>
        /// Returns true if the query has the paging enabled
        /// </summary>
        bool IsPagedQuery
        {
            get;
        }


        /// <summary>
        /// Source of the query
        /// </summary>
        QuerySource QuerySource
        {
            get;
            set;
        }


        /// <summary>
        /// Default source of the query in case source is not defined
        /// </summary>
        QuerySource DefaultQuerySource
        {
            get;
            set;
        }


        /// <summary>
        /// If set to true, returns only distinct (different) values.
        /// </summary>
        bool SelectDistinct
        {
            get;
            set;
        }


        /// <summary>
        /// Where condition for the group by on the data, e.g. "DocumentName = 'ABC'"
        /// </summary>
        string HavingCondition
        {
            get;
            set;
        }


        /// <summary>
        /// List of columns used for extra filtering within the query, e.g. "CMS_C, CMS_RN"
        /// </summary>
        QueryColumnList FilterColumns
        {
            get;
            set;
        }
        

        /// <summary>
        /// If true, the query is a sub-query used in another query.
        /// This brings certain constraints such as that it cannot use order by or CTE.
        /// </summary>
        bool IsSubQuery
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates that this query is nested within another query as its source.
        /// This brings certain constraints such as that is cannot use CTE.
        /// </summary>
        bool IsNested
        {
            get;
            set;
        }


        /// <summary>
        /// Returns true if the given query has group by set
        /// </summary>
        bool HasGroupBy
        {
            get;
        }

        #endregion
    }
}
