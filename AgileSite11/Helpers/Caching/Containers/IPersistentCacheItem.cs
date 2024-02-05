namespace CMS.Helpers
{
    /// <summary>
    /// Cache item interface
    /// </summary>
    internal interface IPersistentCacheItem
    {
        /// <summary>
        /// Site name
        /// </summary>
        string SiteName
        {
            get;
            set;
        }


        /// <summary>
        /// Cache key
        /// </summary>
        string CacheKey
        {
            get;
            set;
        }


        /// <summary>
        /// Item value
        /// </summary>
        object Value
        {
            get;
            set;
        }
    }
}