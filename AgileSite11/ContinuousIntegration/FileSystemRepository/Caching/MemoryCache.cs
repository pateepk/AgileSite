using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.ContinuousIntegration
{
    /// <summary>
    /// Provides in-memory cache for storing <typeparamref name="TItem"/> objects based on a <see cref="string"/> key.
    /// </summary>
    /// <typeparam name="TItem">Type of objects that will be stored in the cache.</typeparam>
    internal sealed class MemoryCache<TItem>
    {
        #region "Internal classes and variables"

        /// <summary>
        /// Internal class holding cached data and their status.
        /// </summary>
        private class CacheItem<T>
        {
            /// <summary>
            /// Indicates the item has changed (externally set flag).
            /// </summary>
            public bool IsDirty
            {
                get;
                set;
            }


            /// <summary>
            /// Content stored in cache under specific key.
            /// </summary>
            public T Data
            {
                get;
                private set;
            }


            /// <summary>
            /// Creates new instance of the <see cref="CacheItem{T}"/>.
            /// </summary>
            public CacheItem(T data)
            {
                Data = data;
            }
        }


        private Dictionary<string, CacheItem<TItem>> mCachedItems;


        /// <summary>
        /// Dictionary of cached items (index by their string keys).
        /// </summary>
        private Dictionary<string, CacheItem<TItem>> CachedItems
        {
            get
            {
                return mCachedItems ?? (mCachedItems = new Dictionary<string, CacheItem<TItem>>(StringComparer.InvariantCultureIgnoreCase));
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Marks cached document for given <paramref name="key"/> with a flag that it has been modified specially processed eventually.
        /// </summary>
        /// <param name="key"><see cref="String"/> key identifying the cached data.</param>
        /// <remarks>Non-existing keys are silently skipped.</remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is <see langword="null"/>.</exception>
        public void MarkDirty(string key)
        {
            CheckKey(key);

            CacheItem<TItem> cachedItem;
            if (CachedItems.TryGetValue(key, out cachedItem))
            {
                cachedItem.IsDirty = true;
            }
        }


        /// <summary>
        /// Stores given <paramref name="item"/> in the cache under provided <paramref name="key"/>.
        /// Eventually sets <paramref name="item"/>'s <see cref="CacheItem{TItem}.IsDirty"/> flag.
        /// </summary>
        /// <param name="key"><see cref="String"/> key identifying the cached data.</param>
        /// <param name="item">Data to cache.</param>
        /// <param name="markAsDirty">Data purity flag</param>
        /// <remarks>If <paramref name="item"/> resents <see langword="null"/>, <see langword="null"/> is cached for the <paramref name="key"/>.</remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is <see langword="null"/>.</exception>
        public void SetItem(string key, TItem item, bool markAsDirty = false)
        {
            CheckKey(key);

            CachedItems[key] = new CacheItem<TItem>(item)
            {
                IsDirty = markAsDirty
            };
        }


        /// <summary>
        /// Reads data cached under the provided <paramref name="key"/> (if present)
        /// or executes provided <paramref name="loadFunction"/>, caching its result and returning obtained value.
        /// In addition, method eventually sets <paramref name="key"/>'s <see cref="CacheItem{TItem}.IsDirty"/> flag for newly cached data.
        /// </summary>
        /// <param name="key"><see cref="String"/> key identifying the cached data.</param>
        /// <param name="loadFunction">Delegate executed if cache item is not hit.</param>
        /// <param name="markLoadedItemDirty">Newly loaded data purity flag</param>
        /// <remarks>If <paramref name="loadFunction"/> returns <see langword="null"/>, <see langword="null"/> is cached for the <paramref name="key"/>.</remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is <see langword="null"/>.</exception>
        public TItem FetchItem(string key, Func<TItem> loadFunction = null, bool markLoadedItemDirty = false)
        {
            CheckKey(key);

            if (CachedItems.ContainsKey(key))
            {
                // Cache hit, return existing data
                return CachedItems[key].Data;
            }

            if (loadFunction == null)
            {
                // no load function provided, return default value (null)
                return default(TItem);
            }

            // Load new item's data through load function and store them in cache before returning
            var item = loadFunction();
            SetItem(key, item, markLoadedItemDirty);

            return item;
        }


        /// <summary>
        /// Returns collection of keys and data stored in the cached.
        /// </summary>
        /// <param name="dirtyOnly">If <see langword="true"/>, only items marked as <see cref="CacheItem{TItem}.IsDirty"/> are returned, all items are returned otherwise.</param>
        public IEnumerable<KeyValuePair<string, TItem>> GetItems(bool dirtyOnly = false)
        {
            var returnedItems = CachedItems.AsEnumerable();
            if (dirtyOnly)
            {
                returnedItems = returnedItems.Where(x => x.Value.IsDirty);
            }

            return returnedItems.Select(x => new KeyValuePair<string, TItem>(x.Key, x.Value.Data));
        }


        /// <summary>
        /// Removes cached data stored under <paramref name="key"/>.
        /// </summary>
        /// <param name="key"><see cref="String"/> key identifying the cached data.</param>
        /// <returns>Stored data or default value of <typeparamref name="TItem"/> (e.g. null) if no item stored under the <paramref name="key"/>.</returns>
        /// <remarks>Stored data might equal to default value of <typeparamref name="TItem"/>.</remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is <see langword="null"/>.</exception>
        public TItem RemoveItem(string key)
        {
            CheckKey(key);

            CacheItem<TItem> cachedItem;
            if (!CachedItems.TryGetValue(key, out cachedItem))
            {
                // Key's data not cached, return default value
                return default(TItem);
            }

            // Remove item from cache and return the data
            CachedItems.Remove(key);
            return cachedItem.Data;
        }


        /// <summary>
        /// Clears whole cache.
        /// </summary>
        public void Clear()
        {
            CachedItems.Clear();
        }

        #endregion


        #region "Helper methods"

        /// <summary>
        /// Throws an exception if provided <paramref name="key"/> is not suitable cache key.
        /// </summary>
        /// <param name="key"><see cref="String"/> key identifying the cached data.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="key"/> is <see langword="null"/>.</exception>
        private static void CheckKey(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
        }

        #endregion
    }
}
