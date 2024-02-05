using System.Collections.Generic;

namespace CMS.Base
{
    /// <summary>
    /// Thread queue worker event arguments
    /// </summary>
    public class ThreadQueueWorkerEventArgs<TItem> : CMSEventArgs
    {
        /// <summary>
        /// Number of items processed in current cycle.
        /// </summary>
        public int ItemsProcessed
        {
            get;
            set;
        }


        /// <summary>
        /// Items to process. Does not support multiple enumerations since it is working over a queue.
        /// </summary>
        public IEnumerable<TItem> Items
        {
            get;
            set;
        }
    }
}