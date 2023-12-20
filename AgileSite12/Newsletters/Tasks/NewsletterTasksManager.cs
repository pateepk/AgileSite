using System;
using System.Linq;

using CMS.Core;
using CMS.Core.Internal;
using CMS.Helpers;
using CMS.Scheduler;
using CMS.SiteProvider;

namespace CMS.Newsletters
{
    /// <summary>
    /// Class for managing scheduled tasks that relate to newsletters (<see cref="QueueSender"/>).
    /// </summary>
    public static class NewsletterTasksManager
    {
        private const string TASK_ASSEMBLY = "CMS.Newsletters";
        private const string TASKCLASS_WINNERSELECTION = "CMS.Newsletters.WinnerSelection";
        internal const string TASKCLASS_QUEUESENDER = "CMS.Newsletters.QueueSender";


        /// <summary>
        /// Returns scheduled mail-out task for the given issue or creates new one.
        /// </summary>
        /// <param name="issue">Issue</param>
        /// <param name="when">Indicates when to run scheduled task</param>
        /// <param name="taskEnabled">Determines whether the newly created task will be enabled</param>
        public static TaskInfo EnsureMailoutTask(IssueInfo issue, DateTime when, bool taskEnabled)
        {
            if (issue == null)
            {
                throw new ArgumentNullException(nameof(issue));
            }

            return TaskInfoProvider.GetTaskInfo(issue.IssueScheduledTaskID) ?? CreateMailoutTask(issue, when, taskEnabled);
        }


        /// <summary>
        /// Prepares scheduler TaskInfo object for an issue mail-out.
        /// </summary>
        /// <param name="issue">Issue</param>
        /// <param name="when">Date and time when the task should be executed</param>
        /// <param name="taskEnabled">Creates enabled/disabled task</param>
        /// <returns>A scheduler task that represents an issue mail-out</returns>
        public static TaskInfo CreateMailoutTask(IssueInfo issue, DateTime when, bool taskEnabled = true)
        {
            if (issue == null)
            {
                throw new ArgumentNullException(nameof(issue));
            }

            var newsletter = NewsletterInfoProvider.GetNewsletterInfo(issue.IssueNewsletterID);

            // Create a task interval
            var interval = new TaskInterval
            {
                Period = SchedulingHelper.PERIOD_ONCE,
                StartTime = when
            };

            var newsletterName = TextHelper.LimitLength(newsletter.NewsletterDisplayName, 65);
            var issueName = TextHelper.LimitLength(issue.IssueDisplayName, 65);
            var variantName = string.Empty;

            if (IsVariantWithName(issue))
            {
                variantName = $" (variant '{TextHelper.LimitLength(issue.GetVariantName(), 30)}')";
            }

            // Create a task
            var task = new TaskInfo
            {
                TaskAssemblyName = TASK_ASSEMBLY,
                TaskClass = TASKCLASS_QUEUESENDER,
                TaskData = issue.IssueGUID.ToString(),
                TaskDisplayName = $"Send marketing email '{newsletterName}': {issueName}{variantName}",
                TaskEnabled = taskEnabled,
                TaskInterval = SchedulingHelper.EncodeInterval(interval),
                TaskDeleteAfterLastRun = true,
                TaskLastResult = string.Empty,
                TaskName = $"Email{issue.IssueID}_{DateTime.Now:MMddyyyyHHmmss}",
                TaskSiteID = issue.IssueSiteID,
                TaskNextRunTime = SchedulingHelper.GetFirstRunTime(interval),
                TaskType = ScheduledTaskTypeEnum.System
            };

            return task;
        }


        private static bool IsVariantWithName(IssueInfo issue)
        {
            return issue.IssueIsABTest && !string.IsNullOrEmpty(issue.GetVariantName());
        }


