namespace CMS.Base
{
    /// <summary>
    /// Events fired by the worker
    /// </summary>
    public class ThreadQueueWorkerEvents<TItem, TWorker>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        internal ThreadQueueWorkerEvents()
        {
        }


        /// <summary>
        /// Fires when the items from the queue are processed
        /// </summary>
        public readonly ThreadQueueWorkerHandler<TItem> ProcessItems = new ThreadQueueWorkerHandler<TItem>
        {
            Name = typeof(TWorker).Name + ".Events.ProcessItems",
            Debug = false
        };
    }
}