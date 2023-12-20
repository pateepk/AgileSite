using CMS.Base;

namespace CMS.Newsletters
{
    /// <summary>
    /// Event arguments for the <see cref="UnsubscriptionHandler"/>.
    /// </summary>
    public class UnsubscriptionEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Newsletter that subscriber is unsubscribing from.
        /// Can be null in case that subscriber unsubscribed from all newsletter communication.
        /// </summary>
        public NewsletterInfo Newsletter
        {
            get;
            set;
        }


        /// <summary>
        /// Email address that is being unsubscribed.
        /// </summary>
        public string Email
        {
            get;
            set;
        }


        /// <summary>
        /// ID of issue that subscriber used for unsubscription. Is present only when IssueID is known (is null for example when unsubscribing from administration UI or when issue was deleted).
        /// </summary>
        public int? IssueID
        {
            get;
            set;
        }
    }
}
