using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.Activities;
using CMS.DataEngine;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Calculates campaign visitors count based on relevant <see cref="ActivityInfo"/> records.
    /// </summary>
    internal class CampaignVisitorsProcessor
    {
        /// <summary>
        /// Calculates the campaign visitors count for the given site.
        /// Visitors count is stored in the <see cref="CampaignInfo.CampaignVisitors"/> property.
        /// </summary>
        /// <param name="siteID">Represents site ID on which campaigns` visitors are calculated. 
        /// If <c>0</c>, visitors for all campaigns on all sites will be calculated.</param>
        public void CalculateVisitors(int siteID)
        {
            var campaigns = GetCampaigns(siteID).ToList();
            CalculateVisitorsInternal(campaigns);
        }


        /// <summary>
        /// Calculates the campaign visitors count for the given campaign.
        /// Visitors count is stored in the <see cref="CampaignInfo.CampaignVisitors"/> property.
        /// </summary>
        /// <param name="campaign">Instance of <see cref="CampaignInfo"/> for which visitors are calculated.</param>
        public void CalculateVisitors(CampaignInfo campaign)
        {
            CalculateVisitorsInternal(new List<CampaignInfo> { campaign });
        }


        /// <summary>
        /// Calculates the campaign visitors counts for all given campaigns.
        /// Numbers for all campaigns are calculated at once using only one database query.
        /// </summary>
        /// <param name="campaigns">Instances of <see cref="CampaignInfo"/> for which visitors are calculated.</param>
        private void CalculateVisitorsInternal(List<CampaignInfo> campaigns)
        {
            if ((campaigns == null) || !campaigns.Any())
            {
                return;
            }

            // Get visitors count (unique contacts) for every campaign
            var query = new ObjectQuery<ActivityInfo>()
                .From(new QuerySource(new QuerySourceTable(new ObjectSource<ActivityInfo>(), hints: SqlHints.NOLOCK)))
                .WhereIn("ActivityCampaign", campaigns.Select(c => c.CampaignUTMCode).ToList())
                .Columns("ActivityCampaign")
                .AddColumn(new AggregatedColumn(AggregationType.Count, "DISTINCT ActivityContactID").As("Visitors"))
                .GroupBy("ActivityCampaign");

            if (!query.HasResults())
            {
                return;
            }

            var table = query.Result.Tables[0];
            var campaignsVisitors = table.AsEnumerable().ToDictionary(
                                row => row.Field<string>("ActivityCampaign"),
                                row => row.Field<int>("Visitors"));

            // Update visitors count for campaigns
            foreach (var campaign in campaigns)
            {
                if (campaignsVisitors.ContainsKey(campaign.CampaignUTMCode))
                {
                    var visitorsCount = campaignsVisitors[campaign.CampaignUTMCode];

                    if ((visitorsCount != campaign.CampaignVisitors) || (campaign.GetValue("CampaignVisitors") == null))
                    {
                        // Ensure that NULL value for CampaignVisitors is replaced with count (even for zero value)
                        // This will exclude finished campaigns from the next calculation
                        campaign.SetValue("CampaignVisitors", visitorsCount);
                        campaign.Update();
                    }
                }
            }
        }


        /// <summary>
        /// Returns query for campaigns for which visitors will be calculated.
        /// </summary>
        /// <param name="siteID">ID of the site to return campaigns for. Use <c>0</c> for all sites.</param>
        private ObjectQuery<CampaignInfo> GetCampaigns(int siteID)
        {
            var now = DateTime.Now;

            // Calculate visitors statistics for campaigns which are currently running
            // or campaigns which have stopped recently but there may be still data to process.
            var campaigns = CampaignInfoProvider.GetCampaigns()
                                                .WhereLessOrEquals("CampaignOpenFrom", now)
                                                .Where(new WhereCondition()
                                                    .WhereGreaterOrEquals("CampaignOpenTo", now)
                                                    .Or()
                                                    .WhereNull("CampaignOpenTo")
                                                    .Or()
                                                    .WhereNull("CampaignVisitors"));

            if (siteID > 0)
            {
                campaigns = campaigns.OnSite(siteID);
            }

            return campaigns;
        }
    }
}
