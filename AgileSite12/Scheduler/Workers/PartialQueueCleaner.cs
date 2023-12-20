using System;

using CMS.EmailEngine;
using CMS.Helpers;

namespace CMS.Scheduler
{
    /// <summary>
    /// Deletes expired archived emails (worker task).
    /// </summary>
    public class PartialQueueCleaner : ITask
    {
        #region "Constants"

        /// <summary>
        /// Common description used as a base for task display name.
        /// </summary>
        private const string DESCRIPTION = "Clean e-mail queue";

        #endregion


        #region "Variables"

        private int mSiteId = -1;
        private DateTime mExpirationDate = DateTime.Now;
        private int mBatchSize = -1;

        #endregion


        #region "Methods"

        /// <summary>
        /// Runs a partial queue cleaner using the task info specified.
        /// </summary>
        /// <param name="task">Task info</param>
        /// <returns>A message describing the result of operation</returns>
        public string Execute(TaskInfo task)
        {
            try
            {
                // Get "serialized" task data (use Convert as we need the exception in here)
                string[] taskData = task.TaskData.Split('|');
                mSiteId = Convert.ToInt32(taskData[0]);
                mExpirationDate = Convert.ToDateTime(taskData[1], CultureHelper.EnglishCulture);
                mBatchSize = Convert.ToInt32(taskData[2]);

                EmailInfoProvider.DeleteArchived(mSiteId, mExpirationDate, mBatchSize);

                return string.Empty;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }


        /// <summary>
        /// Factory method that creates partial queue cleaner tasks.
        /// </summary>        
        /// <param name="siteId">Site ID</param>
        /// <param name="expirationDate">Expiration date for archived emails</param>
        /// <param name="scheduleTime">Task run time</param>
        /// <param name="batchSize">Number of emails to delete</param>
        /// <param name="identifier">Unique batch identifier</param>
        /// <returns>A partial queue cleaner wrapped in a task object</returns>
        internal static TaskInfo Create(int siteId, DateTime expirationDate, DateTime scheduleTime, int batchSize, string identifier)
        {
            TaskInterval taskInterval = new TaskInterval()
            {
                Period = SchedulingHelper.PERIOD_ONCE,
                StartTime = scheduleTime
            };

            return new TaskInfo
            {
                TaskClass = typeof(PartialQueueCleaner).FullName,
                TaskAssemblyName = typeof(PartialQueueCleaner).Assembly.GetName().Name,
                TaskData = string.Format("{0}|{1}|{2}", siteId, Convert.ToString(expirationDate, CultureHelper.EnglishCulture), batchSize),
                TaskDisplayName = string.Format("{0} (worker {1} for SiteID {2})", DESCRIPTION, identifier.Replace("of", " of "), siteId),
                TaskName = string.Format("Email.{0}.{1}.{2}", typeof(PartialQueueCleaner).Name, identifier, siteId),
                TaskEnabled = true,
                TaskInterval = SchedulingHelper.EncodeInterval(taskInterval),
                TaskDeleteAfterLastRun = true,
                TaskLastResult = string.Empty,
                TaskSiteID = 0,
                TaskNextRunTime = SchedulingHelper.GetFirstRunTime(taskInterval, scheduleTime),
                // Set task for processing in external service
                TaskAllowExternalService = true,
                TaskUseExternalService = SchedulingHelper.UseExternalService,
                TaskType = ScheduledTaskTypeEnum.System
            };
        }


        /// <summary>
        /// Deletes all existing partial queue cleaners tasks.
        /// </summary>
        internal static void DeleteAll()
        {
            string partialQueueCleaner = typeof(PartialQueueCleaner).FullName;
            TaskInfoProvider
                .GetTasks().WhereEquals("TaskClass", partialQueueCleaner)
                .ForEachObject(TaskInfoProvider.DeleteTaskInfo);
        }

        #endregion
    }
}