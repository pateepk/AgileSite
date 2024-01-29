using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Scheduler;

namespace CMS.Newsletters
{
    /// <summary>
    /// Scheduled task for newsletter issue mailout.
    /// </summary>    
    public class QueueSender : ITask
    {
        /// <summary>
        /// Lock object for issue variant generation to newsletter queue
        /// </summary>
        private static readonly object generateLock = new object();


        #region "Methods"

        /// <summary>
        /// Generates e-mails of given issue (GUID or ID of an issue is specified in taskData property of task parameter) and send all emails in queue to subscribers.
        /// </summary>
        /// <param name="task">Task to process</param>
        public string Execute(TaskInfo task)
        {
            Guid issueGuid = ValidationHelper.GetGuid(task.TaskData, Guid.Empty);
            IssueInfo issue;

            // Check if issue guid provided, otherwise get issue id from task data
            if (issueGuid != Guid.Empty)
            {
                issue = IssueInfoProvider.GetIssueInfo(issueGuid, task.TaskSiteID);
            }
            else
            {
                int issueId = ValidationHelper.GetInteger(task.TaskData, 0);
                issue = IssueInfoProvider.GetIssueInfo(issueId);
            }

            if (issue == null)
            {
                // No task data provided? => silently delete task for nonexistent issue
                return null;
            }

            try
            {
                // Skip newsletter queue initialization for A/B test winner issue
                if ((!issue.IssueIsABTest || issue.IssueIsVariant) && (issue.IssueStatus == IssueStatusEnum.ReadyForSending))
                {
                    if (!issue.IssueIsABTest)
                    {
                        // Generate newsletter e-mails to newsletter queue for a classic issue
                        EmailQueueManager.GenerateEmails(issue);
                    }
                    else
                    {
                        int parentId = issue.IssueVariantOfIssueID;

                        // If a variant of A/B test is sent ensure that only first one generates items into newsletter queue
                        if (AllowGenerateQueue(parentId))
                        {
                            lock (generateLock)
                            {
                                if (AllowGenerateQueue(parentId))
                                {
                                    // Generate newsletter e-mails to newsletter queue
                                    EmailQueueManager.GenerateEmails(issue);
                                }
                                else if (IsQueueGenerated(parentId))
                                {
                                    // Postpone scheduled task for a minute if e-mails are still generated
                                    PostponeScheduledTask(issue);
                                    return null;
                                }
                            }
                        }
                        else if (IsQueueGenerated(parentId))
                        {
                            // Postpone scheduled task for a minute if e-mails are still generated
                            PostponeScheduledTask(issue);
                            return null;
                        }
                    }
                }

                // Ensure that winner variant is finished
                if (issue.IssueIsABTest && !issue.IssueIsVariant)
                {
                    FinishWinnerVariant(issue.IssueID);
                }

                // Thread can be blocked by another scheduled task ("Send marketing emails").
                // Wait and retry if the thread is blocked
                int retryCount = 3;
                while (retryCount > 0)
                {
                    // Ensure that only one variant is sent at the moment
                    if (ThreadEmailSender.SendingThreads <= 0)
                    {
                        lock (generateLock)
                        {
                            if (ThreadEmailSender.SendingThreads <= 0)
                            {
                                // Send the e-mails
                                EmailQueueManager.SendAllEmails(false, true, issue.IssueID);
                                return null;
                            }
                        }
                    }

                    retryCount--;

                    Thread.Sleep(3000);
                }

                // Postpone scheduled task for a minute
                PostponeScheduledTask(issue);
            }
            catch (Exception e)
            {
                EventLogProvider.LogException("Newsletter", "QueueSender", e);
                return e.Message;
            }

            return null;
        }


        /// <summary>
        /// Returns if data can be generated into newsletter queue when a variant of A/B test is being sent.
        /// </summary>
        /// <param name="issueId">ID of parent issue</param>
        private static bool AllowGenerateQueue(int issueId)
        {
            // Check if parent issue is in status 'Ready for sending'
            return CheckStatus(issueId, IssueStatusEnum.ReadyForSending);
        }


        /// <summary>
        /// Returns if data are prepared in newsletter queue when a variant of A/B test is being sent.
        /// </summary>
        /// <param name="issueId">ID of parent issue</param>
        private static bool IsQueueGenerated(int issueId)
        {
            // Check if parent issue is in status 'Preparing data'
            return CheckStatus(issueId, IssueStatusEnum.PreparingData);
        }


        /// <summary>
        /// Check if parent issue is in specified status.
        /// </summary>
        /// <param name="issueId">ID of parent issue</param>
        /// <param name="status">Status to be checked</param>
        private static bool CheckStatus(int issueId, IssueStatusEnum status)
        {
            // Get parent issue status
            var issues = IssueInfoProvider.GetIssues().WhereEquals("IssueID", issueId).Columns("IssueID", "IssueStatus").TopN(1);
            if (issues.Any())
            {
                var issue = issues.First();
                // Check if parent issue is in specified status
                return issue.IssueStatus == status;
            }

            return false;
        }


        /// <summary>
        /// Postpones scheduled task for given <paramref name="issue"/> for a minute.
        /// </summary>
        /// <param name="issue">Issue to be sent</param>
        private static void PostponeScheduledTask(IssueInfo issue)
        {
            var postponedTask = NewsletterTasksManager.CreateMailoutTask(issue, DateTime.Now.AddMinutes(1), true);
            TaskInfoProvider.SetTaskInfo(postponedTask);
        }


        /// <summary>
        /// Ensures that winner variant's status is 'Finished'.
        /// </summary>
        /// <param name="issueId">ID of main issue</param>
        private static void FinishWinnerVariant(int issueId)
        {
            // Get A/B test
            ABTestInfo test = ABTestInfoProvider.GetABTestInfoForIssue(issueId);
            if (test != null)
            {
                // Get winner variant
                IssueInfo winnerVariant = IssueInfoProvider.GetIssueInfo(test.TestWinnerIssueID);
                if ((winnerVariant != null) && (winnerVariant.IssueStatus != IssueStatusEnum.Finished))
                {
                    // Delete scheduled task for variant mail-out
                    TaskInfoProvider.DeleteTaskInfo(winnerVariant.IssueScheduledTaskID);

                    // Set 'Finished' status
                    IssueInfoProvider.SetIssueStatus(winnerVariant.IssueID, IssueStatusEnum.Finished);
                }
            }
        }

        #endregion
    }
}