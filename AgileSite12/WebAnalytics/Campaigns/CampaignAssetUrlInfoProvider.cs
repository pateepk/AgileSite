using System;
using System.Linq;

using CMS.DataEngine;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Class providing CampaignAssetUrlInfo management.
    /// </summary>
    public class CampaignAssetUrlInfoProvider : AbstractInfoProvider<CampaignAssetUrlInfo, CampaignAssetUrlInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public CampaignAssetUrlInfoProvider()
            : base(CampaignAssetUrlInfo.TYPEINFO)
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the CampaignAssetUrlInfo objects.
        /// </summary>
        public static ObjectQuery<CampaignAssetUrlInfo> GetCampaignAssetUrls()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns CampaignAssetUrlInfo with specified ID.
        /// </summary>
        /// <param name="id">CampaignAssetUrlInfo ID</param>
        public static CampaignAssetUrlInfo GetCampaignAssetUrlInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Returns CampaignAssetUrlInfo with specified GUID.
        /// </summary>
        /// <param name="guid">CampaignAssetUrlInfo GUID</param>
        public static CampaignAssetUrlInfo GetCampaignAssetUrlInfo(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Returns CampaignAssetUrlInfo that belongs to campaign with <paramref name="campaignID"/> with given URL.
        /// </summary>
        /// <param name="url">Normalized URL <see cref="CampaignAssetUrlInfoHelper.NormalizeAssetUrlTarget"/>.</param>
        /// <param name="campaignID">Campaign ID</param>
        /// <see cref="CampaignAssetUrlInfoHelper.IsNormalizedUrlTarget"/>
        internal static CampaignAssetUrlInfo GetCampaignAssetUrlInfo(string url, int campaignID)
        {
            return ProviderObject.GetCampaignAssetUrlInfoInternal(url, campaignID);
        }


        /// <summary>
        /// Sets (updates or inserts) specified CampaignAssetUrlInfo.
        /// </summary>
        /// <param name="infoObj">CampaignAssetUrlInfo to be set</param>
        public static void SetCampaignAssetUrlInfo(CampaignAssetUrlInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified CampaignAssetUrlInfo.
        /// </summary>
        /// <param name="infoObj">CampaignAssetUrlInfo to be deleted</param>
        public static void DeleteCampaignAssetUrlInfo(CampaignAssetUrlInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes CampaignAssetUrlInfo with specified ID.
        /// </summary>
        /// <param name="id">CampaignAssetUrlInfo ID</param>
        public static void DeleteCampaignAssetUrlInfo(int id)
        {
            CampaignAssetUrlInfo infoObj = GetCampaignAssetUrlInfo(id);
            DeleteCampaignAssetUrlInfo(infoObj);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns CampaignAssetUrlInfo that belongs to campaign with <paramref name="campaignID"/> with given URL.
        /// </summary>
        /// <param name="url">Normalized URL <see cref="CampaignAssetUrlInfoHelper.NormalizeAssetUrlTarget"/>.</param>
        /// <param name="campaignID">Campaign ID</param>
        /// <see cref="CampaignAssetUrlInfoHelper.IsNormalizedUrlTarget"/>
        protected virtual CampaignAssetUrlInfo GetCampaignAssetUrlInfoInternal(string url, int campaignID)
        {
            // Get url assets by matching URL
            var urlAssetsQuery = CampaignAssetUrlInfoProvider.GetCampaignAssetUrls()
                                                             .WhereEquals("CampaignAssetURLTarget", url);

            // Get assets matching supplied campaign UTM code
            var campaignAssetsQuery = CampaignAssetInfoProvider.GetCampaignAssets()
                                                               .WhereEquals("CampaignAssetCampaignID", campaignID);

            // Return url asset that belongs to supplied campaign and has matching URL
            return urlAssetsQuery.WhereIn("CampaignAssetUrlCampaignAssetID", campaignAssetsQuery).TopN(1).FirstOrDefault();
        }

        #endregion
    }
}