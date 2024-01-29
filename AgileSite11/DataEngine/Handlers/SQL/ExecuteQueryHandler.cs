using System;

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


        /// <summary>
        /// Adds the conditional before query handler for the query executed with specified mark
        /// </summary>
        /// <param name="mark">Query mark</param>
        [Obsolete("Method was not intended for public use and will be removed in the next version.")]
        public BeforeConditionalEvent<ExecuteQueryEventArgs<TResult>> WhenMarkedWith(string mark)
        {
            return AddBefore()
                .When(args =>
                      args.Query.Text.Contains(mark)
                );
        }
    }
}