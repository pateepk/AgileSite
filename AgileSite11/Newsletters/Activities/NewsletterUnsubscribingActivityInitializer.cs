using CMS.Activities;
using CMS.Helpers;

namespace CMS.Newsletters
{
    /// <summary>
    /// Provides initialization for Unsubscription from a single email campaign activity.
    /// </summary>
    internal class NewsletterUnsubscribingActivityInitializer : IActivityInitializer
    {
        private readonly int mNewsletterId;        
        private readonly string mNewsletterName;
        private readonly int mIssueId;
        private readonly ActivityTitleBuilder mTitleBuilder = new ActivityTitleBuilder();


        /// <summary>
        /// Initializes new instance of <see cref="NewsletterUnsubscribingActivityInitializer"/>.
        /// </summary>
        /// <param name="newsletter">Newsletter object</param>
        /// <param name="issueID">Issue ID</param>
        internal NewsletterUnsubscribingActivityInitializer(NewsletterInfo newsletter, int? issueID = null)
        {
            mNewsletterId = newsletter.NewsletterID;
            mNewsletterName = newsletter.NewsletterDisplayName;
            mIssueId = issueID ?? 0;
        }


        /// <summary>
        /// Initializes <see cref="IActivityInfo"/> properties.
        /// </summary>
        /// <param name="activity">Activity info</param>
        public void Initialize(IActivityInfo activity)
        {
            activity.ActivityItemID = mNewsletterId;
            activity.ActivityItemDetailID = mIssueId;

            activity.ActivityTitle = mTitleBuilder.CreateTitle(ActivityType, mNewsletterName);
        }


        /// <summary>
        /// Activity type.
        /// </summary>
        public string ActivityType
        {
            get
            {
                return PredefinedActivityType.NEWSLETTER_UNSUBSCRIBING;
            }
        }


        /// <summary>
        /// Settings key name.
        /// </summary>
        public string SettingsKeyName
        {
            get
            {
                return "CMSCMNewsletterUnsubscribe";
            }
        }
    }
}