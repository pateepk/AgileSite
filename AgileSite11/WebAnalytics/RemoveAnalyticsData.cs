using System;

using CMS.Helpers;
using CMS.Scheduler;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// RemoveAnalyticsData class.
    /// </summary>
    public class RemoveAnalyticsData : ITask
    {
        #region "ITask members"

        /// <summary>
        /// Executes the remove analytics task.
        /// </summary>
        /// <param name="task">Task to process</param>
        public string Execute(TaskInfo task)
        {
            try
            {
                ProcessTask(task.TaskData);
            }
            catch (Exception e)
            {
                return e.Message;
            }

            return null;
        }

        #endregion


        #region "PrivateMethods"

        private void ProcessTask(string taskData)
        {
            int days = ValidationHelper.GetInteger(taskData, 0);
            if (days > 0)
            {
                StatisticsInfoProvider.RemoveAnalyticsData(DateTime.Now.AddYears(-50), DateTime.Now.AddDays(-days), 0, String.Empty);
            }
        }

        #endregion
    }
}