using System;

using CMS.Core;
using CMS.Helpers;
using CMS.Scheduler;

namespace CMS.Newsletters
{
    /// <summary>
    /// Scheduled task - selects the best issue variant and sends it to subscribers.
    /// </summary>
    public class WinnerSelection : ITask
    {
        /// <summary>
        /// Selects the best issue variant and sends it to subscribers.
        /// </summary>
        /// <param name="task">Task to process</param>
        public string Execute(TaskInfo task)
        {
            IssueInfo parentIssue = null;
            Guid issueGuid = ValidationHelper.GetGuid(task.TaskData, Guid.Empty);
            if (issueGuid != Guid.Empty)
            {
                parentIssue = IssueInfoProvider.GetIssueInfo(issueGuid, task.TaskSiteID);
            }
            if (parentIssue == null)
            {
                return "Task data not provided (issue not found).";
            }

            var abTest = ABTestInfoProvider.GetABTestInfoForIssue(parentIssue.IssueID);
            if (abTest == null)
            {
                return "Task data not provided (A/B test record not found).";
            }

            Service.Resolve<IWinnerIssueSender>().ProcessWinner(parentIssue, abTest);

            return null;
        }
    }
}