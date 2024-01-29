using CMS.Activities;

namespace CMS.Newsletters
{
    /// <summary>
    /// Provides initialization for newsletter activity.
    /// </summary>
    internal class NewsletterOpenActivityInitializer : IActivityInitializer
    {
        private readonly IssueInfo mIssue;
        private readonly ActivityTitleBuilder mTitleBuilder = new ActivityTitleBuilder();


        /// <summary>
        /// Initializes new instance of <see cref="NewsletterOpenActivityInitializer"/>.
        /// </summary>
        /// <param name="issue">Opened issue</param>
        public NewsletterOpenActivityInitializer(IssueInfo issue)
        {
            mIssue = issue;
        }


        /// <summary>
        /// Initializes <see cref="IActivityInfo"/> properties.
        /// </summary>
        /// <param name="activity">Activity info</param>
        public void Initialize(IActivityInfo activity)
        {
            activity.ActivityItemID = mIssue.IssueNewsletterID;
            activity.ActivityItemDetailID = mIssue.IssueID;

            activity.ActivityTitle = mTitleBuilder.CreateTitle(ActivityType, mIssue.IssueDisplayName);
        }


        /// <summary>
        /// Activity type.
        /// </summary>
        public string ActivityType
        {
            get
            {
                return PredefinedActivityType.NEWSLETTER_OPEN;
            }
        }


        /// <summary>
        /// Activity settings key name, used to check whether activity logging is enabled.
        /// </summary>
        public string SettingsKeyName
        {
            get
            {
                return "CMSCMEmailOpening";
            }
        }
    }
}