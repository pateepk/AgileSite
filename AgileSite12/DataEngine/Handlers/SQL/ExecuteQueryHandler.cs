using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Execute query handler
    /// </summary>
    public class ExecuteQueryHandler<TResult> : AdvancedHandler<ExecuteQueryHandler<TResult>, ExecuteQueryEventArgs<TResult>>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="query">Query that executes</param>
        /// <param name="conn">Connection executing the query</param>
        public ExecuteQueryHandler<TResult> StartEvent(QueryParameters query, IDataConnection conn)
        {
            var e = new ExecuteQueryEventArgs<TResult>()
            {
                Query = query,
                Connection = conn,
            };

            return StartEvent(e);
        }
    }
}