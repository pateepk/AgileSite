namespace CMS.DataEngine
{
    /// <summary>
    /// Data query source which gets the data from specific SQL expression. That can be table name, view name, or more complex SQL expression.
    /// </summary>
    public class QuerySource : QuerySourceBase<QuerySource>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public QuerySource()
        {
        }

        
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source">Source table</param>
        public QuerySource(QuerySourceTable source)
            : base(source)
        {
        }



        /// <summary>
        /// Implicit operator for conversion from string to query source
        /// </summary>
        /// <param name="expression">Source expression</param>
        public static implicit operator QuerySource(string expression)
        {
            if (expression == null)
            {
                return null;
            }

            return new QuerySource(expression);
        }


        /// <summary>
        /// Implicit operator for conversion from source table to query source
        /// </summary>
        /// <param name="sourceTable">Source table</param>
        public static implicit operator QuerySource(QuerySourceTable sourceTable)
        {
            if (sourceTable == null)
            {
                return null;
            }

            return new QuerySource(sourceTable);
        }
    }
}
