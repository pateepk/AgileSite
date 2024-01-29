using System;
using System.Linq;
using System.Text;

using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Represents list of OrderBy columns
    /// </summary>
    public sealed class OrderByColumnList : QueryColumnListBase
    {
        #region "Constructors"

        /// <summary>
        /// Creates an empty column list built from the given columns
        /// </summary>
        /// <param name="columns">List of columns</param>
        public OrderByColumnList(string columns)
        {
            Load(columns);
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Loads the given columns to the list
        /// </summary>
        /// <param name="columns">Lists of columns separated by commas</param>
        private void Load(string columns)
        {
            var cols = SqlHelper.ParseColumnList(columns);
            cols.ForEach(AddColumnInternal);
        }


        /// <summary>
        /// Adds column into list
        /// </summary>
        /// <param name="column">column expression including name of the column with ASC or DESC expression</param>
        private void AddColumnInternal(string column)
        {
            string direction;
            var columnName = SqlHelper.GetOrderByColumnName(column, out direction);
            var orderByDirection = GetDirection(direction);

            AddUniqueInternal(new OrderByColumn(columnName, orderByDirection), false);
        }


        /// <summary>
        /// Translates string direction value to enum-based value
        /// </summary>
        /// <param name="direction">string direction (ASC or DESC)</param>
        /// <returns>Enum direction representation</returns>
        private OrderDirection GetDirection(string direction)
        {
            if (!String.IsNullOrEmpty(direction))
            {
                if (direction.Equals(SqlHelper.ORDERBY_ASC, StringComparison.InvariantCultureIgnoreCase))
                {
                    return OrderDirection.Ascending;
                }
                if (direction.Equals(SqlHelper.ORDERBY_DESC, StringComparison.InvariantCultureIgnoreCase))
                {
                    return OrderDirection.Descending;
                }
            }

            return OrderDirection.Default;
        }


        /// <summary>
        /// Removes from the order by columns those not present in select columns. For the purposes of nested distinct queries.
        /// </summary>
        /// <param name="selectColumns">Columns returned by the nested query.</param>
        internal void FilterOrderByColumns(QueryColumnList selectColumns)
        {
            if (!ReturnsAllColumns)
            {
                // Remove all ORDER BY columns missing in the selection, if all columns are removed => there is a check in data query implementation
                var columnsToRemove = this.Where(ob => selectColumns.All(sc => sc.Name != ob.Name)).ToList();
                columnsToRemove.ForEach(Remove);
            }
        }


        /// <summary>
        /// Translates order column alias for real column name for given select columns
        /// </summary>
        /// <param name="selectColumnsList">Select query columns holding name and alias column names</param>
        internal void TranslateAliases(QueryColumnList selectColumnsList)
        {
            foreach (var orderColumn in this)
            {
                var selectColumn = selectColumnsList.OfType<SelectQueryColumnBase<QueryColumn>>()
                                                    .FirstOrDefault(c => SqlHelper.RemoveSquareBrackets(orderColumn.Name).EqualsCSafe(SqlHelper.RemoveSquareBrackets(c.ColumnAlias)));
                if (selectColumn != null)
                {
                    orderColumn.Expression = selectColumn.Expression;
                }
            }
        }

        #endregion
    }
}
