using System;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.WebAnalytics.Internal
{
    /// <summary>
    /// Class with shared logic for all asset model strategies.
    /// </summary>
    public abstract class AbstractAssetModelStrategy : ICampaignAssetModelStrategy
    {
        /// <summary>
        /// Returns asset info from view model.
        /// </summary>
        /// <param name="model">Asset view model.</param>
        public virtual CampaignAssetInfo GetAssetInfo(CampaignAssetViewModel model)
        {
            if (model.AssetID > 0)
            {
                return CampaignAssetInfoProvider.GetCampaignAssetInfo(model.AssetID);
            }

            var baseInfoObject = ProviderHelper.GetInfoById(model.Type, model.ID).Generalized;
            if (baseInfoObject == null)
            {
                throw new ArgumentException("[AbstractAssetModelStrategy.GetAssetInfo]: Asset not found.");
            }

            return new CampaignAssetInfo
            {
                CampaignAssetType = model.Type,
                CampaignAssetCampaignID = model.CampaignID,
                CampaignAssetAssetGuid = baseInfoObject.ObjectGUID
            };
        }


        /// <summary>
        /// Returns view model from given asset info.
        /// </summary>
        /// <param name="info">Asset info.</param>
        public abstract CampaignAssetViewModel GetAssetViewModel(CampaignAssetInfo info);


        /// <summary>
        /// Sets data from asset model and returns updated view model.
        /// This method is called when existing asset is being updated.
        /// </summary>
        /// <param name="model">Asset view model.</param>
        public abstract CampaignAssetViewModel SetAssetInfo(CampaignAssetViewModel model);

        /// <summary>
        /// Returns default view model for asset with not existing linked object.
        /// </summary>
        /// <param name="asset">Asset</param>
        protected CampaignAssetViewModel GetRemovedAssetViewModel(CampaignAssetInfo asset)
        {
            return new CampaignAssetViewModel
            {
                AssetID = asset.CampaignAssetID,
                Type = asset.CampaignAssetType,
                ID = -1,
                CampaignID = asset.CampaignAssetCampaignID,
                Name = ResHelper.GetString("campaign.assetlist.deleted." + asset.CampaignAssetType)
            };
        }
    }
}
