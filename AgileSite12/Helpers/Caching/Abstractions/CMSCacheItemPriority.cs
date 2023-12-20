namespace CMS.Helpers
{
    /// <summary>
    /// Specifies the relative priority of items stored in the cache.
    /// </summary>
    public enum CMSCacheItemPriority
    {
        /// <summary>
        /// Cache items with this priority level are the most likely to be deleted from the cache as the server frees system memory.
        /// </summary>
        Low = 0,

        /// <summary>
        /// Cache items with this priority level are likely to be deleted from the cache as the server frees system memory.
        /// </summary>
        Normal = 1,

        /// <summary>
        /// Cache items with this priority level are the least likely to be deleted from the cache as the server frees system memory.
        /// </summary>
        High = 2,

        /// <summary>
        /// The cache items with this priority level will not be automatically deleted from the cache as the server frees system memory.
        /// </summary>
        NotRemovable = 3
    }
}
