using System;

using CMS.Base;
using CMS.Helpers;

namespace CMS.DataEngine
{
    /// <summary>
    /// Simple query column e.g. "DocumentName" / "DocumentName AS Name"
    /// </summary>
    public sealed class QueryColumn : SelectQueryColumnBase<QueryColumn>
    {
        #region "Properties"

        /// <summary>
        /// Returns true if this column represents a single column
        /// </summary>
        public override bool IsSingleColumn
        {
            get
            {
                return !Expression.Contains("*");
            }
        }


        /// <summary>
        /// Gets or sets the value that indicates whether square brackets should be used for non-base column defined by IdentifierRegExp
        /// </summary>
        internal bool ForceColumnBrackets
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        public QueryColumn()
            : base(null)
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <remarks>
        /// <seealso cref="QueryColumn.FromExpression"/> for the difference how the created column looks like
        /// when you name the columns "A as B" in this constructor or in the given method
        /// </remarks>
        /// <param name="columnName">Column name</param>
        public QueryColumn(string columnName)
            : base(columnName)
        {
            EnsureBracketsForAlias = false;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="forceColumnBrackets">Indicates whether for brackets should be used for non-base column defined by IdentifierRegExp</param>
        internal QueryColumn(string columnName, bool forceColumnBrackets)
            : this(columnName)
        {
            ForceColumnBrackets = forceColumnBrackets;
        }


        /// <summary>
        /// Gets the value expression of the columns
        /// </summary>
        protected internal override string GetValueExpression()
        {
            string expression = base.GetValueExpression();

            // Ensure the square brackets for base columns without alias
            if (String.IsNullOrEmpty(ColumnAlias) && (ForceColumnBrackets || IsBasicColumnName(expression)))
            {
                return SqlHelper.AddSquareBrackets(expression);
            }

            return expression;
        }


        /// <summary>
        /// Creates a query column from the given expression
        /// </summary>
        /// <param name="expression">Column expression</param>
        public static QueryColumn FromExpression(string expression)
        {
            string expr;
            string alias;

            SqlHelper.ParseColumn(expression, out expr, out alias);

            var col = new QueryColumn(expr);

            if (alias != null)
            {
                // If input is column with alias, include the alias
                col.ColumnAlias = alias;
                col.EnsureBracketsForAlias = false;
            }
            else
            {
                col.EnsureBracketsForAlias = true;
            }

            return col;
        }


        /// <summary>
        /// Returns true if column name match the identifier rules and is not NULL
        /// </summary>
        /// <param name="columnName">Column name</param>
        internal static bool IsBasicColumnName(string columnName)
        {
            return (ValidationHelper.IsIdentifier(columnName) && !columnName.EqualsCSafe("null", true));
        }

        #endregion
    }
}