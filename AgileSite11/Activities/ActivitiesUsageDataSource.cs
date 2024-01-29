using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using CMS.Activities;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DataEngine.Query;

using Newtonsoft.Json;

[assembly: RegisterModuleUsageDataSource(typeof(ActivitiesUsageDataSource))]

namespace CMS.Activities
{
    /// <summary>
    /// Module usage tracking data source for activities module.
    /// </summary>
    internal class ActivitiesUsageDataSource : IModuleUsageDataSource
    {
        /// <summary>
        /// Settings keys which are logged.
        /// </summary>
        private static readonly ReadOnlyCollection<string> mSettingsToLog = new List<string>
        {
            // On-line marketing -> Contact management -> Activities
            "CMSCMLandingPage",
            "CMSCMPageVisits",
            "CMSCMUserLogin",
            "CMSCMUserRegistration",
            "CMSCMAddingProductToSC",
            "CMSCMAddingProductToWL",
            "CMSCMPurchase",
            "CMSCMPurchasedProduct",
            "CMSCMRemovingProductFromSC",
            "CMSCMEmailOpening",
            "CMSCMClickthroughTracking",
            "CMSCMNewsletterSubscribe",
            "CMSCMNewsletterUnsubscribe",
            "CMSCMNewsletterUnsubscribedFromAll",
            "CMSCMExternalSearch",
            "CMSCMSearch",
            "CMSCMContentRating",
            "CMSCMPollVoting",
            "CMSCMBizFormSubmission",
            "CMSCMEventBooking",
            "CMSCMCustomActivities",
            "CMSCMCustomTableForm",
            "CMSCMBlogPostSubscription",
            "CMSCMBlogPostComments",
            "CMSCMForumPostSubscription",
            "CMSCMForumPosts",
            "CMSCMMessageBoardPosts",
            "CMSCMMessageBoardSubscription",
            "CMSCMActivitiesEnabled",
        }.AsReadOnly();


        /// <summary>
        /// Get the data source name.
        /// </summary>
        public string Name
        {
            get
            {
                return "CMS.Activities";
            }
        }


        /// <summary>
        /// Get all module statistical data.
        /// </summary>
        public IModuleUsageDataCollection GetData()
        {
            var usageData = ObjectFactory<IModuleUsageDataCollection>.New();

            usageData.Add("ActivitiesSettingsUsageOnSites", JsonConvert.SerializeObject(GetSettingsUsage(global: false)));
            usageData.Add("ActivitiesSettingsUsageGlobal", JsonConvert.SerializeObject(GetSettingsUsage(global: true).Select(s => new { s.KeyName, s.KeyValue })));

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
