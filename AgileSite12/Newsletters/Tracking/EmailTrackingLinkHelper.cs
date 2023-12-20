using System;

using CMS.Base;
using CMS.SiteProvider;

namespace CMS.Newsletters
{
    /// <summary>
    /// Provides links for email tracking.
    /// </summary>
    public class EmailTrackingLinkHelper : AbstractHelper<EmailTrackingLinkHelper>
    {
        #region "Constants"

        /// <summary>
        /// Default route for tracking opened emails.
        /// </summary>
        public const string DEFAULT_OPENED_EMAIL_TRACKING_ROUTE_HANDLER_URL = "CMSModules/Newsletters/CMSPages/Track.ashx";


        /// <summary>
        /// Default route to track clicked links in emails.
        /// </summary>
        public const string DEFAULT_LINKS_TRACKING_ROUTE_HANDLER_URL = "CMSModules/Newsletters/CMSPages/Redirect.ashx";

        #endregion


        #region "Static methods"

        /// <summary>
        /// Returns URL dedicated to track opened emails for given site.
        /// </summary>
        /// <param name="site">Site for which tracking page url is returned</param>
        public static string GetOpenedEmailTrackingPage(SiteInfo site)
        {
            return HelperObject.GetOpenedEmailTrackingPageInternal(site);
        }


        /// <summary>
        /// Returns URL dedicated to track clicked links in emails for given site.
        /// </summary>
        /// <param name="site">Site for which clicked links in emails tracking page url is returned</param>
        public static string GetClickedLinkTrackingPageUrl(SiteInfo site)
        {
            return HelperObject.GetClickedLinkTrackingPageUrlInternal(site);
        }


        /// <summary>
        /// Creates a tracking link used for open-email tracking.
        /// </summary>
        /// <param name="domainName">Domain name</param>
        /// <param name="site">Site to generate link for</param>
        /// <returns>Absolute URL to tracking page</returns>
        internal static string CreateOpenedEmailTrackingLink(string domainName, SiteInfo site)
        {
            if (string.IsNullOrEmpty(domainName))
            {
                throw new ArgumentException("domainName can not be null or empty.", "domainName");
            }

            if (site == null)
            {
                throw new ArgumentNullException("site");
            }

            return LinkConverter.ConvertUrlToAbsolute(GetOpenedEmailTrackingPage(site), domainName);
        }


        #endregion


        #region "Virtual methods"

        /// <summary>
        /// Returns URL dedicated to track opened emails for given site.
        /// </summary>
        /// <param name="site">Site for which tracking page url is returned</param>
        protected virtual string GetOpenedEmailTrackingPageInternal(SiteInfo site)
        {
            return (site.SiteIsContentOnly ? String.Empty : "~/") + DEFAULT_OPENED_EMAIL_TRACKING_ROUTE_HANDLER_URL;
        }


        /// <summary>
        /// Returns URL dedicated to track clicked links in emails for given site.
        /// </summary>
        /// <param name="site">Site for which clicked links in emails tracking page url is returned</param>
        protected virtual string GetClickedLinkTrackingPageUrlInternal(SiteInfo site)
        {
            return (site.SiteIsContentOnly ? String.Empty : "~/") + DEFAULT_LINKS_TRACKING_ROUTE_HANDLER_URL;
        }

        #endregion
    }
}
