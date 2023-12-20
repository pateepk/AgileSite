using System;

using CMS;
using CMS.WebAnalytics;

[assembly: RegisterImplementation(typeof(ICampaignValidationService), typeof(CampaignValidationService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Provides methods to validate campaign.
    /// </summary>
    public interface ICampaignValidationService
    {
        /// <summary>
        /// Checks that <paramref name="campaign"/> is not <c>null</c>.
        /// </summary>
        /// <param name="campaign">Campaign object to be checked.</param>
        /// <returns><c>True</c> if <paramref name="campaign"/> is not <c>null</c>.</returns>
        bool Exists(CampaignInfo campaign);


        /// <summary>
        /// Checks that <paramref name="campaign"/> is on site with ID defined in <paramref name="siteID"/>.       
        /// /// </summary>
        /// <param name="campaign">Campaign object to be checked.</param>
        /// <param name="siteID">ID of site on which campaign should exist.</param>
        /// <returns><c>True</c> if <paramref name="campaign"/> exists on site with ID defined in <paramref name="siteID"/>.</returns>
        bool IsOnSite(CampaignInfo campaign, int siteID);


        /// <summary>
        /// Checks if the campaign has at least one conversion.
        /// </summary>
        /// <param name="campaign">Campaign object to be checked.</param>
        /// <returns><c>True</c> if campaign has at least one conversion.</returns>
        bool HasConversion(CampaignInfo campaign);


        /// <summary>
        /// Checks if a conversion can be deleted from a running campaign.
        /// </summary>
        /// <param name="campaign">Campaign containing conversions.</param>
        /// <returns><c>True</c> if conversion can be deleted from the running campaign.</returns>
        bool CanBeConversionDeleted(CampaignInfo campaign);


        /// <summary>
        /// Checks if the campaign can be finished on given site.
        /// </summary>
        /// <param name="campaign">Campaign to be checked.</param>
        /// <param name="siteID">Site of which campaign should be finished.</param>
        /// <returns><c>True</c> if campaign can be finished in defined time and on given site.</returns>
        bool CanBeFinished(CampaignInfo campaign, int siteID);


        /// <summary>
        /// Checks if the campaign can be launched on given site.
        /// </summary>
        /// <param name="campaign">Campaign to be checked.</param>
        /// <param name="siteID">Site of which campaign should be launched.</param>
        /// <returns><c>True</c> if campaign can be launched in defined time and on given site.</returns>
        bool CanBeLaunched(CampaignInfo campaign, int siteID);


        /// <summary>
        /// Checks if the campaign can be launched on given site.
        /// </summary>
        /// <param name="campaign">Campaign to be checked.</param>
        /// <param name="siteID">Site of which campaign should be launched.</param>
        /// <returns><c>True</c> if campaign can be scheduled in defined time and on given site.</returns>
        bool CanBeScheduled(CampaignInfo campaign, int siteID);


        /// <summary>
        /// Checks if the campaign can be re-scheduled.
        /// </summary>
        /// <param name="campaign">Campaign to be checked.</param>
        /// <param name="siteID">ID of the site on which the campaign is re-scheduled.</param>
        /// <returns><c>True</c> if campaign can be re-scheduled.</returns>
        bool CanBeRescheduled(CampaignInfo campaign, int siteID);


        /// <summary>
        /// Checks if the campaign is finished.
        /// </summary>
        /// <param name="campaign">Campaign to be checked.</param>
        /// <param name="time">Time when campaign should be finished.</param>
        /// <returns><c>True</c> if campaign is finished.</returns>
        bool IsFinished(CampaignInfo campaign, DateTime time);


        /// <summary>
        /// Checks if the campaign is launched.
        /// </summary>
        /// <param name="campaign">Campaign to be checked.</param>
        /// <param name="time">Time when campaign should be launched.</param>
        /// <returns><c>True</c> if campaign is launched.</returns>
        bool IsLaunched(CampaignInfo campaign, DateTime time);


        /// <summary>
        /// Checks if the campaign is scheduled.
        /// </summary>
        /// <param name="campaign">Campaign to be checked.</param>
        /// <param name="time">Time when campaign should be scheduled.</param>
        /// <returns><c>True</c> if campaign is scheduled.</returns>
        bool IsScheduled(CampaignInfo campaign, DateTime time);
    }
}
