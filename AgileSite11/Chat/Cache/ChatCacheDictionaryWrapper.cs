using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Helpers;

namespace CMS.Chat
{
    /// <summary>
    /// This class stores cached objects in dictionary and allows to get/invalidate them individually.
    /// </summary>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <typeparam name="TValue">Value type</typeparam>
    public class ChatCacheDictionaryWrapper<TKey, TValue>
    {
        #region "Private fields"

        private Dictionary<TKey, ChatCacheWrapper<TValue>> innerCache = new Dictionary<TKey,ChatCacheWrapper<TValue>>();
        private string key;
        private string allKeysDependency;
        private Func<TKey, TValue> fetchDataFunc;
        private TimeSpan? persistence;
        private TimeSpan? slidingExpiration;

        private object syncObject = new object();

        #endregion


        #region "Constructor"

        /// <summary>
        /// Constructs cache dictionary wrapper.
        /// </summary>
        /// <param name="uniqueKey">Unique key for caching</param>
        /// <param name="fetchDataFunc">Function used to get one item by its key</param>
        /// <param name="cachePersistence">Item in cache will stay in cache for the amount of time specified by this parameter. Then it will be renewed by calling fetchDataFunc. (not used if null).</param>
        /// <param name="slidingExpiration">Item will stay in cache as long as it is accessed once in that time. (not used if null)</param>
        public ChatCacheDictionaryWrapper(string uniqueKey, Func<TKey, TValue> fetchDataFunc, TimeSpan? cachePersistence, TimeSpan? slidingExpiration)
        {
            this.fetchDataFunc = fetchDataFunc;
            persistence = cachePersistence;
            this.slidingExpiration = slidingExpiration;
            key = uniqueKey;
            allKeysDependency = key + "|allKeys";
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Invalidates one item.
        /// </summary>
        /// <param name="id">Key</param>
        public void InvalidateItem(TKey id)
        {
            GetCacheWrapper(id).Invalidate();
        }


        /// <summary>
        /// Invalidates all items.
        /// </summary>
        public void InvalidateAll()
        {
            lock (syncObject)
            {
                CacheHelper.TouchKey(allKeysDependency);

                innerCache.Clear();
            }
        }


        /// <summary>
        /// Gets item from cache.
        /// </summary>
        /// <param name="id">Key</param>
        public TValue GetItem(TKey id)
        {
            return GetCacheWrapper(id).Data;
        }

        #endregion


        #region "Private methods"

        private ChatCacheWrapper<TValue> GetCacheWrapper(TKey id)
        {
            ChatCacheWrapper<TValue> wrapper;

            if (innerCache.TryGetValue(id, out wrapper))
            {
                return wrapper;
            }
            lock (syncObject)
            {
                // Recheck
                if (innerCache.TryGetValue(id, out wrapper))
                {
                    return wrapper;
                }

                wrapper = new ChatCacheWrapper<TValue>(string.Format("{0}|{1}", key, id), () => fetchDataFunc(id), persistence, slidingExpiration);
                wrapper.AddDependency(allKeysDependency);

                innerCache[id] = wrapper;

                return wrapper;
            }
        }

        #endregion
    }
}
