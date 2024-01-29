using System;
using System.Data;

using CMS.EmailEngine;
using CMS.Helpers;

namespace CMS.Scheduler
{
    /// <summary>
    /// Deletes expired archived e-mails (manager task).
    /// </summary>
    public class QueueCleaner : ITask
    {
        #region "Constants"

        /// <summary>
        /// Gets the number of archived emails to delete in one batch.
        /// </summary>
        private const int DELETE_ARCHIVED_BATCH_SIZE = 2000;

        #endregion


        #region "Methods"

        /// <summary>
        /// Removes archived e-mails which are older then specified number of days.
        /// </summary>
        /// <param name="task">Task to process</param>
        public string Execute(TaskInfo task)
        {
            try
            {
                // Delete all existing partial cleaners tasks that might still exist due to failure
                PartialQueueCleaner.DeleteAll();

                // Get number of expired emails per site along with the expiration date
                DataSet expiredEmails = EmailInfoProvider.GetExpiredEmailCount();
                if (DataHelper.IsEmpty(expiredEmails))
                {
                    return string.Empty;
                }

                // Scheduler necessary number of partial cleaners for each site
                DateTime scheduleTime = DateTime.Now;
                foreach (DataRow dr in expiredEmails.Tables[0].Rows)
                {
                    int expiredEmailsCount = ValidationHelper.GetInteger(dr["ExpiredEmailsCount"], 0);

                    if (expiredEmailsCount == 0)
                    {
                        continue;
                    }

                    int siteId = ValidationHelper.GetInteger(dr["SiteID"], 0);
                    DateTime expirationDate = ValidationHelper.GetDateTime(dr["ExpirationDate"], DateTime.Now);

                    SchedulePartialCleaners(siteId, expiredEmailsCount, expirationDate, ref scheduleTime, task);
                }

                return string.Empty;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }


        /// <summary>
        /// Schedules a batch of partial queue cleaners.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        /// <param name="expiredEmailsCount">Number of expired emails to delete</param>
        /// <param name="expirationDate">Expiration date for archived emails</param>
        /// <param name="scheduleTime">Task run time</param>
        /// <param name="parentTask">Parent task info</param>
        private static void SchedulePartialCleaners(int siteId, int expiredEmailsCount, DateTime expirationDate, ref DateTime scheduleTime, TaskInfo parentTask)
        {
            int batches = (expiredEmailsCount / DELETE_ARCHIVED_BATCH_SIZE) + 1;

            // Schedule necessary number of partial cleaners depending on the batch size
            TaskInfo partialCleanerTask;
            string identifier;
            for (int i = 0; i < batches; i++)
            {
                scheduleTime = scheduleTime.AddMinutes(1);

                identifier = string.Format("{0}of{1}", i + 1, batches);
                partialCleanerTask = PartialQueueCleaner.Create(siteId, expirationDate, scheduleTime, DELETE_ARCHIVED_BATCH_SIZE, identifier);
                
                // Copy parent's settings
                partialCleanerTask.TaskUseExternalService = parentTask.TaskUseExternalService;
                partialCleanerTask.TaskRunInSeparateThread = parentTask.TaskRunInSeparateThread;
                partialCleanerTask.TaskServerName = parentTask.TaskServerName;

                TaskInfoProvider.SetTaskInfo(partialCleanerTask);
            }
        }

        #endregion
    }
}