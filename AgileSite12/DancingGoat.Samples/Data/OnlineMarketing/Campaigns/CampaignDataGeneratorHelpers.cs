using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Activities;
using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.WebAnalytics;

namespace CMS.DancingGoat.Samples
{
    /// <summary>
    /// Helper methods for campaign generation.
    /// </summary>
    internal static class CampaignDataGeneratorHelpers
    {
        /// <summary>
        /// Removes pre-generated activities.
        /// </summary>
        /// <param name="campaignUTMCode">UTM code of the campaign.</param>
        public static void DeleteOldActivities(string campaignUTMCode)
        {
            var oldActivities = ActivityInfoProvider.GetActivities()
                                                    .WhereStartsWith("ActivityTitle", "GeneratedActivity_")
                                                    .WhereEquals("ActivityCampaign", campaignUTMCode)
                                                    .ToList();

            oldActivities.ForEach(ActivityInfoProvider.DeleteActivityInfo);
        }


        /// <summary>
        /// Generates activities for campaign.
        /// </summary>
        /// <param name="activityDataParameters">Contains count to specify how many activities should be generated with given utm content and utm source</param>
        /// <param name="campaign">Campaign for which are activities generated.</param>
        /// <param name="type">Activity type (<see cref="PredefinedActivityType"/>).</param>
        /// <param name="contactsIDs">Represents IDs of all contacts for generating activities.</param>
        /// <param name="conversionItemID">Conversion item ID.</param>
        public static void GenerateActivities(IEnumerable<ActivityDataParameters> activityDataParameters, CampaignInfo campaign, string type, ContactsIDData contactsIDs, int conversionItemID = 0)
        {
            var nodeID = 0;
            var itemID = 0;

            switch (type)
            {
                case PredefinedActivityType.PAGE_VISIT:
                    nodeID = conversionItemID;
                    break;

                default:
                    itemID = conversionItemID;
                    break;
            }

            foreach (var activityDataParameter in activityDataParameters)
            {
                for (var i = 0; i < activityDataParameter.Count; i++)
                {
                    GenerateFakeActivity(campaign.CampaignUTMCode, type, activityDataParameter, nodeID, itemID, campaign.CampaignSiteID, contactsIDs.GetNextContactID());
                }                
            }
        }


        private static void GenerateFakeActivity(string campaignUTMcode, string type, ActivityDataParameters activityDataParameter, int nodeID, int itemID, int siteID, int contactID)
        {
            var activity = new ActivityInfo
            {
                ActivitySiteID = siteID,
                ActivityContactID = contactID,
                ActivityCampaign = campaignUTMcode,
                ActivityType = type,
                ActivityNodeID = nodeID,
                ActivityItemID = itemID,
                ActivityUTMSource = activityDataParameter.UtmSource,
                ActivityUTMContent = activityDataParameter.UtmContent,
                ActivityTitle = "GeneratedActivity_" + type + "_" + contactID
            };

            ActivityInfoProvider.SetActivityInfo(activity);

        }


        /// <summary>
        /// Creates conversion for the campaign.
        /// </summary>
        /// <param name="campaignId">ID of the campaign.</param>
        /// <param name="conversionData">Campaign conversion data for generating.</param>
        public static void CreateConversion(int campaignId, CampaignConversionData conversionData)
        {
            var conversion = CampaignConversionInfoProvider.GetCampaignConversions()
                                                           .WhereEquals("CampaignConversionCampaignID", campaignId)
                                                           .WhereEquals("CampaignConversionActivityType", conversionData.ConversionActivityType)
                                                           .WhereEquals("CampaignConversionItemID", conversionData.ConversionItemID)
                                                           .WhereEquals("CampaignConversionIsFunnelStep", conversionData.ConversionIsFunnelStep)
                                                           .ToList().FirstOrDefault();

            if (conversion != null)
            {
                return;
            }

            conversion = new CampaignConversionInfo
            {
                CampaignConversionName = conversionData.ConversionName,
                CampaignConversionDisplayName = conversionData.ConversionDisplayName,
                CampaignConversionCampaignID = campaignId,
                CampaignConversionActivityType = conversionData.ConversionActivityType,
                CampaignConversionItemID = conversionData.ConversionItemID.GetValueOrDefault(),
                CampaignConversionIsFunnelStep = conversionData.ConversionIsFunnelStep,
                CampaignConversionOrder = conversionData.ConversionOrder

            };

            CampaignConversionInfoProvider.SetCampaignConversionInfo(conversion);
        }


