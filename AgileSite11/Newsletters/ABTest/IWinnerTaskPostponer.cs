using CMS;
using CMS.Newsletters;

[assembly: RegisterImplementation(typeof(IWinnerTaskPostponer), typeof(WinnerTaskPostponer), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Newsletters
{
    internal interface IWinnerTaskPostponer
    {
        /// <summary>
        /// Creates new scheduled task for A/B test winner selection that is going to be run one hour after the current time. The ID of a new task is set to the <see cref="ABTestInfo"/>.
        /// </summary>
        /// <remarks>
        /// The method does not delete any possibly existing scheduled tasks.
        /// </remarks>
        void PostponeScheduledTask(ABTestInfo abTest, IssueInfo parentIssue);
    }
}