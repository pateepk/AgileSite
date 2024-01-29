using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Represents a list of query columns
    /// </summary>
    public class QueryColumnList : QueryColumnListBase
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parent">Parent query parameters</param>
        public QueryColumnList(IQueryObject parent = null)
            : base(parent)
        {
        }


        /// <summary>
        /// Creates an empty column list built from the given columns
        /// </summary>
        /// <param name="columns">List of columns</param>
        public QueryColumnList(string columns)
        {
            Load(columns);
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Adds unique <see cref="IQueryColumn"/>s with aliases, even though all columns selector was present in the list.
        /// Columns without alias won't be added. 
        /// </summary>
        /// <remarks>
        /// Should be used when * is present in the list and user wants to include SQL expression as a column.
        /// Column with expression must have an alias or won't be added. If the alias is the same as column name
        /// contained in *, then DB exception (Ambiguous column name) may occur, e.g. Select *, X AS DocumentName From CMS_Documents Order By DocumentName.
        /// </remarks>
        /// <example>
        /// { A } is not added.
        /// { A AS B } is added.
        /// { SUBSTR(A, 0, 5) } is not added.
        /// { SUBSTR(A, 0, 5) AS X } is added.
        /// { A + A } is not added.
        /// { A + A AS X } is added.
        /// </example>
        /// <param name="queryColumnsWithAliases">Query columns</param>
        internal void AddRangeUniqueColumnsWithAliases(IEnumerable<IQueryColumn> queryColumnsWithAliases)
        {
            queryColumnsWithAliases = queryColumnsWithAliases
                .Where(c => !c.Name.EqualsCSafe(c.Expression, StringComparison.InvariantCultureIgnoreCase));

            AddObjectColumns(queryColumnsWithAliases, true, false);
        }


        /// <summary>
        /// Adds one <see cref="QueryColumn"/> into list, regardless of existing columns.
        /// <see cref="QueryColumn"/> added to the list is created via <see cref="QueryColumn.FromExpression"/>.
        /// </summary>
        /// <example>
        /// <para>"A, B, C" will result in exception</para>
        /// <para>"A as B", "A" or "5 as B" are accepted</para>
        /// </example>
        /// <param name="column">String representation of column</param>
        /// <exception cref="ArgumentException">Only one column should be passed as argument</exception>
        public void Add(string column)
        {
            if (SqlHelper.ParseColumnListEvenNoColumnMacro(column).Count > 1)
            {
                throw new ArgumentException("Only one column should be passed as argument, use AddRange or AddRangeUnique methods.");
            }

            Add(QueryColumn.FromExpression(column));
        }


        /// <summary>
        /// Adds column into list, regardless of existing columns.
        /// </summary>
        /// <param name="queryColumn">Query column</param>
        public void Add(IQueryColumn queryColumn)
        {
            AddInternal(queryColumn);
        }


        /// <summary>
        /// Adds columns into list, regardless of existing columns.
        /// </summary>
        /// <param name="queryColumns">Query columns</param>
        public void AddRange(IEnumerable<IQueryColumn> queryColumns)
        {
            foreach (var col in queryColumns)
            {
                Add(col);
            }
        }


        /// <summary>
        /// Ensures that the order by columns are provided within the list of columns as aliases, modifies the order by expression to the aliases of those columns.
        /// </summary>
        /// <param name="orderBy">Order by expression</param>
        public void EnsureOrderByColumns(ref string orderBy)
        {
            if (!String.IsNullOrEmpty(orderBy))
            {
                var orderBycolumns = SqlHelper.ParseColumnList(orderBy);

                var i = 1;
                orderBy = null;

                foreach (var orderBycolumn in orderBycolumns)
                {
                    var alias = SystemColumns.ORDER + i;

                    // Parse the order by column
                    string suffix;
                    var column = SqlHelper.GetOrderByColumnName(orderBycolumn, out suffix);

                    // Add column to list of output columns
                    AddUnique(new QueryColumn(column).As(alias));

                    // Add order alias to result
                    orderBy = SqlHelper.AddOrderBy(orderBy, alias + suffix);

                    i++;
                }
            }
        }


        /// <summary>
        /// Loads the given columns to the list.
        /// </summary>
        /// <param name="columns">Lists of columns separated by commas</param>
        public void Load(string columns)
        {
            Clear();

            if (!String.IsNullOrEmpty(columns))
            {
                var parsedColumns = SqlHelper.ParseColumnListEvenNoColumnMacro(columns);
                if (parsedColumns.Any())
                {
                    AddStringColumns(parsedColumns, true, false);
                }
            }
        }


        /// <summary>
        /// Loads the given list of columns to the list.
        /// </summary>
        /// <param name="columns">Array of columns</param>
        /// <example>
        /// <code>
        /// var queryColumnListInstance = new QueryColumnList();
        /// queryColumnListInstance.Load(new[] {"ColumnA", "ColumnB", "ColumnC"})
        /// </code>
        /// </example>
        public void Load(params string[] columns)
        {
            Clear();

            AddStringColumns(columns, true, false);
        }


        /// <summary>
        /// Adds columns that are not present in the list.
        /// </summary>
        /// <remarks>
        /// Query columns to add are created from string columns via <see cref="QueryColumn.FromExpression"/>.
        /// Adds only <see cref="IQueryColumn"/> columns which names are not already present.
        /// </remarks>
        /// <param name="columns">Columns to be added</param>
        /// <param name="ignoreAllColumnsSelector">
        /// Indicates whether column will be added if all columns selector is present in the list.
        /// If the list is empty, <see cref="QueryColumnListBase.NoColumns"/> is false and <paramref name="ignoreAllColumnsSelector"/> is true, all columns selector is added.
        /// </param>
        public void AddRangeUnique(IEnumerable<string> columns, bool ignoreAllColumnsSelector = true)
        {
            AddStringColumns(columns, ignoreAllColumnsSelector, ignoreAllColumnsSelector);
        }


        /// <summary>
        /// Adds the given list of query columns.
        /// </summary>
        /// <param name="columns">Columns to add</param>
        /// <param name="ignoreAllColumnsSelector">Indicates whether column will be added if all columns selector is present in the list</param>
        /// <param name="ensureAllColumns">If true, the collection ensures all columns to be present if no columns are defined</param>
        private void AddObjectColumns(IEnumerable<IQueryColumn> columns, bool ignoreAllColumnsSelector, bool ensureAllColumns)
        {
            // If the added columns are extra columns, make sure all columns are included in case the previous state of the list was that it includes all columns
            if (ensureAllColumns)
            {
                EnsureAllColumns();
            }

            // Add all columns
            foreach (IQueryColumn col in columns)
            {
                if (col != null)
                {
                    if (col.Name == SqlHelper.NO_COLUMNS)
                    {
                        SetNoColumns();
                    }
                    else
                    {
                        AddUniqueInternal(col, ignoreAllColumnsSelector);
                    }
                }
            }
        }


        /// <summary>
        /// Adds the given list of columns to the list.
        /// </summary>
        /// <param name="cols">Columns represented as string enumerable</param>
        /// <param name="ignoreAllColumnsSelector">Indicates whether column will be added if all columns selector is present in the list</param>
        /// <param name="ensureAllColumns">If true, the collection ensures all columns to be present if no columns are defined</param>
        private void AddStringColumns(IEnumerable<string> cols, bool ignoreAllColumnsSelector, bool ensureAllColumns)
        {
            if (cols == null)
            {
                return;
            }

            AddObjectColumns(cols.Select(c => QueryColumn.FromExpression(c)), ignoreAllColumnsSelector, ensureAllColumns);
        }


        /// <summary>
        /// Sets no columns flag if no columns added yet.
        /// </summary>
        private void SetNoColumns()
        {
            if (Count == 0)
            {
                NoColumns = true;
                Changed();
            }
        }


        /// <summary>
        /// Loads the given list of columns to the list.
        /// </summary>
        /// <param name="columns">Enumeration of columns</param>
        /// <example>
        /// <code>
        /// var queryColumnListInstance = new QueryColumnList();
        /// var columns = new List&lt;IQueryColumn&gt;() { new QueryColumn("ColumnA"), new QueryColumn("ColumnB") })
        /// queryColumnListInstance.Load(columns);
        /// </code>
        /// </example>
        public void Load(IEnumerable<IQueryColumn> columns)
        {
            Clear();

            AddObjectColumns(columns, true, false);
        }


        /// <summary>
        /// Adds the given column to the column list, if column is not already present.
        /// </summary>
        /// <param name="column">Column</param>
        /// <param name="ignoreAllColumnsSelector">
        /// Indicates whether column will be added if all columns selector is present in the list
        /// If the list is empty, <see cref="QueryColumnListBase.NoColumns"/> is false and <paramref name="ignoreAllColumnsSelector"/> is true, all columns selector is added.
        /// </param>
        public void AddUnique(IQueryColumn column, bool ignoreAllColumnsSelector = true)
        {
            AddRangeUnique(new[] { column }, ignoreAllColumnsSelector);
        }


        /// <summary>
        /// Adds the given enumerable of query columns, if not already present in the list.
        /// </summary>
        /// <param name="columns">Columns to be added</param>
        /// <param name="ignoreAllColumnsSelector">
        /// Indicates whether column will be added if all columns selector is present in the list.
        /// If the list is empty, <see cref="QueryColumnListBase.NoColumns"/> is false and <paramref name="ignoreAllColumnsSelector"/> is true, all columns selector is added.
        /// </param>
        public void AddRangeUnique(IEnumerable<IQueryColumn> columns, bool ignoreAllColumnsSelector = true)
        {
            AddObjectColumns(columns, ignoreAllColumnsSelector, ignoreAllColumnsSelector);
        }


        /// <summary>
        /// Adds the given list of query columns, if not already present in the list.
        /// </summary>
        /// <param name="columns">Columns to be added</param>
        /// <param name="ignoreAllColumnsSelector">
        /// Indicates whether column will be added if all columns selector is present in the list.
        /// If the list is empty, <see cref="QueryColumnListBase.NoColumns"/> is false and <paramref name="ignoreAllColumnsSelector"/> is true, all columns selector is added.
        /// </param>
        public void AddRangeUnique(QueryColumnList columns, bool ignoreAllColumnsSelector = true)
        {
            // If no columns provided within other list, do not include
            if ((columns == null) || (columns.NoColumns))
            {
                return;
            }

            // If input collection contains *, we have to add * as first item, because ignoreAllColumnsSelector
            // can be false, that may result in different outputs, depending on position of * in input collection
            if (columns.ContainsAllColumnsSelector && !ContainsAllColumnsSelector && !ignoreAllColumnsSelector)
            {
                Add(SqlHelper.COLUMNS_ALL);
            }

            // Adds unique columns, that are not currently present
            AddRangeUnique((IEnumerable<IQueryColumn>)columns, ignoreAllColumnsSelector);
        }


        /// <summary>
        /// Returns the column list transformed to the aliases of the columns.
        /// </summary>
        public QueryColumnList AsAliases()
        {
            // Copy if the value is true, so the all columns selector is not copied
            var result = new QueryColumnList();
            result.NoColumns = NoColumns;

            if (ContainsAllColumnsSelector)
            {
                // If the list includes all columns, the list of aliases are just undefined (all) columns
                return result;
            }

            result.AddObjectColumns(this.Select(col => col.AsAlias()), true, false);
            return result;
        }

        #endregion
    }
}
