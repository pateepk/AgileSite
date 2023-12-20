namespace CMS.Helpers
{
    /// <summary>
    /// URL rewriting results enum.
    /// </summary>
    public enum RequestStatusEnum
    {
        /// <summary>
        /// Unknown result.
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// Url path isn't document page.
        /// </summary>
        NotPage = 0,

        /// <summary>
        /// Url path is rewritten, the page is document page.
        /// </summary>
        PathRewritten = 1,

        /// <summary>
        /// Url path is redirected.
        /// </summary>
        PathRedirected = 2,

        /// <summary>
        /// Url path is excluded.
        /// </summary>
        PathExcluded = 3,

        /// <summary>
        /// Url path is rewritten and output filter is disabled.
        /// </summary>
        PathRewrittenDisableOutputFilter = 4,

        /// <summary>
        /// Page not found.
        /// </summary>
        PageNotFound = 5,

        /// <summary>
        /// Custom handler.
        /// </summary>
        CustomHandler = 6,

        /// <summary>
        /// Url path is processed by CMS handlers (GetAttachmentHandler, GetMetafileHandler, GetMediaHandler etc.)
        /// </summary>
        GetFileHandler = 7,

        /// <summary>
        /// System page.
        /// </summary>
        SystemPage = 8,

        // Blog post trackback page is not supported
        //TrackbackPage = 9,

        /// <summary>
        /// Gets product page.
        /// </summary>
        GetProduct = 10,
        
        /// <summary>
        /// Gets e-product file.
        /// </summary>
        GetProductFile = 11,

        /// <summary>
        /// REST Service request
        /// </summary>
        RESTService = 12,

        /// <summary>
        /// The output was sent from the cache
        /// </summary>
        SentFromCache = 13
    }
}