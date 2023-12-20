using System.Collections.Generic;

using CMS.Base;

namespace CMS.Synchronization
{
    /// <summary>
    /// Queue worker for processing of synchronization tasks
    /// </summary>
    public class SynchronizationQueueWorker : SimpleQueueWorker<SynchronizationQueueWorker>
    {
        /// <summary>
        /// Processes the given list of items. Override this method to process queued items as a bulk. Returns the number of processed items.
        /// </summary>
        /// <param name="items">Worker queue.</param>
        protected override int ProcessItems(IEnumerable<SimpleQueueItem> items)
        {
            var result = base.ProcessItems(items);

            // After tasks are logged, run the corresponding connectors to process them
            IntegrationHelper.ProcessInternalTasksAsync(IntegrationHelper.TouchedConnectorNames);

            return result;
        }
    }
}