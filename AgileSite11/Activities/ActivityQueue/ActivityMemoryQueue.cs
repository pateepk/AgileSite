using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

using CMS.Activities.Internal;

namespace CMS.Activities
{
    /// <summary>
    /// Provides methods for thread-safe storing of performed activities to the memory queue.
    /// </summary>
    /// <threadsafety static="true" instance="true"/>
    internal sealed class ActivityMemoryQueue : IActivityQueue
    {
        private static readonly ConcurrentQueue<IActivityInfo> mQueue = new ConcurrentQueue<IActivityInfo>();
        

        /// <summary>
        /// Gets queue storing the performed activities.
        /// </summary>
        internal ConcurrentQueue<IActivityInfo> Queue
        {
            get
            {
                return mQueue;
            }
        }


        /// <summary>
        /// Tries to remove and return <see cref="IActivityInfo"/> at the beginning of the concurrent queue.
        /// </summary>
        /// <param name="activity">
        /// When this method returns, if the operation was successful, <paramref name="activity"/> contains the object removed. If no object was available to be removed, the <paramref name="activity"/> is unspecified.
        /// </param>
        /// <returns>
        /// True if <see cref="IActivityInfo"/> was removed and returned from the beginning of the <see cref="ActivityMemoryQueue"/> successfully; otherwise, false.
        /// </returns>
        public bool TryDequeueActivity(out IActivityInfo activity)
        {
            return Queue.TryDequeue(out activity);
        }


        /// <summary>
        /// Adds <see cref="IActivityInfo"/> to the end of the <see cref="ActivityMemoryQueue"/>.
        /// </summary>
        /// <param name="activity">The <see cref="IActivityInfo"/> to be added to the end of the <see cref="ActivityMemoryQueue"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="activity"/> is <c>null</c></exception>
        public void EnqueueActivity(IActivityInfo activity)
        {
            if (activity == null)
            {
                throw new ArgumentNullException("activity");
            }

            Queue.Enqueue(activity);
        }


        /// <summary>
        /// Enumerates all items in the queue of activities.
        /// </summary>
        /// <returns>Collection containing all the <see cref="IActivityInfo"/> from the queue</returns>
        public IEnumerable<IActivityInfo> GetQueuedActivities()
        {
            IActivityInfo current;

            while (TryDequeueActivity(out current))
            {
                yield return current;
            }
        }
    }
}