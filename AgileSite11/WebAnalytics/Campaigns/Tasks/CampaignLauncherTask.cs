using System;

using CMS.Core;
using CMS.EventLog;
using CMS.Scheduler;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Task which is created by API for every campaign in order to ensure scheduled campaign launch.
    /// </summary>
    internal class CampaignLauncherTask : ITask
    {
        public string Execute(TaskInfo task)
        {
            // Check if campaign exists
            var campaign = CampaignInfoProvider.GetCampaignInfo(task.TaskObjectID);

            // Task will be removed after execution.
            if (campaign == null)
            {
                return null;
            }

            try
            {
                Service.Resolve<ICampaignScheduleService>().Launch(campaign, task.TaskSiteID);
            }
            catch (Exception e)
            {
                EventLogProvider.LogException("Campaign", "CampaignLauncher", e);
                return e.Message;
            }
            finally
            {
                EventLogProvider.LogInformation("Campaign", "CampaignLauncher", campaign.CampaignDisplayName + " has been launched.");
            }

            return null;
        }
    }
}
