using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;

namespace CMS.Helpers
{
    /// <summary>
    /// List of the cache dependencies.
    /// </summary>
    public class CacheDependencyList
    {
        #region "Variables"

        /// <summary>
        /// Dependency lists.
        /// </summary>
        protected static SafeDictionary<string, CacheDependencyList> mLists = new SafeDictionary<string, CacheDependencyList>();

        /// <summary>
        /// Dependencies within current list.
        /// </summary>
        protected HashSet<string> mDependencies = new HashSet<string>();

        #endregion


        #region "Methods"

        /// <summary>
        /// Clears the cached lists of the page cache dependencies.
        /// </summary>
        public static void ClearLists()
        {
            mLists.Clear();
        }


        /// <summary>
        /// Clears the dependency list.
        /// </summary>
        public void Clear()
        {
            mDependencies.Clear();
        }


        /// <summary>
        /// Adds the dependency keys to the collection.
        /// </summary>
        /// <param name="keys">Keys to add</param>
        public void Add(params string[] keys)
        {
            foreach (string key in keys)
            {
                Add(key);
            }
        }


        /// <summary>
        /// Adds the dependency key to the collection.
        /// </summary>
        /// <param name="key">Key to add</param>
        public void Add(string key)
        {
            if (key != null)
            {
                mDependencies.Add(key);
            }
        }


        /// <summary>
        /// Removes the dependency key from the collection.
        /// </summary>
        /// <param name="key">Key to add</param>
        public void Remove(string key)
        {
            mDependencies.Remove(key);
        }


        /// <summary>
        /// Gets the cache dependency based on the list.
        /// </summary>
        public CMSCacheDependency GetCacheDependency()
        {
            // If no dependencies found, do not return anything
            int count = mDependencies.Count;
            if (count == 0)
            {
                return null;
            }

            // Create the dependency
            var cd = CacheHelper.GetCacheDependency(null, mDependencies.ToArray());

            return cd;
        }


        /// <summary>
        /// Ensures list of cache dependencies with specific key.
        /// </summary>
        /// <param name="listKey">List key</param>
        public static CacheDependencyList EnsureList(string listKey)
        {
            // Try to get the list from cache
            var list = mLists[listKey];
            if (list == null)
            {
                list = new CacheDependencyList();
                mLists[listKey] = list;
            }

            return list;
        }

        #endregion
    }
}