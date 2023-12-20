using System;
using System.Linq;

using CMS.Activities;
using CMS.EventLog;
using CMS.Helpers;
using CMS.SiteProvider;
using CMS.WebAnalytics;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Class for Log custom activity action.
    /// </summary>
    public class CustomActivityAction : ContactAutomationAction
    {
        #region "Parameters"

        /// <summary>
        /// Activity type.
        /// </summary>
        protected virtual string ActivityType
        {
            get
            {
                return GetResolvedParameter<string>("ActivityType", null);
            }
        }


        /// <summary>
        /// Activity title.
        /// </summary>
        protected virtual string ActivityTitle
        {
            get
            {
                return GetResolvedParameter<string>("ActivityTitle", null);
            }
        }


        /// <summary>
        /// Activity value.
        /// </summary>
        protected virtual string ActivityValue
        {
            get
            {
                return GetResolvedParameter<string>("ActivityValue", null);
            }
        }


        /// <summary>
        /// Activity URL.
        /// </summary>
        protected virtual string ActivityUrl
        {
            get
            {
                return GetResolvedParameter<string>("ActivityUrl", null);
            }
        }


        /// <summary>
        /// Campaign.
        /// </summary>
        protected virtual string ActivityCampaign
        {
            get
            {
                int campaignId = GetResolvedParameter("ActivityCampaign", 0);
                if (campaignId != 0)
                {
                    var campaign = CampaignInfoProvider.GetCampaigns()
                                                       .WhereEquals("CampaignID", campaignId)
                                                       .Column("CampaignUTMCode")
                                                       .FirstOrDefault();

                    return campaign == null ? null : campaign.CampaignUTMCode;
                }
                return null;
            }
        }


        /// <summary>
        /// Campaign UTM source.
        /// </summary>
        protected virtual string ActivityUTMSource
        {
            get
            {
                return GetResolvedParameter<string>("ActivityUTMSource", null);
            }
        }


        /// <summary>
        /// Comment.
        /// </summary>
        protected virtual string ActivityComment
        {
            get
            {
                return GetResolvedParameter<string>("ActivityComment", null);
            }
        }


        /// <summary>
        /// Site name.
        /// </summary>
        protected virtual string ActivitySiteName
        {
            get
            {
                return GetResolvedParameter("ActivitySiteName", String.Empty);
            }
        }

        #endregion


        /// <summary>
        /// Executes action.
        /// </summary>
        public override void Execute()
        {
            int siteId = SiteInfoProvider.GetSiteID(ActivitySiteName);
            if (siteId <= 0)
            {
                LogMessage(EventType.WARNING, "LOGCUSTOMACTIVITY", ResHelper.GetAPIString("ma.action.missingSite", "The selected site was not found."), InfoObject);
                return;
            }

            if (Contact == null)
            {
                return;
            }

            var activity = new ActivityInfo
            {
                ActivityTitle = ActivityTitle,
                ActivityType = ActivityType,
                ActivityValue = ActivityValue,
                ActivityURL = ActivityUrl,
                ActivityCampaign = ActivityCampaign,
                ActivityUTMSource = ActivityUTMSource,
                ActivityContactID = Contact.ContactID,
                ActivityComment = ActivityComment,
                ActivitySiteID = siteId
            };

            ActivityInfoProvider.SetActivityInfo(activity);
        }
    }
}
