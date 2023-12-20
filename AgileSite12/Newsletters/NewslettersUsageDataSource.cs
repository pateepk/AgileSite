using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.Newsletters;
using CMS.Helpers;

using Newtonsoft.Json;

[assembly: RegisterModuleUsageDataSource(typeof(NewslettersUsageDataSource))]

namespace CMS.Newsletters
{
    /// <summary>
    /// Contains methods for retrieving statisticts of newsletter module usage.
    /// </summary>
    internal class NewslettersUsageDataSource : IModuleUsageDataSource
    {
        /// <summary>
        /// Settings keys which are logged.
        /// </summary>
        private static readonly ReadOnlyCollection<string> mSettingsToLog = new List<string>
        {
            // On-line marketing -> Email marketing
            "CMSGenerateNewsletters",
            "CMSNewsletterUseExternalService",
            "CMSMonitorBouncedEmails",
            "CMSBlockSubscribersGlobally",
        }.AsReadOnly();


        /// <summary>
        /// Get the data source name.
        /// </summary>
        public string Name
        {
            get
            {
                return "CMS.Newsletters";
            }
        }


        /// <summary>
        /// Get all module statistical data.
        /// </summary>
        public IModuleUsageDataCollection GetData()
        {
            var usageData = ObjectFactory<IModuleUsageDataCollection>.New();
            var newsletters = NewsletterInfoProvider.GetNewsletters().ToList();
            var settings = new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeHtml };

            usageData.Add("NewslettersCount", newsletters.Count(n => n.NewsletterType == EmailCommunicationTypeEnum.Newsletter));
            usageData.Add("EmailCampaignsCount", newsletters.Count(n => n.NewsletterType == EmailCommunicationTypeEnum.EmailCampaign));
            usageData.Add("DynamicNewslettersCount", GetDynamicNewslettersCount(newsletters));
            usageData.Add("IssuesInNewslettersCount", JsonConvert.SerializeObject(GetIssuesInNewslettersCount(newsletters, EmailCommunicationTypeEnum.Newsletter), settings));
            usageData.Add("IssuesInEmailCampaignCount", JsonConvert.SerializeObject(GetIssuesInNewslettersCount(newsletters, EmailCommunicationTypeEnum.EmailCampaign), settings));
            usageData.Add("SubscribersInNewslettersCount", JsonConvert.SerializeObject(GetSubscribersInNewslettersCount(), settings));
            usageData.Add("TotalNewsletterSubscribersCount", SubscriberInfoProvider.GetSubscribers().Count);
            usageData.Add("TemplatesCount", EmailTemplateInfoProvider.GetEmailTemplates().Count());
            usageData.Add("EmailTemplatesCount", GetNewsletterEmailTemplatesCount());
            usageData.Add("AdditionalTemplatesPerEmailFeedCount", JsonConvert.SerializeObject(GetAdditionalTemplatesInNewslettersCount(), settings));
            usageData.Add("IssuesWithABTestCount", ABTestInfoProvider.GetABTests().Count);
            usageData.Add("TrackOpenedEmailUsedInEmailFeedsCount", GetTrackOpenedEmailUsedInNewslettersCount(newsletters));
            usageData.Add("TrackClickedLinksUsedInEmailFeedsCount", GetTrackClickedLinksUsedInNewslettersCount(newsletters));
            usageData.Add("DoubleOptInUsedInNewslettersCount", GetDoubleOptInUsedInNewslettersCount(newsletters));

            usageData.Add("NewslettersSettingsUsageOnSites", JsonConvert.SerializeObject(GetSettingsUsage(global: false), settings));
            usageData.Add("NewslettersSettingsUsageGlobal", JsonConvert.SerializeObject(GetSettingsUsage(global: true).Select(s => new { s.KeyName, s.KeyValue }), settings));

            return usageData;
        }


        /// <summary>
        /// Returns number of dynamic newsletters.
        /// </summary>
        internal int GetDynamicNewslettersCount(IEnumerable<NewsletterInfo> newsletters)
        {
            return newsletters.Count(n => n.NewsletterSource == NewsletterSource.Dynamic);
        }


        /// <summary>
        /// Returns number of issues per newsletter. Newsletters without issues are ignored.
        /// </summary>
        internal List<int> GetIssuesInNewslettersCount(List<NewsletterInfo> newsletters, EmailCommunicationTypeEnum communicationType)
        {
            return IssueInfoProvider.GetIssues()
                                    .WhereIn("IssueNewsletterID", newsletters.Where(n => n.NewsletterType == communicationType).Select(n => n.NewsletterID).ToList())
                                    .Column("IssueNewsletterID")
                                    .AddColumn(new CountColumn("IssueNewsletterID").As("IssuesCount"))
                                    .GroupBy("IssueNewsletterID")
                                    .Select(i => i["IssuesCount"].ToInteger(0))
                                    .ToList();
        }


        /// <summary>
        /// Returns number of subscribers within a newsletter (group subscriber counts as 1). Newsletters without subscribers are ignored.
        /// </summary>
        internal List<int> GetSubscribersInNewslettersCount()
        {
            return SubscriberNewsletterInfoProvider.GetSubscriberNewsletters()
                                                   .Column("NewsletterID")
                                                   .AddColumn(new CountColumn("NewsletterID").As("SubscribersCount"))
                                                   .GroupBy("NewsletterID")
                                                   .Select(s => s["SubscribersCount"].ToInteger(0))
                                                   .ToList();
        }


        /// <summary>
        /// Returns number of email templates.
        /// </summary>
        internal int GetNewsletterEmailTemplatesCount()
        {
            return EmailTemplateInfoProvider.GetEmailTemplates()
                                            .WhereEquals("TemplateType", EmailTemplateTypeEnum.Issue.ToStringRepresentation())
                                            .Count;
        }


        /// <summary>
        /// Returns number of additional templates per newsletter. Newsletters without additional templates are ignored.
        /// </summary>
        internal List<int> GetAdditionalTemplatesInNewslettersCount()
        {
            return EmailTemplateNewsletterInfoProvider.GetEmailTemplateNewsletters()
                                                      .Column("NewsletterID")
                                                      .AddColumn(new CountColumn("NewsletterID").As("TemplatesCount"))
                                                      .GroupBy("NewsletterID")
                                                      .Select(t => t["TemplatesCount"].ToInteger(0))
                                                      .ToList();
        }


        /// <summary>
        /// Returns number of newsletters where opening of email tracking is enabled.
        /// </summary>
        internal int GetTrackOpenedEmailUsedInNewslettersCount(IEnumerable<NewsletterInfo> newsletters)
        {
            return newsletters.Count(n => n.NewsletterTrackOpenEmails);
        }


        /// <summary>
        /// Returns number of newsletters where links tracking is enabled.
        /// </summary>
        internal int GetTrackClickedLinksUsedInNewslettersCount(IEnumerable<NewsletterInfo> newsletters)
        {
            return newsletters.Count(n => n.NewsletterTrackClickedLinks);
        }


        /// <summary>
        /// Returns number of newsletters where double opt-in functionality is enabled.
        /// </summary>
        internal int GetDoubleOptInUsedInNewslettersCount(IEnumerable<NewsletterInfo> newsletters)
        {
            return newsletters.Count(n => n.NewsletterEnableOptIn && n.NewsletterType == EmailCommunicationTypeEnum.Newsletter);
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
