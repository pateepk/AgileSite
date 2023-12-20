namespace CMS.Base
{
    /// <summary>
    /// Provides enumerated values that are used to set the <see langword="Cache-Control" /> HTTP header.
    /// </summary>
    public enum HttpCacheability
    {
        /// <summary>
        /// Indicates that without a field name, a cache must force successful revalidation with the origin server before satisfying the request.
        /// With a field name, the cache may use the response to satisfy a subsequent request.
        /// </summary>
        NoCache = 1,

        /// <summary>
        /// Default value. Specifies that the response is cacheable only on the client, not by shared caches.
        /// </summary>
        Private = 2,

        /// <summary>
        /// Specifies that the response should only be cached at the server. Clients receive headers equivalent to a NoCache directive.
        /// </summary>
        Server = 3,

        /// <summary>
        /// 
        /// </summary>
        ServerAndNoCache = Server,

        /// <summary>
        /// Specifies that the response is cacheable by clients and shared caches.
        /// </summary>
        Public = 4,

        /// <summary>
        /// 
        /// </summary>
        ServerAndPrivate = 5
    }
}
