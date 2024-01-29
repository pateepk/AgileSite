using CMS.Activities;

namespace CMS.Newsletters
{
    /// <summary>
    /// Provides initialization for Newsletter subscribing activity.
    /// </summary>
    internal class NewsletterSubscribingActivityInitializer : IActivityInitializer
    {
        private readonly int? mSubscriberID;
        private readonly NewsletterInfo mNewsletter;
        private readonly ActivityTitleBuilder mTitleBuilder = new ActivityTitleBuilder();


        /// <summary>
        /// Initializes new instance of <see cref="NewsletterSubscribingActivityInitializer"/>.
        /// </summary>
        /// <param name="newsletter">Newsletter subscriber subscribed to</param>
        /// <param name="subscriberID">Specifies ID of the subscriber the activity is initialized for</param>
        public NewsletterSubscribingActivityInitializer(NewsletterInfo newsletter, int? subscriberID)
        {
            mNewsletter = newsletter;
            mSubscriberID = subscriberID;
        }


        /// <summary>
        /// Initializes <see cref="IActivityInfo"/> properties.
        /// </summary>
        /// <param name="activity">Activity info</param>
        public void Initialize(IActivityInfo activity)
        {
            activity.ActivityItemID = mNewsletter.NewsletterID;
            activity.ActivityItemDetailID = mSubscriberID ?? 0;
            activity.ActivityTitle = mTitleBuilder.CreateTitle(ActivityType, mNewsletter.NewsletterName);
        }


        /// <summary>
        /// Activity type.
        /// </summary>
        public string ActivityType
        {
            get
            {
                return PredefinedActivityType.NEWSLETTER_SUBSCRIBING;
            }
        }


        /// <summary>
        /// Activity settings key name, used to check whether activity logging is enabled.
        /// </summary>
        public string SettingsKeyName
        {
            get
            {
                return "CMSCMNewsletterSubscribe";
            }
        }
    }
}
