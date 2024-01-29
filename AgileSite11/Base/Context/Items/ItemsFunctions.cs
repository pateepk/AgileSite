using System;
using System.Collections;
using System.Collections.Generic;

namespace CMS.Base
{
    /// <summary>
    /// Items collections functions.
    /// </summary>
    public static class ItemsFunctions
    {
        /// <summary>
        /// Add item.
        /// </summary>
        /// <param name="items">Items collection</param>
        /// <param name="key">Key to add</param>
        /// <param name="value">Value to add</param>
        /// <param name="caseSensitive">If true, the key is case sensitive</param>
        public static void Add(IDictionary items, string key, object value, bool caseSensitive)
        {
            ActionInternal(items, key, caseSensitive, (i, k) => i[k] = value);
        }


        /// <summary>
        /// Remove item.
        /// </summary>
        /// <param name="items">Items collection</param>
        /// <param name="key">Object key</param>
        /// <param name="caseSensitive">If true, the key is case sensitive</param>
        public static void Remove(IDictionary items, string key, bool caseSensitive)
        {
            ActionInternal(items, key, caseSensitive, (i, k) => i[k] = null);
        }


        /// <summary>
        /// Check if stock contain item.
        /// </summary>
        /// <param name="items">Items collection</param>
        /// <param name="key">Object key</param>
        /// <param name="caseSensitive">If true, the key is case sensitive</param>
        public static bool Contains(IDictionary items, string key, bool caseSensitive)
        {
            return ActionInternal(items, key, caseSensitive, (i, k) => i[k] != null);
        }


        /// <summary>
        /// Returns object which matches by key.
        /// </summary>
        /// <param name="items">Items collection</param>
        /// <param name="key">Object key</param>
        /// <param name="caseSensitive">If true, the key is case sensitive</param>
        public static object GetItem(IDictionary items, string key, bool caseSensitive)
        {
            return ActionInternal(items, key, caseSensitive, (i, k) => i[k]);
        }


        /// <summary>
        /// Remove items which start with selected name.
        /// </summary>
        /// <param name="items">Items collection</param>
        /// <param name="startsWith">Starting string of the objects to be removed</param>
        public static void Clear(IDictionary items, string startsWith)
        {
            if ((items == null) || (startsWith == null))
            {
                return;
            }

            // Clear all items
            if (items.Count > 0)
            {
                startsWith = startsWith.ToLowerInvariant();

                List<string> keyList = new List<string>();
                IDictionaryEnumerator ItemEnum = items.GetEnumerator();

                // Build the items list
                while (ItemEnum.MoveNext())
                {
                    string itemKey = ItemEnum.Key.ToString();
                    string key = itemKey.ToLowerInvariant();
                    if (key.StartsWith(startsWith, StringComparison.CurrentCulture))
                    {
                        keyList.Add(itemKey);
                    }
                }

                // Remove the items
                foreach (string key in keyList)
                {
                    Remove(items, key, true);
                }
            }
        }


        private static T ActionInternal<T>(IDictionary items, string key, bool caseSensitive, Func<IDictionary, string, T> action)
        {
            if ((items == null) || (key == null))
            {
                return default(T);
            }

            // Add to current items
            if (!caseSensitive)
            {
                key = key.ToLowerInvariant();
            }

            return action(items, key);
        }
    }
}