using CMS.Activities;
using CMS.SiteProvider;
using CMS.WebAnalytics.Internal;

namespace CMS.WebAnalytics
{
    internal class UrlHashActivityModifier : IActivityModifier
    {
        private readonly IActivityUrlHashService mHashService;


        public UrlHashActivityModifier(IActivityUrlHashService hashService)
        {
            mHashService = hashService;
        }


        public void Modify(IActivityInfo activity)
        {
            if (IsPageVisit(activity) && SiteIsContentOnly(activity.ActivitySiteID))
            {
                activity.ActivityURLHash = mHashService.GetActivityUrlHash(activity.ActivityURL);
            }
        }


        private bool IsPageVisit(IActivityInfo activity)
        {
            return activity.ActivityType == PredefinedActivityType.PAGE_VISIT;
        }


        private bool SiteIsContentOnly(int siteId)
        {
            var site = SiteInfoProvider.GetSiteInfo(siteId);

            return (site != null) && site.SiteIsContentOnly;
        }
    }
}
