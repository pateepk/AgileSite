using CMS.Core.Internal;
using CMS.Scheduler;

namespace CMS.Newsletters
{
    internal class WinnerTaskPostponer : IWinnerTaskPostponer
    {
        private readonly IDateTimeNowService mDateTimeNowService;


        public WinnerTaskPostponer(IDateTimeNowService dateTimeNowService)
        {
            mDateTimeNowService = dateTimeNowService;
        }


        /// <summary>
        /// Creates new scheduled task for A/B test winner selection that is going to be run one hour after the current time. The ID of a new task is set to the <see cref="ABTestInfo"/>.
        /// </summary>
        /// <remarks>
        /// The method does not delete any possibly existing scheduled tasks.
        /// </remarks>
        public void PostponeScheduledTask(ABTestInfo abTest, IssueInfo parentIssue)
        {
            var postponedTask = NewsletterTasksManager.CreateWinnerSelectionTask(abTest, parentIssue);

            var interval = new TaskInterval
            {
                Period = SchedulingHelper.PERIOD_ONCE,
                StartTime = mDateTimeNowService.GetDateTimeNow().AddHours(1)
            };

            postponedTask.TaskInterval = SchedulingHelper.EncodeInterval(interval);
            postponedTask.TaskNextRunTime = SchedulingHelper.GetFirstRunTime(interval);
            postponedTask.TaskEnabled = true;

            TaskInfoProvider.SetTaskInfo(postponedTask);

            // Ensure integrity
            abTest.TestWinnerScheduledTaskID = postponedTask.TaskID;
            ABTestInfoProvider.SetABTestInfo(abTest);
        }
    }
}