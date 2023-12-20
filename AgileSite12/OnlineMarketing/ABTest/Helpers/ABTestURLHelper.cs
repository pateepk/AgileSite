using System;

using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Provides URL processing methods for A/B test conversion logging.
    /// </summary>
    public static class ABTestURLHelper
    {
        /// <summary>
        /// Tries to parse the application-relative path from the provided <paramref name="url"/>. Query string and anchor (#) are omitted.
        /// </summary>
        /// <param name="url">Absolute live site url or url relative to presentation url.</param>
        /// <param name="site">Content-Only site which SitePresentationUrl corresponds with <paramref name="url"/> </param>
        /// <param name="relativeUrl">In case of successful operation, the result is stored here.</param>
        /// <returns>
        /// Returns <c>true</c> when given <paramref name="url"/> was successfully stripped by presentation url and query string; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">When <paramref name="url"/> or <paramref name="site"/> is null.</exception>
        public static bool TryRemovePresentationURLAnchorAndQuery(string url, SiteInfo site, out string relativeUrl)
        {
            if (url == null)
            {
                throw new ArgumentNullException(nameof(url));
            }

            if (site == null)
            {
                throw new ArgumentNullException(nameof(site));
            }

            relativeUrl = null;

            var presentationUrl = site.SitePresentationURL;

            if (!site.SiteIsContentOnly || String.IsNullOrEmpty(presentationUrl))
            {
                return false;
            }

            return ParseAbsoluteUrl(url, ref relativeUrl, presentationUrl);
        }


        private static bool ParseAbsoluteUrl(string url, ref string relativeUrl, string presentationUrl)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out Uri inputUri))
            {
                return false;
            }

            if (!Uri.TryCreate(presentationUrl, UriKind.Absolute, out Uri presentationUri))
            {
                return false;
            }

            if (!inputUri.Authority.Equals(presentationUri.Authority, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!inputUri.AbsolutePath.StartsWith(presentationUri.AbsolutePath, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            url = inputUri.AbsolutePath.Substring(presentationUri.AbsolutePath.Length);

            ParseRelativeUrl(url, ref relativeUrl);

            return true;
        }


        private static void ParseRelativeUrl(string url, ref string relativeUrl)
        {
            if (!Uri.TryCreate(url, UriKind.Relative, out _))
            {
                return;
            }

            url = RemoveQueryAnchorsAndSlashes(url);

            if (!url.StartsWith("/", StringComparison.Ordinal))
            {
                url = "/" + url;
            }

            relativeUrl = url;
        }


        private static string RemoveQueryAnchorsAndSlashes(string url)
        {
            url = URLHelper.RemoveQuery(url);
            url = url.Split('#')[0];
            url = url.TrimEnd('/');

            return url;
        }
    }
}
