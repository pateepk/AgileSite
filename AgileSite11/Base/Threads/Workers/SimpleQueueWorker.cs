using System;
using System.Threading;

using CMS.Core;

namespace CMS.Base
{
    /// <summary>
    /// Worker for running various simple asynchronous actions.
    /// </summary>
    /// <remarks>Duplicity of items being enqueued is checks by <see cref="SimpleQueueItem.Key"/>.</remarks>
    public abstract class SimpleQueueWorker<T> : ThreadQueueWorker<SimpleQueueItem, T>
        where T : SimpleQueueWorker<T>, new()
    {
        private int mProcessedItems;

        /// <summary>
        /// Default interval of processing cycle.
        /// </summary>
        protected override int DefaultInterval
        {
            get
            {
                return 100;
            }
        }


        /// <summary>
        /// Maintenance interval for turning off the thread.
        /// </summary>
        protected override int MaintenanceInterval
        {
            get
            {
                return 60000;
            }
        }


        /// <summary>
        /// Turns off thread in case of inactivity.
        /// </summary>
        protected override void DoMaintenance()
        {
            if (mProcessedItems > 0)
            {
                mProcessedItems = 0;
            }
            else
            {
                StopExecution(() => ItemsInQueue == 0);
            }
        }


        /// <summary>
        /// Initialize item counter event.
        /// </summary>
        protected override void Initialize()
        {
            Events.ProcessItems.After += (object sender, ThreadQueueWorkerEventArgs<SimpleQueueItem> e) => mProcessedItems += e.ItemsProcessed;
            base.Initialize();
        }


        /// <summary>
        /// Finishes the worker process.
        /// </summary>
        protected override void Finish()
        {
            // Run the process for the one last time
            RunProcess();
        }


        /// <summary>
        /// Enqueue action with random unique key.
        /// </summary>
        /// <param name="action">Action to be enqueued.</param>
        public void Enqueue(Action action)
        {
            Enqueue(new SimpleQueueItem
            {
                Action = action
            });
        }


        /// <summary>
        /// Enqueue action with given key.
        /// </summary>
        /// <param name="key">Action key.</param>
        /// <param name="action">Action to be enqueued.</param>
        public void Enqueue(string key, Action action)
        {
            Enqueue(new SimpleQueueItem
            {
                Action = action,
                Key = key
            });
        }


        /// <summary>
        /// Processing of single action.
        /// </summary>
        /// <param name="item">Item to be processed.</param>
        protected override void ProcessItem(SimpleQueueItem item)
        {
            try
            {
                if (item != null && item.Action != null)
                {
                    item.Action();
                }
            }
            catch (ThreadAbortException ex)
            {
                if (!CMSThread.Stopped(ex))
                {
                    var exception = new Exception("Action " + item.Key + " was aborted", ex);
                    CoreServices.EventLog.LogException("ActionQueueWorker", "PROCESS", exception);
                }
            }
            catch (Exception ex)
            {
                var exception = new Exception("An exception occurred while processing action " + item.Key + " was aborted", ex);
                CoreServices.EventLog.LogException("ActionQueueWorker", "PROCESS", exception);
            }
        }
    }
}
