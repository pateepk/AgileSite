using System;

using CMS;
using CMS.Newsletters;

[assembly: RegisterImplementation(typeof(IIssueScheduler), typeof(IssueScheduler), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Newsletters
{
    /// <summary>
    /// Provide method for scheduling the issue mailout.
    /// </summary>
    public interface IIssueScheduler
    {
        /// <summary>
        /// Schedules the given <paramref name="issue"/> to be sent at timed specified by <paramref name="date"/>.
        /// </summary>
        /// <param name="issue">Issue to be scheduled</param>
        /// <param name="date">Date the <paramref name="issue"/> should be scheduled to</param>
        /// <exception cref="ArgumentNullException"><paramref name="issue"/> is null</exception>
        void ScheduleIssue(IssueInfo issue, DateTime date);
    }
}