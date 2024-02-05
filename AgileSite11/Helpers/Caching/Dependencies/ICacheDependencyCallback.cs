namespace CMS.Helpers
{
    /// <summary>
    /// Interface which allows the object to execute a PerformCallback method when the object is removed from the cache if the cache dependency has changed
    /// </summary>
    public interface ICacheDependencyCallback
    {
        /// <summary>
        /// Executes the callback to the target object
        /// </summary>
        void PerformCallback();
    }
}
