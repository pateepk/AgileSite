namespace CMS.Helpers
{
    /// <summary>
    /// Specifies the reason an item was removed from the cache.
    /// </summary>
    public enum CMSCacheItemRemovedReason
    {
        /// <summary>
        /// The item is removed from the cache because of method remove or replace.
        /// </summary>
		Removed,

        /// <summary>
        /// The item is removed from the cache because it expired.
        /// </summary>
        Expired,

        /// <summary>
        /// The item is removed from the cache because the system removed it to free memory.
        /// </summary>
        Underused,

        /// <summary>
        /// The item is removed from the cache because the cache dependency associated with it changed.
        /// </summary>
        DependencyChanged
    }
}
