using CMS.Activities;
using CMS.Helpers;

namespace CMS.Newsletters
{
    /// <summary>
    /// Activity for unsubscribe from all marketing communication (all newsletters). 
    /// </summary>
    internal class NewsletterUnsubscribingFromAllActivityInitializer : IActivityInitializer
    {
        private readonly ActivityTitleBuilder mTitleBuilder = new ActivityTitleBuilder();
        private readonly IssueInfo mIssue;


        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="issue">Issue object</param>
        public NewsletterUnsubscribingFromAllActivityInitializer(IssueInfo issue = null)
        {
            mIssue = issue;
        }


        /// <summary>
        /// Initializes <see cref="IActivityInfo"/> properties.
        /// </summary>
        /// <param name="activity">Activity info</param>
        public void Initialize(IActivityInfo activity)
        {
            if (mIssue != null)
            {
                activity.ActivitySiteID = mIssue.IssueSiteID;
                activity.ActivityTitle = mTitleBuilder.CreateTitle(ActivityType, mIssue.IssueDisplayName);
                activity.ActivityItemID = mIssue.IssueID;
            }
            else
            {
                activity.ActivityTitle = ResHelper.GetString("om.acttitle.newsletterunsubscriptionfromalldeleted", CultureHelper.DefaultUICultureCode);
            }
        }


        /// <summary>
        /// Activity type.
        /// </summary>
        public string ActivityType
        {
            get
            {
                return PredefinedActivityType.NEWSLETTER_UNSUBSCRIBING_FROM_ALL;
            }
        }


        /// <summary>
        /// Activity settings key name, used to check whether activity logging is enabled.
        /// </summary>
        public string SettingsKeyName
        {
            get
            {
                return "CMSCMNewsletterUnsubscribedFromAll";
            }
        }
    }
}
