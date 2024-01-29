using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Execute query event arguments
    /// </summary>
    public class ExecuteQueryEventArgs<TResult> : CMSEventArgs
    {
        /// <summary>
        /// Processed query
        /// </summary>
        public QueryParameters Query
        {
            get;
            set;
        }


        /// <summary>
        /// Query connection
        /// </summary>
        public IDataConnection Connection
        {
            get;
            set;
        }


        /// <summary>
        /// Result of the query
        /// </summary>
        public TResult Result
        {
            get;
            set;
        }
    }
}