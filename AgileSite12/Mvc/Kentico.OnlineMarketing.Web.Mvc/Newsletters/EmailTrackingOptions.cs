using CMS.Newsletters;
using CMS.SiteProvider;

namespace Kentico.Newsletters.Web.Mvc
{
    /// <summary>
    /// Options configuring the <see cref="ApplicationBuilderExtensions.UseEmailTracking"/> feature.
    /// </summary>
    /// <seealso cref="ApplicationBuilderExtensions.UseEmailTracking"/>
    public class EmailTrackingOptions
    {
        /// <summary>
        /// Gets or sets the URL where handler responsible for tracking clicked links in emails is mapped. See the remarks section when using non-default URL.
        /// The default value is <see cref="EmailTrackingLinkHelper.DEFAULT_LINKS_TRACKING_ROUTE_HANDLER_URL"/>.
        /// </summary>
        /// <remarks>
        /// The Kentico administration application uses <see cref="EmailTrackingLinkHelper"/> to obtain tracking URLs when sending emails. When non-default URL is used,
        /// the <see cref="EmailTrackingLinkHelper.GetClickedLinkTrackingPageUrlInternal(SiteInfo)"/> method has to be customized accordingly.
        /// </remarks>
        public string EmailLinkHandlerRouteUrl { get; set; } = EmailTrackingLinkHelper.DEFAULT_LINKS_TRACKING_ROUTE_HANDLER_URL;


        /// <summary>
        /// Gets or sets the URL where handler responsible for tracking opened emails is mapped. See the remarks section when using non-default URL.
        /// The default value is <see cref="EmailTrackingLinkHelper.DEFAULT_OPENED_EMAIL_TRACKING_ROUTE_HANDLER_URL"/>.
        /// </summary>
        /// <remarks>
        /// The Kentico administration application uses <see cref="EmailTrackingLinkHelper"/> to obtain tracking URLs when sending emails. When non-default URL is used,
        /// the <see cref="EmailTrackingLinkHelper.GetOpenedEmailTrackingPageInternal(SiteInfo)"/> method has to be customized accordingly.
        /// </remarks>
        public string OpenedEmailHandlerRouteUrl { get; set; } = EmailTrackingLinkHelper.DEFAULT_OPENED_EMAIL_TRACKING_ROUTE_HANDLER_URL;
    }
}
