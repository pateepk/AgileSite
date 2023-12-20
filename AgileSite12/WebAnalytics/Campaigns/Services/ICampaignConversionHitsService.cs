using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.WebAnalytics;

[assembly: RegisterImplementation(typeof(ICampaignConversionHitsService), typeof(CampaignConversionHitsService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Provides access to the campaign report source data.
    /// </summary>
    public interface ICampaignConversionHitsService
    {
        /// <summary>
        /// Returns dictionary with campaign report source data. The key represents <see cref="CampaignConversionInfo.CampaignConversionID"/> ID 
        /// and value is a List collection of related <see cref="CampaignConversionHitsInfo"/> objects.
        /// </summary>
        /// <param name="campaignID">ID of <see cref="CampaignInfo"/> object for which should be report data returned.</param>
        /// <returns>Campaign report source data.</returns>
        Dictionary<int, List<CampaignConversionHitsInfo>> GetCampaignHits(int campaignID);


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
        void UpsertCampaignConversionHits(int campaignID, DataTable calculatedHits, DateTime calculatedTo);
    }
}
