using System.Collections;
using System.Linq.Expressions;

namespace CMS.DataEngine
{
    /// <summary>
    /// Provides the ability to replace particular constant in the expression tree by a new value
    /// </summary>
    internal class ExpressionTreeModifier : ExpressionVisitor
    {
        private readonly object mFrom;
        private readonly object mTo;

        
        /// <summary>
        /// Converts the constant expression to the results
        /// </summary>
        /// <param name="c">Constant to visit</param>
        protected override Expression VisitConstant(ConstantExpression c)
        {
            // Convert only 
            if (c.Value == mFrom)
            {
                return Expression.Constant(mTo);
            }

            return c;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public ExpressionTreeModifier(object from, object to)
        {
            mFrom = from;
            mTo = to;
        }


        /// <summary>
        /// Provides a copy of the expression tree with modifications applied
        /// </summary>
        /// <param name="expression">Expression to copy</param>
        internal Expression CopyAndModify(Expression expression)
        {
            return Visit(expression);
        }
    }
}