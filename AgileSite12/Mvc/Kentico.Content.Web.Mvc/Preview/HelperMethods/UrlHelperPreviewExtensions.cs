using System;
using System.Web;
using System.Web.Mvc;

using CMS.Core;
using CMS.Helpers;

using Kentico.Web.Mvc;

namespace Kentico.Content.Web.Mvc
{
    /// <summary>
    /// Provides extension methods for the extension point <see cref="UrlHelperExtensions.Kentico(UrlHelper)"/>.
    /// </summary>
    public static class UrlHelperPreviewExtensions
    {
        /// <summary>
        /// Extends the given <paramref name="url"/> by additional authentication information when the current request carries authentication information in the request URL.
        /// </summary>
        /// <param name="instance">HtmlHelper extension point.</param>
        /// <param name="url">URL that is to be authenticated. The following URL formats will be authenticated: Relative (virtual) or absolute path.
        /// Query string parameters are not secured and can be changed even after the <paramref name="url"/> is authenticated.</param>
        /// <returns>
        /// If the current request carries authentication information in the URL, returns the given <paramref name="url"/> extended by authentication information.
        /// If the current request does not carry authentication information in the URL, returns the original <paramref name="url"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="instance"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="url"/> is null or empty.</exception>
        public static HtmlString AuthenticateUrl(this ExtensionPoint<UrlHelper> instance, string url)
        {
            return AuthenticateUrl(instance, url, VirtualContext.ReadonlyMode);
        }


        /// <summary>
        /// Extends the given <paramref name="url"/> by additional authentication information when the current request carries authentication information in the request URL.
        /// </summary>
        /// <param name="instance">HtmlHelper extension point.</param>
        /// <param name="url">URL that is to be authenticated. The following URL formats will be authenticated: Relative (virtual) or absolute path.
        /// Query string parameters are not secured and can be changed even after the <paramref name="url"/> is authenticated.</param>
        /// <param name="readonlyMode">Indicates if readonly mode should be enabled to disallow modify actions and POST requests.</param>
        /// <returns>
        /// If the current request carries authentication information in the URL, returns the given <paramref name="url"/> extended by authentication information.
        /// If the current request does not carry authentication information in the URL, returns the original <paramref name="url"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="instance"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="url"/> is null or empty.</exception>
        public static HtmlString AuthenticateUrl(this ExtensionPoint<UrlHelper> instance, string url, bool readonlyMode)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("URL needs to be provided.", nameof(url));
            }

            return AuthenticateUrlInternal(instance.Target, url, readonlyMode);
        }


        private static HtmlString AuthenticateUrlInternal(UrlHelper urlHelper, string url, bool readonlyMode)
        {
            var currentProcessor = Service.Resolve<ICurrentUrlProcessorService>().GetProcessor();
            if (currentProcessor != null)
            {
                var absolutePath = urlHelper.Content(url);

                url = currentProcessor.DecoratePath(absolutePath, readonlyMode);
            }

            // Use HtmlString for indicating that the authenticatedUrl should not be HTML encoded,
            // otherwise it may loose its query string parameters due to encoded '&' character
            return new HtmlString(url);
        }
    }
}
