using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Row number query column e.g. "ROW_NUMBER() OVER (ORDER BY DefaultCulture) AS CMS_RN"
    /// </summary>
    public class RowNumberColumn : SelectQueryColumnBase<RowNumberColumn>
    {
        #region "Properties"

        /// <summary>
        /// OrderBy expression for the row number
        /// </summary>
        public string OrderBy
        {
            get;
            set;
        }


        /// <summary>
        /// If set, the row order gets partitioned by specific column(s)
        /// </summary>
        public string PartitionBy
        {
            get;
            set;
        }


        /// <summary>
        /// Returns true if this column represents a single column
        /// </summary>
        public override bool IsSingleColumn
        {
            get
            {
                return true;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="orderBy">Order by</param>
        public RowNumberColumn(string orderBy)
            : base(null)
        {
            EnsureBracketsForAlias = false;
            OrderBy = orderBy;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="orderBy">Order by</param>
        public RowNumberColumn(string columnName, string orderBy)
            : base(null)
        {
            EnsureBracketsForAlias = false;
            ColumnAlias = columnName;
            OrderBy = orderBy;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="orderBy">Order by</param>
        public RowNumberColumn(string columnName, IQueryColumn orderBy)
            : this(columnName, orderBy.ToString())
        {
        }


        /// <summary>
        /// Default constructor
        /// </summary>
        public RowNumberColumn()
            : base(null)
        {
        }


        /// <summary>
        /// Gets the expression for the column data
        /// </summary>
        protected internal override string GetValueExpression()
        {
            return SqlHelper.GetRowNumber(OrderBy, PartitionBy);
        }

        #endregion
    }
}
