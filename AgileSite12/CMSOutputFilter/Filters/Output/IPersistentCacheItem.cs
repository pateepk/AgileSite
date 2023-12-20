using CMS.Helpers;

namespace CMS.OutputFilter
{
    /// <summary>
    /// Cache item interface
    /// </summary>
    internal interface IPersistentCacheItem : ICacheItemContainer
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