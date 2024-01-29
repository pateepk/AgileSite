using System;

namespace CMS.Newsletters.Internal
{
    /// <summary>
    /// Event arguments for Email Builder related events.
    /// </summary>
    public class EmailBuilderEventArgs : EventArgs
    {
        /// <summary>
        /// Newsletter ID.
        /// </summary>
        public int NewsletterID { get; set; }


        /// <summary>
        /// Email issue ID.
        /// </summary>
        public int IssueID { get; set; }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="newsletterId">Newsletter identifier.</param>
        /// <param name="issueId">Issue identifier.</param>
        public EmailBuilderEventArgs(int newsletterId, int issueId)
        {
            NewsletterID = newsletterId;
            IssueID = issueId;
        }
    }
}
