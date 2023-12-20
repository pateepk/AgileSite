using CMS.Core;

namespace CMS.Helpers
{
    /// <summary>
    /// Web farm task used to clear cache items.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class ClearCacheItemsWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Gets or sets prefix of all identifiers of caches to be flushed.
        /// </summary>
        public string KeyPrefix { get; set; }


        /// <summary>
        /// Get or sets a value that indicates whether cache prefix key is case sensitive.
        /// </summary>
        public bool IsCaseSensitive { get; set; }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="CacheHelper.ClearCache(string, bool, bool)"/> method while passing it current class' properties as its parameters.
        /// </summary>
        public override void ExecuteTask()
        {
            CacheHelper.ClearCache(KeyPrefix, IsCaseSensitive, false);
        }
    }
}