        /// <summary>
        /// Creates or updates scheduler TaskInfo object for the dynamic newsletter.
        /// If the <paramref name="taskToUpdate"/> is specified, task is updated according <paramref name="taskInterval"/> and <paramref name="newsletter"/>.
        /// Otherwise new <see cref="TaskInfo"/> is used.
        /// </summary>
        /// <param name="newsletter">Dynamic newsletter</param>
        /// <param name="taskInterval">Task interval object</param>
        /// <param name="taskToUpdate">Task to be updated according current <paramref name="taskInterval"/></param>
        /// <seealso cref="SchedulingHelper.EncodeInterval(TaskInterval)"/>
        /// <seealso cref="SchedulingHelper.DecodeInterval(string)"/>
        public static TaskInfo CreateOrUpdateDynamicNewsletterTask(NewsletterInfo newsletter, TaskInterval taskInterval, TaskInfo taskToUpdate = null)
        {
            var task = taskToUpdate ?? new TaskInfo();
            var newsletterGUID = newsletter.NewsletterGUID.ToString();
            var maxLength = 200;

            task.TaskAssemblyName = "CMS.Newsletters";
            task.TaskClass = "CMS.Newsletters.DynamicNewsletterSender";
            task.TaskDisplayName = TextHelper.LimitLength(ResHelper.GetString("DynamicNewsletter.TaskName") + newsletter.NewsletterDisplayName, maxLength, CutTextEnum.End);
            task.TaskEnabled = true;
            task.TaskInterval = SchedulingHelper.EncodeInterval(taskInterval);
            task.TaskLastResult = string.Empty;
            task.TaskName = ValidationHelper.GetCodeName("DynamicNewsletter." + newsletter.NewsletterName, "_", maxLength - newsletterGUID.Length) + newsletterGUID;
            task.TaskSiteID = SiteContext.CurrentSiteID;
            task.TaskNextRunTime = SchedulingHelper.GetFirstRunTime(taskInterval);
            task.TaskData = newsletterGUID;
            task.TaskAllowExternalService = true;
            task.TaskUseExternalService = (SchedulingHelper.UseExternalService && NewsletterHelper.UseExternalServiceForDynamicNewsletters(SiteContext.CurrentSiteName));
            task.TaskType = ScheduledTaskTypeEnum.System;
            task.TaskDeleteAfterLastRun = taskInterval.Period == SchedulingHelper.PERIOD_ONCE;

            return task;
        }


        /// <summary>
        /// Removes scheduled mail-out task for specified issue.
        /// </summary>
        /// <param name="guid">Issue GUID</param>
        /// <param name="siteId">Site ID</param>
        public static void DeleteMailoutTask(Guid guid, int siteId)
        {
            TaskInfoProvider
                .GetTasks()
                .WhereEquals("TaskSiteID", siteId)
                .WhereEquals("TaskData", guid.ToString())
                .WhereEquals("TaskClass", TASKCLASS_QUEUESENDER)
                .ForEachObject(TaskInfoProvider.DeleteTaskInfo);
        }


        /// <summary>
        /// Removes scheduled winner selection task.
        /// </summary>
        /// <param name="abTest">AB test object holding the winner selection scheduled task ID.</param>
        public static void DeleteWinnerSelectionTask(ABTestInfo abTest)
        {
            var task = TaskInfoProvider.GetTaskInfo(abTest.TestWinnerScheduledTaskID);

            if (task == null)
            {
                return;
            }

            TaskInfoProvider.DeleteTaskInfo(task);
            abTest.TestWinnerScheduledTaskID = 0;
            ABTestInfoProvider.SetABTestInfo(abTest);
        }


