using CMS.Activities;
using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.SiteProvider;
using CMS.WebAnalytics.Internal;

namespace CMS.WebAnalytics.Web.UI
{
    internal class CampaignActivitiesQueryBuilder : ICampaignActivitiesQueryBuilder
    {
        private readonly IActivityUrlHashService mActivityUrlHashService;
        private readonly ISiteService mSiteService;


        public CampaignActivitiesQueryBuilder(IActivityUrlHashService activityUrlHashService, ISiteService siteService)
        {
            mActivityUrlHashService = activityUrlHashService;
            mSiteService = siteService;
        }


        public ObjectQuery<ActivityInfo> GetActivitiesQuery(CampaignConversionInfo campaignConversion, string utmSource, string utmContent)
        {
            var campaign = CampaignInfoProvider.GetCampaignInfo(campaignConversion.CampaignConversionCampaignID);
            if (campaign.CampaignSiteID != mSiteService.CurrentSite.SiteID)
            {
                throw new PermissionException("Cannot read campaign data from a different site.");    
            }

            string utmCampaign = campaign.CampaignUTMCode;

            return campaignConversion.CampaignConversionActivityType == PredefinedActivityType.PAGE_VISIT ?
                GetActivitiesQueryForPageVisit(campaignConversion.CampaignConversionItemID, campaignConversion.CampaignConversionURL, utmCampaign, utmSource, utmContent, SiteIsContentOnly(campaign)) :
                GetActivitiesQueryForOtherTypes(campaignConversion.CampaignConversionActivityType, campaignConversion.CampaignConversionItemID, utmCampaign, utmSource, utmContent);
        }


        private static bool SiteIsContentOnly(CampaignInfo campaign)
        {
            var site = SiteInfoProvider.GetSiteInfo(campaign.CampaignSiteID);
            return site.SiteIsContentOnly;
        }


        private ObjectQuery<ActivityInfo> GetActivitiesQueryForOtherTypes(string activityType, int? activityItemID, string utmCampaign, string utmSource, string utmContent)
        {
            return GetCommonActivitiesQuery(activityType, utmCampaign, utmSource, utmContent).WhereEqualsOrNull("ActivityItemID", activityItemID);
        }


        private ObjectQuery<ActivityInfo> GetActivitiesQueryForPageVisit(int itemID, string activityUrl, string utmCampaign, string utmSource, string utmContent, bool isContentOnlySite)
        {
            var query = GetCommonActivitiesQuery(PredefinedActivityType.PAGE_VISIT, utmCampaign, utmSource, utmContent);
            return isContentOnlySite ? 
                UpdateActivitiesQueryForPageVisitOnMVC(query, activityUrl) : 
                UpdateActivitiesQueryForPageVisitOnCMS(query, itemID);
        }


        private ObjectQuery<ActivityInfo> UpdateActivitiesQueryForPageVisitOnCMS(ObjectQuery<ActivityInfo> query, int nodeID)
        {
            return query.WhereEquals("ActivityNodeID", nodeID);
        }


        private ObjectQuery<ActivityInfo> UpdateActivitiesQueryForPageVisitOnMVC(ObjectQuery<ActivityInfo> query, string activityUrl)
        {
            return query.WhereEquals("ActivityURLHash", mActivityUrlHashService.GetActivityUrlHash(activityUrl));
        }


        private ObjectQuery<ActivityInfo> GetCommonActivitiesQuery(string activityType, string utmCampaign, string utmSource, string utmContent)
        {
            var activities = ActivityInfoProvider.GetActivities()
                                                 .WhereEquals("ActivityType", activityType)
                                                 .WhereEquals("ActivityCampaign", utmCampaign);
                                             
            activities = IfNullSkipOtherwiseEqualsOrNull(activities, "ActivityUTMSource", utmSource);
            activities = IfNullSkipOtherwiseEqualsOrNull(activities, "ActivityUTMContent", utmContent);
                                        
            return activities;
        }


        private ObjectQuery<ActivityInfo> IfNullSkipOtherwiseEqualsOrNull(ObjectQuery<ActivityInfo> query, string column, string value)
        {
            if (value == null)
            {
                return query;
            }

            return value == string.Empty ? query.WhereEmpty(column) : query.WhereEquals(column, value);
        }
    }
}
