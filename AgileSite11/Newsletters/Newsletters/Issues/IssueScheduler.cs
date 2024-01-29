using System;

using CMS.Base;
using CMS.Core.Internal;
using CMS.Scheduler;

namespace CMS.Newsletters
{
    /// <summary>
    /// Provide method for scheduling the issue mailout.
    /// </summary>
    internal class IssueScheduler : IIssueScheduler
    {
        private readonly IDateTimeNowService mDateTimeNowService;


        /// <summary>
        /// Instantiates new instance of <see cref="IssueScheduler"/>.
        /// </summary>
        /// <param name="dateTimeNowService">Provides method for getting current <see cref="DateTime"/></param>
        public IssueScheduler(IDateTimeNowService dateTimeNowService)
        {
            mDateTimeNowService = dateTimeNowService;
        }


        /// <summary>
        /// Schedules the given <paramref name="issue"/> to be sent at timed specified by <paramref name="date"/>.
        /// </summary>
        /// <param name="issue">Issue to be scheduled</param>
        /// <param name="date">Date the <paramref name="issue"/> should be scheduled to</param>
        /// <exception cref="ArgumentNullException"><paramref name="issue"/> is null</exception>
        public void ScheduleIssue(IssueInfo issue, DateTime date)
        {
            if (issue == null)
            {
                throw new ArgumentNullException(nameof(issue));
            }

            if (IsSchedulingABTestVariants(issue))
            {
                ScheduleAllIssueVariants(issue);
            }
            else
            {
                ScheduleIssueInternal(issue, date);
            }
        }


        private void ScheduleIssueInternal(IssueInfo issue, DateTime date)
        {
            TaskInfo task = NewsletterTasksManager.CreateMailoutTask(issue, date);
            RunSchedulerImmediatelyIfTaskNextRunTimeIsInPast(task);

            TaskInfoProvider.SetTaskInfo(task);

            UpdateIssueWithTaskAndDateWithoutVersioning(issue, date, task);

            IssueInfoProvider.SetIssueStatus(issue.IssueID, IssueStatusEnum.ReadyForSending);
        }


        private void UpdateIssueWithTaskAndDateWithoutVersioning(IssueInfo issue, DateTime date, TaskInfo task)
        {
            using (CMSActionContext context = new CMSActionContext())
            {
                context.CreateVersion = false;
                UpdateIssueWithTaskAndDate(issue, date, task);
            }
        }


        private void UpdateIssueWithTaskAndDate(IssueInfo issue, DateTime date, TaskInfo task)
        {
            issue.IssueMailoutTime = date;
            issue.IssueScheduledTaskID = task.TaskID;
            IssueInfoProvider.SetIssueInfo(issue);
        }


        private void RunSchedulerImmediatelyIfTaskNextRunTimeIsInPast(TaskInfo task)
        {
            if (task.TaskNextRunTime <= mDateTimeNowService.GetDateTimeNow())
            {
                SchedulingTimer.RunSchedulerImmediately = true;
            }
        }


        private bool IsSchedulingABTestVariants(IssueInfo issue)
        {
            return issue.IssueIsABTest && !IsSchedulingABTestWinner(issue);
        }


        private bool IsSchedulingABTestWinner(IssueInfo issue)
        {
            var abTestInfoForIssue = ABTestInfoProvider.GetABTestInfoForIssue(issue.IssueID);
            return abTestInfoForIssue != null && abTestInfoForIssue.TestWinnerIssueID > 0 ;
        }


        private void ScheduleAllIssueVariants(IssueInfo issue)
        {
            NewsletterTasksManager.EnableVariantScheduledTasks(issue);
        }
    }
}