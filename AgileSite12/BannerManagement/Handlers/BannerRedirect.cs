using System.Web;

using CMS.Core;
using CMS.Helpers;
using CMS.PortalEngine;
using CMS.SiteProvider;
using CMS.WebAnalytics;

namespace CMS.BannerManagement
{
    /// <summary>
    /// Redirects user to the banner's address and logs banner click for banner with specified ID.
    /// </summary>
    public class BannerRedirect : IHttpHandler
    {
        /// <summary>
        /// This handler can be reused.
        /// </summary>
        public bool IsReusable => true;


        /// <summary>
        /// Redirects user to the address of the banner specified by the query parameter "bannerID" and logs banner click.
        /// </summary>
        public void ProcessRequest(HttpContext context)
        {
            var banner = BannerInfoProvider.GetBannerInfo(QueryHelper.GetInteger("bannerID", 0));

            if ((banner == null) || (!banner.IsGlobal && (banner.BannerSiteID != SiteContext.CurrentSiteID)))
            {
                context.Response.ContentType = "text/plain";
                context.Response.Write("Unknown banner or wrong site");

                return;
            }

            // Count click only on live site
            if (PortalContext.ViewMode == ViewModeEnum.LiveSite)
            {
                string currentSiteName = SiteContext.CurrentSiteName;

                if (AnalyticsHelper.AnalyticsEnabled(currentSiteName) && Service.Resolve<IAnalyticsConsentProvider>().HasConsentForLogging())
                {
                    HitLogProvider.LogHit("bannerclick", currentSiteName, null, null, banner.BannerID);
                }

                BannerInfoProvider.DecrementClicksLeft(banner.BannerID);
            }

            URLHelper.ResponseRedirect(banner.BannerURL);
        }
    }
}
