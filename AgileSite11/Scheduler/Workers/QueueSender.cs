using System;

using CMS.EmailEngine;

namespace CMS.Scheduler
{
    /// <summary>
    /// Scheduled task that dispatches e-mail messages from the e-mail queue.
    /// </summary>
    public class QueueSender : ITask
    {
        #region ITask Members

        /// <summary>
        /// Sends all emails in a queue.
        /// </summary>
        /// <param name="task">Container with task information</param>
        /// <returns>Textual description of task run's failure if any.</returns>
        public string Execute(TaskInfo task)
        {
            EmailHelper.Queue.SendScheduledAndFailed(DateTime.Now.AddHours(-1));

            // Everything is asynchronous, so there is no message here
            return null;
        }

        #endregion
    }
}