using System.Collections.Generic;

namespace CMS.DataEngine
{
    /// <summary>
    /// Represents a general query expression
    /// </summary>
    public class QueryExpression : QueryExpressionBase<QueryExpression>
    {
        /// <summary>
        /// Empty constructor
        /// </summary>
        public QueryExpression()
        {
        }


        /// <summary>
        /// Constructor with expression
        /// </summary>
        /// <param name="expression">Expression text</param>
        /// <param name="parameters">Parameters used in <paramref name="expression"/>.</param>
        public QueryExpression(string expression, QueryDataParameters parameters = null)
        {
            Expression = expression;

            if (parameters != null)
            {
                Parameters = parameters;
            }
        }
    }
}