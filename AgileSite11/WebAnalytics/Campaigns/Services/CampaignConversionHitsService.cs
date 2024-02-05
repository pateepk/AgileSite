using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.Base;
using CMS.Helpers;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Provides access to the campaign report source data.
    /// </summary>
    internal class CampaignConversionHitsService : ICampaignConversionHitsService
    {
        /// <summary>
        /// Returns dictionary with campaign report source data. The key represents <see cref="CampaignConversionInfo.CampaignConversionID"/> ID 
        /// and value is a List collection of conversion related <see cref="CampaignConversionHitsInfo"/> objects. 
        /// </summary>
        /// <param name="campaignID">ID of <see cref="CampaignInfo"/> object for which should be report data returned.</param>
        /// <returns>Campaign report source data.</returns>
        public Dictionary<int, List<CampaignConversionHitsInfo>> GetCampaignHits(int campaignID)
        {
            // Get all conversion from given campaign
            var conversions = CampaignConversionInfoProvider.GetCampaignConversions()
                .WhereEquals("CampaignConversionCampaignID", campaignID)
                .OrderBy("CampaignConversionOrder")
                .ToList();

            var conversionIds = conversions.Select(c => c.CampaignConversionID).ToList();

            return CampaignConversionHitsInfoProvider.GetCampaignConversionHits()
                .WhereIn("CampaignConversionHitsConversionID", conversionIds)
                .ToList()
                .GroupBy(h => h.CampaignConversionHitsConversionID)
                .ToDictionary(g => g.Key, g => g.ToList());
        }


        /// <summary>
        /// Updates number of hits in <see cref="CampaignConversionHitsInfo.CampaignConversionHitsCount"/> 
        /// or inserts new <see cref="CampaignConversionHitsInfo"/> object if related hit record does not exist yet.
        /// </summary>
        /// <param name="campaignID">ID of <see cref="CampaignInfo"/> object for which is report calculated.</param>
        /// <param name="calculatedHits">Calculated source <see cref="DataTable"/> to upsert <see cref="CampaignConversionHitsInfo"/> objects with following table definition:
        /// <code>
        ///     var table = new DataTable();
        ///     table.TableName = "HitsToUpsertTable";
        ///     table.Columns.Add("Hits", typeof(int));
        ///     table.Columns.Add("CampaignConversionID", typeof(int));
        ///     table.Columns.Add("ActivityUTMSource", typeof(string));
        /// </code>
        /// The order of column must be preserved.
        /// </param>
        /// <param name="calculatedTo">Datetime of last report processing.</param>
        public void UpsertCampaignConversionHits(int campaignID, DataTable calculatedHits, DateTime calculatedTo)
        {
            if (!DataHelper.DataSourceIsEmpty(calculatedHits))
            {
                CampaignConversionHitsInfoProvider.BulkUpsertCampaignConversionHits(calculatedHits, campaignID);
            }

            SetReportCalculatedTo(campaignID, calculatedTo);
        }


        /// <summary>
        /// Updates campaign to store information about last report processing. 
        /// </summary>
        /// <param name="campaignID">ID of <see cref="CampaignInfo"/> object to be updated.</param>
        /// <param name="calculatedTo">Datetime of last report processing.</param>
        private void SetReportCalculatedTo(int campaignID, DateTime calculatedTo)
        {
            var campaign = CampaignInfoProvider.GetCampaignInfo(campaignID);
            if (campaign != null)
            {
                campaign.CampaignCalculatedTo = calculatedTo;

                using (new CMSActionContext() { LogEvents = false, LogSynchronization = false, LogIntegration = false })
                {
                    CampaignInfoProvider.SetCampaignInfo(campaign);
                }
            }
        }
    }
}
