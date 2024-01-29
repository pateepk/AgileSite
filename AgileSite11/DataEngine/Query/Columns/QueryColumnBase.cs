using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Query column
    /// </summary>
    public abstract class QueryColumnBase<TColumn> : QueryParametersBase<TColumn>, IQueryColumn
        where TColumn : QueryColumnBase<TColumn>, new()
    {
        #region "Properties"

        /// <summary>
        /// Gets the column name
        /// </summary>
        public override string Name
        {
            get
            {
                return Expression;
            }
        }


        /// <summary>
        /// Expression (column name)
        /// </summary>
        public string Expression 
        { 
            get; 
            set; 
        }
        

        /// <summary>
        /// Returns specifically typed current instance
        /// </summary>
        protected TColumn TypedThis
        {
            get
            {
                return (TColumn)this;
            }
        }


        /// <summary>
        /// Returns true if this column represents a single column
        /// </summary>
        public abstract bool IsSingleColumn
        {
            get;
        }


        /// <summary>
        /// Returns the new column created from this column alias
        /// </summary>
        public IQueryColumn AsAlias()
        {
            if (String.IsNullOrEmpty(Name))
            {
                throw new NotSupportedException("Cannot extract alias for the column '" + ToString() + "', the column does not have an alias.");
            }

            return new QueryColumn(Name);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="expression">Expression (column name)</param>
        protected QueryColumnBase(string expression)
        {
            Expression = expression;
        }
        
        #endregion


        #region "Methods"

        /// <summary>
        /// Sets the expression
        /// </summary>
        /// <param name="expression">New column expression</param>
        public TColumn WithExpression(string expression)
        {
            var result = GetTypedQuery();
            result.Expression = expression;

            return result;
        }


        /// <summary>
        /// Returns the string representation of the expression, with possibility of expanding parameters
        /// </summary>
        /// <param name="expand">If true, the result is expanded with parameters so it can act as standalone value.</param>
        public override string ToString(bool expand)
        {
            var expr = GetExpression();

            if (expand)
            {
                expr = Parameters.Expand(expr);
            }

            return expr;
        }


        /// <summary>
        /// Gets a query expression representing this object as a value
        /// </summary>
        public virtual IQueryExpression AsValue()
        {
            var expr = GetExpression();

            return new QueryExpression(expr);
        }


        /// <summary>
        /// Gets the expression for the column data
        /// </summary>
        public virtual string GetExpression()
        {
            return Expression;
        }
        
        #endregion


        #region "Operators"

        /// <summary>
        /// Implicit operator for conversion from QueryColumn class to string
        /// </summary>
        /// <param name="col">Column object</param>
        public static explicit operator string(QueryColumnBase<TColumn> col)
        {
            if (col == null)
            {
                return null;
            }

            return col.ToString();
        }

        #endregion
    }
}
