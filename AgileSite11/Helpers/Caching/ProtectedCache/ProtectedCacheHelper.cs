using System;
using System.Collections.Concurrent;

namespace CMS.Helpers
{
    /// <summary>
    /// Manages instances of <see cref="ProtectedCache"/>.
    /// </summary>
    internal static class ProtectedCacheHelper
    {
        private static readonly ConcurrentDictionary<string, ProtectedCache> Caches = new ConcurrentDictionary<string, ProtectedCache>(StringComparer.OrdinalIgnoreCase);


        /// <summary>
        /// Gets value from cache or loads it using load function of <see cref="ProtectedCache{T}"/>.
        /// </summary>
        /// <param name="loadFunc">Function, which loads value to be cached.</param>
        /// <param name="cacheKey">Cache key.</param>
        /// <param name="expirationMinutes">Cache expiration time in minutes.</param>
        /// <typeparam name="T">Type of the cached value.</typeparam>
        public static T Cache<T>(Func<CacheSettings, T> loadFunc, double expirationMinutes, string cacheKey)
        {
            var cache = Caches.GetOrAdd(cacheKey, key => new ProtectedCache<T>(key, expirationMinutes)) as ProtectedCache<T>;
            if (cache == null)
            {
                throw new ArgumentException($"The cache key '{cacheKey}' is not valid in context of cached value type '{typeof(T).FullName}'.");
            }
            
            return cache.GetValue(loadFunc);
        }
    }
}