using CMS.Core;

namespace CMS.Helpers
{
    /// <summary>
    /// Web farm synchronization for cache.
    /// </summary>
    internal class CacheTasks
    {
        /// <summary>
        /// Initializes the tasks.
        /// </summary>
        public static void Init()
        {
            WebFarmHelper.RegisterTask<TouchCacheItemWebFarmTask>(true, WebFarmTaskOptimizeActionEnum.GroupData);
            WebFarmHelper.RegisterTask<RemoveCacheItemWebFarmTask>(true);
            WebFarmHelper.RegisterTask<ClearCacheItemsWebFarmTask>(true);
            WebFarmHelper.RegisterTask<ClearFullPageCacheWebFarmTask>();
            WebFarmHelper.RegisterTask<RemovePersistentStorageKeyWebFarmTask>();
        }
    }
}
