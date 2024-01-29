using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Caching;

using CMS.Base;
using CMS.Helpers;

namespace CMS.Chat
{
    /// <summary>
    /// This class wraps cached data and handles loading on background.
    /// 
    /// It is designed to be used as a static field.
    /// </summary>
    /// <typeparam name="TData">Type of cached data</typeparam>
    public class ChatCacheWrapper<TData>
    {
        #region "Private fields"

        private readonly Func<TData> fetchFunc;
        private readonly string key;
        private readonly string dummyKey;
        private TimeSpan? persistence;
        private readonly TimeSpan? slidingExpiration;
        private readonly List<string> dependencies = new List<string>();

        #endregion


        #region "Constructor"

        /// <summary>
        /// Constructs CacheWrapper. 
        /// </summary>
        /// <param name="uniqueKey">Unique key of this cache item</param>
        /// <param name="fetchDataFunc">Function which gets data</param>
        /// <param name="cachePersistence">Item in cache will stay in cache for the amount of time specified by this parameter. Then it will be renewed by calling fetchDataFunc. (not used if null).</param>
        /// <param name="slidingExpiration">Item will stay in cache as long as it is accessed once in that time. (not used if null)</param>
        public ChatCacheWrapper(string uniqueKey, Func<TData> fetchDataFunc, TimeSpan? cachePersistence, TimeSpan? slidingExpiration)
        {
            fetchFunc = fetchDataFunc;
            key = uniqueKey;
            persistence = cachePersistence;
            this.slidingExpiration = slidingExpiration;

            dummyKey = key + "|(dummy)";

            dependencies.Add(dummyKey);
        }

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets data from cache. Loads it using fetchDataFunc if needed.
        /// </summary>
        public TData Data
        {
            get
            {
                // Not using CachedSection here, because it does not support sliding expiration.

                // Prepare data for returning
                TData data;

                // Try to get item from cache
                if (!CacheHelper.TryGetItem(key, out data))
                {
                    // Data was not found - lets get them and save them in cache
                    object lockObject = LockHelper.GetLockObject(key);

                    lock (lockObject)
                    {
                        // Check cache one more time inside lock
                        if (!CacheHelper.TryGetItem(key, out data))
                        {
                            // Date were not found - get data from function
                            data = fetchFunc();

                            DateTime absoluteExpiration = persistence != null ? DateTime.Now.Add(persistence.Value) : Cache.NoAbsoluteExpiration;
                            TimeSpan slidingExp = slidingExpiration ?? Cache.NoSlidingExpiration;

                            // Add data to cache
                            CacheHelper.Add(key, data, CacheHelper.GetCacheDependency(dependencies), absoluteExpiration, slidingExp);
                        }
                    }
                }

                return data;
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// This cache item will be dependent on this cache key.
        /// </summary>
        /// <param name="dependencyName">Dependency name</param>
        public void AddDependency(string dependencyName)
        {
            dependencies.Add(dependencyName);
        }


        /// <summary>
        /// Removes data from cache.
        /// </summary>
        public void Invalidate()
        {
            CacheHelper.TouchKey(dummyKey);
        }

        #endregion
    }
}
