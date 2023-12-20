using System;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;
using CMS.Scheduler;

namespace CMS.Newsletters
{
    /// <summary>
    /// Encapsulates methods intended to reset issue/newsletter queue item status.
    /// </summary>
    public static class NewsletterSendingStatusModifier
    {
        /// <summary>
        /// Resets email sending status for all emails in queue for specified issue.
        /// </summary>
        /// <param name="issueId">Issue id</param>
        public static void ResetAllEmailsInQueueForIssue(int issueId)
        {
            var where = new WhereCondition()
                .WhereEquals("EmailNewsletterIssueID", issueId)
                .WhereTrue("EmailSending");

            ProcessReset(where);
        }


        /// <summary>
        /// Resets email sending status for all old not send emails in queue.
        /// </summary>
        internal static void ResetFailedEmailsInQueue()
        {
            var where = new WhereCondition()
               .WhereTrue("EmailSending")
               .WhereLessOrEquals("EmailLastSendAttempt", GetThresholdTime());

            ProcessReset(where);
        }


        /// <summary>
        /// Schedules mailout tasks for old not send variants.
        /// </summary>
        internal static void ResendFailedVariantIssues()
        {
            var issues = IssueInfoProvider.GetIssues()
                .WhereNotNull("IssueVariantOfIssueID")
                .WhereEquals("IssueStatus", IssueStatusEnum.Sending)
                .WhereLessOrEquals("IssueMailoutTime", GetThresholdTime())
                .ToList();

            foreach (var issue in issues)
            {
                TaskInfo mailoutTask = NewsletterTasksManager.CreateMailoutTask(issue, DateTime.Now, true);
                TaskInfoProvider.SetTaskInfo(mailoutTask);
            }
        }


        private static DateTime GetThresholdTime()
        {
            return DateTime.Now.Subtract(new TimeSpan(0, 30, 0));
        }


        private static void ProcessReset(WhereCondition where)
        {
            EmailQueueItemInfoProvider.BulkUpdateEmailQueueItems(
                new WhereCondition().Where(where), new Dictionary<string, object>
                {
                    { "EmailSending", null }
                });
        }
    }
}
