using System;
using System.Linq;
using System.Text;

using CMS.DataEngine;
using CMS.LicenseProvider;
using CMS.Scheduler;
using CMS.SiteProvider;


namespace CMS.SocialMarketing
{
    /// <summary>
    /// Handles LinkedIn Insights analytics data collection when activated by the scheduler.
    /// </summary>
    public sealed class LinkedInInsightsCollectionTask : ITask
    {
        #region "Constants - public"

        /// <summary>
        /// Scheduled task name prefix for LinkedIn Insights.
        /// </summary>
        public const string TASK_CODENAME_PREFIX = "LinkedInInsightsCollection";


        /// <summary>
        /// Scheduled task name format for LinkedIn Insights.
        /// Usage: <code>var completeTaskCodeName = String.Format(LinkedInInsightsCollectionTask.TASK_CODENAME_FORMAT, {LinkedInApplicationInfoID});</code>
        /// </summary>
        public const string TASK_CODENAME_FORMAT = TASK_CODENAME_PREFIX + "_{0}";

        #endregion


        #region "Public methods"

        /// <summary>
        /// Executes scheduled analytics data collection.
        /// The task should be executed once a day.
        /// </summary>
        /// <param name="taskInfo">Contains task data with information related to the LinkedIn application.</param>
        /// <returns>Null on success, error description otherwise.</returns>
        public string Execute(TaskInfo taskInfo)
        {
            if (!IsFeatureAvailable(taskInfo))
            {
                return "Feature Social Marketing Insights is not available in the Kentico edition you are using.";
            }

            // Read task data
            LinkedInInsightsCollectionTaskData taskData = LinkedInInsightsCollectionTaskData.FromXmlString(taskInfo.TaskData);

            // Process LinkedIn Insights
            bool res = LinkedInInsightsHelper.ProcessLinkedInAccountInsightsByApplication(taskData.ApplicationId, taskData.AccountInsights);

            // Update task data
            taskInfo.TaskData = LinkedInInsightsCollectionTaskData.ToXmlString(taskData);
            TaskInfoProvider.SetTaskInfo(taskInfo);

            return (res) ? null : "One or more insights collection failed, see Event log for details.";
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Checks whether Insights collection is available.
        /// </summary>
        /// <param name="taskInfo">Task info.</param>
        /// <returns>True if Insights collection is available, false otherwise.</returns>
        private bool IsFeatureAvailable(TaskInfo taskInfo)
        {
            SiteInfo siteInfo = SiteInfoProvider.GetSiteInfo(taskInfo.TaskSiteID);
            if (!LicenseKeyInfoProvider.IsFeatureAvailable(siteInfo.DomainName, FeatureEnum.SocialMarketingInsights))
            {
                return false;
            }
            return true;
        }

        #endregion
    }
}
