using System;
using System.Web;
using System.Web.Mvc;

using CMS.Helpers;

using Kentico.OnlineMarketing.Web.Mvc;
using Kentico.Web.Mvc;

namespace Kentico.Activities.Web.Mvc
{
    /// <summary>
    /// Provides system extension methods for <see cref="Kentico.Web.Mvc.HtmlHelperExtensions.Kentico(HtmlHelper)"/> extension point.
    /// </summary>
    public static class HtmlHelperExtensions
    {
        /// <summary>
        /// Renders a script for logging page related activities.
        /// </summary>
        /// <param name="instance">HtmlHelper extension point.</param>
        public static IHtmlString ActivityLoggingScript(this ExtensionPoint<HtmlHelper> instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (VirtualContext.IsInitialized)
            {
                return null;
            }

            var urlHelper = new UrlHelper(instance.Target.ViewContext.RequestContext);
            var script = urlHelper.RouteUrl("KenticoLogActivityScript");
            return instance.RenderScriptsTag(script);
        }
    }
}
