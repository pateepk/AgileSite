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
    /// Handles Facebook Insights analytics data collection when activated by the scheduler.
    /// </summary>
    public sealed class FacebookInsightsCollectionTask : ITask
    {
        #region "Constants - public"

        /// <summary>
        /// Scheduled task name prefix for Facebook Insights.
        /// </summary>
        public const string TASK_CODENAME_PREFIX = "FacebookInsightsCollection";
        
        
        /// <summary>
        /// Scheduled task name format for Facebook Insights.
        /// Usage: <code>var completeTaskCodeName = String.Format(FacebookInsightsCollectionTask.TASK_CODENAME_FORMAT, {FacebookApplicationInfoID});</code>
        /// </summary>
        public const string TASK_CODENAME_FORMAT = TASK_CODENAME_PREFIX + "_{0}";

        #endregion


        #region "Public methods"

        /// <summary>
        /// Executes scheduled analytics data collection.
        /// The task should be executed once an hour.
        /// </summary>
        /// <param name="taskInfo">Contains task data with information related to the Facebook application.</param>
        /// <returns>Null on success, error description otherwise.</returns>
        public string Execute(TaskInfo taskInfo)
        {
            if (!IsFeatureAvailable(taskInfo))
            {
                return "Feature Social Marketing Insights is not available in the Kentico edition you are using.";
            }

            // Read task data
            FacebookInsightsCollectionTaskData taskData = FacebookInsightsCollectionTaskData.FromXmlString(taskInfo.TaskData);
            
            // Process Facebook Insights
            FacebookInsightsHelper.ProcessFacebookAccountInsightsByApplication(taskData.ApplicationId, taskData.FacebookInsightsState);

            // Update task data
            taskInfo.TaskData = FacebookInsightsCollectionTaskData.ToXmlString(taskData);
            TaskInfoProvider.SetTaskInfo(taskInfo);

            return null;
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
