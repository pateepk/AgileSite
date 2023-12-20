using System;
using System.Runtime.Caching;

using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Helper methods and properties related to <see cref="CacheItemPolicy"/> used for <see cref="MemoryCacheStorage{TKey, TValue}"/>.
    /// </summary>
    internal static class MemoryCacheItemPolicy
    {
        internal const string CLEAR_CACHE_ITEM = "MemoryCacheStorage.ClearCacheKey";


        public static int AbsoluteCacheExpirationSeconds
        {
            get;
            set;
        } = SettingsHelper.AppSettings["CMSMemoryCacheStorageAbsoluteExpirationSeconds"].ToInteger(0);


        public static int SlidingCacheExpirationSeconds
        {
            get;
            set;
        } = SettingsHelper.AppSettings["CMSMemoryCacheStorageSlidingExpirationSeconds"].ToInteger(0);


        /// <summary>
        /// Returns <see cref="CacheItemPolicy"/> with <see cref="CacheItemPriority.Default"/> priority and cache entry cache monitor attached to <see cref="CLEAR_CACHE_ITEM"/> name.
        /// </summary>
        public static CacheItemPolicy GetDefaultPolicyWithClearCacheMonitor(MemoryCache cache)
        {
            var policy = new CacheItemPolicy { Priority = CacheItemPriority.Default };

            if (AbsoluteCacheExpirationSeconds > 0)
            {
                policy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(AbsoluteCacheExpirationSeconds);
            }
            else if (SlidingCacheExpirationSeconds > 0)
            {
                policy.SlidingExpiration = TimeSpan.FromSeconds(SlidingCacheExpirationSeconds);
            }

            policy.ChangeMonitors.Add(cache.CreateCacheEntryChangeMonitor(new[] { CLEAR_CACHE_ITEM }));
            return policy;
        }
    }
}
