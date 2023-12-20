using System;
using System.Threading;

using CMS.Base;

namespace CMS.Helpers
{
    /// <summary>
    /// Represents cache protected against value tampering.
    /// The cache always has <see cref="CMSCacheItemPriority.NotRemovable"/>.
    /// </summary>
    /// <typeparam name="T">Type of the cached value.</typeparam>
    internal class ProtectedCache<T> : ProtectedCache
    {
        private T cachedValue;

        private readonly string cacheKey;
        private readonly double expirationMinutes;


        /// <summary>
        /// Creates instance of <see cref="ProtectedCache{T}"/> for value of type T.
        /// </summary>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="expirationMinutes">Cache expiration time in minutes.</param>
        public ProtectedCache(string cacheKey, double expirationMinutes)
        {
            this.cacheKey = cacheKey;
            this.expirationMinutes = expirationMinutes;
        }


        /// <summary>
        /// Gets value from cache or loads it, if the cache is invalid.
        /// </summary>
        /// <param name="loadFunc">Function, which loads value to be cached.</param>
        public T GetValue(Func<CacheSettings, T> loadFunc)
        {
            CacheHelper.Cache(cs => SetPrivateCache(loadFunc, cs), new CacheSettings(expirationMinutes, cacheKey)
            {
                CacheItemPriority = CMSCacheItemPriority.NotRemovable
            });

            Thread.MemoryBarrier();

            return cachedValue;
        }


        /// <summary>
        /// Sets value into private memory cache.
        /// </summary>
        /// <param name="loadFunc">Function, which loads value to be cached.</param>
        /// <param name="cacheSettings">Cache settings.</param>
        /// <remarks>Return value is intended to be used, just to satisfy standard cache method delegate.</remarks>
        private T SetPrivateCache(Func<CacheSettings, T> loadFunc, CacheSettings cacheSettings)
        {
            cachedValue = loadFunc(cacheSettings);

            // In development mode, let propagate protected cache value to the cache (by returning the private cached value), 
            // so it is visible in Cache items, which makes cache debugging easier.
            // In standard mode, return just default value of T to not expose sensitive information.
            return SystemContext.DevelopmentMode ? cachedValue : default(T);
        }
    }
}
