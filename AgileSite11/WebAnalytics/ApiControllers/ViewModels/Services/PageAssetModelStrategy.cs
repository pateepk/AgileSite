using System;
using System.Collections.Generic;

using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Modules;
using CMS.SiteProvider;
using CMS.DataEngine;
using CMS.Localization;
using CMS.WebAnalytics.Internal;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Logic connected to page modeling and storing.
    /// </summary>
    internal class PageAssetModelStrategy : AbstractAssetModelStrategy
    {
        /// <summary>
        /// Returns asset info from view model.
        /// </summary>
        /// <param name="model">Asset view model.</param>
        public override CampaignAssetInfo GetAssetInfo(CampaignAssetViewModel model)
        {
            if (model.AssetID > 0)
            {
                return CampaignAssetInfoProvider.GetCampaignAssetInfo(model.AssetID);
            }

            var node = new TreeProvider().SelectSingleNode(model.ID);
            if (node == null)
            {
                throw new ArgumentException("[PageAssetModelStrategy.GetAssetInfo]: Page not found.");
            }

            return new CampaignAssetInfo
            {
                CampaignAssetType = model.Type,
                CampaignAssetCampaignID = model.CampaignID,
                CampaignAssetAssetGuid = node.NodeGUID
            };
        }


        /// <summary>
        /// Returns view model from given asset info.
        /// </summary>
        /// <param name="info">Asset info.</param>
        public override CampaignAssetViewModel GetAssetViewModel(CampaignAssetInfo info)
        {
            var campaign = CampaignInfoProvider.GetCampaignInfo(info.CampaignAssetCampaignID);
            var siteName = SiteInfoProvider.GetSiteName(campaign.CampaignSiteID);
            var node = new TreeProvider().SelectSingleNode(info.CampaignAssetAssetGuid, LocalizationContext.PreferredCultureCode, siteName, true);
         
            if (node == null)
            {
                return GetRemovedAssetViewModel(info);
            }

            var typeName = DataClassInfoProvider.GetDataClassInfo(node.NodeClassName).ClassDisplayName;

            return new CampaignAssetViewModel
            {
                AssetID = info.CampaignAssetID,
                Type = info.CampaignAssetType,
                Name = node.DocumentName,
                ID = node.NodeID,
                CampaignID = info.CampaignAssetCampaignID,
                Link = URLHelper.GetAbsoluteUrl(ApplicationUrlHelper.GetPageEditLink(node.NodeID)),
                AdditionalProperties = new Dictionary<string, object>
                {
                    {"isPublished", node.IsPublished},
                    {"liveSiteLink", node.AbsoluteURL},
                    {"documentType", typeName}
                }
            };
        }


        /// <summary>
        /// Sets data from asset model and returns updated view model.
        /// This method is called when existing asset is being updated.
        /// </summary>
        /// <param name="model">Asset view model.</param>
        public override CampaignAssetViewModel SetAssetInfo(CampaignAssetViewModel model)
        {
            var assetInfo = GetAssetInfo(model);
            CampaignAssetInfoProvider.SetCampaignAssetInfo(assetInfo);

            return GetAssetViewModel(assetInfo);
        }
    }
}
