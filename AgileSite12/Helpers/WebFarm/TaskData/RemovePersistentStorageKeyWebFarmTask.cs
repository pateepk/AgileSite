using CMS.Core;

namespace CMS.Helpers
{
    /// <summary>
    /// Web farm task used to remove key from internal cache.
    /// </summary>
    /// <seealso cref="IWebFarmTask.TaskTextData"/>
    public class RemovePersistentStorageKeyWebFarmTask : WebFarmTaskBase
    {
        /// <summary>
        /// Gets or sets cache identifier of the cache to be flushed.
        /// </summary>
        public string Key { get; set; }


        /// <summary>
        /// Processes the web farm task by invoking the <see cref="PersistentStorageHelper.RemoveKeyFromInternalCache(string)"/> method while passing it current class' properties as its parameters.
        /// </summary>
        public override void ExecuteTask()
        {
            PersistentStorageHelper.RemoveKeyFromInternalCache(Key);
        }
    }
}