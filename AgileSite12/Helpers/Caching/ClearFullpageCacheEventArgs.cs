using CMS.Base;

namespace CMS.Helpers
{
    /// <summary>
    /// <see cref="CacheEvents.ClearFullPageCache"/> event arguments.
    /// </summary>
    /// <remarks>
    /// This API supports the framework infrastructure and is not intended to be used directly from your code.
    /// </remarks>
    /// <exclude />
    public sealed class ClearFullpageCacheEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Indicates whether web farm tasks should be logged.
        /// </summary>
        public bool LogTask { get; }


        /// <summary>
        /// Creates new instance of <see cref="ClearFullpageCacheEventArgs"/>.
        /// </summary>
        /// <param name="logTask">Indicates whether web farm tasks should be logged.</param>
        public ClearFullpageCacheEventArgs(bool logTask)
        {
            LogTask = logTask;
        }
    }
}
