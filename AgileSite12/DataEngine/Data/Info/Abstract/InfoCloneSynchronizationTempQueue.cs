using System;
using System.Collections.Generic;

namespace CMS.DataEngine
{
    /// <summary>
    /// Temporary queue with unique items (defined by key) for optimized synchronization logging within cloning process.
    /// </summary>
    /// <seealso cref="InfoCloneActionContext"/>
    internal sealed class InfoCloneSynchronizationTempQueue
    {
        private readonly List<Action> items = new List<Action>();
        private readonly HashSet<string> keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);


        /// <summary>
        /// Original queue method.
        /// </summary>
        internal Action<Action> OriginalQueueAction { get; }


        /// <summary>
        /// Collection of items to be queued.
        /// </summary>
        internal IEnumerable<Action> Items => items;


        /// <summary>
        /// Creates new instance of <see cref="InfoCloneSynchronizationTempQueue"/>.
        /// </summary>
        /// <param name="originalQueueAction">Original queue method.</param>
        public InfoCloneSynchronizationTempQueue(Action<Action> originalQueueAction)
        {
            OriginalQueueAction = originalQueueAction;
        }


        /// <summary>
        /// Adds item to the collection of items if not present yet.
        /// </summary>
        /// <param name="key">Item key</param>
        /// <param name="action">Item value</param>
        public void TryAdd(string key, Action action)
        {
            if (keys.Add(key))
            {
                items.Add(action);
            }
        }
    }
}
