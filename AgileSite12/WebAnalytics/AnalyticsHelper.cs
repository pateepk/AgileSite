using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.WebAnalytics.Internal;

namespace CMS.WebAnalytics
{
    using IPsTable = StringSafeDictionary<Regex>;
    using SiteTable = StringSafeDictionary<Hashtable>;

    /// <summary>
    /// Analytics helper class.
    /// </summary>
    public static class AnalyticsHelper
    {
        #region "Variables"

        /// <summary>
        /// Hashtable of sorted dictionaries of lists with visitors IP addresses
        /// HashTable[ siteName -> { lastTime, ArrayList of Hashtable[IP -> true] } ]
        /// </summary>
        private static readonly CMSStatic<SiteTable> mSiteList = new CMSStatic<SiteTable>(() => new SiteTable(false));


        /// <summary>
        /// Hashtable of regular expressions of sites' excluded IPs.
        /// </summary>
        private static readonly CMSStatic<IPsTable> mIPsRegExpTable = new CMSStatic<IPsTable>(() => new IPsTable(false));


        /// <summary>
        /// Indicates if IP should be logged every time for anonymous user.
        /// </summary>
        private static bool? mSlidingIPExpiration;


        /// <summary>
        /// Locking object for list of IP addresses per site
        /// </summary>
        private static readonly object mIpLocker = new object();


        private static readonly Lazy<IWebAnalyticsSettingsProvider> mWebAnalyticsSettingsProvider = new Lazy<IWebAnalyticsSettingsProvider>(Service.Resolve<IWebAnalyticsSettingsProvider>); 

        #endregion


        #region "Properties"

        /// <summary>
        /// Hashtable of sorted dictionaries of lists with visitors IP addresses
        /// HashTable[ siteName -> { lastTime, ArrayList of Hashtable[IP -> true] } ]
        /// </summary>
        private static SiteTable SiteList
        {
            get
            {
                return mSiteList;
            }
        }


        /// <summary>
        /// Hashtable of regular expressions of sites' excluded IPs.
        /// </summary>
        internal static IPsTable IPsRegExpTable
        {
            get
            {
                return mIPsRegExpTable;
            }
        }

        #endregion


        #region "Constants"

        /// <summary>
        /// Width of subscription window
        /// </summary>
        public const int SUBSCRIPTION_WINDOW_WIDTH = 800;


        /// <summary>
        /// Height of subscription window
        /// </summary>
        public const int SUBSCRIPTION_WINDOW_HEIGHT = 760;


        /// <summary>
        /// Width of manage data window
        /// </summary>
        public const int MANAGE_WINDOW_WIDTH = 450;


        /// <summary>
        /// Height of manage data window
        /// </summary>
        public const int MANAGE_WINDOW_HEIGHT = 280;


        /// <summary>
        /// Replacement for the semicolon character used in the url parameter
        /// </summary>
        public const string PARAM_SEMICOLON = "%%SEMICOLON%%";

        #endregion


        #region "Properties"

        /// <summary>
        /// Returns visitor status.
        /// </summary>
        public static VisitorStatusEnum VisitorStatus
        {
            get
            {
                // Get primarily from the session, secondarily from cookie
                string value = (string)ContextHelper.GetItem("VisitorStatus", false, true, true);

                return VisitorStatusCode.ToEnum(ValidationHelper.GetInteger(value, 0));
            }
            set
            {
                // Set the values
                string status = VisitorStatusCode.FromEnumString(value);

                ContextHelper.Add("VisitorStatus", status, false, true, true, DateTime.Now.AddYears(1));
            }
        }


        /// <summary>
        /// Returns "Use JavaScript logging" value from the database.
        /// </summary>
        public static bool JavascriptLoggingEnabled(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue("CMSWebAnalyticsUseJavascriptLogging", siteName);
        }


        /// <summary>
        /// Returns "Excluded IPs" value from the database.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static string ExcludedIPs(string siteName)
        {
            return SettingsKeyInfoProvider.GetValue("CMSAnalyticsExcludedIPs", siteName).Trim();
        }


        /// <summary>
        /// Returns "Excluded URLs" value from the database.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static string ExcludedURLs(string siteName)
        {
            return SettingsKeyInfoProvider.GetValue("CMSAnalyticsExcludedURLs", siteName);
        }


