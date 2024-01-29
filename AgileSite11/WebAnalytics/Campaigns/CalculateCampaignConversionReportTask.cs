using System;

using CMS.Core;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.LicenseProvider;
using CMS.Scheduler;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Calculates the campaign conversion and campaign visitors statistics.
    /// </summary>
    /// <remarks>
    /// For global task, hits for all running campaigns will be recalculated.
    /// For site specific task, hits for campaigns running on given site will be recalculated.
    /// </remarks>
    public class CalculateCampaignConversionReportTask : ITask
    {
        /// <summary>
        /// Calculates the campaign conversion and campaign visitors statistics. 
        /// Updates or creates the <see cref="CampaignConversionHitsInfo"/> objects.
        /// Visitors count for campaigns is stored in the <see cref="CampaignInfo.CampaignVisitors"/> property.
        /// </summary>
        /// <remarks>
        /// If the task is site specific, only statistics data on the given site are updated.
        /// To calculate statistics, feature <see cref="FeatureEnum.FullContactManagement"/> is required.
        /// </remarks>
        /// <param name="task">Task to process.</param>
        public string Execute(TaskInfo task)
        {
            if (!ObjectFactory<ILicenseService>.StaticSingleton().IsFeatureAvailable(FeatureEnum.FullContactManagement))
            {
                return null;
            }

            try
            {
                LicenseCheckDisabler.ExecuteWithoutLicenseCheck(() => CalculateStatistics(task));
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("CalculateCampaignConversionReportTask", EventType.ERROR, ex);

                return ex.Message;
            }

            return null;
        }


        /// <summary>
        /// Calculates the campaign conversion and campaign visitors statistics.
        /// </summary>
        /// <param name="task">Report calculation scheduled task</param>
        [CanDisableLicenseCheck("WN0JqMcPU0kJJby82lRk4AhPvn2T6Durdp7dZi3VrjY6U+rdOzwIhOGCXvYoE2tEC2d8eSYRwGzNGU9pAPuSKQ==")]
        private static void CalculateStatistics(TaskInfo task)
        {
            var conversionHitsprocessor = new CampaignConversionHitsProcessor();
            var campaignVisitorsProcessor = new CampaignVisitorsProcessor();

            conversionHitsprocessor.CalculateReports(task.TaskSiteID);
            campaignVisitorsProcessor.CalculateVisitors(task.TaskSiteID);
        }
    }
}
