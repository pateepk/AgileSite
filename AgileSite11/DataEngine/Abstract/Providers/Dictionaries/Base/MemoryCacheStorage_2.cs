using System;
using System.Runtime.Caching;

namespace CMS.DataEngine
{
    /// <summary>
    /// Cache storage based on <see cref="MemoryCache"/>. 
    /// </summary>
    internal sealed class MemoryCacheStorage<TKey, TValue> : ICacheStorage<TKey, TValue>
    {
        private MemoryCache mCache;

        // Indicates whether current TKey is string.
        private bool IsString { get; }
            = (typeof(TKey) == typeof(string));


        public long Count
        {
            get { return mCache.GetCount(); }
        }


        public bool AllowNulls
        {
            get;
        }


        public MemoryCacheStorage(string name, bool allowNulls)
        {
            mCache = new MemoryCache(name);
            AllowNulls = allowNulls;
        }


        public void Add(TKey key, TValue value)
        {
            var cacheItem = (value == null) ?
                new CacheItem(NormalizeCacheKey(key), MemoryCacheStorageConstants.NullReference) :
                new CacheItem(NormalizeCacheKey(key), value);

            mCache.Set(cacheItem, MemoryCacheStorageConstants.DefaultCachePolicy);
        }


        public void Clear()
        {
            var oldCache = mCache;
            mCache = new MemoryCache(oldCache.Name);

            // The oldCache cannot be disposed immediately as pending Add/Get/Set operations might still have reference to the oldCache
            // Release items and let the GC reclaim the oldCache object
            oldCache.Trim(100);
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
    }
}
