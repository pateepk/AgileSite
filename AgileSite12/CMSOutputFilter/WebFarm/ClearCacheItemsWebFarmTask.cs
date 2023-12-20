using CMS.Core;

namespace CMS.OutputFilter
{
    /// <summary>
    /// Web farm task used to clear <see cref="OutputHelper.CacheItems"/>.
    /// </summary>
    internal class ClearCacheItemsWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Clears <see cref="OutputHelper.CacheItems"/>.
        /// </summary>
        public override void ExecuteTask()
        {
            OutputHelper.ClearCacheItems(false);
        }
    }
}
