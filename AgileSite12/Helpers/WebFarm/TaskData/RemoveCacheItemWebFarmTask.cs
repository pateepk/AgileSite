using CMS.Core;

namespace CMS.Helpers
{
    /// <summary>
    /// Web farm task used to clear cache.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class RemoveCacheItemWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Gets or sets the identifier of the cache to be flushed.
        /// </summary>
        public string Key { get; set; }


        /// <summary>
        /// Get or sets a value that indicates if cache key is case sensitive.
        /// </summary>
        public bool IsCaseSensitive { get; set; }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="CacheHelper.Remove(string, bool, bool, bool)"/> method while passing it current class' properties as its parameters.
        /// </summary>
        public override void ExecuteTask()
        {
            CacheHelper.Remove(Key, IsCaseSensitive, true, false);
        }
    }
}
