namespace CMS.Helpers
{
    /// <summary>
    /// Cache item removed event arguments
    /// </summary>
    /// <remarks>
    /// This API supports the framework infrastructure and is not intended to be used directly from your code.
    /// </remarks>
    /// <exclude />
    public sealed class CacheItemRemovedEventArgs
    {
        /// <summary>
        /// Cache key.
        /// </summary>
        public string Key { get; }


        /// <summary>
        /// Cached object.
        /// </summary>
        public object Value { get; }


        /// <summary>
        /// Specifies the reason an item was removed from the cache.
        /// </summary>
        public CMSCacheItemRemovedReason Reason { get; }


        /// <summary>
        /// Creates new instance of <see cref="CacheItemRemovedEventArgs"/>.
        /// </summary>
        /// <param name="key">Cache key.</param>
        /// <param name="value">Cached object.</param>
        /// <param name="reason">Specifies the reason an item was removed from the cache.</param>
        public CacheItemRemovedEventArgs(string key, object value, CMSCacheItemRemovedReason reason)
        {
            Key = key;
            Value = value;
            Reason = reason;
        }
    }
}
