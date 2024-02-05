using System;

using CMS.DocumentEngine;
using CMS.DataEngine;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Publishes assets added to the campaign.
    /// </summary>
    internal class CampaignAssetsPublisher : ICampaignAssetsPublisher
    {
        /// <summary>
        /// Publishes page assets and file assets added to the given campaign.
        /// </summary>
        /// <param name="campaign">Campaign whose assets are published.</param>
        public void PublishPagesAndFiles(CampaignInfo campaign)
        {
            var campaignLaunched = campaign.CampaignOpenFrom;
            var pagesToPublish = GetPagesToPublish(campaign);

            foreach (var page in pagesToPublish)
            {
                // Always set DocumentPublishFrom to now
                page.DocumentPublishFrom = campaignLaunched;

                // Set DocumentPublishTo to null if DocumentPublishTo is set to past
                if (page.DocumentPublishTo < campaignLaunched)
                {
                    page.DocumentPublishTo = DateTime.MaxValue;
                }

                page.Update();
            }
        }


        private MultiDocumentQuery GetPagesToPublish(CampaignInfo campaign)
        {
            var pageGuids = GetCampaignPageGuids(campaign.CampaignID);

            return DocumentHelper.GetDocuments()
                                 .OnSite(campaign.CampaignSiteID)
                                 .Published(false)
                                 .AllCultures()
                                 .WhereEqualsOrNull("DocumentWorkflowStepID", 0)
                                 .WhereIn("NodeGUID", pageGuids);
        }


        private ObjectQuery<CampaignAssetInfo> GetCampaignPageGuids(int campaignId)
        {
            return CampaignAssetInfoProvider.GetCampaignAssets()
                                            .WhereEquals("CampaignAssetCampaignID", campaignId)
                                            .WhereEquals("CampaignAssetType", TreeNode.OBJECT_TYPE)
                                            .Column("CampaignAssetAssetGuid")
                                            .Distinct();
        }
    }
}
