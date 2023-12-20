using System;
using System.Web.Mvc;

using Kentico.Web.Mvc;

namespace Kentico.Content.Web.Mvc.Routing
{
    /// <summary>
    /// Provides extension methods for the extension point <see cref="Kentico.Web.Mvc.UrlHelperExtensions.Kentico(UrlHelper)"/>.
    /// </summary>
    public static class UrlHelperExtensions
    {
        /// <summary>
        /// Returns main URL of a current page if the request was rewritten via Alternative URLs feature.
        /// </summary>
        /// <param name="instance">HtmlHelper extension point.</param>
        /// <exception cref="ArgumentNullException"><paramref name="instance"/> is null.</exception>
        public static string PageMainUrl(this ExtensionPoint<UrlHelper> instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            return instance.Target.RequestContext.HttpContext.Items[AlternativeUrlsRouteConstants.PAGE_MAIN_URL_CONTEXT_ITEM_NAME] as string;
        }
    }
}
