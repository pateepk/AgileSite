using CMS.Core.Internal;

namespace CMS.Newsletters
{
    internal class WinnerIssueSender : IWinnerIssueSender
    {
        private readonly IDateTimeNowService mDateTimeNowService;
        private readonly IWinnerTaskPostponer mWinnerTaskPostponer;


        public WinnerIssueSender(IDateTimeNowService dateTimeNowService, IWinnerTaskPostponer winnerTaskPostponer)
        {
            mDateTimeNowService = dateTimeNowService;
            mWinnerTaskPostponer = winnerTaskPostponer;
        }


        /// <summary>
        /// Tries to get the winner. If the winner can't be decided postpones the winner selection for an hour.
        /// Winner is send to the subscribers after the selection.
        /// </summary>
        public void ProcessWinner(IssueInfo parentIssue, ABTestInfo abTest)
        {
            var winner = IssueHelper.GetWinnerIssue(parentIssue, abTest);
            if (winner != null)
            {
                UpdateABTestAndSendWinner(abTest, winner, parentIssue);
            }
            else
            {
                // Postpone scheduled task for an hour if there are no results yet
                if (abTest.TestWinnerOption != ABTestWinnerSelectionEnum.Manual)
                {
                    mWinnerTaskPostponer.PostponeScheduledTask(abTest, parentIssue);
                }
            }
        }


        private void UpdateABTestAndSendWinner(ABTestInfo abTest, IssueInfo winner, IssueInfo parentIssue)
        {
            UpdateABTestWithWinner(abTest, winner);
            PrepareWinnerIssueForSending(winner, parentIssue);
            EmailQueueManager.SendAllEmails(false, parentIssue.IssueID);
        }


        private void UpdateABTestWithWinner(ABTestInfo abTest, IssueInfo winner)
        {
            abTest.TestWinnerSelected = mDateTimeNowService.GetDateTimeNow();
            abTest.TestWinnerIssueID = winner.IssueID;
            ABTestInfoProvider.SetABTestInfo(abTest);
        }


        private void PrepareWinnerIssueForSending(IssueInfo winner, IssueInfo parentIssue)
        {
            NewsletterSendingStatusModifier.ResetAllEmailsInQueueForIssue(parentIssue.IssueID);
            IssueHelper.CopyWinningVariantIssueProperties(winner, parentIssue);
            IssueInfoProvider.SetIssueInfo(parentIssue);
        }
    }
}
