using System;

using CMS.Core;
using CMS.Core.Internal;
using CMS.Helpers;
using CMS.Modules;
using CMS.Scheduler;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Creates scheduled task for campaign launch.
    /// </summary>
    internal class CampaignTaskManager
    {
        private readonly IDateTimeNowService mDateTimeNowService;


        public CampaignTaskManager(IDateTimeNowService dateTimeNowService)
        {
            mDateTimeNowService = dateTimeNowService;
        }
        private const string TASK_ASSEMBLY = "CMS.WebAnalytics";
        private const string TASKCLASS_CAMPAIGN_LAUNCHER = "CMS.WebAnalytics.CampaignLauncherTask";

        /// <summary>
        /// Creates scheduled task for a campaign launch.
        /// </summary>
        /// <param name="campaign">Campaign which should be launched in a future.</param>
        /// <returns>A scheduled task that represents a campaign launcher.</returns>
        public TaskInfo CreateLaunchCampaignTask(CampaignInfo campaign)
        {
            if (campaign == null)
            {
                throw new ArgumentNullException("campaign");
            }
            
            // Create a task interval
            var interval = new TaskInterval
            {
                Period = SchedulingHelper.PERIOD_ONCE,
                StartTime = campaign.CampaignOpenFrom
            };

            var resource = ResourceInfoProvider.GetResourceInfo(ModuleName.WEBANALYTICS);

            // Create an object scheduled task (TaskObjectID and TaskObjectType are required)
            var now = mDateTimeNowService.GetDateTimeNow();
            var task = new TaskInfo
            {
                TaskAssemblyName = TASK_ASSEMBLY,
                TaskClass = TASKCLASS_CAMPAIGN_LAUNCHER,
                TaskData = campaign.CampaignID.ToString(),
                TaskDisplayName = string.Format("Launch campaign '{0}'", TextHelper.LimitLength(campaign.CampaignDisplayName, 200)),
                TaskEnabled = true,
                TaskObjectID = campaign.CampaignID,
                TaskObjectType = CampaignInfo.OBJECT_TYPE,
                TaskInterval = SchedulingHelper.EncodeInterval(interval),
                TaskDeleteAfterLastRun = true,
                TaskLastResult = string.Empty,
                TaskResourceID = resource.ResourceID,
                TaskName = string.Format("{0}_{1}", campaign.CampaignName, now.ToString("MMddyyyyHHmmss")),
                TaskSiteID = campaign.CampaignSiteID,
                TaskNextRunTime = SchedulingHelper.GetFirstRunTime(interval),
                TaskType = ScheduledTaskTypeEnum.System
            };

            TaskInfoProvider.SetTaskInfo(task);

            return task;
        }
    }
}