        /// <summary>
        /// Adds the specific newsletter to campaign. If the newsletter does not exists it is created.
        /// </summary>
        /// <param name="campaign">Campaign where the newsletter is added.</param>
        /// <param name="issueGuid">Guid of the newsletter issue.</param>
        public static void AddNewsletterAsset(CampaignInfo campaign, Guid issueGuid)
        {
            var issue = ProviderHelper.GetInfoByGuid(PredefinedObjectType.NEWSLETTERISSUE, issueGuid, campaign.CampaignSiteID);
            if (issue == null)
            {
                return;
            }

            var newsletter = ProviderHelper.GetInfoById(PredefinedObjectType.NEWSLETTER, issue.GetValue("IssueNewsletterID").ToInteger(0));
            if (newsletter == null)
            {
                return;
            }

            var source = newsletter.GetValue("NewsletterDisplayName").ToString().Replace(' ', '_').ToLowerInvariant();

            issue.SetValue("IssueUseUTM", true);
            issue.SetValue("IssueUTMCampaign", campaign.CampaignUTMCode);
            issue.SetValue("IssueUTMSource", source);
            issue.Update();

            CreateNewsletterAsset(campaign.CampaignID, issueGuid);
        }


        /// <summary>
        /// Creates newsletter if it does not exist.
        /// </summary>
        /// <param name="campaignId">ID of the campaign.</param>
        /// <param name="nodeGuid">Newsletter node guid.</param>
        private static void CreateNewsletterAsset(int campaignId, Guid nodeGuid)
        {
            var campaignAsset = CampaignAssetInfoProvider.GetCampaignAssets()
                                                         .WhereEquals("CampaignAssetCampaignID", campaignId)
                                                         .WhereEquals("CampaignAssetAssetGuid", nodeGuid)
                                                         .ToList().FirstOrDefault();

            if (campaignAsset != null)
            {
                return;
            }

            campaignAsset = new CampaignAssetInfo
            {
                CampaignAssetType = PredefinedObjectType.NEWSLETTERISSUE,
                CampaignAssetCampaignID = campaignId,
                CampaignAssetAssetGuid = nodeGuid,
            };

            CampaignAssetInfoProvider.SetCampaignAssetInfo(campaignAsset);
        }


        /// <summary>
        /// Adds the specific asset to campaign. If the asset does not exist it is created.
        /// </summary>
        /// <param name="campaignId">ID of the campaign.</param>
        /// <param name="pagePath">Assets page path.</param>
        public static void AddPageAsset(int campaignId, string pagePath)
        {
            var page = GetDocument(pagePath);
            var nodeGuid = page.NodeGUID;

            var campaignAsset = CampaignAssetInfoProvider.GetCampaignAssets()
                                                         .WhereEquals("CampaignAssetCampaignID", campaignId)
                                                         .WhereEquals("CampaignAssetAssetGuid", nodeGuid)
                                                         .ToList().FirstOrDefault();

            /* If the assest already exists, then do not create it. */
            if (campaignAsset != null)
            {
                return;
            }

            campaignAsset = new CampaignAssetInfo
            {
                CampaignAssetType = PredefinedObjectType.DOCUMENT,
                CampaignAssetCampaignID = campaignId,
                CampaignAssetAssetGuid = nodeGuid,
            };

            CampaignAssetInfoProvider.SetCampaignAssetInfo(campaignAsset);
        }


        /// <summary>
        /// Gets the document according to path.
        /// </summary>
        /// <param name="path">Path of the document.</param>
        /// <returns>Document.</returns>
        public static TreeNode GetDocument(string path)
        {
            return DocumentHelper.GetDocuments()
                                 .All()
                                 .Culture("en-US")
                                 .Path(path)
                                 .OnCurrentSite()
                                 .ToList().First();
        }

    }
}

