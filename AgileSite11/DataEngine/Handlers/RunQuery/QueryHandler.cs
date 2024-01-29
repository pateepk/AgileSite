namespace CMS.Base
{
    /// <summary>
    /// Query handler
    /// </summary>
    public class QueryHandler : AdvancedHandler<QueryHandler, QueryEventArgs>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="query">Handled query</param>
        public QueryHandler StartEvent(string query)
        {
            var e = new QueryEventArgs
            {
                Query = query
            };

            return StartEvent(e);
        }
    }
}