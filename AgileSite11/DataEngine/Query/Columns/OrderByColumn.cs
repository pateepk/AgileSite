namespace CMS.DataEngine
{
    /// <summary>
    /// Order by column
    /// </summary>
    public sealed class OrderByColumn : QueryColumnBase<OrderByColumn>
    {
        /// <summary>
        /// Order direction
        /// </summary>
        public OrderDirection Direction
        {
            get;
            set;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public OrderByColumn()
            : base(null)
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="expression">Expression (column name)</param>
        /// <param name="direction">Order direction</param>
        public OrderByColumn(string expression, OrderDirection direction = OrderDirection.Ascending)
            : base(expression)
        {
            Direction = direction;
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


        /// <summary>
        /// Gets the expression for the column data
        /// </summary>
        public override string GetExpression()
        {
            var expression = base.GetExpression();

            if (IsValidColumnName(expression))
            {
                expression = SqlHelper.AddSquareBrackets(expression);
            }

            switch (Direction)
            {
                case OrderDirection.Ascending:
                    expression += SqlHelper.ORDERBY_ASC;
                    break;

                case OrderDirection.Descending:
                    expression += SqlHelper.ORDERBY_DESC;
                    break;
            }

            return expression;
        }


        /// <summary>
        /// Returns true if given expression is valid column name (no SQL query is contained)
        /// </summary>
        private static bool IsValidColumnName(string expression)
        {
            return SqlSecurityHelper.ColumnsRegex.IsMatch(expression);
        }
    }
}
