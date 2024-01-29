using System;
using System.Web.UI;

using CMS.Helpers;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Resolves URL using Page context.
    /// </summary>
    public class UrlResolver
    {
        /// <summary>
        /// Resolves an application relative URL to the absolute form. Uses page context by default.
        /// </summary>
        /// <param name="url">Virtual application root relative URL</param>
        /// <param name="usePage">If true, the page object can be used to resolve URLs</param>
        /// <param name="ensurePrefix">If true, the current URL prefix is ensured for the URL</param>
        /// <returns>Absolute URL</returns>
        /// <remarks>Use <see cref="URLHelper.ResolveUrl"/> if the URL is not relative.</remarks>
        public static string ResolveUrl(string url, bool usePage = true, bool ensurePrefix = false)
        {
            if (String.IsNullOrEmpty(url))
            {
                return url;
            }

            if (URLHelper.ContainsProtocol(url))
            {
                return url;
            }

            string appPath = SystemContext.ApplicationPath;

            // Try to resolve using current page if present
            Page currentPage = (usePage ? PageContext.CurrentPage : null);
            if (currentPage != null)
            {
                // Un-resolve the URL in case it is already resolved
                url = URLHelper.UnResolveUrl(url, appPath);
                url = currentPage.ResolveUrl(url);
            }
            else
            {
                // Resolve manually
                if (url.StartsWithCSafe("~/"))
                {
                    url = appPath.TrimEnd('/') + "/" + url.Substring(2);
                }
            }

            // Ensure the virtual URL prefix
            if (ensurePrefix)
            {
                string virtualPrefix = VirtualContext.CurrentURLPrefix;
                if (!String.IsNullOrEmpty(virtualPrefix) && !VirtualContext.ContainsVirtualContextPrefix(url))
                {
                    // Ensure custom prefix for preview link URLs
                    URLHelper.PathModifierHandler previewHash = null;
                    if (VirtualContext.IsPreviewLinkInitialized)
                    {
                        previewHash = VirtualContext.AddPreviewHash;
                    }

                    // Ensure the virtual prefix
                    url = URLHelper.EnsureURLPrefix(url, appPath, virtualPrefix, previewHash);
                }
            }

            return url;
        }
    }
}
