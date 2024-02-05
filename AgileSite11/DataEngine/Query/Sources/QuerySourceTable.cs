using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Defines the query source table (or view) with optional alias and hints.
    /// </summary>
    public class QuerySourceTable
    {
        #region "Properties"

        /// <summary>
        /// Table name or expression
        /// </summary>
        public string Expression
        {
            get;
            set;
        }


        /// <summary>
        /// Optional table alias
        /// </summary>
        public string Alias
        {
            get;
            set;
        }


        /// <summary>
        /// Table hints
        /// </summary>
        public string[] Hints
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sourceExpression">Source expression</param>
        /// <param name="sourceAlias">Source alias</param>
        /// <param name="hints">Hints</param>
        public QuerySourceTable(string sourceExpression, string sourceAlias = null, params string[] hints)
        {
            Expression = sourceExpression;
            Alias = sourceAlias;
            Hints = hints;
        }


        /// <summary>
        /// Gets the full query source table expression made from the configured components
        /// </summary>
        public string GetFullExpression()
        {
            // Ensure proper source expression
            var expression = Expression;

            // Add alias
            if (!String.IsNullOrEmpty(Alias))
            {
                expression += " AS " + Alias;
            }

            // Add hints
            if ((Hints != null) && (Hints.Length > 0))
            {
                expression += " " + SqlHints.GetTableHints(Hints);
            }

            return expression;            
        }


        /// <summary>
        /// Gets the source name, either alias or the expression
        /// </summary>
        public string GetName()
        {
            return Alias ?? Expression;
        }


        /// <summary>
        /// Implicit conversion from the given expression (as table name) to query source table
        /// </summary>
        /// <param name="sourceExpression">Source expression</param>
        public static implicit operator QuerySourceTable(string sourceExpression)
        {
            return new QuerySourceTable(sourceExpression);
        }

        #endregion
    }
}
