using System;
using System.Collections;
using System.Collections.Generic;

namespace CMS.EmailEngine.Extensions
{
    /// <summary>
    /// Extension methods for the generic list.
    /// </summary>
    internal static class ListExtensions
    {
        #region "Methods"

        /// <summary>
        /// Adds an object to the end of the list. Thread-safe.
        /// </summary>
        /// <typeparam name="T">Type of the item in the list</typeparam>
        /// <param name="list">The list</param>
        /// <param name="item">The item</param>
        public static void SynchronizedAdd<T>(this List<T> list, T item)
        {
            ICollection syncList = list;
            lock (syncList)
            {
                list.Add(item);
            }
        }


        /// <summary>
        /// Removes the first occurrence of the object from the list. Thread-safe.
        /// </summary>
        /// <typeparam name="T">Type of the item in the list</typeparam>
        /// <param name="list">The list</param>
        /// <param name="item">The item</param>
        /// <returns>
        /// <c>true</c> if item is successfully removed, otherwise, <c>false</c>. 
        /// This method also returns <c>false</c> if item was not found in the list.
        /// </returns>
        public static bool SynchronizedRemove<T>(this List<T> list, T item)
        {
            ICollection syncList = list;
            lock (syncList)
            {
                return list.Remove(item);
            }
        }

        #endregion
    }
}