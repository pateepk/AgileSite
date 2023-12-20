using System;
using System.Collections.Generic;

using CMS.DataEngine;
using CMS.SiteProvider;
using CMS.WebAnalytics.Internal;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Logic connected to <see cref="CampaignAssetUrlInfo"/> modeling and storing.
    /// </summary>
    internal class CampaignAssetUrlModelStrategy : AbstractAssetModelStrategy
    {
        /// <summary>
        /// Returns asset info from view model to check if <see cref="CampaignAssetInfo"/> already exists.
        /// </summary>
        /// <param name="model">Asset view model sent from client.</param>
        public override CampaignAssetInfo GetAssetInfo(CampaignAssetViewModel model)
        {
            var infoToReturn = new CampaignAssetInfo
            {
                CampaignAssetAssetGuid = Guid.Empty,
                CampaignAssetType = CampaignAssetUrlInfo.OBJECT_TYPE,
                CampaignAssetCampaignID = model.CampaignID,
            };

            var url = CampaignAssetUrlInfoHelper.NormalizeAssetUrlTarget(model.Link, SiteContext.CurrentSite);
            var urlAsset = CampaignAssetUrlInfoProvider.GetCampaignAssetUrlInfo(url, model.CampaignID);

            if (urlAsset != null)
            {
                infoToReturn.CampaignAssetAssetGuid = urlAsset.CampaignAssetUrlGuid;
            }

            return infoToReturn;
        }


        /// <summary>
        /// Returns view model from given asset info.
        /// </summary>
        /// <param name="info">Asset info.</param>
        public override CampaignAssetViewModel GetAssetViewModel(CampaignAssetInfo info)
        {
            var urlAsset = CampaignAssetUrlInfoProvider.GetCampaignAssetUrlInfo(info.CampaignAssetAssetGuid);
            return CreateCampaignAssetViewModel(info.CampaignAssetCampaignID, urlAsset);
        }


        /// <summary>
        /// Sets data from asset model and returns updated view model.
        /// This method is called when existing asset is being updated.
        /// </summary>
        /// <param name="model">Asset view model.</param>
        public override CampaignAssetViewModel SetAssetInfo(CampaignAssetViewModel model)
        {
            var campaignAssetUrl = EnsureAssetUrlInfo(model);
            return CreateCampaignAssetViewModel(model.CampaignID, campaignAssetUrl);
        }


        /// <summary>
        /// Returns existing campaign asset url, or creates a new one.
        /// </summary>
        /// <param name="model">Model of the campaign asset url.</param>
        private CampaignAssetUrlInfo EnsureAssetUrlInfo(CampaignAssetViewModel model)
        {
            // Get existing
            if (model.ID != 0)
            {
                return CampaignAssetUrlInfoProvider.GetCampaignAssetUrlInfo(model.ID);
            }

            CampaignAssetUrlInfo campaignAssetUrl;

            // Create new
            using (var transaction = new CMSTransactionScope())
            {
            var campaignAsset = new CampaignAssetInfo
            {
                CampaignAssetID = model.AssetID,
                CampaignAssetCampaignID = model.CampaignID,
                CampaignAssetType = CampaignAssetUrlInfo.OBJECT_TYPE,
                CampaignAssetAssetGuid = Guid.NewGuid()
            };
            CampaignAssetInfoProvider.SetCampaignAssetInfo(campaignAsset);
            
            campaignAssetUrl = new CampaignAssetUrlInfo
            {
                CampaignAssetUrlID = model.ID,
                CampaignAssetUrlPageTitle = model.Name,
                CampaignAssetUrlTarget = CampaignAssetUrlInfoHelper.NormalizeAssetUrlTarget(model.Link, SiteContext.CurrentSite),
                CampaignAssetUrlCampaignAssetID = campaignAsset.CampaignAssetID,
                CampaignAssetUrlGuid = campaignAsset.CampaignAssetAssetGuid
            };
            CampaignAssetUrlInfoProvider.SetCampaignAssetUrlInfo(campaignAssetUrl);

            transaction.Commit();
        }

            return campaignAssetUrl;
        }


        /// <summary>
        /// Creates <see cref="CampaignAssetViewModel"/> from <see cref="CampaignInfo"/>'s ID and <see cref="CampaignAssetUrlInfo"/>.
        /// </summary>
        /// <returns><see cref="CampaignAssetViewModel"/> for client to use.</returns>
        private CampaignAssetViewModel CreateCampaignAssetViewModel(int campaignID, CampaignAssetUrlInfo campaignAssetUrl)
        {
            var viewModel = new CampaignAssetViewModel
            {
                CampaignID = campaignID,
                Type = CampaignAssetUrlInfo.OBJECT_TYPE,
                AdditionalProperties = new Dictionary<string, object>()
            };

            if (campaignAssetUrl != null)
            {
                viewModel.ID = campaignAssetUrl.CampaignAssetUrlID;
                viewModel.Name = campaignAssetUrl.CampaignAssetUrlPageTitle;
                viewModel.AssetID = campaignAssetUrl.CampaignAssetUrlCampaignAssetID;

                // Do not construct full URL when site presentation URL is not correctly set
                if (Uri.IsWellFormedUriString(SiteContext.CurrentSite.SitePresentationURL, UriKind.Absolute))
                {
                    var uri = CampaignAssetUrlInfoHelper.GetCampaignAssetUrlInfoFullUri(campaignAssetUrl);
                    viewModel.Link = uri.ToString();
                    viewModel.AdditionalProperties.Add("liveSiteLink", uri.ToString());
                }
            }

            return viewModel;
        }
    }
}
