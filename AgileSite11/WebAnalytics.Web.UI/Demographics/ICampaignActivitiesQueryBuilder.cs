using CMS;
using CMS.Activities;
using CMS.DataEngine;
using CMS.WebAnalytics.Web.UI;

[assembly: RegisterImplementation(typeof(ICampaignActivitiesQueryBuilder), typeof(CampaignActivitiesQueryBuilder), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.WebAnalytics.Web.UI
{
    internal interface ICampaignActivitiesQueryBuilder
    {
        ObjectQuery<ActivityInfo> GetActivitiesQuery(CampaignConversionInfo campaignConversion, string utmSource, string utmContent);
    }
}