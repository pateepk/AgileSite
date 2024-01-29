using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace CMS.DataEngine
{
    /// <summary>
    /// Cache storage based on <see cref="ConcurrentDictionary{TKey, TValue}"/> collection.
    /// </summary>
    internal sealed class ConcurrentDictionaryCacheStorage<TKey, TValue> : ICacheStorage<TKey, TValue>
    {
        private readonly ConcurrentDictionary<TKey, TValue> mCache;


        public long Count
        {
            get { return mCache.Count; }
        }


        public bool AllowNulls
        {
            get;
        }


        public ConcurrentDictionaryCacheStorage(IEqualityComparer<TKey> comparer, bool allowNulls)
        {
            AllowNulls = allowNulls;

            mCache = (comparer == null) ?
                new ConcurrentDictionary<TKey, TValue>() :
                new ConcurrentDictionary<TKey, TValue>(comparer);
        }


        public void Add(TKey key, TValue value)
        {
            mCache[key] = value;
        }


        public void Clear()
        {
            mCache.Clear();
        }


        public bool ContainsKey(TKey key)
        {
            return mCache.ContainsKey(key);
        }


        public void Remove(TKey key)
        {
            TValue value;
            mCache.TryRemove(key, out value);
        }


        public bool TryGet(TKey key, out TValue value)
        {
            var found = mCache.TryGetValue(key, out value);

            if (value == null && !AllowNulls)
            {
                found = false;
            }

            // Convert DBNull to default value
            if (AllowNulls && (value is DBNull))
            {
                value = default(TValue);
            }

            return found;
        }
    }
}
