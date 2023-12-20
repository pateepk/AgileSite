using System;

using CMS.Activities;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.OutputFilter;

namespace CMS.WebAnalytics.Web.UI
{
    /// <summary>
    /// Web analytics Web UI events handlers
    /// </summary>
    public class WebAnalyticsWebUIHandlers
    {
        /// <summary>
        /// Query parameter name for the campaign tracking.
        /// </summary>
        private const string CAMPAIGN_URL_TRACKING_PARAMETER = "utm_campaign";


        /// <summary>
        /// Query parameter name for the source tracking.
        /// </summary>
        private const string SOURCE_URL_TRACKING_PARAMETER = "utm_source";


        /// <summary>
        /// Query parameter name for the content tracking.
        /// </summary>
        private const string CONTENT_URL_TRACKING_PARAMETER = "utm_content";


        /// <summary>
        /// Initializes the events handlers.
        /// </summary>
        public static void Init()
        {
            if (SystemContext.IsCMSRunningAsMainApplication)
            {
                OutputFilterEvents.SendCacheOutput.After += DelayEndOfTheRequest;

                // On ProcessCachedOutput because MapRequestHandler event is not fire when using full page caching.
                OutputFilterEvents.SendCacheOutput.After += LogCampaign;

                // LogCampaign has to be on PostMapRequestHandler.After event
                // because Session_Start event is sometimes fired right after this event where contact 
                // could be created without proper initialization of analytics data
                RequestEvents.PostMapRequestHandler.Execute += SetCampaign;
                RequestEvents.PostAcquireRequestState.Execute += LogPageViewConversion;
            }
        }


        /// <summary>
        /// Logs campaign.
        /// </summary>
        internal static void LogCampaign(object sender, OutputCacheEventArgs e)
        {
            if (CanProceedInLogging())
            {
                SetCampaign();
            }
        }


        /// <summary>
        /// If URL contains campaign code this sets campaign and also makes hit to the analytics.
        /// </summary>
        internal static void SetCampaign()
        {
            // Log campaign stored in context
            string urlCampaign = QueryHelper.GetString(CAMPAIGN_URL_TRACKING_PARAMETER, null);
            if (String.IsNullOrEmpty(urlCampaign))
            {
                return;
            }

            var pageInfo = DocumentContext.CurrentPageInfo;
            string urlSource = QueryHelper.GetString(SOURCE_URL_TRACKING_PARAMETER, null);
            string urlContent = QueryHelper.GetString(CONTENT_URL_TRACKING_PARAMETER, null);

            var campaignService = Service.Resolve<ICampaignService>();
            campaignService.SetCampaign(urlCampaign, pageInfo.SiteName, urlSource, urlContent);
        }


        /// <summary>
        /// Determines whether conversions can be logged.
        /// </summary>
        /// <returns>True, if can proceed; otherwise, false</returns>
        private static bool CanProceedInLogging()
        {
            if (!Service.Resolve<ISiteService>().IsLiveSite)
            {
                return false;
            }

            PageInfo info = DocumentContext.CurrentPageInfo;
            if (info == null)
            {
                return false;
            }

            if (!RequestHelper.IsPostBack() &&
                AnalyticsHelper.IsLoggingEnabled(info.SiteName, info.NodeAliasPath) &&
                LicenseKeyInfoProvider.IsFeatureAvailable(RequestContext.CurrentDomain, FeatureEnum.CampaignAndConversions))
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Logs page view conversion. Requires access to the Session object.
        /// </summary>
        private static void LogPageViewConversion(object sender, EventArgs e)
        {
            PageInfo info = DocumentContext.CurrentPageInfo;
            if (RequestContext.IsContentPage && CanProceedInLogging() && !AnalyticsHelper.JavascriptLoggingEnabled(info.SiteName))
            {
                AnalyticsMethods.LogConversion(info.SiteName, info.DocumentTrackConversionName, info.DocumentConversionValue);
            }
        }


        /// <summary>
        /// Sets campaign.
        /// </summary>
        private static void SetCampaign(object sender, EventArgs e)
        {
            // there has to be check for both output cache and OutputFilterContext because request is somehow executed again
            // after successful OutputFilterEvents.SendCacheOutput event -> it would be logged twice
            if (RequestContext.CurrentStatus == RequestStatusEnum.SentFromCache || !RequestContext.IsContentPage || !CanProceedInLogging())
            {
                return;
            }

            SetCampaign();
        }


        /// <summary>
        /// Delays the end of the request when full page caching if Analytics or Activities are enabled.
        /// Fired when cache successfully loaded.
        /// </summary>
        private static void DelayEndOfTheRequest(object sender, OutputCacheEventArgs e)
        {
            if (AnalyticsHelper.AnalyticsEnabled(e.Output.SiteName) || ActivitySettingsHelper.ActivitiesEnabledAndModuleLoaded(e.Output.SiteName))
            {
                // Analytics enabled, end request later
                RequestContext.LogPageHit = true;
                OutputFilterContext.OutputFilterEndRequestRequired = true;
                OutputFilterContext.EndRequest = false;
            }
        }
    }
}