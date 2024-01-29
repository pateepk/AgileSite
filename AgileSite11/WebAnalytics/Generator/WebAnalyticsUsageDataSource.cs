using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.WebAnalytics;

using Newtonsoft.Json;

[assembly: RegisterModuleUsageDataSource(typeof(WebAnalyticsUSageDataSource))]

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Module usage tracking data source for web analytics module.
    /// </summary>
    internal class WebAnalyticsUSageDataSource : IModuleUsageDataSource
    {
        /// <summary>
        /// Settings keyes which are logged.
        /// </summary>
        private static readonly ReadOnlyCollection<string> mSettingsToLog = new List<string>
        {
            // On-line marketing -> Web analytics
            "CMSAnalyticsEnabled",
            "CMSWebAnalyticsUseJavascriptLogging",
            "CMSAnalyticsExcludeSearchEngines",
            "CMSAnalyticsTrackBrowserTypes",
            "CMSAnalyticsTrackCountries",
            "CMSAnalyticsTrackRegisteredUsers",
            "CMSAnalyticsTrackVisits",
            "CMSTrackMobileDevices",
            "CMSTrackSearchCrawlers",
            "CMSAnalyticsTrackFileDownloads",
            "CMSAnalyticsTrackInvalidPages",
            "CMSAnalyticsTrackPageViews",
            "CMSAnalyticsTrackAggregatedViews",
            "CMSTrackExitPages",
            "CMSTrackLandingPages",
            "CMSTrackAverageTimeOnPage",
            "CMSTrackSearchKeywords",
            "CMSTrackOnSiteKeywords",
            "CMSTrackSearchEngines",
            "CMSTrackReferringSitesLocal",
            "CMSTrackReferringSitesReferring",
            "CMSTrackReferringSitesDirect",
            "CMSAnalyticsTrackReferrals",
        }.AsReadOnly();


        /// <summary>
        /// Get the data source name.
        /// </summary>
        public string Name
        {
            get
            {
                return "CMS.WebAnalytics";
            }
        }


        /// <summary>
        /// Get all module statistical data.
        /// </summary>
        public IModuleUsageDataCollection GetData()
        {
            var usageData = ObjectFactory<IModuleUsageDataCollection>.New();

            usageData.Add("WASettingsUsageOnSites", JsonConvert.SerializeObject(GetSettingsUsage(global: false)));
            usageData.Add("WASettingsUsageGlobal", JsonConvert.SerializeObject(GetSettingsUsage(global: true).Select(s => new { s.KeyName, s.KeyValue })));

            return usageData;
        }


        /// <summary>
        /// Returns how many times is specific setting used with value which is not same as default value. 
        /// Only settings on running sites (if <paramref name="global"/> is true), not hidden and not custom are retrieved.
        /// Only global settings when <paramref name="global"/> is false.
        /// </summary>
        internal IEnumerable<dynamic> GetSettingsUsage(bool global = false)
        {
            var query = SettingsKeyInfoProvider.GetSettingsKeys()
                                               .Columns("KeyName", "KeyValue")
                                               .AddColumn(new CountColumn("KeyValue").As("Count"))
                                               .WhereEquals("KeyType", "boolean")
                                               .WhereIn("KeyName", mSettingsToLog);
            if (global)
            {
                query.WhereNull("SiteID")
                     .WhereNotEquals("KeyDefaultValue", "KeyValue".AsColumn());
            }
            else
            {
                query.WhereNotNull("SiteID");
            }

            return query.GroupBy("KeyName", "KeyValue")
                        .OrderBy("KeyName")
                        .Select(dataRow => new
                        {
                            KeyName = dataRow[0].ToString(),
                            KeyValue = dataRow[1].ToBoolean(false),
                            Count = dataRow[2].ToInteger(0),
                        });
        }
    }
}
