using System;

using CMS.Core;
using CMS.UIControls;
using CMS.DataEngine;

namespace CMS.Newsletters.Web.UI.Controls
{
    /// <summary>
    /// Control for the issue send actions.
    /// </summary>
    public class IssueSenderControl : CMSAdminControl
    {
        /// <summary>
        /// ID of newsletter issue that should be sent, required for template based newsletters.
        /// </summary>
        public int IssueID
        {
            get;
            set;
        }


        /// <summary>
        /// Sends issue immediately.
        /// </summary>
        public bool SendNow()
        {
            RemovePreviousTaskAndSendIssue(DateTime.Now);
            return true;
        }


        /// <summary>
        /// Schedules mail-out of the issue to the future.
        /// </summary>
        /// <param name="dateTime">Schedule time of the mail-out</param>
        /// <returns></returns>
        protected bool SendScheduled(DateTime dateTime)
        {
            if (DataTypeManager.IsValidDate(dateTime) && dateTime >= DateTime.Now)
            {
                RemovePreviousTaskAndSendIssue(dateTime);
            }
            else
            {
                ErrorMessage = GetString("newsletter.incorrectdate");
            }

            return String.IsNullOrEmpty(ErrorMessage);
        }


        /// <summary>
        /// Removes all scheduled tasks for the given issue.
        /// </summary>
        /// <param name="when">DateTime when to send</param>
        private void RemovePreviousTaskAndSendIssue(DateTime when)
        {
            IssueInfo issue = IssueInfoProvider.GetIssueInfo(IssueID);
            if (issue != null)
            {
                // Remove all previously scheduled tasks (if any)
                NewsletterTasksManager.DeleteMailoutTask(issue.IssueGUID, issue.IssueSiteID);
                // Schedule new task for new mail-out time
                Service.Resolve<IIssueScheduler>().ScheduleIssue(issue, when);
            }
        }
    }
}
