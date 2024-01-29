namespace CMS.Helpers
{
    /// <summary>
    /// Web farm task types for Cache operations
    /// </summary>
    public class CacheTaskType
    {
        /// <summary>
        /// Touch the cache item.
        /// </summary>
        public const string TouchCacheItem = "TOUCHCACHEITEM";

        /// <summary>
        /// Clear all the system cached items.
        /// </summary>
        public const string ClearCacheItems = "CLEARCACHEITEMS";

        /// <summary>
        /// Remove system cached item.
        /// </summary>
        public const string RemoveCacheItem = "REMOVECACHEITEM";

        /// <summary>
        /// Clear full page cache
        /// </summary>
        public const string ClearFullPageCache = "CLEARFULLPAGECACHE";

        /// <summary>
        /// Clear full page cache
        /// </summary>
        public const string RemovePersistentStorageKey = "REMOVEPERSISTENTSTORAGEKEY";
    }
}