        /// <summary>
        /// Returns "Excluded file extensions" value from the database.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static string ExcludedFileExtensions(string siteName)
        {
            return SettingsKeyInfoProvider.GetValue("CMSAnalyticsExcludedFileExtensions", siteName);
        }


        /// <summary>
        /// Returns "Exclude search engines" value from the database.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool ExcludeSearchEngines(string siteName)
        {
            return mWebAnalyticsSettingsProvider.Value.ExcludeSearchEngines(siteName);
        }


        /// <summary>
        /// Returns true if web analytics is enabled for specified site.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool AnalyticsEnabled(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue("CMSAnalyticsEnabled", siteName);
        }


        /// <summary>
        /// Returns true if page views tracking is enabled for specified site.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool TrackPageViewsEnabled(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue("CMSAnalyticsTrackPageViews", siteName);
        }


        /// <summary>
        /// Returns true if aggregated views tracking is enabled for specified site.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool TrackAggregatedViewsEnabled(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue("CMSAnalyticsTrackAggregatedViews", siteName);
        }


        /// <summary>
        /// Returns true if visits tracking is enabled for specified site.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool TrackVisitsEnabled(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue("CMSAnalyticsTrackVisits", siteName);
        }


        /// <summary>
        /// Returns how long should be visitors' IP addresses stored in memory.
        /// Visitors smart checking is disabled if 0 is returned.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static int VisitorsSmartCheckingTime(string siteName)
        {
            return SettingsKeyInfoProvider.GetIntValue("CMSAnalyticsVisitorsSmartCheck", siteName);
        }


        /// <summary>
        /// Returns true if file downloads tracking is enabled for specified site.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool TrackFileDownloadsEnabled(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue("CMSAnalyticsTrackFileDownloads", siteName);
        }


        /// <summary>
        /// Returns true if browser types tracking is enabled for specified site.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool TrackBrowserTypesEnabled(string siteName)
        {
            return mWebAnalyticsSettingsProvider.Value.TrackBrowserTypesEnabled(siteName);
        }


        /// <summary>
        /// Returns true if invalid pages tracking is enabled for specified site.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool TrackInvalidPagesEnabled(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue("CMSAnalyticsTrackInvalidPages", siteName);
        }


        /// <summary>
        /// Returns true if search keywords tracking is enabled for specific site.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool TrackSearchKeywordsEnabled(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue("CMSTrackSearchKeywords", siteName);
        }


        /// <summary>
        /// Returns true if landing page tracking is enabled for specific site.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool TrackLandingPageEnabled(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue("CMSTrackLandingPages", siteName);
        }


        /// <summary>
        /// Returns true if exit page tracking is enabled for specific site.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool TrackExitPageEnabled(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue("CMSTrackExitPages", siteName);
        }


        /// <summary>
        /// Returns true if search engine tracking is enabled for specific site.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool TrackSearchEnginesEnabled(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue("CMSTrackSearchEngines", siteName);
        }


        /// <summary>
        /// Returns true if search crawlers tracking is enabled for specific site.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool TrackSearchCrawlersEnabled(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue("CMSTrackSearchCrawlers", siteName);
        }


        /// <summary>
        /// Returns true if referrals tracking is enabled for specified site.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool TrackReferralsEnabled(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue("CMSAnalyticsTrackReferrals", siteName);
        }


        /// <summary>
        /// Returns true if countries tracking is enabled for specified site.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool TrackCountriesEnabled(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue("CMSAnalyticsTrackCountries", siteName);
        }


        /// <summary>
        /// Returns true if registered users tracking is enabled for specified site.
        /// </summary>
        /// <param name="siteName">Site name</param>        
        public static bool TrackRegisteredUsersEnabled(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue("CMSAnalyticsTrackRegisteredUsers", siteName);
        }


        /// <summary>
        /// Returns true if onsite keywords tracking is enabled for specified site.
        /// </summary>
        /// <param name="siteName">Site name</param>        
        public static bool TrackOnSiteKeywords(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue("CMSTrackOnSiteKeywords", siteName);
        }


        /// <summary>
        /// Returns true if referring sites direct link  tracking is enabled for specified site.
        /// </summary>
        /// <param name="siteName">Site name</param>        
        public static bool TrackReferringSitesDirect(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue("CMSTrackReferringSitesDirect", siteName);
        }


        /// <summary>
        /// Returns true if referring sites by local links  tracking is enabled for specified site.
        /// </summary>
        /// <param name="siteName">Site name</param>        
        public static bool TrackReferringSitesLocal(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue("CMSTrackReferringSitesLocal", siteName);
        }


        /// <summary>
        /// Returns true if average time on page tracking is enabled
        /// </summary>
        /// <param name="siteName">Site name</param>        
        public static bool TrackAverageTimeOnPage(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue("CMSTrackAverageTimeOnPage", siteName);
        }


        /// <summary>
        /// Returns true if referring sites by other domain tracking is enabled for specific site.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool TrackReferringSitesReferring(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue("CMSTrackReferringSitesReferring", siteName);
        }


        /// <summary>
        /// Returns true if mobile device tracking is enabled
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool TrackMobileDevicesEnabled(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue("CMSTrackMobileDevices", siteName);
        }


        /// <summary>
        /// Returns true, if query string should be removed from the referral URL; otherwise, false.
        /// </summary>
        public static bool RemoveReferralQuery
        {
            get
            {
                return ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSWebAnalyticsRemoveReferralQuery"], true);
            }
        }


        /// <summary>
        /// Returns true if IP for anonymous visitor should be logged every time. 
        /// Returns false if IP should be logged only once per user visit.
        /// </summary>
        public static bool SlidingIPExpiration
        {
            get
            {
                if (mSlidingIPExpiration == null)
                {
                    mSlidingIPExpiration = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSWebAnalyticsSlidingIPExpiration"], true);
                }
                return (bool)mSlidingIPExpiration;
            }
            set
            {
                mSlidingIPExpiration = value;
            }
        }


        /// <summary>
        /// Replaceable referrer (checks "AnalyticsReferrerString" item in RequestStockHelper first).
        /// </summary>
        public static Uri Referrer
        {
            get
            {
                string fakeReferrer = (RequestStockHelper.GetItem("AnalyticsReferrerString") as string);
                if (fakeReferrer == null)
                {
                    return CMSHttpContext.Current.Request.UrlReferrer;
                }
                if (!string.IsNullOrEmpty(fakeReferrer))
                {
                    return new Uri(fakeReferrer);
                }
                return null;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets or sets the value in minutes for how long user should be counted as active(1380 minutes by default = 23 hours).
        /// </summary>
        public static int GetVisitorStatusIdle(string siteName)
        {
            return SettingsKeyInfoProvider.GetIntValue("CMSVisitorStatusIdle", siteName);
        }


        /// <summary>
        /// Returns current visitor status
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="idleExpired">Indicates whether idle expired</param>
        public static VisitorStatusEnum GetContextStatus(string siteName, ref bool idleExpired)
        {
            VisitorStatusEnum status = VisitorStatusEnum.Unknown;
            idleExpired = false;

            // Get status from cookie
            var value = CookieHelper.GetValue(CookieName.VisitorStatus);

            // Try get status by IP address
            if (String.IsNullOrEmpty(value))
            {
                string ip = RequestContext.UserHostAddress;
                status = GetVisitorStatusByIP(ip, siteName, ref idleExpired);
            }

            // Check only if status wasn't set by IP table
            if (status == VisitorStatusEnum.Unknown)
            {
                // Get status by backward compatibility
                if (String.IsNullOrEmpty(value))
                {
                    status = TryGetStatusBackwardCompatible(ref idleExpired);
                }

                // Decode state and idle expiration
                if (!String.IsNullOrEmpty(value))
                {
                    status = VisitorStatusCode.ToEnum(ValidationHelper.GetInteger(value[0], 0));
                    if (status != VisitorStatusEnum.Unknown)
                    {
                        double ticks = ValidationHelper.GetDouble(value.Substring(1), 0);
                        if (ticks > 0)
                        {
                            long diff = (long)TimeSpan.FromTicks(DateTime.Now.Ticks).TotalMinutes - GetVisitorStatusIdle(siteName);
                            if (diff > ticks)
                            {
                                idleExpired = true;
                            }
                        }
                    }
                    else
                    {
                        idleExpired = true;
                    }
                }
                else
                {
                    idleExpired = true;
                }
            }

            return status;
        }


        /// <summary>
        /// Tries get status from previous type of cookies
        /// </summary>
        /// <param name="idleExpired">Indicates whether idle expired</param>
        private static VisitorStatusEnum TryGetStatusBackwardCompatible(ref bool idleExpired)
        {
            VisitorStatusEnum status = VisitorStatusEnum.Unknown;

            string cvs = ContextHelper.GetItem(CookieName.CurrentVisitStatus, false, true, true) as string;
            string vs = ContextHelper.GetItem(CookieName.VisitStatus, false, true, true) as string;

            // Keep first visit
            if (!String.IsNullOrEmpty(cvs))
            {
                status = VisitorStatusEnum.FirstVisit;
                idleExpired = false;

                // Remove old logging
                CookieHelper.Remove(CookieName.CurrentVisitStatus);
                if (!String.IsNullOrEmpty(vs))
                {
                    CookieHelper.Remove(CookieName.VisitStatus);
                }
            }
            // Log more visits
            else if (!String.IsNullOrEmpty(vs))
            {
                status = VisitorStatusEnum.FirstVisit;
                idleExpired = true;

                // Remove old logging
                CookieHelper.Remove(CookieName.VisitStatus);
            }

            return status;
        }


        /// <summary>
        /// Cleans list of IP addresses from site list 
        /// </summary>
        /// <param name="siteName">Site name</param>
        private static void CleanIPAdresses(string siteName)
        {
            int ipMinutes = VisitorsSmartCheckingTime(siteName);
            if (ipMinutes > 0)
            {
                var ht = SiteList[siteName];
                if (ht != null)
                {
                    // Get ticks equal to max limit to keep in list
                    long ticks = (long)TimeSpan.FromTicks(DateTime.Now.AddMinutes(ipMinutes * -1).Ticks).TotalMinutes;

                    // Get keys
                    string[] keys;
                    lock (mIpLocker)
                    {
                        keys = new string[ht.Keys.Count];
                        ht.Keys.CopyTo(keys, 0);
                    }

                    // Loop thru all items and remove expired
                    foreach (string ip in keys)
                    {
                        if (ValidationHelper.GetDouble(ht[ip], Double.MaxValue) < ticks)
                        {
                            ht.Remove(ip);
                        }
                    }

                    SetupIPExpiration(siteName);
                }
            }
        }


        /// <summary>
        /// Sets up the IP expiration
        /// </summary>
        /// <param name="siteName">Site name</param>
        private static void SetupIPExpiration(string siteName)
        {
            // Re-init timer for current site
            if (SlidingIPExpiration)
            {
                CacheHelper.RegisterAutomaticCallback("waiptable|" + siteName, DateTime.Now.AddMinutes(VisitorsSmartCheckingTime(siteName) + 1), () => CleanIPAdresses(siteName));
            }
        }


        /// <summary>
        /// Sets context status.
        /// </summary>
        /// <param name="siteName">Name of context's site</param>
        /// <param name="status">Visitor's status</param>
        /// <param name="lastActivityDate">Date time of visitor's last activity</param>
        public static void SetContextStatus(string siteName, VisitorStatusEnum status, DateTime lastActivityDate)
        {
            // Keep current status for current request
            AnalyticsContext.CurrentVisitStatus = status;

            string value = VisitorStatusCode.FromEnum(status).ToString();
            value += ((long)(TimeSpan.FromTicks(lastActivityDate.Ticks).TotalMinutes)).ToString();

            // Set status with idle
            CookieHelper.SetValue(CookieName.VisitorStatus, value, DateTime.Now.AddYears(20));

            string ip = RequestContext.UserHostAddress;
            LogIPVisit(ip, siteName, lastActivityDate);
        }


        /// <summary>
        /// Return true if analytics log enabled for given data
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="nodeAliasPath">Node alias path</param>
        /// <param name="excludingOptions">Excluding options flag for logging</param>
        public static bool IsLoggingEnabled(string siteName, string nodeAliasPath = "", LogExcludingFlags excludingOptions = LogExcludingFlags.CheckAll)
        {
            return
                (AnalyticsEnabled(siteName) &&
                 (((excludingOptions & LogExcludingFlags.SkipIpCheck) == LogExcludingFlags.SkipIpCheck) || !IsIPExcluded(siteName, RequestContext.UserHostAddress)) &&
                 (((excludingOptions & LogExcludingFlags.SkipFileExtensionCheck) == LogExcludingFlags.SkipFileExtensionCheck) || !IsFileExtensionExcluded(siteName, RequestContext.CurrentUrlExtension)) &&
                 (((excludingOptions & LogExcludingFlags.SkipUrlCheck) == LogExcludingFlags.SkipUrlCheck) || !IsURLExcluded(siteName, nodeAliasPath)) &&
                 (((excludingOptions & LogExcludingFlags.SkipCrawlerCheck) == LogExcludingFlags.SkipCrawlerCheck) || !IsSearchEngineExcluded(siteName)));
        }


        /// <summary>
        /// Determines whether IP is excluded or not.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="ip">IP</param>
        public static bool IsIPExcluded(string siteName, string ip)
        {
            // If IP not given, not excluded
            if (String.IsNullOrEmpty(ip))
            {
                return false;
            }

            // Get regular expression of the list of excluded IPs of given site
            Regex re = IPsRegExpTable[siteName];
            if (re == null)
            {
                // Get the list of excluded IPs; remove closing semicolon if exists
                string iplist = ExcludedIPs(siteName);
                iplist = iplist.Replace(" ", "");
                iplist = iplist.Trim(';');

                if (String.IsNullOrEmpty(iplist))
                {
                    return false;
                }

                // Prepare the regular expression from the list of IPs
                iplist = iplist.Replace("*", "(\\d*)");
                iplist = iplist.Replace(";", ")|(");
                iplist = "(" + iplist + ")";

                re = RegexHelper.GetRegex(iplist, RegexOptions.None);
                IPsRegExpTable[siteName] = re;
            }

            // Check match
            return re.IsMatch(ip);
        }


        /// <summary>
        /// Generates reports parameters for query string
        /// </summary>
        /// <param name="dr">Datarow with report's parameters</param>        
        public static string GetQueryStringParameters(DataRow dr)
        {
            if (dr == null)
            {
                return string.Empty;
            }

            var result = new List<string>(dr.Table.Columns.Count);

            // Build the results array
            foreach (DataColumn col in dr.Table.Columns)
            {
                var dataType = DataTypeManager.GetDataType(col.DataType);
                var stringValue = dataType != null ? dataType.GetString(dr[col.ColumnName], CultureHelper.EnglishCulture) : Convert.ToString(dr[col.ColumnName]);

                result.Add(col.ColumnName);
                result.Add(stringValue.Replace(";", PARAM_SEMICOLON));
            }

            return HttpUtility.UrlEncode(string.Join(";", result));
        }


        /// <summary>
        /// Determines whether URL is excluded or not.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="relativeUrl">URL</param>
        public static bool IsURLExcluded(string siteName, string relativeUrl)
        {
            if (String.IsNullOrEmpty(relativeUrl))
            {
                return false;
            }

            var urlList = ExcludedURLs(siteName)
                .Split(';')
                .Select(excludedUrl => excludedUrl.Trim())
                .ToList();

            // In case excluded URLs setting contains root, all relative URLs have to be excluded
            if (urlList.Contains("/", StringComparer.OrdinalIgnoreCase))
            {
                return true;
            }

            urlList = urlList
                .Select(excludedUrl => excludedUrl.TrimEnd('/'))
                .Where(excludedUrl => !string.IsNullOrEmpty(excludedUrl))
                .ToList();

            relativeUrl += "/";

            return urlList.Any(url => relativeUrl.StartsWith(url + "/", StringComparison.OrdinalIgnoreCase));
        }


        /// <summary>
        /// Determines whether extension is excluded or not.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="extension">File extension (with or without '.', e.g. 'txt' or '.txt')</param>
        public static bool IsFileExtensionExcluded(string siteName, string extension)
        {
            // Get extensions
            string extlist = ExcludedFileExtensions(siteName);
            // Check whether at least one extension is defined (for extensionless must be semicolon used)
            if (!String.IsNullOrEmpty(extlist))
            {
                extlist = ";" + extlist + ";";
                return ((extlist.IndexOf(";" + extension + ";", StringComparison.OrdinalIgnoreCase) >= 0) || (extlist.IndexOf(";" + extension.TrimStart('.') + ";", StringComparison.OrdinalIgnoreCase) >= 0));
            }
            // Extension is enabled by default
            return false;
        }


        /// <summary>
        /// Returns true if current request is search engine and should be excluded from the analytics log.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool IsSearchEngineExcluded(string siteName)
        {
            var searchEnginesDetector = Service.Resolve<ISearchEnginesDetector>();
            return searchEnginesDetector.IsSearchEngine(siteName);
        }


        /// <summary>
        /// Logs registered user to the statistics.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="ui">UserInfo of registered user - username and userid of user is logged</param>
        /// <exception cref="ArgumentNullException">Argument ui cannot be null</exception>
        public static void LogRegisteredUser(string siteName, UserInfo ui)
        {
            if (ui == null)
            {
                throw new ArgumentNullException(nameof(ui));
            }

            // Log registered user statistics
            if (IsLoggingEnabled(siteName) && Service.Resolve<IAnalyticsConsentProvider>().HasConsentForLogging() && TrackRegisteredUsersEnabled(siteName))
            {
                // Log new user - username and userid 
                HitLogProvider.LogHit(HitLogProvider.REGISTEREDUSER, siteName, null, UserInfoProvider.TrimSitePrefix(ui.UserName), ui.UserID);
            }
        }


        /// <summary>
        /// Logs keywords.
        /// </summary>
        /// <param name="siteName">Site name to log</param>
        /// <param name="documentCulture">Document's culture</param>
        /// <param name="searchKeywords">Keywords to lock</param>
        /// <param name="objectID">Object ID</param>
        /// <param name="count">Count of keywords to log</param>
        public static void LogSearchKeywords(string siteName, string documentCulture, string searchKeywords, int objectID, int count)
        {
            // Do not log empty keywords
            if (!String.IsNullOrEmpty(searchKeywords.Trim()))
            {
                if (Service.Resolve<IAnalyticsConsentProvider>().HasConsentForLogging() && TrackSearchKeywordsEnabled(siteName))
                {
                    HitLogProvider.LogHit(HitLogProvider.SEARCHKEYWORD, siteName, documentCulture, searchKeywords, objectID, count);
                }
            }
        }


        /// <summary>
        /// Logs average time on page
        /// </summary>
        /// <param name="siteName">Site name to log</param>
        /// <param name="documentCulture">Document's culture</param>
        /// <param name="objectName">Object name</param>
        /// <param name="objectID">Object ID</param>
        /// <param name="count">Count of logged items</param>
        /// <param name="seconds">Time spent on given page</param>
        public static void LogAverageTimeOnPage(string siteName, string documentCulture, string objectName, int objectID, int count, int seconds)
        {
            if (Service.Resolve<IAnalyticsConsentProvider>().HasConsentForLogging() && TrackAverageTimeOnPage(siteName))
            {
                HitLogProvider.LogHit(HitLogProvider.AVGTIMEONPAGE, siteName, documentCulture, objectName, objectID, count, seconds);
            }
        }


        /// <summary>
        /// Logs exit page candidate
        /// </summary>
        /// <param name="siteName">Site name to log</param>
        /// <param name="documentCulture">Document's culture</param>
        /// <param name="objectName">Object name</param>
        /// <param name="objectID">Object ID</param>
        /// <param name="count">Count of logged items</param>
        public static void LogExitPageCandidate(string siteName, string documentCulture, string objectName, int objectID, int count)
        {
            if (Service.Resolve<IAnalyticsConsentProvider>().HasConsentForLogging() && TrackExitPageEnabled(siteName))
            {
                HitLogProvider.LogHit(HitLogProvider.EXITPAGECANDIDATE, siteName, documentCulture, objectName, objectID, count);
            }
        }


        /// <summary>
        /// Logs exit page
        /// </summary>
        /// <param name="siteName">Site name to log</param>
        /// <param name="documentCulture">Document's culture</param>
        /// <param name="objectName">Object name</param>
        /// <param name="objectID">Object ID</param>
        /// <param name="count">Count of logged items</param>
        public static void LogExitPage(string siteName, string documentCulture, string objectName, int objectID, int count)
        {
            if (Service.Resolve<IAnalyticsConsentProvider>().HasConsentForLogging() && TrackExitPageEnabled(siteName))
            {
                HitLogProvider.LogHit(HitLogProvider.EXITPAGE, siteName, documentCulture, objectName, objectID, count);
            }
        }


        /// <summary>
        /// Logs onsite search keywords.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="aliasPath">Node alias path (path with results webpart)</param>
        /// <param name="documentCulture">Document's culture code</param>
        /// <param name="keywords">Keywords to log</param>
        /// <param name="objectID">Object ID</param>
        /// <param name="count">Count to log</param>
        public static void LogOnSiteSearchKeywords(string siteName, string aliasPath, string documentCulture, string keywords, int objectID, int count)
        {
            if (IsLoggingEnabled(siteName, aliasPath) && Service.Resolve<IAnalyticsConsentProvider>().HasConsentForLogging() && !IsSearchEngineExcluded(siteName))
            {
                if (TrackOnSiteKeywords(siteName))
                {
                    HitLogProvider.LogHit(HitLogProvider.ONSITESEARCHKEYWORD, siteName, documentCulture, keywords, objectID, count);
                }
            }
        }


        /// <summary>
        /// Logs landing page.
        /// </summary>
        /// <param name="siteName">Site name to log</param>
        /// <param name="documentCulture">Document's culture</param>
        /// <param name="objectName">Object name</param>
        /// <param name="objectID">Object ID</param>
        /// <param name="count">Count of keywords to log</param>
        public static void LogLandingPage(string siteName, string documentCulture, string objectName, int objectID, int count)
        {
            if (Service.Resolve<IAnalyticsConsentProvider>().HasConsentForLogging() && TrackLandingPageEnabled(siteName))
            {
                HitLogProvider.LogHit(HitLogProvider.LANDINGPAGE, siteName, documentCulture, objectName, objectID, count);
            }
        }


        /// <summary>
        /// Logs crawler visits
        /// </summary>
        /// <param name="siteName">Site name to log</param>
        /// <param name="nodeAliasPath">Node alias path</param>
        /// <param name="documentCulture">Document's culture</param>
        /// <param name="objectName">Object name</param>
        /// <param name="objectID">Object ID</param>
        /// <param name="count">Count of keywords to log</param>
        public static void LogSearchCrawler(string siteName, string nodeAliasPath, string documentCulture, string objectName, int objectID, int count)
        {
            if (TrackSearchCrawlersEnabled(siteName) &&
                AnalyticsEnabled(siteName) &&
                Service.Resolve<IAnalyticsConsentProvider>().HasConsentForLogging() &&
                !IsIPExcluded(siteName, RequestContext.UserHostAddress) &&
                !IsFileExtensionExcluded(siteName, RequestContext.CurrentUrlExtension) &&
                !IsURLExcluded(siteName, nodeAliasPath))
            {
                HitLogProvider.LogHit(HitLogProvider.CRAWLER, siteName, documentCulture, objectName, objectID, count);
            }
        }


        /// <summary>
        /// Track traffic sources (used by search engines, direct url, inner site links..).
        /// </summary>
        /// <param name="codeName">Type of referring site</param>
        /// <param name="siteName">Site name to log</param>
        /// <param name="documentCulture">Document culture</param>
        /// <param name="objectName">Object name</param>
        /// <param name="objectID">Object ID</param>
        /// <param name="count">Count of views</param>
        public static void LogReferringSite(string codeName, string siteName, string documentCulture, string objectName, int objectID, int count)
        {
            if (!Service.Resolve<IAnalyticsConsentProvider>().HasConsentForLogging())
            {
                return;
            }

            bool enabled;
            switch (codeName)
            {
                case HitLogProvider.REFERRINGSITE + "_search":
                    enabled = TrackSearchEnginesEnabled(siteName);
                    break;

                case HitLogProvider.REFERRINGSITE + "_local":
                    enabled = TrackReferringSitesLocal(siteName);
                    break;

                case HitLogProvider.REFERRINGSITE + "_direct":
                    enabled = TrackReferringSitesDirect(siteName);
                    break;

                case HitLogProvider.REFERRINGSITE + "_referring":
                    enabled = TrackReferringSitesReferring(siteName);
                    break;

                default:
                    return;
            }

            if (enabled)
            {
                HitLogProvider.LogHit(codeName, siteName, documentCulture, objectName, objectID, count);
            }
        }


        /// <summary>
        /// Tracks registered user conversion.
        /// </summary>
        /// <param name="trackConversionName">Conversion name</param>
        /// <param name="conversionCount">Value of tracked conversion</param>
        /// <param name="siteName">Site name</param>
        /// <param name="ui">User info</param>
        /// <exception cref="ArgumentException">Thrown when conversion name is empty</exception>
        public static void TrackRegisteredUserConversion(string trackConversionName, double conversionCount, string siteName, UserInfo ui)
        {
            if (String.IsNullOrEmpty(trackConversionName))
            {
                throw new ArgumentException("[AnalyticsHelper.TrackRegisteredUserConversion]: Conversion name cannot be empty.");
            }

            if (IsLoggingEnabled(siteName))
            {
                string objectName = MacroResolver.Resolve(trackConversionName);
                HitLogProvider.LogHit(HitLogProvider.CONVERSIONS, siteName, null, objectName, ui.UserID, conversionCount);
            }
        }


        /// <summary>
        /// Returns visitor status by IP visit
        /// </summary>
        /// <param name="ip">IP address</param>
        /// <param name="siteName">Site name</param>
        /// <param name="idleExpired">Indicates whether idle time expired</param>
        public static VisitorStatusEnum GetVisitorStatusByIP(string ip, string siteName, ref bool idleExpired)
        {
            // Set unknown status by default
            VisitorStatusEnum status = VisitorStatusEnum.Unknown;

            // Check whether IP check is enabled
            if (Service.Resolve<IAnalyticsConsentProvider>().HasConsentForLogging() && VisitorsSmartCheckingTime(siteName) > 0)
            {
                var ht = SiteList[siteName];
                if (ht == null)
                {
                    lock (mIpLocker)
                    {
                        ht = SiteList[siteName];
                        if (ht == null)
                        {
                            ht = new Hashtable();

                            SetupIPExpiration(siteName);
                        }
                    }
                }

                // Check status and idle expiration
                double ticks = ValidationHelper.GetDouble(ht[ip], 0);

                // Check idle
                if (ticks > 0)
                {
                    long diff = (long)TimeSpan.FromTicks(DateTime.Now.Ticks).TotalMinutes - GetVisitorStatusIdle(siteName);
                    if (diff > ticks)
                    {
                        idleExpired = true;
                        status = VisitorStatusEnum.MoreVisits;
                    }
                    else
                    {
                        status = VisitorStatusEnum.FirstVisit;
                    }
                }
            }
            return status;
        }


        /// <summary>
        /// Logs given IP address to the IP table
        /// </summary>
        /// <param name="ip">IP address</param>
        /// <param name="siteName">Site name</param>
        public static void LogIPVisit(string ip, string siteName)
        {
            LogIPVisit(ip, siteName, DateTime.Now);
        }


        /// <summary>
        /// Logs given IP address to the IP table
        /// </summary>
        /// <param name="ip">IP address</param>
        /// <param name="siteName">Site name</param>
        /// <param name="lastActivityDate">DateTime corresponding to the last activity date</param>
        public static void LogIPVisit(string ip, string siteName, DateTime lastActivityDate)
        {
            // Set by IP if required
            if (Service.Resolve<IAnalyticsConsentProvider>().HasConsentForLogging() && VisitorsSmartCheckingTime(siteName) > 0)
            {
                var ht = SiteList[siteName];

                if (ht == null)
                {
                    lock (mIpLocker)
                    {
                        ht = SiteList[siteName] ?? new Hashtable();
                    }
                }

                ht[ip] = TimeSpan.FromTicks(lastActivityDate.Ticks).TotalMinutes;
                SiteList[siteName] = ht;
            }
        }


        /// <summary>
        /// Logs user registration into the web analytics and if conversion name and value are set, the conversion is tracked.
        /// </summary>
        /// <param name="siteName">Site name.</param>
        /// <param name="user">Registered user.</param>
        /// <param name="conversionName">Name of the conversion which shall be tracked. If null or empty, conversion is not tracked.</param>
        /// <param name="conversionValue">Conversion value.</param>
        /// <exception cref="ArgumentNullException">Argument user cannot be null</exception>
        public static void TrackUserRegistration(string siteName, UserInfo user, string conversionName = null, double conversionValue = 0)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            // Log to analytics conversion 
            if (!String.IsNullOrEmpty(conversionName))
            {
                TrackRegisteredUserConversion(conversionName, conversionValue, siteName, user);
            }

            // Log to analytics registration
            if (user.Enabled)
            {
                LogRegisteredUser(siteName, user);
            }
        }

        #endregion
    }
}
