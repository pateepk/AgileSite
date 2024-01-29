using System;

using CMS.Core.Internal;
using CMS.Helpers;
using CMS.PortalEngine;
using CMS.PortalEngine.Internal;
using CMS.WebAnalytics.Internal;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Provides methods for obtaining link to single campaign object.
    /// </summary>
    internal class CampaignLinkService : ICampaignLinkService
    {
        private readonly IUILinkProvider mUiLinkProvider;
        private readonly IDateTimeNowService mDateTimeNowService;

        internal const string CAMPAIGN_ELEMENT_CODENAME = "CampaignProperties";
        internal const string CAMPAIGN_TAB_REPORTS = "Campaign.Reports";


        /// <summary>
        /// Instantiates new instance of <see cref="CampaignLinkService"/>.
        /// </summary>
        /// <param name="uiLinkProvider">Provides methods for generating links to access single objects within the module. (e.g. single Site)</param>
        /// <param name="dateTimeNowService">Provides method for getting current <see cref="DateTime"/></param>
        public CampaignLinkService(IUILinkProvider uiLinkProvider, IDateTimeNowService dateTimeNowService)
        {
            mUiLinkProvider = uiLinkProvider;
            mDateTimeNowService = dateTimeNowService;
        }


        /// <summary>
        /// Gets link leading to the given <paramref name="campaign"/>. Takes the <paramref name="campaign"/> status into account
        /// and changes the target tab accordingly.
        /// </summary>
        /// <param name="campaign">Campaign object the link is being obtained for</param>
        /// <returns>Link leading to the given <paramref name="campaign"/></returns>
        public string GetCampaignLink(CampaignInfo campaign)
        {
            var linkParameters = GetObjectLinkParameters(campaign);
            return URLHelper.GetAbsoluteUrl(mUiLinkProvider.GetSingleObjectLink(CampaignInfo.TYPEINFO.ModuleName, CAMPAIGN_ELEMENT_CODENAME, linkParameters));
        }


        private ObjectDetailLinkParameters GetObjectLinkParameters(CampaignInfo campaign)
        {
            var linkParameters = CreateObjectLinkParameters(campaign);
            UpdateTabNameAccordingToCampaignStatus(linkParameters, campaign);
            return linkParameters;
        }


        private void UpdateTabNameAccordingToCampaignStatus(ObjectDetailLinkParameters linkParameters, CampaignInfo campaign)
        {
            var campaignStatus = campaign.GetCampaignStatus(mDateTimeNowService.GetDateTimeNow());
            if ((campaignStatus == CampaignStatusEnum.Running) || (campaignStatus == CampaignStatusEnum.Finished))
            {
                linkParameters.TabName = CAMPAIGN_TAB_REPORTS;
            }
        }


        private ObjectDetailLinkParameters CreateObjectLinkParameters(CampaignInfo campaign)
        {
            return new ObjectDetailLinkParameters
            {
                ObjectIdentifier = campaign.CampaignID,
                AllowNavigationToListing = true
            };
        }
    }
}
