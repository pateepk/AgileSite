using CMS.Core;

namespace CMS.Helpers
{
    /// <summary>
    /// Web farm task used to touch cache items.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class TouchCacheItemWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Gets or sets collection of cache keys which will be touched.
        /// </summary>
        public string[] Keys { get; set; }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="CacheHelper.TouchKey(string, bool, bool)"/> method while passing it current class' properties as its parameters.
        /// </summary>
        public override void ExecuteTask()
        {
            CacheHelper.TouchKeys(Keys, false, false);
        }
    }
}
