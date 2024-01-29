namespace CMS.DocumentEngine
{
    /// <summary>
    /// Page info source.
    /// </summary>
    public enum PageInfoSource : int
    {
        /// <summary>
        /// Unknown source.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// URL path selection.
        /// </summary>
        UrlPath = 1,

        /// <summary>
        /// Alias path selection.
        /// </summary>
        AliasPath = 2,

        /// <summary>
        /// /getdoc/ reference
        /// </summary>
        GetDoc = 3,

        /// <summary>
        /// /getdoc/ reference with culture
        /// </summary>
        GetDocCulture = 4,

        /// <summary>
        /// Document alias.
        /// </summary>
        DocumentAlias = 5,

        /// <summary>
        /// Default alias path.
        /// </summary>
        DefaultAliasPath = 6
    }
}