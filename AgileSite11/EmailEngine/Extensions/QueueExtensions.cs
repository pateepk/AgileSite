using System;
using System.Collections;
using System.Collections.Generic;

namespace CMS.EmailEngine.Extensions
{
    /// <summary>
    /// Extension methods for a generic queue.
    /// </summary>
    internal static class QueueExtensions
    {
        #region "Methods"

        /// <summary>
        /// Adds an object to the end of the queue. Thread-safe.
        /// </summary>
        /// <typeparam name="T">Type of the item in the queue</typeparam>
        /// <param name="queue">The queue</param>
        /// <param name="item">The item</param>
        public static void SynchronizedEnqueue<T>(this Queue<T> queue, T item)
        {
            ICollection syncQueue = queue;
            lock (syncQueue.SyncRoot)
            {
                queue.Enqueue(item);
            }
        }


        /// <summary>
        /// Removes and returns the object at the beginning of the queue. Thread-safe.
        /// </summary>
        /// <typeparam name="T">Type of the item in the queue</typeparam>
        /// <param name="queue">The queue</param>
        /// <returns>Item at the beginning of the queue</returns>
        public static T SynchronizedDequeue<T>(this Queue<T> queue)
        {
            ICollection syncQueue = queue;
            lock (syncQueue.SyncRoot)
            {
                return queue.Count > 0 ? queue.Dequeue() : default(T);
            }
        }

        #endregion
    }
}