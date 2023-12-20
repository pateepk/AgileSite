using CMS.Core;

namespace CMS.PortalEngine.Web.UI
{
    /// <summary>
    /// Web farm task used to clear cached partial cache items.
    /// </summary>
    internal class ClearPartialCacheItemsWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Clears cached partial cache items.
        /// </summary>
        public override void ExecuteTask()
        {
            PartialCacheItemsProvider.ClearEnabledCacheItemsCache(false);
        }
    }
}
