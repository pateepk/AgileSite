using System;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

using CMS.Core;
using CMS.Helpers;
using CMS.OnlineMarketing.Internal;
using CMS.SiteProvider;

using Kentico.Web.Mvc;

namespace Kentico.OnlineMarketing.Web.Mvc
{
    /// <summary>
    /// Provides system extension methods for <see cref="Kentico.Web.Mvc.HtmlHelperExtensions.Kentico(HtmlHelper)"/> extension point.
    /// </summary>
    public static class HtmlHelperExtensions
    {
        /// <summary>
        /// Renders a script for logging A/B test related analytics data.
        /// </summary>
        /// <param name="instance">HtmlHelper extension point.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="instance"/> is <c>null</c>.</exception>
        public static IHtmlString ABTestLoggerScript(this ExtensionPoint<HtmlHelper> instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }
             
            return instance.ABTestLoggerScriptInternal(Service.Resolve<IEventLogService>());
        }


        /// <summary>
        /// Renders a script for logging A/B test conversions.
        /// </summary>
        /// <param name="instance">HtmlHelper extension point.</param>        
        /// <param name="eventLogService">Event log service.</param>
        internal static IHtmlString ABTestLoggerScriptInternal(this ExtensionPoint<HtmlHelper> instance, IEventLogService eventLogService)
        {
            var currentSiteName = SiteContext.CurrentSiteName;
            if (VirtualContext.IsInitialized)
            {
                return null;
            }

            var currentThreadCulture = Thread.CurrentThread.CurrentCulture.Name;

            if (!CultureSiteInfoProvider.IsCultureOnSite(currentThreadCulture, currentSiteName))
            {
                var exception = new InvalidOperationException($"Current thread culture '{currentThreadCulture}' is not allowed on site '{currentSiteName}'. Make sure the thread culture is configured correctly on your server.");
                eventLogService.LogException("ABTestConversionLogger", "RENDERSCRIPT", exception, new LoggingPolicy(TimeSpan.FromDays(1)));

                return null;
            }

            var routeData = new RouteValueDictionary { { ABTestRouteConstants.CULTURE_ROUTE_DATA_KEY, Thread.CurrentThread.CurrentCulture.Name } };

            if (!String.IsNullOrWhiteSpace(ABVisitRequestHelper.ABVisitRequestTestName))
            {
                routeData[ABTestRouteConstants.ABTEST_ROUTE_DATA_KEY] = ABVisitRequestHelper.ABVisitRequestTestName;
            }

            var urlHelper = new UrlHelper(instance.Target.ViewContext.RequestContext);
            var script = urlHelper.RouteUrl(ABTestRouteConstants.ABTEST_LOGGER_SCRIPT_ROUTE_NAME, routeData);            

            return instance.RenderScriptsTag(script);
        }
    }
}
