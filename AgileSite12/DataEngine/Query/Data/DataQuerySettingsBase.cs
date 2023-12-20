using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

using CMS.Base;
using CMS.DataEngine.Query;
using CMS.Helpers;

namespace CMS.DataEngine
{
    /// <summary>
    /// Defines parameters for the data selection
    /// </summary>
    public abstract class DataQuerySettingsBase<TQuery> : WhereConditionBase<TQuery>, IDataQuerySettings<TQuery>
        where TQuery : DataQuerySettingsBase<TQuery>, new()
    {
        #region "Variables"

        /// <summary>
        /// Order by columns
        /// </summary>
        protected string mOrderByColumns;

        /// <summary>
        /// Columns to select
        /// </summary>
        protected QueryColumnList mSelectColumnsList;

        /// <summary>
        /// Columns to provide extra filtering (which are not included into the output)
        /// </summary>
        private QueryColumnList mFilterColumns;

        /// <summary>
        /// Columns to group by
        /// </summary>
        private string mGroupByColumns;

        /// <summary>
        /// Having condition
        /// </summary>
        private string mHavingCondition;

        /// <summary>
        /// TopN
        /// </summary>
        private int mTopNRecords;

        /// <summary>
        /// Offset
        /// </summary>
        protected int mOffset;

        /// <summary>
        /// Maximum number of records to select
        /// </summary>
        protected int mMaxRecords;

        /// <summary>
        /// Source of the query
        /// </summary>
        private QuerySource mQuerySource;

        /// <summary>
        /// Default source of the query in case source is not defined
        /// </summary>
        private QuerySource mDefaultQuerySource;

        /// <summary>
        /// If set to true, returns only distinct (different) values.
        /// </summary>
        private bool mSelectDistinct;

        #endregion


        #region "Properties"

        /// <summary>
        /// List of columns by which the result should be sorted, e.g. "NodeLevel, DocumentName DESC"
        /// </summary>
        public string OrderByColumns
        {
            get
            {
                return mOrderByColumns;
            }
            set
            {
                mOrderByColumns = value;
                Changed();
            }
        }


        /// <summary>
        /// List of columns used for extra filtering within the query, e.g. "CMS_C, CMS_RN"
        /// </summary>
        public QueryColumnList FilterColumns
        {
            get
            {
                return mFilterColumns ?? (mFilterColumns = CreateFilterColumns());
            }
            set
            {
                // Clone the columns in case they already have a parent
                if (value != null)
                {
                    if (value.Parent != null)
                    {
                        value = value.Clone(this);
                    }
                    else
                    {
                        value.Parent = this;
                    }
                }

                mFilterColumns = value;
                Changed();
            }
        }


        /// <summary>
        /// List of columns to return, by default returns all columns, e.g. "DocumentName, DocumentID"
        /// </summary>
        public QueryColumnList SelectColumnsList
        {
            get
            {
                return mSelectColumnsList ?? (mSelectColumnsList = new QueryColumnList(this));
            }
            set
            {
                // Release parent link from previously attached list
                if (mSelectColumnsList != null)
                {
                    mSelectColumnsList.Parent = null;
                }

                // Clone the columns in case they already have a parent
                if (value != null)
                {
                    if (value.Parent != null)
                    {
                        value = value.Clone(this);
                    }
                    else
                    {
                        value.Parent = this;
                    }
                }

                mSelectColumnsList = value;

                Changed();
            }
        }


        /// <summary>
        /// List of columns to group by, by default doesn't group, e.g. "NodeLevel, NodeOwner"
        /// </summary>
        public string GroupByColumns
        {
            get
            {
                return mGroupByColumns;
            }
            set
            {
                mGroupByColumns = value;
                Changed();
            }
        }


        /// <summary>
        /// Where condition for the group by on the data, e.g. "DocumentName = 'ABC'"
        /// </summary>
        public string HavingCondition
        {
            get
            {
                return mHavingCondition;
            }
            set
            {
                mHavingCondition = value;

                if (String.IsNullOrEmpty(value))
                {
                    HavingIsComplex = false;
                }

                Changed();
            }
        }


        /// <summary>
        /// If set, selects only first top N number of records
        /// </summary>
        public int TopNRecords
        {
            get
            {
                return mTopNRecords;
            }
            set
            {
                mTopNRecords = value;
                Changed();
            }
        }


        /// <summary>
        /// Query source object
        /// </summary>
        public QuerySource QuerySource
        {
            get
            {
                return mQuerySource;
            }
            set
            {
                mQuerySource = value;

                Changed();
            }
        }


        /// <summary>
        /// If set to true, returns only distinct (different) values.
        /// </summary>
        public bool SelectDistinct
        {
            get
            {
                return mSelectDistinct;
            }
            set
            {
                mSelectDistinct = value;
                Changed();
            }
        }


        /// <summary>
        /// Default source of the query in case source is not defined
        /// </summary>
        public QuerySource DefaultQuerySource
        {
            get
            {
                return mDefaultQuerySource;
            }
            set
            {
                mDefaultQuerySource = value;
                Changed();
            }
        }


        /// <summary>
        /// Index of the first record to return (use for paging together with MaxRecords)
        /// </summary>
        public int Offset
        {
            get
            {
                return mOffset;
            }
            set
            {
                mOffset = value;
                Changed();
            }
        }


        /// <summary>
        /// Maximum number of results to return (use for paging together with Offset)
        /// </summary>
        public int MaxRecords
        {
            get
            {
                return mMaxRecords;
            }
            set
            {
                mMaxRecords = value;
                Changed();
            }
        }


        /// <summary>
        /// Total items expression. When defined, used instead default total items for a paged query.
        /// </summary>
        public string TotalExpression
        {
            get;
            set;
        }


        /// <summary>
        /// Returns true if the query has the paging enabled
        /// </summary>
        public bool IsPagedQuery
        {
            get
            {
                return MaxRecords > 0;
            }
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


        /// <summary>
        /// If true, the order by should be forced in the process of execution
        /// </summary>
        protected bool ForceOrderBy
        {
            get;
            set;
        }


        /// <summary>
        /// Returns true if the given having condition is a complex condition
        /// </summary>
        public bool HavingIsComplex
        {
            get;
            set;
        }


        /// <summary>
        /// Returns true if the having condition is empty
        /// </summary>
        public bool HavingIsEmpty
        {
            get
            {
                return String.IsNullOrEmpty(HavingCondition);
            }
        }


        /// <summary>
        /// Returns true if the given query has group by set
        /// </summary>
        public bool HasGroupBy
        {
            get
            {
                return !String.IsNullOrEmpty(GroupByColumns);
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Creates the list for filter columns
        /// </summary>
        private QueryColumnList CreateFilterColumns()
        {
            var cols = new QueryColumnList(this);

            // No columns by default
            cols.Load(SqlHelper.NO_COLUMNS);

            return cols;
        }


        /// <summary>
        /// Gets the default source for this query
        /// </summary>
        protected virtual QuerySource GetDefaultSource()
        {
            return null;
        }


        /// <summary>
        /// Applies this query parameters to the target object
        /// </summary>
        /// <param name="target">Target object defining parameters</param>
        public override void ApplyParametersTo(IQueryObject target)
        {
            var t = target as IDataQuerySettings;
            if (t != null)
            {
                // Paging parameters
                if (MaxRecords > 0)
                {
                    t.MaxRecords = MaxRecords;
                    t.Offset = Offset;
                }

                // Top N records
                if (TopNRecords > 0)
                {
                    t.TopNRecords = TopNRecords;
                }

                // Total expression
                if (!String.IsNullOrEmpty(TotalExpression))
                {
                    t.TotalExpression = TotalExpression;
                }

                // Order by
                if (!String.IsNullOrEmpty(OrderByColumns))
                {
                    t.OrderByColumns = OrderByColumns;
                }

                // Select columns, do not override if set from outside to empty columns as we want to keep already present extra columns in such case.
                if (SelectColumnsList.AnyColumnsDefined)
                {
                    t.SelectColumnsList = SelectColumnsList;
                }
                
                // Filter columns
                if (!FilterColumns.NoColumns)
                {
                    t.FilterColumns = FilterColumns;
                }

                // Source
                if (mQuerySource != null)
                {
                    t.QuerySource = mQuerySource;
                }
                if (mDefaultQuerySource != null)
                {
                    t.DefaultQuerySource = mDefaultQuerySource;
                }

                // Distinct
                if (SelectDistinct)
                {
                    t.SelectDistinct = SelectDistinct;
                }

                // Group by columns and having
                if (!String.IsNullOrEmpty(GroupByColumns))
                {
                    t.GroupByColumns = GroupByColumns;
                    t.HavingCondition = HavingCondition;
                }
            }

            base.ApplyParametersTo(target);
        }


        /// <summary>
        /// Creates the clone of the collection.
        /// </summary>
        /// <param name="target">Target class</param>
        public override void CopyPropertiesTo(IQueryObject target)
        {
            var t = target as IDataQuerySettings;
            if (t != null)
            {
                t.MaxRecords = MaxRecords;
                t.Offset = Offset;
                t.TotalExpression = TotalExpression;

                t.OrderByColumns = OrderByColumns;
                t.SelectColumnsList = SelectColumnsList;
                t.FilterColumns = FilterColumns;
                t.TopNRecords = TopNRecords;
                t.QuerySource = QuerySource;
                t.DefaultQuerySource = DefaultQuerySource;
                t.SelectDistinct = SelectDistinct;
                t.GroupByColumns = GroupByColumns;
                t.HavingCondition = HavingCondition;

                t.IsNested = IsNested;
                t.IsSubQuery = IsSubQuery;
            }

            base.CopyPropertiesTo(target);
        }


        /// <summary>
        /// Gets the query expressions
        /// </summary>
        /// <param name="parameters">Query data parameters</param>
        public QueryMacros GetExpressions(QueryDataParameters parameters)
        {
            var cols = SelectColumnsList;

            // Add filter columns
            if (!FilterColumns.NoColumns)
            {
                cols.AddRangeUnique(FilterColumns);
            }

            // Get columns and include parameters
            var columns = SelectColumnsList.GetColumns(parameters);

            // Get query source and include parameters
            var querySource = QuerySource;
            var source = (querySource != null) ? querySource.GetSourceExpression(parameters) : null;

            // Get default query source and include parameters
            var defaultQuerySource = DefaultQuerySource;
            var defaultSource = (defaultQuerySource != null) ? defaultQuerySource.GetSourceExpression(parameters) : null;

            return new QueryMacros
            {
                Where = WhereCondition,
                OrderBy = OrderByColumns,
                TopN = TopNRecords,
                Columns = columns,
                Source = source,
                DefaultSource = defaultSource,
                Distinct = SelectDistinct,
                GroupBy = GroupByColumns,
                Having = HavingCondition,
                Total = TotalExpression
            };
        }


        /// <summary>
        /// Returns true if the object has any settings defined that influence the resulting query
        /// </summary>
        protected override bool AnySettingsDefined()
        {
            return
                base.AnySettingsDefined() ||
                !String.IsNullOrEmpty(OrderByColumns) ||
                (TopNRecords > 0) ||
                SelectColumnsList.AnyColumnsDefined ||
                !FilterColumns.NoColumns ||
                !String.IsNullOrEmpty(GroupByColumns) ||
                !String.IsNullOrEmpty(HavingCondition) ||
                SelectDistinct;
        }


        /// <summary>
        /// Replaces the selected column with a new name
        /// </summary>
        /// <param name="originalName">Original column name</param>
        /// <param name="newName">New column name</param>
        protected void ReplaceSelectedColumn(string originalName, string newName)
        {
            if (originalName != newName)
            {
                SelectColumnsList.ReplaceColumn(originalName, newName.AsColumn());
            }
        }


        /// <summary>
        /// Replaces the selected column with a new name
        /// </summary>
        /// <param name="originalName">Original column name</param>
        /// <param name="newName">New column name</param>
        protected void ReplaceOrderByColumn(string originalName, string newName)
        {
            if (originalName != newName)
            {
                OrderByColumns = ReplaceColumnName(OrderByColumns, originalName, newName);
            }
        }


        /// <summary>
        /// Replaces the column name in the given list of columns
        /// </summary>
        /// <param name="columns">List of columns</param>
        /// <param name="originalName">Original column name</param>
        /// <param name="newName">New column name</param>
        private string ReplaceColumnName(string columns, string originalName, string newName)
        {
            if (String.IsNullOrEmpty(columns))
            {
                return columns;
            }

            // Change the parameter name in the where condition
            var re = RegexHelper.GetRegex("\\b" + originalName + "\\b", RegexHelper.DefaultOptions | RegexOptions.IgnoreCase);

            return re.Replace(columns, newName);
        }


        /// <summary>
        /// Returns the string representation of the expression, with possibility of expanding parameters
        /// </summary>
        /// <param name="expand">If true, the result is expanded with parameters so it can act as standalone value.</param>
        public override string ToString(bool expand)
        {
            var result = GetExpressions(null).ToString();

            if (expand)
            {
                result = Expand(result);
            }

            return result;
        }


        /// <summary>
        /// Adds the given having condition to the final having condition
        /// </summary>
        /// <param name="having">Having condition</param>
        private void AddHavingInternal(string having)
        {
            if (!String.IsNullOrEmpty(having))
            {
                var wasEmpty = HavingIsEmpty;

                HavingCondition = WhereBuilder.AddWhereCondition(HavingCondition, having, WhereOperator, false);
                ResetWhereOperator();

                if (!wasEmpty)
                {
                    HavingIsComplex = true;
                }
            }
        }


        /// <summary>
        /// Adds the given having condition
        /// </summary>
        /// <param name="condition">Condition to add</param>
        protected void AddHavingInternal(IWhereCondition condition)
        {
            if (condition == null)
            {
                return;
            }

            Using(condition);

            string having = condition.WhereCondition;
            QueryDataParameters parameters = condition.Parameters;

            // Include the data parameters to this query
            having = IncludeDataParameters(parameters, having);
            if (condition.WhereIsComplex)
            {
                having = WhereBuilder.GetNestedWhereCondition(having);
            }

            // Add the final where condition
            AddHavingInternal(having);
        }

        #endregion


        #region "Setup methods"

        /// <summary>
        /// Identity method to make the query expression more readable. Use before the OrderBy method. Doesn't provide any functionality.
        /// </summary>
        public TQuery Then()
        {
            return GetTypedQuery();
        }


        /// <summary>
        /// Identity method to make the query expression more readable. Use before the Columns or Page method. Doesn't provide any functionality.
        /// </summary>
        public TQuery Take()
        {
            return GetTypedQuery();
        }


        /// <summary>
        /// Sets the query to use distinct selection over the given columns
        /// </summary>
        /// <param name="distinct">If set to true, returns only distinct (different) values.</param>
        public TQuery Distinct(bool distinct = true)
        {
            var result = GetTypedQuery();

            result.SelectDistinct = distinct;

            return result;
        }


        /// <summary>
        /// Selects only first top N number of records
        /// </summary>
        public TQuery TopN(int topN)
        {
            var result = GetTypedQuery();
            result.TopNRecords = topN;

            return result;
        }


        /// <summary>
        /// Sets the column to select
        /// </summary>
        /// <param name="column">Column to set to be selected</param>
        public TQuery Column(string column)
        {
            return Columns(column);
        }


        /// <summary>
        /// Sets the column to select
        /// </summary>
        /// <param name="column">Column to set to be selected</param>
        public TQuery Column(IQueryColumn column)
        {
            return Columns(column);
        }


        /// <summary>
        /// Sets the columns to select
        /// </summary>
        /// <param name="columns">Columns to set to be selected</param>
        public TQuery Columns(IEnumerable<string> columns)
        {
            var result = GetTypedQuery();

            result.SelectColumnsList.Load(columns.ToArray());

            return result;
        }


        /// <summary>
        /// Sets the columns to select
        /// </summary>
        /// <param name="columns">Columns to set to be selected</param>
        public TQuery Columns(params string[] columns)
        {
            if (columns.Length >= 1)
            {
                // Handle distinct at the first list of columns (backward compatibility)
                var cols = columns[0];
                if (cols != null)
                {
                    cols = cols.TrimStart();
                    if (cols.StartsWithCSafe("DISTINCT ", true))
                    {
                        columns[0] = cols.Substring(9);
                        return Distinct().Columns(columns);
                    }
                }

                // Process single string input as list of columns separated by commas e.g "ColA, ColB, ColC"
                if (columns.Length == 1)
                {
                    var result = GetTypedQuery();
                    result.SelectColumnsList.Load(columns[0]);
                    return result;
                }
            }

            return Columns((IEnumerable<string>)columns);
        }


        /// <summary>
        /// Sets the columns to select
        /// </summary>
        /// <param name="columns">Columns to set to be selected</param>
        public TQuery Columns(params IQueryColumn[] columns)
        {
            var result = GetTypedQuery();

            // Start with no columns
            result.SelectColumnsList.Load(columns);

            return result;
        }


        /// <summary>
        /// Adds the additional column to the query
        /// </summary>
        /// <param name="column">Column to add</param>
        public TQuery AddColumn(IQueryColumn column)
        {
            return AddColumns(column);
        }


        /// <summary>
        /// Adds the additional columns to the query
        /// </summary>
        /// <param name="columns">Columns to add</param>
        public TQuery AddColumns(params IQueryColumn[] columns)
        {
            var result = GetTypedQuery();

            // Ensure all columns are included
            var list = result.SelectColumnsList;

            list.AddRangeUnique(columns);

            return result;
        }


        /// <summary>
        /// Adds the additional column to the query
        /// </summary>
        /// <param name="column">Column to add</param>
        public TQuery AddColumn(string column)
        {
            return AddColumns(column);
        }


        /// <summary>
        /// Adds the additional columns to the query
        /// </summary>
        /// <param name="columns">Columns to add</param>
        public TQuery AddColumns(IEnumerable<string> columns)
        {
            var result = GetTypedQuery();

            var list = result.SelectColumnsList;

            list.AddRangeUnique(columns.ToArray());

            return result;
        }


        /// <summary>
        /// Adds the additional columns to the query
        /// </summary>
        /// <param name="columns">Columns to add</param>
        public TQuery AddColumns(params string[] columns)
        {
            return AddColumns((IEnumerable<string>)columns);
        }


        /// <summary>
        /// Adds the additional filter column to the query
        /// </summary>
        /// <param name="col">Column to add</param>
        public TQuery AddFilterColumn(IQueryColumn col)
        {
            var result = GetTypedQuery();

            result.FilterColumns.AddUnique(col);

            return result;
        }


        /// <summary>
        /// Sets the query to return no columns at all
        /// </summary>
        protected TQuery NoColumns()
        {
            return Columns(SqlHelper.NO_COLUMNS);
        }


        /// <summary>
        /// Replaces the selected column with a new name
        /// </summary>
        /// <param name="originalName">Original column name</param>
        /// <param name="newName">New column name</param>
        public TQuery ReplaceColumn(string originalName, string newName)
        {
            var result = GetTypedQuery();

            result.ReplaceSelectedColumn(originalName, newName);
            result.ReplaceOrderByColumn(originalName, newName);

            return result;
        }


        /// <summary>
        /// Specifies the page to select with given page index and page size. Page number is indexed from 0 (first page)
        /// </summary>
        public TQuery Page(int pageIndex, int pageSize)
        {
            var result = GetTypedQuery();

            result.Offset = pageIndex * pageSize;
            result.MaxRecords = pageSize;

            return result;
        }


        /// <summary>
        /// Sets up the query as a paged query with the given page size. Resets the page index to first page. Use in combination with NextPageAvailable and NextPage to iterate over the data in batches.
        /// </summary>
        /// <remarks>
        /// Note that if you iterate the results after calling this method, the results will cover only a single page. To iterate through all items page-by-page, use methods ForEachRow or ForEachObject.
        /// </remarks>
        public TQuery PagedBy(int pageSize)
        {
            var result = GetTypedQuery();

            result.Offset = 0;
            result.MaxRecords = pageSize;

            return result;
        }


        /// <summary>
        /// Sets up the query to become not paged query and output all results at once.
        /// </summary>
        public TQuery NotPaged()
        {
            var result = GetTypedQuery();

            result.Offset = 0;
            result.MaxRecords = 0;

            return result;
        }


        /// <summary>
        /// Adjusts the query to a next page, using the current page size. Use in combination with PagedBy and NextPageAvailable to iterate over the data in batches.
        /// </summary>
        /// <remarks>
        /// Number of records in one page can be specified by setting MaxRecords property or calling PagedBy() method.
        /// </remarks>
        public TQuery NextPage()
        {
            if (MaxRecords <= 0)
            {
                throw new InvalidOperationException("[DataQuerySettingsBase.NextPage]: This query is not set up to be a paged query. You need to call method PagedBy(...) or Page(...) first, to specify the page size.");
            }

            var result = GetTypedQuery();

            result.Offset += result.MaxRecords;

            return result;
        }


        /// <summary>
        /// Adds the columns to the order by query
        /// </summary>
        /// <param name="columns">Columns to add to order by</param>
        public TQuery OrderBy(params string[] columns)
        {
            return OrderBy(OrderDirection.Default, columns);
        }


        /// <summary>
        /// Adds the columns to the order by query to order by the given columns in descending order
        /// </summary>
        /// <param name="columns">Columns to add to order by</param>
        public TQuery OrderByDescending(params string[] columns)
        {
            return OrderBy(OrderDirection.Descending, columns);
        }


        /// <summary>
        /// Adds the columns to the order by query to order by the given columns in ascending order
        /// </summary>
        /// <param name="columns">Columns to add to order by</param>
        public TQuery OrderByAscending(params string[] columns)
        {
            return OrderBy(OrderDirection.Ascending, columns);
        }


        /// <summary>
        /// Adds the columns to the order by query
        /// </summary>
        /// <param name="dir">Order direction</param>
        /// <param name="columns">Columns to add to order by</param>
        public TQuery OrderBy(OrderDirection dir, params string[] columns)
        {
            var result = GetTypedQuery();

            string orderBy = result.OrderByColumns;

            // Add all column lists
            foreach (var list in columns)
            {
                orderBy = SqlHelper.AddOrderBy(orderBy, list, dir);
            }

            result.OrderByColumns = orderBy;

            return result;
        }


        /// <summary>
        /// Clears the current group by, reverting the source of data to the original. Note, that this method also resets the existing having condition which is closely bound to the group by.
        /// </summary>
        public TQuery NewGroupBy(params string[] columns)
        {
            var result = GetTypedQuery();

            result.GroupByColumns = null;
            result.HavingCondition = null;

            return result;
        }


        /// <summary>
        /// Specifies the columns to group by
        /// </summary>
        /// <param name="columns">List of columns to group by</param>
        public TQuery GroupBy(params string[] columns)
        {
            // If the query already has group by, do not allow
            if (HasGroupBy)
            {
                throw new NotSupportedException("[DataQuerySettingsBase.GroupBy]: The query is already specified with group by '" + GroupByColumns + "', in order to apply additional group by to the current result, you need to call .AsNested().GroupBy(...). Alternatively, you can alter the current group by used on the source data by calling .NewGroupBy().GroupBy(...)");
            }

            var result = GetTypedQuery();

            result.GroupByColumns = SqlHelper.JoinColumnList(columns);

            return result;
        }


        /// <summary>
        /// Clears the current having condition
        /// </summary>
        public TQuery NewHaving()
        {
            var result = GetTypedQuery();
            result.HavingCondition = null;

            return result;
        }


        /// <summary>
        /// Specifies the having condition
        /// </summary>
        /// <param name="having">Having condition</param>
        /// <param name="replace">If true, the having condition replaces the original having condition</param>
        public TQuery Having(string having, bool replace = false)
        {
            var result = replace ? NewHaving() : GetTypedQuery();

            result.AddHavingInternal(having);

            return result;
        }


        /// <summary>
        /// Adds the given having conditions to the query.
        /// </summary>
        /// <param name="conditions">Having where conditions</param>
        public TQuery Having(params IWhereCondition[] conditions)
        {
            var result = GetTypedQuery();

            // Process all conditions
            foreach (var condition in conditions)
            {
                result.AddHavingInternal(condition);
            }

            return result;
        }


        /// <summary>
        /// Adds the given where condition to the query. Creates a new where condition object and runs the setup actions on it.
        /// </summary>
        /// <param name="condition">Nested where condition</param>
        public TQuery Having(Action<WhereCondition> condition)
        {
            return Having(DataEngine.WhereCondition.From(condition));
        }


        /// <summary>
        /// Adds the comment to the given query
        /// </summary>
        /// <param name="comment">Comment to add</param>
        public TQuery WithComment(string comment)
        {
            var result = GetTypedQuery();

            result.EnsureParameters().QueryAfter += SqlHelper.GetComment(comment);

            return result;
        }

        #endregion
    }
}
