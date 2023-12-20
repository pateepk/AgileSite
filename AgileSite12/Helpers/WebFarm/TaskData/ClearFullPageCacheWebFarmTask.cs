using CMS.Core;

namespace CMS.Helpers
{
    /// <summary>
    /// Web farm task used to clear full page caches.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class ClearFullPageCacheWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Processes the web farm task by invoking the <see cref="CacheHelper.ClearFullPageCache(bool)"/> method.
        /// </summary>
        public override void ExecuteTask()
        {
            CacheHelper.ClearFullPageCache(false);
        }
    }
}
