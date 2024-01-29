using System;
using System.Collections.Generic;

using CMS.Base;
using CMS.Core;
using CMS.Helpers;
using CMS.PortalEngine;
using CMS.PortalEngine.Internal;
using CMS.SiteProvider;
using CMS.WebAnalytics;
using CMS.WebAnalytics.Internal;

namespace CMS.Newsletters
{
    /// <summary>
    /// Logic connected to newsletter issue modeling and storing.
    /// </summary>
    internal class NewsletterAssetModelStrategy : AbstractAssetModelStrategy
    {
        /// <summary>
        /// Returns view model from given asset info.
        /// </summary>
        /// <param name="info">Asset info.</param>
        public override CampaignAssetViewModel GetAssetViewModel(CampaignAssetInfo info)
        {
            var issue = IssueInfoProvider.GetIssueInfo(info.CampaignAssetAssetGuid, SiteContext.CurrentSiteID);
            if (issue == null)
            {
                return GetRemovedAssetViewModel(info);
            }

            var link = URLHelper.GetAbsoluteUrl(Service.Resolve<IUILinkProvider>().GetSingleObjectLink("CMS.Newsletter", "EditIssueProperties", new ObjectDetailLinkParameters
            {
                ObjectIdentifier = issue.IssueID,
                AllowNavigationToListing = true,
                ParentObjectIdentifier = issue.IssueNewsletterID
            }));

            return new CampaignAssetViewModel
            {
                AssetID = info.CampaignAssetID,
                Type = info.CampaignAssetType,
                Name = issue.IssueDisplayName,
                ID = issue.IssueID,
                CampaignID = info.CampaignAssetCampaignID,
                Link = link,
                AdditionalProperties = new Dictionary<string, object>
                {
                    {"utmSource", issue.IssueUTMSource },
                    {"isEditable", IsIssueEditable(issue.IssueStatus) }
                }
            };
        }


        private bool IsIssueEditable(IssueStatusEnum issueStatus)
        {
            switch (issueStatus)
            {
                case IssueStatusEnum.Idle:
                case IssueStatusEnum.ReadyForSending:
                    return true;
                default:
                    return false;
            }
        }


        /// <summary>
        /// Sets data from asset model and returns updated view model.
        /// This method is called when existing asset is being updated.
        /// </summary>
        /// <param name="model">Asset view model.</param>
        public override CampaignAssetViewModel SetAssetInfo(CampaignAssetViewModel model)
        {
            var campaign = CampaignInfoProvider.GetCampaignInfo(model.CampaignID);
            var campaignAsset = GetAssetInfo(model);

            object utmSource;
            model.AdditionalProperties.TryGetValue("utmSource", out utmSource);

            WebAnalyticsEvents.CampaignUTMChanged.StartEvent(new CMSEventArgs<CampaignUTMChangedData>
            {
                Parameter = new CampaignUTMChangedData
                {
                    Campaign = campaign,
                    OriginalEmailID = model.ID,
                    NewUTMSource = utmSource as string
                }
            });

            CampaignAssetInfoProvider.SetCampaignAssetInfo(campaignAsset);

            return GetAssetViewModel(campaignAsset);
        }
    }
}
