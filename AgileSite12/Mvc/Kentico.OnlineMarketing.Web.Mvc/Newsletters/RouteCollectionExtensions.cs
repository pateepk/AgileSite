using System;
using System.Web.Routing;

using CMS.Newsletters;
using CMS.SiteProvider;

using Kentico.Web.Mvc;

namespace Kentico.Newsletters.Web.Mvc
{
    /// <summary>
    /// Extends a <see cref="RouteCollection"/> object for MVC routing.
    /// </summary>
    public static class RouteCollectionExtensions
    {
        internal static Route openedEmailRoute;
        internal static Route emailLinkRoute;


        /// <summary>
        /// Maps handler responsible for tracking opened emails to <paramref name="routeUrl"/>.
        /// </summary>
        /// <param name="extensionPoint">The object that provides extensibility for <see cref="RouteCollection"/>.</param>
        /// <param name="routeUrl">URL where handler responsible for tracking opened emails is mapped. See the remarks section when using non-default URL.</param>
        /// <remarks>
        /// The Kentico administration application uses <see cref="EmailTrackingLinkHelper"/> to obtain tracking URLs when sending emails. When non-default URL for <paramref name="routeUrl"/> is used,
        /// the <see cref="EmailTrackingLinkHelper.GetOpenedEmailTrackingPageInternal(SiteInfo)"/> method has to be customized accordingly.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="extensionPoint"/> is null.</exception>
        /// <returns>Route where handler responsible for tracking opened emails is mapped.</returns>
        [Obsolete("Use Kentico.Newsletters.Web.Mvc.ApplicationBuilderExtensions.UseEmailTracking() to enable the email tracking feature. The handler route is registered automatically as part of the feature.")]
        // When resolving this obsolete member, do not remove it completely. Make the class internal instead
        public static Route MapOpenedEmailHandlerRoute(this ExtensionPoint<RouteCollection> extensionPoint, string routeUrl = EmailTrackingLinkHelper.DEFAULT_OPENED_EMAIL_TRACKING_ROUTE_HANDLER_URL)
        {
            if (extensionPoint == null)
            {
                throw new ArgumentNullException(nameof(extensionPoint));
            }

            if (openedEmailRoute != null)
            {
                // Prevent duplicate route mapping in case the UseEmailTracking method is called in App_Start while direct call to this obsolete member is not removed from RouteConfig
                // Can be removed when resolving this obsolete member
                return openedEmailRoute;
            }

            var routes = extensionPoint.Target;

            var constraints = new RouteValueDictionary(new { controller = String.Empty, action = String.Empty });
            var defaults = new RouteValueDictionary();

            openedEmailRoute = new Route(routeUrl, defaults, constraints, new RouteHandlerWrapper<OpenEmailTracker>());

            using (routes.GetWriteLock())
            {
                routes.Add(openedEmailRoute);
            }

            return openedEmailRoute;
        }


        /// <summary>
        /// Maps handler responsible for tracking clicked links in emails to <paramref name="routeUrl"/>.
        /// </summary>
        /// <param name="extensionPoint">The object that provides extensibility for <see cref="RouteCollection"/>.</param>
        /// <param name="routeUrl">URL where handler responsible for tracking clicked links in emails is mapped. See the remarks section when using non-default URL.</param>
        /// <remarks>
        /// The Kentico administration application uses <see cref="EmailTrackingLinkHelper"/> to obtain tracking URLs when sending emails. When non-default URL for <paramref name="routeUrl"/> is used,
        /// the <see cref="EmailTrackingLinkHelper.GetClickedLinkTrackingPageUrlInternal(SiteInfo)"/> method has to be customized accordingly.
        /// </remarks>
        /// <exception cref="ArgumentNullException"><paramref name="extensionPoint"/> is null.</exception>
        /// <returns>Route where handler responsible for tracking clicked links in emails is mapped.</returns>
        [Obsolete("Use Kentico.Newsletters.Web.Mvc.ApplicationBuilderExtensions.UseEmailTracking() to enable the email tracking feature. The handler route is registered automatically as part of the feature.")]
        // When resolving this obsolete member, do not remove it completely. Make the class internal instead
        public static Route MapEmailLinkHandlerRoute(this ExtensionPoint<RouteCollection> extensionPoint, string routeUrl = EmailTrackingLinkHelper.DEFAULT_LINKS_TRACKING_ROUTE_HANDLER_URL)
        {
            if (extensionPoint == null)
            {
                throw new ArgumentNullException(nameof(extensionPoint));
            }

            if (emailLinkRoute != null)
            {
                // Prevent duplicate route mapping in case the UseEmailTracking method is called in App_Start while direct call to this obsolete member is not removed from RouteConfig
                // Can be removed when resolving this obsolete member
                return emailLinkRoute;
            }

            var routes = extensionPoint.Target;

            var constraints = new RouteValueDictionary(new { controller = String.Empty, action = String.Empty });
            var defaults = new RouteValueDictionary();

            emailLinkRoute = new Route(routeUrl, defaults, constraints, new RouteHandlerWrapper<LinkTracker>());

            using (routes.GetWriteLock())
            {
                routes.Add(emailLinkRoute);
            }

            return emailLinkRoute;
        }
    }
}
