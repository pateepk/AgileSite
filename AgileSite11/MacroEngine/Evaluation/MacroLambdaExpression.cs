using System.Collections.Generic;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Represents lambda expression in macro engine
    /// </summary>
    internal class MacroLambdaExpression
    {
        #region "Properties"

        /// <summary>
        /// Expression (right side of "=>" operator).
        /// </summary>
        public MacroExpression Expression
        {
            get;
            private set;
        }


        /// <summary>
        /// List of parameters (left side of "=>" operator).
        /// </summary>
        public List<string> Variables
        {
            get;
            private set;
        }

        #endregion


        #region "Constructor"

        /// <summary>
        /// Constructor - creates a macro lambda expression.
        /// </summary>
        /// <param name="variables">List of parameters (left side of "=>" operator)</param>
        /// <param name="expression">Expression (right side of "=>" operator)</param>
        public MacroLambdaExpression(List<string> variables, MacroExpression expression)
        {
            Variables = variables;
            Expression = expression;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// String representation of the lambda expression is empty string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "";
        }

        #endregion
    }
}