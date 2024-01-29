using System;

using CMS;
using CMS.WebAnalytics;

[assembly: RegisterImplementation(typeof(ICampaignScheduleService), typeof(CampaignScheduleService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Provides methods for campaign scheduling.
    /// </summary>
    public interface ICampaignScheduleService
    {
        /// <summary>
        /// The method launches the specified campaign (<paramref name="campaign"/>).
        /// </summary>
        /// <remarks>
        /// Launching a campaign includes the following steps:
        /// <ul>
        ///     <li>The campaign starts tracking data.</li>
        ///     <li>All linked page assets are published.</li>
        ///     <li>All linked email assets are sent.</li>
        /// </ul>  
        /// Does not launch an already launched campaign or a campaign from a different site.
        /// When pages are published, <see cref="DocumentEngine.TreeNode.DocumentPublishFrom"/> is set to <see cref="DateTime.Now"/>. 
        /// <see cref="DocumentEngine.TreeNode.DocumentPublishTo"/> is reset if the date is set to past.
        /// Pages under workflow are left untouched and all culture variants are published.
        /// </remarks>
        /// <param name="campaign">Campaign which is launched.</param>
        /// <param name="siteID">ID of the site on which the campaign is launched.</param>
        /// <returns><c>True</c> if the campaign has been launched.</returns>
        bool Launch(CampaignInfo campaign, int siteID);


        /// <summary>
        /// Finishes the specified campaign (<paramref name="campaign"/>).
        /// Data will not be tracked anymore. Doesn't finish an already finished campaign.
        /// </summary>
        /// <param name="campaign">Campaign which is finished.</param>
        /// <param name="siteID">ID of the site on which the campaign is finished.</param>
        /// <param name="finishDate">Time when the campaign is finished. If <c>null</c>, <see cref="DateTime.Now"/> is used.</param>
        /// <returns><c>True</c> if the campaign has been finished.</returns>
        bool Finish(CampaignInfo campaign, int siteID, DateTime? finishDate = null);


        /// <summary>
        /// Plans the launch and finish of the specified campaign (<paramref name="campaign"/>) in future.
        /// </summary>
        /// <remarks>
        /// A scheduled task is created for the campaign launch. The task is executed based on the <paramref name="from"/> parameter.
        /// </remarks>
        /// <param name="campaign">Campaign which is scheduled.</param>
        /// <param name="from">Planned launch (start) of the campaign.</param>
        /// <param name="to">Planned finish (end) of the campaign.</param>
        /// <param name="siteID">ID of the site on which the campaign is scheduled.</param>
        /// <returns><c>True</c> if the campaign has been scheduled.</returns>
        bool Schedule(CampaignInfo campaign, DateTime from, DateTime to, int siteID);


        /// <summary>
        /// Re-plans the launch and finish of the specified campaign (<paramref name="campaign"/>) in future.
        /// This is only possible if the campaign has not been launched or finished yet.
        /// </summary>
        /// <remarks>
        /// A scheduled task is updated for the campaign launch. The task is executed based on the <paramref name="from"/> parameter.
        /// </remarks>
        /// <param name="campaign">Campaign which is re-scheduled.</param>
        /// <param name="from">Planned launch (start) of the campaign.</param>
        /// <param name="to">Planned finish (end) of the campaign.</param>
        /// <param name="siteID">ID of site on which the campaign is re-scheduled.</param>
        /// <returns><c>True</c> if the campaign has been re-scheduled.</returns>
        bool Reschedule(CampaignInfo campaign, DateTime from, DateTime to, int siteID);


        /// <summary>
        /// Unplans the launch and finish of the specified campaign.
        /// This is only possible if the campaign has already been planned.
        /// </summary>
        /// <remarks>
        /// A scheduled task is removed and will not be executed.
        /// </remarks>
        /// <param name="campaign">Campaign which is unscheduled.</param>
        /// <returns><c>True</c> if the campaign has been unscheduled.</returns>
        bool Unschedule(CampaignInfo campaign);
    }
}