using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Represents required cache storage members.
    /// </summary>
    internal interface ICacheStorage<in TKey, TValue>
    {
        /// <summary>
        /// Gets the number of items in cache.
        /// </summary>
        long Count { get; }


        /// <summary>
        /// Add or update value in cache under specified key.
        /// </summary>
        void Add(TKey key, TValue value);


        /// <summary>
        /// Removes cache item for specified key.
        /// </summary>
        void Remove(TKey key);


        /// <summary>
        /// Gets the cache item associated with specified <paramref name="key"/>.
        /// </summary>
        /// <returns><c>true</c> if item was found; otherwise <c>false</c>.</returns>
        bool TryGet(TKey key, out TValue value);


        /// <summary>
        /// Clears all items in cache storage.
        /// </summary>
        void Clear();


        /// <summary>
        /// Indicates whether key is available in cache storage.
        /// </summary>
        bool ContainsKey(TKey key);
    };
}
