using System;

using CMS.Base;

namespace CMS.Activities
{
    /// <summary>
    /// Adds general data to activity.
    /// </summary>
    internal class ActivityModifier : IActivityModifier
    {
        private readonly ISiteService mSiteService;
        private readonly IActivityUrlService mActivityUrlService;


        /// <summary>
        /// Initializes new instance of <see cref="ActivityModifier"/>.
        /// </summary>
        /// <param name="siteService">Site service</param>
        /// <param name="activityUrlService">Service to insert correct URL in activity</param>
        /// <exception cref="ArgumentNullException">Is thrown if <paramref name="siteService"/> or <paramref name="activityUrlService"/> is null.</exception>
        public ActivityModifier(ISiteService siteService, IActivityUrlService activityUrlService)
        {
            mSiteService = siteService ?? throw new ArgumentNullException(nameof(siteService));
            mActivityUrlService = activityUrlService ?? throw new ArgumentNullException(nameof(activityUrlService));
        }


        /// <summary>
        /// Initializes activity URLs, site id and IP address.
        /// </summary>
        /// <param name="activity"></param>
        public void Modify(IActivityInfo activity)
        {
            if (activity.ActivitySiteID == 0)
            {
                activity.ActivitySiteID = mSiteService.CurrentSite.SiteID;
            }

            SetActivityUrl(activity);
            SetActivityReferrer(activity);
        }


        private void SetActivityUrl(IActivityInfo activity)
        {
            if (activity.ActivityURL == null)
            {
                activity.ActivityURL = mActivityUrlService.GetActivityUrl();
            }
        }


        private void SetActivityReferrer(IActivityInfo activity)
        {
            if (activity.ActivityURLReferrer == null)
            {
                activity.ActivityURLReferrer = mActivityUrlService.GetActivityUrlReferrer();
            }
        }
    }
}