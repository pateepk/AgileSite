using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Activities;
using CMS.SiteProvider;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Initializes and stores data for the campaign report calculation
    /// </summary>
    internal class CampaignCalculationData
    {
        /// <summary>
        /// Campaign that the report will be calculated for
        /// </summary>
        public CampaignInfo Campaign
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether the campaign is on content only site
        /// </summary>
        public bool SiteIsContentOnly
        {
            get;
            set;
        }


        /// <summary>
        /// List of all conversions within the <see cref="Campaign"/> 
        /// </summary>
        public List<CampaignConversionInfo> Conversions
        {
            get;
            set;
        }


        /// <summary>
        /// Identifier of the first activity which is not included in the report yet
        /// </summary>
        public int StartActivityID
        {
            get;
            set;
        }


        /// <summary>
        /// List of all distinct conversion types within the <see cref="Campaign"/> 
        /// </summary>
        public IEnumerable<CampaignConversionType> DistinctConversionTypes
        {
            get;
            set;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="CampaignCalculationData"/>.
        /// </summary>
        /// <param name="campaign">Campaign</param>
        public CampaignCalculationData(CampaignInfo campaign)
        {
            if (campaign == null)
            {
                throw new ArgumentNullException("campaign");
            }

            Campaign = campaign;
            Conversions = GetConversions();
            StartActivityID = GetFirstActivityID();
            SiteIsContentOnly = IsSiteContentOnly();
            DistinctConversionTypes = GetConversionTypes();
        }
        

        /// <summary>
        /// Finds ID of the first activity which is not included in the report.
        /// </summary>
        private int GetFirstActivityID()
        {
            // Rely on ActivityID rising over time
            var firstActivity = ActivityInfoProvider.GetActivities()
                                                    .TopN(1)
                                                    .Columns("ActivityID")
                                                    .WhereGreaterThan("ActivityCreated", Campaign.CampaignCalculatedTo)
                                                    .FirstOrDefault();
            if (firstActivity != null)
            {
                return firstActivity.ActivityID;
            }

            return int.MaxValue;
        }


        private List<CampaignConversionInfo> GetConversions()
        {
            return CampaignConversionInfoProvider.GetCampaignConversions()
                                                 .WhereEquals("CampaignConversionCampaignID", Campaign.CampaignID)
                                                 .ToList();
        }


        private bool IsSiteContentOnly()
        {
            var site = SiteInfoProvider.GetSiteInfo(Campaign.CampaignSiteID);
            return site.SiteIsContentOnly;
        }


        /// <summary>
        /// Get all conversion types present in campaign.
        /// </summary>
        private IEnumerable<CampaignConversionType> GetConversionTypes()
        {
            var result = new List<CampaignConversionType>();

            foreach (var type in GetConversionTypesForSpecificItem())
            {
                result.Add(new CampaignConversionType(type, false));
            }

            foreach (var type in GetConversionTypesForAllItems())
            {
                result.Add(new CampaignConversionType(type, true));
            }

            return result;
        }


        /// <summary>
        /// Returns conversions for which hits results should be grouped by identifier.
        /// </summary>
        /// <remarks>
        /// For <see cref="PredefinedActivityType.PAGE_VISIT"/> is the grouping value calculated dynamically for campaigns running on <see cref="SiteInfo.SiteIsContentOnly"/> site.
        /// </remarks>
        private IEnumerable<string> GetConversionTypesForSpecificItem()
        {
            return Conversions.Where(c => (c.CampaignConversionItemID != 0) || (c.CampaignConversionActivityType == PredefinedActivityType.PAGE_VISIT))
                .Select(c => c.CampaignConversionActivityType)
                .Distinct();
        }


        /// <summary>
        /// Returns conversions for which total count of hits should be returned.
        /// </summary>
        /// <remarks>
        /// For <see cref="PredefinedActivityType.PAGE_VISIT"/> is the grouping value calculated dynamically for campaigns running on <see cref="SiteInfo.SiteIsContentOnly"/> site.
        /// </remarks>
        private IEnumerable<string> GetConversionTypesForAllItems()
        {
            return Conversions.Where(c => (c.CampaignConversionItemID == 0) && (c.CampaignConversionActivityType != PredefinedActivityType.PAGE_VISIT))
                .Select(c => c.CampaignConversionActivityType)
                .Distinct();
        }
    }
}
