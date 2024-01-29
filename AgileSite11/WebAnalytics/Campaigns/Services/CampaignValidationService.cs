using System;

using CMS.Core.Internal;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Provides methods to validate campaign.
    /// </summary>
    internal class CampaignValidationService : ICampaignValidationService
    {
        private readonly IDateTimeNowService mDateTimeNowService;


        public CampaignValidationService(IDateTimeNowService mDateTimeNowService)
        {
            this.mDateTimeNowService = mDateTimeNowService;
        }


        /// <summary>
        /// Checks that <paramref name="campaign"/> is not <c>null</c>.
        /// </summary>
        /// <param name="campaign">Campaign object to be checked.</param>
        /// <returns><c>True</c> if <paramref name="campaign"/> is not <c>null</c>.</returns>
        public bool Exists(CampaignInfo campaign)
        {
            return campaign != null;
        }


        /// <summary>
        /// Checks that <paramref name="campaign"/> is on site with ID defined in <paramref name="siteID"/>.       
        /// /// </summary>
        /// <param name="campaign">Campaign object to be checked.</param>
        /// <param name="siteID">ID of site on which campaign should exist.</param>
        /// <returns><c>True</c> if <paramref name="campaign"/> exists on site with ID defined in <paramref name="siteID"/>.</returns>
        public bool IsOnSite(CampaignInfo campaign, int siteID)
        {
            return campaign.CampaignSiteID == siteID;
        }


        /// <summary>
        /// Checks if the campaign has at least one conversion.
        /// </summary>
        /// <param name="campaign">Campaign object to be checked.</param>
        /// <returns><c>True</c> if campaign has at least one conversion.</returns>
        public bool HasConversion(CampaignInfo campaign)
        {
            return CampaignConversionInfoProvider.GetCampaignConversions()
                .WhereFalse("CampaignConversionIsFunnelStep")
                .TopN(1)
                .WhereEquals("CampaignConversionCampaignID", campaign.CampaignID)
                .HasResults();
        }


        /// <summary>
        /// Checks if a conversion can be deleted from a running campaign.
        /// </summary>
        /// <param name="campaign">Campaign containing conversions.</param>
        /// <returns><c>True</c> if conversion can be deleted from the running campaign.</returns>
        public bool CanBeConversionDeleted(CampaignInfo campaign)
        {
            var numberOfConversions = CampaignConversionInfoProvider.GetCampaignConversions()
                                                    .WhereFalse("CampaignConversionIsFunnelStep")
                                                    .WhereEquals("CampaignConversionCampaignID", campaign.CampaignID)
                                                    .Count;

            return numberOfConversions > 1;
        }


        /// <summary>
        /// Checks if the campaign can be finished on given site.
        /// </summary>
        /// <param name="campaign">Campaign to be checked.</param>
        /// <param name="siteID">Site of which campaign should be finished.</param>
        /// <returns><c>True</c> if campaign can be finished in defined time and on given site.</returns>
        public bool CanBeFinished(CampaignInfo campaign, int siteID)
        {
            return Exists(campaign) && IsOnSite(campaign, siteID) && !IsFinished(campaign, mDateTimeNowService.GetDateTimeNow());
        }


        /// <summary>
        /// Checks if the campaign can be launched on given site.
        /// </summary>
        /// <param name="campaign">Campaign to be checked.</param>
        /// <param name="siteID">Site of which campaign should be launched.</param>
        /// <returns><c>True</c> if campaign can be launched in defined time and on given site.</returns>
        public bool CanBeLaunched(CampaignInfo campaign, int siteID)
        {
            var now = mDateTimeNowService.GetDateTimeNow();

            return Exists(campaign) && IsOnSite(campaign, siteID) && !IsLaunched(campaign, now) && !IsFinished(campaign, now) && HasConversion(campaign);
        }


        /// <summary>
        /// Checks if the campaign can be scheduled on given site.
        /// </summary>
        /// <param name="campaign">Campaign to be checked.</param>
        /// <param name="siteID">Site of which campaign should be launched.</param>
        /// <returns><c>True</c> if campaign can be scheduled in defined time and on given site.</returns>
        public bool CanBeScheduled(CampaignInfo campaign, int siteID)
        {
            var now = mDateTimeNowService.GetDateTimeNow();

            return CanBeLaunched(campaign, siteID) && !IsScheduled(campaign, now);
        }


        /// <summary>
        /// Checks if the campaign can be re-scheduled.
        /// </summary>
        /// <param name="campaign">Campaign to be checked.</param>
        /// <param name="siteID">ID of the site on which the campaign is re-scheduled.</param>
        /// <returns><c>True</c> if campaign can be re-scheduled.</returns>
        public bool CanBeRescheduled(CampaignInfo campaign, int siteID)
        {
            var now = mDateTimeNowService.GetDateTimeNow();

            return CanBeLaunched(campaign, siteID) && IsScheduled(campaign, now);
        }


        /// <summary>
        /// Checks if the campaign is finished.
        /// </summary>
        /// <param name="campaign">Campaign to be checked.</param>
        /// <param name="time">Time when campaign should be finished.</param>
        /// <returns><c>True</c> if campaign is finished.</returns>
        public bool IsFinished(CampaignInfo campaign, DateTime time)
        {
            return campaign.GetCampaignStatus(time) == CampaignStatusEnum.Finished;
        }


        /// <summary>
        /// Checks if the campaign is launched.
        /// </summary>
        /// <param name="campaign">Campaign to be checked.</param>
        /// <param name="time">Time when campaign should be launched.</param>
        /// <returns><c>True</c> if campaign is launched.</returns>
        public bool IsLaunched(CampaignInfo campaign, DateTime time)
        {
            return campaign.GetCampaignStatus(time) == CampaignStatusEnum.Running;
        }


        /// <summary>
        /// Checks if the campaign is scheduled.
        /// </summary>
        /// <param name="campaign">Campaign to be checked.</param>
        /// <param name="time">Time when campaign should be scheduled.</param>
        /// <returns><c>True</c> if campaign is scheduled.</returns>
        public bool IsScheduled(CampaignInfo campaign, DateTime time)
        {
            return campaign.GetCampaignStatus(time) == CampaignStatusEnum.Scheduled;
        }
    }
}