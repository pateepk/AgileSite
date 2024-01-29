using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Core;

namespace CMS.Base
{
    /// <summary>
    /// Worker processing queue in single (one per application and generic variant), ever-running asynchronous thread
    /// </summary>
    public abstract class ThreadQueueWorker<TItem, TWorker> : ThreadWorker<TWorker>
        where TWorker : ThreadQueueWorker<TItem, TWorker>, new()
        where TItem : class
    {
        #region "Variables"

        private readonly Queue<TItem> mQueue = new Queue<TItem>();
        private bool? mAllowEnqueue;
        private readonly object syncProcessLock = new object();

        /// <summary>
        /// Events fired by the worker
        /// </summary>
        public readonly ThreadQueueWorkerEvents<TItem, TWorker> Events = new ThreadQueueWorkerEvents<TItem, TWorker>();

        #endregion


        #region "Properties"

        /// <summary>
        /// <para>
        /// Indicates whether <see cref="Enqueue"/> method can put items in queue for asynchronous processing. True by default. Work items are processed synchronously when set to false.
        /// </para>
        /// <para>
        /// Unless explicitly set, the default implementation returns value of <see cref="ThreadQueueWorkerSettings.AllowEnqueue"/> flag.
        /// </para>
        /// </summary>
        /// <remarks>
        /// <para>
        /// Setting this flag to false does not affect existing queue items.
        /// </para>
        /// <para>
        /// Setting this flag to false can cause deadlocks as work items which would be queued and processed from synchronization context of one worker thread are processed synchronously from possibly multiple threads.
        /// </para>
        /// <para>
        /// Setting this flag to false is recommended in context of tests only. It is not intended to be set to false in production environment.
        /// </para>
        /// </remarks>
        internal bool AllowEnqueue
        {
            get
            {
                return mAllowEnqueue ?? ThreadQueueWorkerSettings.AllowEnqueue;
            }
            set
            {
                mAllowEnqueue = value;
            }
        }


        /// <summary>
        /// Return the current number of items in the queue
        /// </summary>
        public int ItemsInQueue
        {
            get
            {
                return mQueue.Count;
            }
        }


        /// <summary>
        /// If true, the queue checks the duplicity when inserting items. The duplicity is checked using the default comparer of the item.
        /// </summary>
        protected virtual bool CheckDuplicity
        {
            get
            {
                return false;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Adds new item to processing queue.
        /// </summary>
        /// <param name="newItem">Adds the item to the queue</param>
        /// <param name="ensureThread">If true, the processing thread is ensured</param>
        public void Enqueue(TItem newItem, bool ensureThread = true)
        {
            if (!AllowEnqueue)
            {
                lock (syncProcessLock)
                {
                    Process(Enumerable.Repeat(newItem, 1));
                }
                return;
            }

            lock (SyncRoot)
            {
                var queue = mQueue;

                if (!CheckDuplicity || queue.All(item => (item == null) || !item.Equals(newItem)))
                {
                    queue.Enqueue(newItem);
                }
            }

            if (ensureThread)
            {
                EnsureRunningThread();
            }
        }


        /// <summary>
        /// Returns the first item in the queue and removes it
        /// </summary>
        private TItem Dequeue()
        {
            TItem item;

            lock (SyncRoot)
            {
                item = mQueue.Dequeue();
            }

            return item;
        }


        /// <summary>
        /// Method processing queued actions.
        /// </summary>
        protected sealed override void Process()
        {
            Process(FetchItemsToProcess());
        }


        /// <summary>
        /// Method processing given actions.
        /// </summary>
        /// <param name="items">Items to be processed.</param>
        private void Process(IEnumerable<TItem> items)
        {
            using (var h = Events.ProcessItems.StartEvent(items))
            {
                if (h.Continue)
                {
                    // Process the items
                    h.EventArguments.ItemsProcessed = ProcessItems(h.EventArguments.Items);
                }

                h.FinishEvent();
            }
        }


        /// <summary>
        /// Gets the queued items to process.
        /// </summary>
        private IEnumerable<TItem> FetchItemsToProcess()
        {
            while (ItemsInQueue > 0)
            {
                var item = Dequeue();
                if (item != null)
                {
                    yield return item;
                }
            }
        }


        /// <summary>
        /// Processes the given list of items. Override this method to process queued items as a bulk. Returns the number of processed items.
        /// </summary>
        /// <param name="items">Worker queue.</param>
        protected virtual int ProcessItems(IEnumerable<TItem> items)
        {
            int count = 0;

            // Process all incoming items
            foreach (var item in items)
            {
                try
                {
                    ProcessItem(item);
                }
                catch
                {
                    // Processing of the queue should continue after exception. Exception handling should be implemented in ProcessItem method override.
                }

                count++;
            }

            return count;
        }


        /// <summary>
        /// Processes the item in the queue. Override this method to process a single item from the queue.
        /// </summary>
        /// <param name="item">Item to process.</param>
        /// <remarks>
        /// If exception from override arises, the thread will not end, the rest of queue will be processed, but exception will not be logged. 
        /// Please consider implementing exception handling mechanism.
        /// </remarks>
        protected abstract void ProcessItem(TItem item);

        /// <summary>
        /// Static constructor registering the generic type.
        /// </summary>
        static ThreadQueueWorker()
        {
            TypeManager.RegisterGenericType(typeof(ThreadQueueWorker<TItem, TWorker>));
        }

        #endregion
    }
}
