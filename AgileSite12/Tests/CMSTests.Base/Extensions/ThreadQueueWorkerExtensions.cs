using System;
using System.Threading;

using CMS.Base;

namespace CMS.Tests.Base
{
    /// <summary>
    /// Extension methods for testing thread queue worker.
    /// </summary>
    public static class ThreadQueueWorkerExtensions
    {
        /// <summary>
        /// Waits for finishing processing the queue.
        /// </summary>
        /// <param name="worker">Thread queue worker instance.</param>
        public static void WaitUntilQueueIsProcessed<TItem, TWorker>(this ThreadQueueWorker<TItem, TWorker> worker)
            where TWorker : ThreadQueueWorker<TItem, TWorker>, new()
            where TItem : class
        {
            if (!worker.IsThreadRunning())
            {
                throw new InvalidOperationException("[ThreadQueueWorkerExtensions.WaitUntilQueueIsProcessed]: Waiting for ThreadQueueWorker that is not running is would cause infinite waiting.");
            }

            // Wait for finishing event to make sure that all tasks were not only grabbed from queue, but also processed
            var waitHandle = new AutoResetEvent(false);

            worker.Events.ProcessItems.After += (object sender, ThreadQueueWorkerEventArgs<TItem> e) =>
            {
                if (worker.ItemsInQueue == 0)
                {
                    waitHandle.Set();
                }
            };

            waitHandle.WaitOne();
        }
    }
}
