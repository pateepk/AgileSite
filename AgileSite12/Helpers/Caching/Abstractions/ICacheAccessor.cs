using System;
using System.Collections;

namespace CMS.Helpers.Caching.Abstractions
{
    /// <summary>
    /// Represents a cache with reading, inserting and removing support.
    /// </summary>
    public interface ICacheAccessor
    {
        /// <summary>
        /// Inserts an object into the cache.
        /// </summary>
        /// <param name="key">The cache key used to reference the object.</param>
        /// <param name="value">The object to be inserted in the cache.</param>
        /// <param name="dependencies">The file or cache key dependencies for the item. When any dependency changes, the object becomes invalid and is removed from the cache.</param>
        /// <param name="absoluteExpiration">The time at which the inserted object expires and is removed from the cache. </param>
        /// <param name="slidingExpiration">The interval between the time the inserted object was last accessed and the time at which that object expires.</param>
        /// <param name="priority">The cost of the object relative to other items stored in the cache.</param>
        /// <param name="onRemoveCallback">A delegate that, if provided, will be called when an object is removed from the cache.</param>
        void Insert(string key, object value, CMSCacheDependency dependencies, DateTime absoluteExpiration, TimeSpan slidingExpiration, CMSCacheItemPriority priority, CMSCacheItemRemovedCallback onRemoveCallback);


        /// <summary>
        /// Retrieves the specified item from the cache.
        /// </summary>
        /// <param name="key">The identifier for the cache item to retrieve.</param>
        object Get(string key);


        /// <summary>
        /// Removes the specified item from the cache.
        /// </summary>
        /// <param name="key">The identifier for the cache item to remove.</param>
        object Remove(string key);


        /// <summary>
        /// <summary>Retrieves a dictionary enumerator used to iterate through the key settings and their values contained in the cache.</summary>
        /// </summary>
        IDictionaryEnumerator GetEnumerator();
    }
}
