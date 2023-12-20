using CMS.Base;

namespace CMS.Helpers
{
    /// <summary>
    /// Cache events.
    /// </summary>
    /// <remarks>
    /// This API supports the framework infrastructure and is not intended to be used directly from your code.
    /// </remarks>
    /// <exclude />
    public static class CacheEvents
    {
        /// <summary>
        /// Fires when the cache item is removed from cache.
        /// </summary>
        public static readonly SimpleHandler<CacheItemRemovedEventArgs> CacheItemRemoved = new SimpleHandler<CacheItemRemovedEventArgs> { Name = "CacheEvents.CacheItemRemoved" };


        /// <summary>
        /// Fires when full page cache clear is requested.
        /// </summary>
        public static readonly SimpleHandler<ClearFullpageCacheEventArgs> ClearFullPageCache = new SimpleHandler<ClearFullpageCacheEventArgs> { Name = "CacheEvents.ClearFullPageCache" };
    }
}
