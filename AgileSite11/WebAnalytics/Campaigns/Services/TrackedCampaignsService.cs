using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS;
using CMS.Helpers;
using CMS.WebAnalytics;

[assembly: RegisterImplementation(typeof(ITrackedCampaignsService), typeof(TrackedCampaignsService), Priority = CMS.Core.RegistrationPriority.SystemDefault)]

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Provides access to the tracked campaigns.
    /// </summary>
    public class TrackedCampaignsService : ITrackedCampaignsService
    {
        /// <summary>
        /// Gets enumerable of campaigns which should be logged in web analytics service called by Javascript.
        /// </summary>
        public IEnumerable<string> TrackedCampaigns
        {
            get
            {
                string campaigns = CookieHelper.GetValue(CookieName.TrackedCampaigns);
                if (string.IsNullOrEmpty(campaigns))
                {
                    return Enumerable.Empty<string>();
                }

                return campaigns.Split(new[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            }
        }


        /// <summary>
        /// Adds new campaign to TrackedCampaigns collection. Adding is performed only when campaign is not already present.
        /// </summary>
        /// <param name="campaignName">Codename of campaign</param>
        public void AddTrackedCampaign(string campaignName)
        {
            if (string.IsNullOrEmpty(campaignName))
            {
                throw new ArgumentNullException("campaignName");
            }

            var set = new HashSet<string>(TrackedCampaigns)
            {
                campaignName
            };

            CookieHelper.SetValue(CookieName.TrackedCampaigns, string.Join("|", set), DateTime.Now.AddMinutes(20));
        }


        /// <summary>
        /// Removes TrackedCampaign cookie from cookie collection.
        /// </summary>
        public void RemoveTrackedCampaigns()
        {
            CookieHelper.Remove(CookieName.TrackedCampaigns);
        }
    }
}
