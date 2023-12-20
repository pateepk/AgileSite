namespace CMS.Base
{
    /// <summary>
    /// Provides enumerated values that are used to set revalidation-specific Cache-Control HTTP headers.
    /// </summary>
    public enum HttpCacheRevalidation
    {
        /// <summary>
        /// Indicates that Cache-Control: must-revalidate should be sent.
        /// </summary>
        AllCaches = 1,

        /// <summary>
        /// Indicates that Cache-Control: proxy-revalidate should be sent.
        /// </summary>
        ProxyCaches = 2,

        /// <summary>
        /// Default value. Indicates that no property has been set. If this is set, no cache revalidation directive is sent.
        /// </summary>
        None = 3
    }
}
