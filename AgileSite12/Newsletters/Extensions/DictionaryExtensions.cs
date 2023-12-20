using System;
using System.Collections.Generic;

namespace CMS.Newsletters.Extensions
{
    /// <summary>
    /// Extension methods for a generic dictionary.
    /// </summary>
    internal static class DictionaryExtensions
    {
        #region "Methods"

        /// <summary>
        /// Performs the specified action on each element of the dictionary.
        /// </summary>
        /// <typeparam name="TKey">The type of the key</typeparam>
        /// <typeparam name="TValue">The type of the value</typeparam>
        /// <param name="dictionary">The dictionary</param>
        /// <param name="action">The action</param>
        public static void ForEach<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Action<TValue> action)
        {
            foreach (var value in dictionary.Values)
            {
                action(value);
            }
        }

        #endregion
    }
}