        /// <summary>
        /// Ensures that the winner selection task is created or updated (or deleted) according to A/B test info.
        /// Start time of winner selection task is calculated according to A/B test setting and highest mail-out time of the variant.
        /// </summary>
        /// <param name="abi">A/B test info</param>
        /// <param name="parentIssue">Parent issue (optional)</param>
        /// <param name="enableTask">Enable scheduled task</param>
        /// <param name="highestMailoutTime">Highest mail-out time (mail-out time of the last variant)</param>
        public static void EnsureWinnerSelectionTask(ABTestInfo abi, IssueInfo parentIssue, bool enableTask, DateTime highestMailoutTime)
        {
            if (abi == null)
            {
                return;
            }

            // Get task info if task exists
            TaskInfo ti = null;
            if (abi.TestWinnerScheduledTaskID > 0)
            {
                ti = TaskInfoProvider.GetTaskInfo(abi.TestWinnerScheduledTaskID);
            }

            // Delete existing task if manual selection is set
            if (abi.TestWinnerOption == ABTestWinnerSelectionEnum.Manual)
            {
                if (ti != null)
                {
                    TaskInfoProvider.DeleteTaskInfo(ti);
                }
                abi.TestWinnerScheduledTaskID = 0;
                ABTestInfoProvider.SetABTestInfo(abi);
                return;
            }

            // Create scheduled task if does not exist yet
            if (ti == null)
            {
                // If issue not used, try to get issue from A/B test info
                if (parentIssue == null)
                {
                    parentIssue = IssueInfoProvider.GetIssueInfo(abi.TestIssueID);
                    if (parentIssue == null)
                    {
                        return;
                    }
                }

                ti = CreateWinnerSelectionTask(abi, parentIssue);
            }

            // Set/update start time to current A/B test setting
            if (highestMailoutTime == DateTimeHelper.ZERO_TIME)
            {
                highestMailoutTime = DateTime.Now;
            }
            DateTime startTime = highestMailoutTime.AddMinutes(abi.TestSelectWinnerAfter);
            TaskInterval interval = new TaskInterval
            {
                Period = SchedulingHelper.PERIOD_ONCE,
                StartTime = startTime
            };
            ti.TaskInterval = SchedulingHelper.EncodeInterval(interval);
            ti.TaskNextRunTime = SchedulingHelper.GetFirstRunTime(interval);

            if (enableTask)
            {
                // Enable task
                ti.TaskEnabled = true;
            }
            TaskInfoProvider.SetTaskInfo(ti);

            // Update task ID in A/B test info
            if (abi.TestWinnerScheduledTaskID != ti.TaskID)
            {
                abi.TestWinnerScheduledTaskID = ti.TaskID;
                ABTestInfoProvider.SetABTestInfo(abi);
            }
        }


        /// <summary>
        /// Creates a new task for selecting the winner.
        /// </summary>
        /// <param name="abi">AB test</param>
        /// <param name="parentIssue">Parent issue</param>
        /// <returns>A scheduler task that represents an winner selection</returns>
        internal static TaskInfo CreateWinnerSelectionTask(ABTestInfo abi, IssueInfo parentIssue)
        {
            var newsletter = NewsletterInfoProvider.GetNewsletterInfo(parentIssue.IssueNewsletterID);

            var newsletterName = TextHelper.LimitLength(newsletter != null ? newsletter.NewsletterDisplayName : parentIssue.IssueDisplayName, 65);
            var issueName = TextHelper.LimitLength(parentIssue.IssueDisplayName, 65);

            return new TaskInfo
            {
                TaskAssemblyName = TASK_ASSEMBLY,
                TaskClass = TASKCLASS_WINNERSELECTION,
                TaskData = parentIssue.IssueGUID.ToString(),
                TaskDisplayName = $"Winner selection '{newsletterName}': {issueName}",
                TaskDeleteAfterLastRun = true,
                TaskLastResult = string.Empty,
                TaskName = $"WinnerSelection{abi.TestID}_{DateTime.Now:MMddyyyyHHmmss}",
                TaskSiteID = SiteContext.CurrentSiteID,
                TaskType = ScheduledTaskTypeEnum.System,
                TaskEnabled = false
            };
        }


        /// <summary>
        /// Enables all scheduled tasks associated to all variants of parent issue.
        /// </summary>
        /// <param name="issue">Parent (original) issue</param>
        public static void EnableVariantScheduledTasks(IssueInfo issue)
        {
            if (issue == null)
            {
                return;
            }

            var issues = IssueInfoProvider.GetIssues()
                                          .WhereTrue("IssueIsABTest")
                                          .WhereEquals("IssueVariantOfIssueID", issue.IssueID);

            if (issues.Any())
            {
                // Set 'Ready for sending' status to parent issue
                IssueInfoProvider.SetIssueStatus(issue.IssueID, IssueStatusEnum.ReadyForSending);

                foreach (var childrenIssue in issues)
                {
                    // Get task info and enable it
                    TaskInfo mailoutTask = EnsureMailoutTask(childrenIssue, Service.Resolve<IDateTimeNowService>().GetDateTimeNow(), false);
                    mailoutTask.TaskEnabled = true;
                    TaskInfoProvider.SetTaskInfo(mailoutTask);

                    // Update task ID
                    childrenIssue.IssueScheduledTaskID = mailoutTask.TaskID;

                    IssueInfoProvider.SetIssueInfo(childrenIssue);

                    // Set status to "ready for sending"
                    IssueInfoProvider.SetIssueStatus(childrenIssue.IssueID, IssueStatusEnum.ReadyForSending);
                }
            }
        }
    }
}