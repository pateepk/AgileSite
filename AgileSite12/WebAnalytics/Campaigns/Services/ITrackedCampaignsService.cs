using System.Collections.Generic;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Provides access to the tracked campaigns.
    /// </summary>
    public interface ITrackedCampaignsService
    {
        /// <summary>
        /// Gets enumerable of campaigns which should be logged in web analytics service called by Javascript.
        /// </summary>
        IEnumerable<string> TrackedCampaigns
        {
            get;
        }


        /// <summary>
        /// Adds new campaign to TrackedCampaigns collection. Adding is performed only when campaign is not already present.
        /// </summary>
        /// <param name="campaignName">Codename of campaign</param>
        void AddTrackedCampaign(string campaignName);


        /// <summary>
        /// Removes TrackedCampaign cookie from cookie collection.
        /// </summary>
        void RemoveTrackedCampaigns();
    }
}