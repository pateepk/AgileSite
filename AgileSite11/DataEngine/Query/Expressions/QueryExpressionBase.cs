using System;
using System.Linq;
using System.Text;

namespace CMS.DataEngine
{
    /// <summary>
    /// Query expression base class
    /// </summary>
    public class QueryExpressionBase<TExpression> : QueryParametersBase<TExpression>, IQueryExpression 
        where TExpression : QueryExpressionBase<TExpression>, new()
    {
        /// <summary>
        /// Expression
        /// </summary>
        public string Expression
        {
            get;
            set;
        }


        /// <summary>
        /// Gets a query expression representing this object as a value
        /// </summary>
        public virtual string GetExpression()
        {
            return Expression;
        }


        /// <summary>
        /// Gets a query expression representing this object as a value
        /// </summary>
        public virtual IQueryExpression AsValue()
        {
            return this;
        }


        /// <summary>
        /// Returns the string representation of the expression, with possibility of expanding parameters
        /// </summary>
        /// <param name="expand">If true, the result is expanded with parameters so it can act as standalone value.</param>
        public override string ToString(bool expand)
        {
            return expand ? Expand(Expression) : Expression;
        }


        /// <summary>
        /// Copies all the object properties to the given target class
        /// </summary>
        /// <param name="target">Target class</param>
        public override void CopyPropertiesTo(IQueryObject target)
        {
            var expr = target as IQueryExpression;
            if (expr != null)
            {
                expr.Expression = Expression;
            }

            base.CopyPropertiesTo(target);
        }


        /// <summary>
        /// Creates a column from the given expression
        /// </summary>
        /// <param name="alias">Column alias</param>
        public QueryColumn AsColumn(string alias)
        {
            // Convert to value expression
            var value = AsValue();
            var valueExpression = value.GetExpression();

            var col = new QueryColumn();

            // Include parameters
            valueExpression = col.IncludeDataParameters(value.Parameters, valueExpression);

            return col.WithExpression(valueExpression).As(alias);
        }
    }
}
