using System.Collections.Generic;

namespace CMS.Base
{
    /// <summary>
    /// Thread queue worker handler
    /// </summary>
    public class ThreadQueueWorkerHandler<TItem> : AdvancedHandler<ThreadQueueWorkerHandler<TItem>, ThreadQueueWorkerEventArgs<TItem>>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="items">Worker queue</param>
        public ThreadQueueWorkerHandler<TItem> StartEvent(IEnumerable<TItem> items)
        {
            var e = new ThreadQueueWorkerEventArgs<TItem>
            {
                Items = items
            };

            var h = StartEvent(e);

            return h;
        }
    }
}