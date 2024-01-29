using CMS;
using CMS.Newsletters;

[assembly: RegisterImplementation(typeof(IWinnerIssueSender), typeof(WinnerIssueSender), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.Newsletters
{
    internal interface IWinnerIssueSender
    {
        /// <summary>
        /// Tries to get the winner. If the winner can't be decided postpones the winner selection for an hour.
        /// Winner is send to the subscribers after the selection.
        /// </summary>
        void ProcessWinner(IssueInfo parentIssue, ABTestInfo abTest);
    }
}
