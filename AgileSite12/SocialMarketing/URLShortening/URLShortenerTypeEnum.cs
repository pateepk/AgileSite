namespace CMS.SocialMarketing
{
    /// <summary>
    /// Enumeration with URL shortener types.
    /// </summary>
    public enum URLShortenerTypeEnum
    {
        /// <summary>
        /// No URL shortener will be used.
        /// </summary>
        None = 0,

        /// <summary>
        /// Bit.ly URL shortener.
        /// </summary>
        Bitly = 1,

        /// <summary>
        /// TinyURL.com shortener.
        /// </summary>
        TinyURL = 2,
    }
}
