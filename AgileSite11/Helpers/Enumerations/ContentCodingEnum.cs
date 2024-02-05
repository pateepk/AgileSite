namespace CMS.Helpers
{
    /// <summary>
    /// Contains enumeration of supported HTTP content codings used for content compression (RFC 2616).
    /// </summary>
    public static class ContentCodingEnum
    {
        /// <summary>
        /// Uses no transformation.
        /// </summary>
        public const string IDENTITY = "identity";


        /// <summary>
        /// The "zlib" format defined in RFC 1950 [31] in combination with the "deflate" compression mechanism described in RFC 1951.
        /// </summary>
        public const string DEFLATE = "deflate";
    }
}