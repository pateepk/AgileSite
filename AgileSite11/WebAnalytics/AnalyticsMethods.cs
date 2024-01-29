using System;

using CMS.Base;
using CMS.Core;
using CMS.DeviceProfiles;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Localization;
using CMS.MacroEngine;
using CMS.PortalEngine;
using CMS.SiteProvider;

using Newtonsoft.Json;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Methods for web analytics requiring the CMS context
    /// </summary>
    public static class AnalyticsMethods
    {
        /// <summary>
        /// Logs the site visitor.
        /// </summary>
        /// <param name="siteName">Site name</param>
        [Obsolete("Use method LogVisitor(string) instead.")]
        public static void LogVisitor(SiteNameOnDemand siteName)
        {
            LogVisitor(siteName.Value);
        }


        /// <summary>
        /// Logs the site visitor.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static void LogVisitor(string siteName)
        {
            if (!String.IsNullOrEmpty(siteName) && Service.Resolve<IAnalyticsConsentProvider>().HasConsentForLogging() &&
                (AnalyticsHelper.TrackVisitsEnabled(siteName) || AnalyticsHelper.TrackBrowserTypesEnabled(siteName) || AnalyticsHelper.TrackCountriesEnabled(siteName) || AnalyticsHelper.TrackMobileDevicesEnabled(siteName)))
            {
                bool idleExpired = false;
                VisitorStatusEnum status = AnalyticsHelper.GetContextStatus(siteName, ref idleExpired);

                if (AnalyticsHelper.TrackVisitsEnabled(siteName))
                {
                    switch (status)
                    {
                        case VisitorStatusEnum.Unknown:
                            // Log first visit
                            HitLogProvider.LogHit(HitLogProvider.VISITORS_FIRST, siteName, null, siteName, 0);

                            LogMobileDevice(siteName);
                            break;

                        default:
                            if (idleExpired)
                            {
                                HitLogProvider.LogHit(HitLogProvider.VISITORS_RETURNING, siteName, null, siteName, 0);

                                LogMobileDevice(siteName);
                            }
                            break;
                    }
                }

                if (idleExpired)
                {
                    LogBrowser(siteName);
                    LogCountry(siteName);
                }

                // Set status to the current context values
                SetVisitorStatusToContext(siteName, status, idleExpired);
            }
        }


        private static void LogBrowser(string siteName)
        {
            var browserLogger = Service.Resolve<BrowserHitLogger>();
            browserLogger.LogBrowser(siteName);
        }


        private static void LogCountry(string siteName)
        {
            if (AnalyticsHelper.TrackCountriesEnabled(siteName))
            {
                // Get country name based on current IP host address
                string countryName = GeoIPHelper.GetCountryByIp(RequestContext.UserHostAddress);
                if (!String.IsNullOrEmpty(countryName))
                {
                    HitLogProvider.LogHit(HitLogProvider.COUNTRIES, siteName, null, countryName, 0);
                }
            }
        }


        /// <summary>
        /// Corrects visitor status value and saves it to context. <see cref="VisitorStatusEnum.MoreVisits"/> is set when visitors' idle time expired. 
        /// <see cref="VisitorStatusEnum.FirstVisit"/> is set in other scenarios.
        /// </summary>
        /// <param name="siteName">Name of current site</param>
        /// <param name="visitorStatus">Visitor status</param>
        /// <param name="idleExpired">True if last action of this visitor happened a long time ago so this visitor is returning to this site</param>
        internal static void SetVisitorStatusToContext(string siteName, VisitorStatusEnum visitorStatus, bool idleExpired)
        {
            if (visitorStatus == VisitorStatusEnum.Unknown)
            {
                visitorStatus = VisitorStatusEnum.FirstVisit;
            }
            else if (idleExpired)
            {
                visitorStatus = VisitorStatusEnum.MoreVisits;
            }

            // Set status to the current context values
            AnalyticsHelper.SetContextStatus(siteName, visitorStatus, DateTime.Now);
        }


        /// <summary>
        /// Logs mobile device.
        /// </summary>
        /// <param name="siteName">Site name</param>
        private static void LogMobileDevice(string siteName)
        {
            if (AnalyticsHelper.TrackMobileDevicesEnabled(siteName) && (DeviceContext.CurrentDevice.IsMobile()))
            {
                string device = String.Empty;

                // Get all device names
                string manufacturer = Convert.ToString(DeviceContext.CurrentDevice.Data["mobileDeviceManufacturer"]);
                string model = Convert.ToString(DeviceContext.CurrentDevice.Data["mobileDeviceModel"]);

                if (!String.IsNullOrEmpty(manufacturer))
                {
                    device += manufacturer + " | ";
                }

                if (!String.IsNullOrEmpty(model))
                {
                    device += model + " | ";
                }
                    
                if (device != String.Empty)
                {
                    device = device.TrimEnd('|', ' ');
                }
                else
                {
                    // Or log unknown
                    device = "Unknown";
                }

                // Log to analytics
                HitLogProvider.LogHit(HitLogProvider.MOBILEDEVICE, siteName, null, device, 0);
            }
        }


        /// <summary>
        /// Logs conversion for given conversion name and value.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="conversionName">Conversion name to track</param>
        /// <param name="conversionValue">Conversion value</param>
        [Obsolete("Use method LogConversion(string, string, string) instead.")]
        public static void LogConversion(SiteNameOnDemand siteName, string conversionName, string conversionValue)
        {
            LogConversion(siteName.Value, conversionName, conversionValue);
        }


        /// <summary>
        /// Logs conversion for given conversion name and value.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="conversionName">Conversion name to track</param>
        /// <param name="conversionValue">Conversion value</param>
        public static void LogConversion(string siteName, string conversionName, string conversionValue)
        {
            if (!Service.Resolve<IAnalyticsConsentProvider>().HasConsentForLogging())
            {
                return;
            }

            if (conversionName != String.Empty)
            {
                // Resolve macros
                MacroResolver macroResolver = MacroContext.CurrentResolver;

                double value = ValidationHelper.GetDouble(macroResolver.ResolveMacros(conversionValue), 0, CultureHelper.EnglishCulture);
                string objectName = macroResolver.ResolveMacros(conversionName);

                HitLogProvider.LogConversions(siteName, LocalizationContext.PreferredCultureCode, objectName, 0, value);
            }
        }


        /// <summary>
        /// Logs search crawler visit for current page
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="currentPage">Current page info</param>
        public static void LogSearchCrawler(string siteName, IPageInfo currentPage)
        {
            if (!Service.Resolve<IAnalyticsConsentProvider>().HasConsentForLogging())
            {
                return;
            }

            string agent = RequestContext.UserAgent;
            if (!String.IsNullOrEmpty(agent) && AnalyticsHelper.TrackSearchCrawlersEnabled(siteName))
            {
                SearchEngineInfo sei = SearchEngineCrawlerAnalyzer.GetSearchEngineFromUserAgent(agent);
                if (sei != null)
                {
                    AnalyticsHelper.LogSearchCrawler(siteName, currentPage.NodeAliasPath, currentPage.DocumentCulture, sei.SearchEngineName, currentPage.NodeID, 1);
                }
            }
        }
        

        /// <summary>
        /// Logs statistics for landing and referring pages.
        /// </summary>
        /// <param name="currentPage">Current page info</param>
        /// <param name="siteName">Site name</param>
        public static void LogAnalytics(IPageInfo currentPage, string siteName)
        {
            if (currentPage != null)
            {
                UserPage userPage = null;

                // Try get user page object for current user
                var userPageString = CookieHelper.GetValue("CMSUserPage");
                if (!String.IsNullOrEmpty(userPageString))
                {
                    userPage = JsonConvert.DeserializeObject<UserPage>(userPageString);
                }
                
                // Check whether object exists
                if (userPage == null)
                {
                    // Create new user page object
                    userPage = new UserPage();
                    
                    // Log landing page
                    AnalyticsHelper.LogLandingPage(siteName, currentPage.DocumentCulture, null, currentPage.NodeID, 1);
                }

                // Log exit page candidate
                AnalyticsHelper.LogExitPageCandidate(siteName, currentPage.DocumentCulture, DateTime.Now.Ticks + "#" + userPage.Identifier, currentPage.NodeID, 1);

                // Indicates whether timestamp should be updated
                bool updateTimeStamp = true;
                Uri referrer = AnalyticsHelper.Referrer;

                // Log direct referrer
                if ((referrer == null) || String.IsNullOrEmpty(referrer.AbsoluteUri))
                {
                    AnalyticsHelper.LogReferringSite(HitLogProvider.REFERRINGSITE + "_direct", siteName, currentPage.DocumentCulture, null, currentPage.NodeID, 1);
                }
                else
                {
                    // Get domain from referrer
                    string domain = URLHelper.GetDomain(referrer.AbsoluteUri);

                    // Check local server
                    SiteInfo si = SiteInfoProvider.GetRunningSiteInfo(domain, SystemContext.ApplicationPath);

                    // Log local referrer
                    if ((si != null) && siteName.Equals(si.SiteName, StringComparison.OrdinalIgnoreCase))
                    {
                        // Prevent log page referring to itself (A/B test case f.e.)
                        if (userPage.LastPageNodeID != currentPage.NodeID)
                        {
                            AnalyticsHelper.LogReferringSite(HitLogProvider.REFERRINGSITE + "_local", siteName, currentPage.DocumentCulture, userPage.LastPageNodeID.ToString(), currentPage.NodeID, 1);
                        }

                        // Avg. time on page - Time stamp must be defined
                        if (userPage.TimeStamp != DateTimeHelper.ZERO_TIME)
                        {
                            // Check whether page is different
                            if (userPage.LastPageDocumentID != currentPage.DocumentID)
                            {
                                // Get difference between now and last action
                                TimeSpan timeOnPage = DateTime.Now - userPage.TimeStamp;
                                AnalyticsHelper.LogAverageTimeOnPage(siteName, currentPage.DocumentCulture, null, userPage.LastPageNodeID, 1, (int)timeOnPage.TotalSeconds);
                            }
                            // Do not update time stamp for same page
                            else
                            {
                                updateTimeStamp = false;
                            }
                        }
                    }
                    else
                    {
                        // Try get search engine name for referring URL
                        string searchKeyword;
                        var sei = SearchEngineAnalyzer.GetSearchEngineFromUrl(referrer.AbsoluteUri, out searchKeyword);
                        var searchEngine = (sei != null) ? sei.SearchEngineDisplayName : "";
                        string url = referrer.AbsoluteUri;
                        if (AnalyticsHelper.RemoveReferralQuery)
                        {
                            url = URLHelper.RemoveQuery(url);
                        }
                        // Check whether domain is search engine
                        if (!String.IsNullOrEmpty(searchEngine))
                        {
                            AnalyticsHelper.LogReferringSite(HitLogProvider.REFERRINGSITE + "_search", siteName, currentPage.DocumentCulture, searchEngine, currentPage.NodeID, 1);
                            AnalyticsHelper.LogSearchKeywords(siteName, currentPage.DocumentCulture, searchKeyword, currentPage.NodeID, 1);

                            CMSDataContext.Current.BrowserHelper.SearchKeywords = searchKeyword;
                        }
                        // Log referring site domain
                        else
                        {
                            AnalyticsHelper.LogReferringSite(HitLogProvider.REFERRINGSITE + "_referring", siteName, currentPage.DocumentCulture, domain, currentPage.NodeID, 1);
                            // Log referral
                            if (AnalyticsHelper.TrackReferralsEnabled(siteName) && Service.Resolve<IAnalyticsConsentProvider>().HasConsentForLogging())
                            {
                                HitLogProvider.LogHit(HitLogProvider.URL_REFERRALS, siteName, currentPage.DocumentCulture, url, currentPage.NodeID);
                            }
                        }

                        // Save the referrer value
                        CookieHelper.SetValue(CookieName.UrlReferrer, url, DateTime.Now.AddDays(1));
                    }
                }


                // Update user page values
                userPage.LastPageDocumentID = currentPage.DocumentID;
                userPage.LastPageNodeID = currentPage.NodeID;

                var now = DateTime.Now;

                // Update timestamp only if current page is not the same as previous
                if (updateTimeStamp)
                {
                    userPage.TimeStamp = now;
                }

                CookieHelper.SetValue("CMSUserPage", JsonConvert.SerializeObject(userPage), now.AddMinutes(20));
            }
        }


        /// <summary>
        /// Logs the page not found.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static void LogPageNotFound(SiteNameOnDemand siteName)
        {
            if (AnalyticsHelper.IsLoggingEnabled(siteName.Value) && Service.Resolve<IAnalyticsConsentProvider>().HasConsentForLogging() && AnalyticsHelper.TrackInvalidPagesEnabled(siteName.Value))
            {
                string url = URLHelper.RemoveQuery(RequestContext.CurrentURL);
                HitLogProvider.LogHit(HitLogProvider.PAGE_NOT_FOUND, siteName.Value, LocalizationContext.PreferredCultureCode, url, 0);
            }
        }
    }
}