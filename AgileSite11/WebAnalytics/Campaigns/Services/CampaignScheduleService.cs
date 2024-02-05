using System;

using CMS.Base;
using CMS.Core;
using CMS.Core.Internal;
using CMS.Helpers;
using CMS.Scheduler;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Provides methods for campaign scheduling.
    /// </summary>
    internal class CampaignScheduleService : ICampaignScheduleService
    {
        private readonly ICampaignValidationService mValidationService;
        private readonly ICampaignAssetsPublisher mAssetsPublisher;
        private readonly IDateTimeNowService mNowService;


        /// <summary>
        /// Constructor which allows to set current time.
        /// </summary>
        /// <param name="nowService">Service used to provide current time used in service methods.</param>
        /// <param name="validationService">Provides methods to validate campaign.</param>
        /// <param name="assetsPublisher">Publishes assets added to the campaign.</param>
        public CampaignScheduleService(IDateTimeNowService nowService, ICampaignValidationService validationService, ICampaignAssetsPublisher assetsPublisher)
        {
            mNowService = nowService;
            mValidationService = validationService;
            mAssetsPublisher = assetsPublisher;
        }


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
        public bool Launch(CampaignInfo campaign, int siteID)
        {
            if (!mValidationService.CanBeLaunched(campaign, siteID))
            {
                return false;
            }

            var now = mNowService.GetDateTimeNow();
            campaign.CampaignOpenFrom = now;
            campaign.CampaignCalculatedTo = now;

            if ((campaign.CampaignOpenTo != DateTimeHelper.ZERO_TIME) && (campaign.CampaignOpenTo < now))
            {
                campaign.CampaignOpenTo = DateTimeHelper.ZERO_TIME;
            }

            // Remove related scheduled task if campaign was scheduled
            CancelScheduledStart(campaign);

            CampaignInfoProvider.SetCampaignInfo(campaign);

            // Publish assets of type file and page added to the given campaign.
            mAssetsPublisher.PublishPagesAndFiles(campaign);

            WebAnalyticsEvents.CampaignLaunched.StartEvent(new CMSEventArgs<CampaignInfo>
            {
                Parameter = campaign
            });

            return true;
        }


        /// <summary>
        /// Finishes the specified campaign (<paramref name="campaign"/>).
        /// Data will not be tracked anymore. Doesn't finish an already finished campaign.
        /// </summary>
        /// <param name="campaign">Campaign which is finished.</param>
        /// <param name="siteID">ID of the site on which the campaign is finished.</param>
        /// <param name="finishDate">Time when the campaign is finished. If <c>null</c>, <see cref="DateTime.Now"/> is used.</param>
        /// <returns><c>True</c> if the campaign has been finished.</returns>
        public bool Finish(CampaignInfo campaign, int siteID, DateTime? finishDate)
        {
            var openTo = ValidationHelper.GetDateTime(finishDate, mNowService.GetDateTimeNow());

            if (!mValidationService.CanBeFinished(campaign, siteID))
            {
                return false;
            }

            campaign.CampaignOpenTo = openTo;
            CampaignInfoProvider.SetCampaignInfo(campaign);

            return true;
        }


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
        public bool Schedule(CampaignInfo campaign, DateTime from, DateTime to, int siteID)
        {
            if (!mValidationService.CanBeScheduled(campaign, siteID) || from == DateTimeHelper.ZERO_TIME)
            {
                return false;
            }

            campaign.CampaignOpenFrom = from;
            campaign.CampaignOpenTo = to;

            var manager = new CampaignTaskManager(mNowService);
            var task = manager.CreateLaunchCampaignTask(campaign);

            campaign.CampaignScheduledTaskID = task.TaskID;
            CampaignInfoProvider.SetCampaignInfo(campaign);

            return true;
        }


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
        public bool Reschedule(CampaignInfo campaign, DateTime from, DateTime to, int siteID)
        {
            if (!mValidationService.CanBeRescheduled(campaign, siteID) || (from == DateTimeHelper.ZERO_TIME))
            {
                return false;
            }

            var task = TaskInfoProvider.GetTaskInfo(campaign.CampaignScheduledTaskID);
            if (task == null)
            {
                return false;
            }

            // Update scheduled start of campaign
            var interval = new TaskInterval
            {
                Period = SchedulingHelper.PERIOD_ONCE,
                StartTime = from
            };

            task.TaskInterval = SchedulingHelper.EncodeInterval(interval);
            task.TaskNextRunTime = from;
            TaskInfoProvider.SetTaskInfo(task);

            campaign.CampaignScheduledTaskID = task.TaskID;

            campaign.CampaignOpenFrom = from;
            campaign.CampaignOpenTo = to;
            CampaignInfoProvider.SetCampaignInfo(campaign);

            return true;
        }


        /// <summary>
        /// Unplans the launch and finish of the specified campaign.
        /// This is only possible if the campaign has already been planned.
        /// </summary>
        /// <remarks>
        /// A scheduled task is removed and will not be executed.
        /// </remarks>
        /// <param name="campaign">Campaign which is unscheduled.</param>
        /// <returns><c>True</c> if the campaign has been unscheduled.</returns>
        public bool Unschedule(CampaignInfo campaign)
        {
            if (campaign.CampaignScheduledTaskID == 0)
            {
                return false;
            }

            CancelScheduledStart(campaign);

            campaign.CampaignOpenFrom = DateTimeHelper.ZERO_TIME;
            campaign.CampaignOpenTo = DateTimeHelper.ZERO_TIME;
            CampaignInfoProvider.SetCampaignInfo(campaign);

            return true;
        }


        private void CancelScheduledStart(CampaignInfo campaign)
        {
            var launchTaskID = campaign.CampaignScheduledTaskID;
            if (launchTaskID > 0)
            {
                campaign.CampaignScheduledTaskID = 0;
                CampaignInfoProvider.SetCampaignInfo(campaign);
                TaskInfoProvider.DeleteTaskInfo(launchTaskID);
            }
        }
    }
}