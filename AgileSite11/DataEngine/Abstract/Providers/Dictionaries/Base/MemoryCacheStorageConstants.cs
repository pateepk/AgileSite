using System.Runtime.Caching;

namespace CMS.DataEngine
{
    /// <summary>
    /// Class containing constants used by <see cref="MemoryCacheStorage{TKey, TValue}"/>.
    /// </summary>
    internal static class MemoryCacheStorageConstants
    {
        public static readonly CacheItemPolicy DefaultCachePolicy = new CacheItemPolicy { Priority = CacheItemPriority.Default };
        public static readonly object NullReference = new object();
    }
}
