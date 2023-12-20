using System;

using CMS.DataEngine;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Class providing CampaignAssetInfo management.
    /// </summary>
    public class CampaignAssetInfoProvider : AbstractInfoProvider<CampaignAssetInfo, CampaignAssetInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public CampaignAssetInfoProvider()
            : base(CampaignAssetInfo.TYPEINFO)
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the CampaignAssetInfo objects.
        /// </summary>
        public static ObjectQuery<CampaignAssetInfo> GetCampaignAssets()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns CampaignAssetInfo with specified ID.
        /// </summary>
        /// <param name="id">CampaignAssetInfo ID</param>
        public static CampaignAssetInfo GetCampaignAssetInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Returns CampaignAssetInfo with specified GUID.
        /// </summary>
        /// <param name="guid">CampaignAssetInfo GUID</param>
        public static CampaignAssetInfo GetCampaignAssetInfo(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Sets (updates or inserts) specified CampaignAssetInfo.
        /// </summary>
        /// <param name="infoObj">CampaignAssetInfo to be set</param>
        public static void SetCampaignAssetInfo(CampaignAssetInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified CampaignAssetInfo.
        /// </summary>
        /// <param name="infoObj">CampaignAssetInfo to be deleted</param>
        public static void DeleteCampaignAssetInfo(CampaignAssetInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes CampaignAssetInfo with specified ID.
        /// </summary>
        /// <param name="id">CampaignAssetInfo ID</param>
        public static void DeleteCampaignAssetInfo(int id)
        {
            CampaignAssetInfo infoObj = GetCampaignAssetInfo(id);
            DeleteCampaignAssetInfo(infoObj);
        }

        #endregion
    }
}