using System;

using CMS.Helpers;
using CMS.Scheduler;

namespace CMS.Newsletters
{
    /// <summary>
    /// Class used by scheduler to execute the dynamic newsletter mailout.
    /// </summary>
    public class DynamicNewsletterSender : ITask
    {
        /// <summary>
        /// Generates issue of dynamic newsletter, then generates emails of that issue into queue and send all emails in queue to subscribers.
        /// </summary>
        /// <param name="task">Task data</param>
        public string Execute(TaskInfo task)
        {
            Guid newsletterGuid = ValidationHelper.GetGuid(task.TaskData, Guid.Empty);
            int newsletterId = ValidationHelper.GetInteger(task.TaskData, 0);
            var newsletter = newsletterGuid != Guid.Empty ? NewsletterInfoProvider.GetNewsletterInfo(newsletterGuid, task.TaskSiteID) : NewsletterInfoProvider.GetNewsletterInfo(newsletterId);

            if (newsletter == null)
            {
                return "Task data not provided.";
            }

            int issueId = EmailQueueManager.GenerateDynamicIssue(newsletter.NewsletterID);
            if (issueId <= 0)
            {
                return "Issue not found.";
            }

            try
            {
                // Generate newsletter e-mails to newsletter queue
                EmailQueueManager.GenerateEmails(issueId);

                // Send the e-mails
                EmailQueueManager.SendAllEmails(false, true, issueId);
            }
            catch (Exception e)
            {
                return (e.Message);
            }

            return null;
        }
    }
}