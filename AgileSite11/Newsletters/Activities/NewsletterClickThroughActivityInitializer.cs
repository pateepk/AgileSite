using System;

using CMS.Activities;

namespace CMS.Newsletters
{
    /// <summary>
    /// Newsletter email click through activity initializer.
    /// </summary>
    internal class NewsletterClickThroughActivityInitializer : IActivityInitializer
    {
        private readonly ActivityTitleBuilder mTitleBuilder = new ActivityTitleBuilder();
        private readonly string mOriginalUrl;
        private readonly IssueInfo mIssue;
        private readonly NewsletterInfo mNewsletter;


        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="originalURL">URL of the origin</param>
        /// <param name="issue">Issue object</param>
        /// <param name="newsletter">Newsletter object</param>
        public NewsletterClickThroughActivityInitializer(string originalURL, IssueInfo issue, NewsletterInfo newsletter)
        {
            mOriginalUrl = originalURL;
            mIssue = issue;
            mNewsletter = newsletter;
        }


        /// <summary>
        /// Initializes <see cref="IActivityInfo"/> properties.
        /// </summary>
        /// <param name="activity">Activity info</param>
        public void Initialize(IActivityInfo activity)
        {
            activity.ActivityTitle = mTitleBuilder.CreateTitle(ActivityType, mIssue.IssueDisplayName);
            activity.ActivityItemID = mNewsletter.NewsletterID;
            activity.ActivityURL = mOriginalUrl;
            activity.ActivityItemDetailID = mIssue.IssueID;
            activity.ActivitySiteID = mIssue.IssueSiteID;
        }


        /// <summary>
        /// Activity type
        /// </summary>
        public string ActivityType
        {
            get
            {
                return PredefinedActivityType.NEWSLETTER_CLICKTHROUGH;
            }
        }


        /// <summary>
        /// Activity settings key name, used to check whether activity logging is enabled.
        /// </summary>
        public string SettingsKeyName
        {
            get
            {
                return "CMSCMClickthroughTracking";
            }
        }
    }
}
