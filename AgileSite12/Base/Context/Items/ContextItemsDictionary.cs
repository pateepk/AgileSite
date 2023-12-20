using System;
using System.Collections;
using System.Linq;

namespace CMS.Base
{
    /// <summary>
    /// Context items dictionary
    /// </summary>
    internal class ContextItemsDictionary<TParent, TValue> : SafeDictionary<object, TValue>
        where TParent : ContextItemsDictionary<TParent, TValue>, new()
    {
        /// <summary>
        /// Copies the items dictionary for a new thread
        /// </summary>
        public TParent Copy()
        {
            return Copy(this);
        }


        /// <summary>
        /// Copies the items dictionary specified as a parameter for a new thread.
        /// </summary>
        /// <param name="source">Items source</param>
        /// <returns>Copy of the <paramref name="source"/>.</returns>
        public static TParent Copy(IDictionary source)
        {
            lock (source)
            {
                return CopyInternal(source);
            }
        }


        /// <summary>
        /// Performs a copy of items from source dictionary to target context items dictionary.
        /// </summary>
        /// <param name="itemsSource">Items source.</param>
        /// <returns>Copy of the <paramref name="itemsSource"/>.</returns>
        private static TParent CopyInternal(IDictionary itemsSource)
        {
            TParent itemsTarget = new TParent();

            // Copy all the items
            var oldItems = itemsSource.Cast<DictionaryEntry>().ToList();

            foreach (var item in oldItems)
            {
                // Ignore classes marked with not copy or lazy to prevent unexpected initialization from another thread
                object value = item.Value;
                if (value is INotCopyThreadItem)
                {
                    continue;
                }

                // Cloned thread value
                var clone = value as ICloneThreadItem;
                if (clone != null)
                {
                    // Clone for new thread
                    value = clone.CloneForNewThread();
                    if (value != null)
                    {
                        itemsTarget[item.Key] = (TValue)value;
                    }
                    continue;
                }

                string key = item.Key.ToString().ToLowerCSafe();

                // Do not assign some default items that could harm the thread
                switch (key)
                {
                    // Logs
                    case "cmswebfarmupdate":
                        break;

                    default:
                        if (key.StartsWithCSafe("selectnodes|") ||
                            key.StartsWithCSafe("selectsinglenode|"))
                        {
                            // Do not copy over the special keys
                        }
                        else
                        {
                            itemsTarget[item.Key] = (TValue)item.Value;
                        }
                        break;
                }
            }

            return itemsTarget;
        }
    }
}