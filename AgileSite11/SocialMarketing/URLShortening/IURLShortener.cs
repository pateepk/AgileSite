using CMS.DataEngine;

namespace CMS.SocialMarketing.URLShortening
{
    /// <summary>
    /// Provides a template for what any URLshortener class should do.
    /// </summary>
    public interface IURLShortener
    {
        /// <summary>
        /// Determines whether shortener is available
        /// </summary>
        /// <param name="site">Site for whom availability you want to know</param>
        /// <returns>True if available.</returns>
        bool IsAvailable(SiteInfoIdentifier site);

        /// <summary>
        /// Shortens the given URL via an URLShortener
        /// </summary>
        /// <param name="url">URL to be shortened</param>
        /// <param name="site">Context to get settings for</param>
        /// <returns>Shortened URL on succes, <paramref name="url"/> otherwise.</returns>
        string Shorten(string url, SiteInfoIdentifier site);
    }
}
