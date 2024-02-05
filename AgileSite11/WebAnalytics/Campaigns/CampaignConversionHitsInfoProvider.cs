using System;
using System.Data;

using CMS.DataEngine;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Class providing CampaignConversionHitsInfo management.
    /// </summary>
    public class CampaignConversionHitsInfoProvider : AbstractInfoProvider<CampaignConversionHitsInfo, CampaignConversionHitsInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public CampaignConversionHitsInfoProvider()
            : base(CampaignConversionHitsInfo.TYPEINFO)
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the CampaignConversionHitsInfo objects.
        /// </summary>
        public static ObjectQuery<CampaignConversionHitsInfo> GetCampaignConversionHits()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns CampaignConversionHitsInfo with specified ID.
        /// </summary>
        /// <param name="id">CampaignConversionHitsInfo ID</param>
        public static CampaignConversionHitsInfo GetCampaignConversionHitsInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Sets (updates or inserts) specified CampaignConversionHitsInfo.
        /// </summary>
        /// <param name="infoObj">CampaignConversionHitsInfo to be set</param>
        public static void SetCampaignConversionHitsInfo(CampaignConversionHitsInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified CampaignConversionHitsInfo.
        /// </summary>
        /// <param name="infoObj">CampaignConversionHitsInfo to be deleted</param>
        public static void DeleteCampaignConversionHitsInfo(CampaignConversionHitsInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes CampaignConversionHitsInfo with specified ID.
        /// </summary>
        /// <param name="id">CampaignConversionHitsInfo ID</param>
        public static void DeleteCampaignConversionHitsInfo(int id)
        {
            var infoObj = GetCampaignConversionHitsInfo(id);
            DeleteCampaignConversionHitsInfo(infoObj);
        }


        /// <summary>
        /// Deletes all CampaignConversionHitsInfos for campaign specified by its ID.
        /// </summary>
        /// <param name="campaignId">ID of the campaign.</param>
        public static void DeleteCampaignConversionHits(int campaignId)
        {
            ProviderObject.DeleteCampaignConversionHitsInternal(campaignId);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Deletes all CampaignConversionHitsInfos for campaign specified by its ID.
        /// </summary>
        /// <param name="campaignId">ID of the campaign.</param>
        protected virtual void DeleteCampaignConversionHitsInternal(int campaignId)
        {
            // Get IDs of all conversions within campaign
            var campaignConversionsIds = new IDQuery<CampaignConversionInfo>("CampaignConversionID")
                .WhereEquals("CampaignConversionCampaignID", campaignId);

            // Prepare where condition
            var where = new WhereCondition().WhereIn("CampaignConversionHitsConversionID", campaignConversionsIds);

            // Delete all matching conversion hits
            BulkDelete(where);
        }

        #endregion


        #region "Internal methods - advanced"

        /// <summary>
        /// Updates number of hits in <see cref="CampaignConversionHitsInfo.CampaignConversionHitsCount"/> or inserts new <see cref="CampaignConversionHitsInfo"/> object if related hit record does not exist yet.
        /// </summary>
        /// <param name="campaignID">ID of <see cref="CampaignInfo"/> object for which is report calculated.</param>
        /// <param name="hitsToUpsertTable">Calculated source <see cref="DataTable"/> to usert <see cref="CampaignConversionHitsInfo"/> objects with following table definition:
        /// <code>
        ///     var table = new DataTable();
        ///     table.TableName = "HitsToUpsertTable";
        ///     table.Columns.Add("Hits", typeof(int));
        ///     table.Columns.Add("CampaignConversionID", typeof(int));
        ///     table.Columns.Add("ActivityUTMSource", typeof(string));
        /// </code>
        /// The order of column must be preserved.
        /// </param>
        internal static void BulkUpsertCampaignConversionHits(DataTable hitsToUpsertTable, int campaignID)
        {
            if (hitsToUpsertTable == null)
            {
                throw new ArgumentNullException("hitsToUpsertTable");
            }

            var parameters = new QueryDataParameters();
            parameters.Add("@CampaignID", campaignID);
            parameters.Add("@HitsToUpsertTable", hitsToUpsertTable, typeof(DataTable));

            ConnectionHelper.ExecuteQuery("analytics.campaignconversionhits.upserthits", parameters);
        }

        #endregion
    }
}