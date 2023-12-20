using System;
using System.Runtime.Caching;

namespace CMS.DataEngine
{
    /// <summary>
    /// Cache storage based on <see cref="MemoryCache"/>. 
    /// </summary>
    internal sealed class MemoryCacheStorage<TKey, TValue> : ICacheStorage<TKey, TValue>
    {
        private readonly MemoryCache mCache;

        // Indicates whether current TKey is string.
        private bool IsString { get; }
            = (typeof(TKey) == typeof(string));
   

        public long Count
        {
            // Cache count  - clear cache item
            get { return mCache.GetCount() - 1L; }
        }


        public bool AllowNulls
        {
            get;
        }


        public MemoryCacheStorage(string name, bool allowNulls)
        {
            mCache = new MemoryCache(name);
            AllowNulls = allowNulls;

            ResetCacheEntryMonitorCacheItem();
        }


        public void Add(TKey key, TValue value)
        {
            var cacheItem = (value == null) ?
                new CacheItem(NormalizeCacheKey(key), MemoryCacheStorageConstants.NullReference) :
                new CacheItem(NormalizeCacheKey(key), value);

            mCache.Set(cacheItem, MemoryCacheItemPolicy.GetDefaultPolicyWithClearCacheMonitor(mCache));
        }


        public void Clear()
        {
            ResetCacheEntryMonitorCacheItem();
        }


        public void Remove(TKey key)
        {
            mCache.Remove(NormalizeCacheKey(key));
        }


        public bool TryGet(TKey key, out TValue value)
        {
            var cachedValue = mCache.Get(NormalizeCacheKey(key));
            var found = (cachedValue != null);

            if ((cachedValue == MemoryCacheStorageConstants.NullReference) || (cachedValue == DBNull.Value && AllowNulls))
            {
                if (!AllowNulls)
                {
                    found = false;
                }
                cachedValue = null;
            }

            // null cannot be returned for value type
            value = (cachedValue != null) ? (TValue)cachedValue : default(TValue);

            return found;
        }


        public bool ContainsKey(TKey key)
        {
            var result = mCache.Get(NormalizeCacheKey(key));
            return ((result != null) && (result != MemoryCacheStorageConstants.NullReference || AllowNulls));
        }


        /// <summary>
        /// Returns key value as string. Key is lowercased for string key type.
        /// </summary>
        private string NormalizeCacheKey(TKey key)
        {
            return IsString ?
                key.ToString().ToLowerInvariant() :
                key.ToString();
        }


        /// <summary>
        /// Initialize or Re-initialize cache entry cache monitor.
        /// </summary>
        private void ResetCacheEntryMonitorCacheItem()
        {
            mCache.Set(MemoryCacheItemPolicy.CLEAR_CACHE_ITEM, Guid.NewGuid(), new CacheItemPolicy { Priority = CacheItemPriority.NotRemovable });
        }
    }
}
