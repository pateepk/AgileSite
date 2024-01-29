using System;

namespace CMS.Newsletters
{
    internal class EmailFeedwithSubscriber : EmailFeed
    {
        private readonly IIssueUrlService urlService;
        private readonly IssueInfo issue;
        private readonly SubscriberNewsletterInfo subscription;
        private readonly SubscriberInfo subscriber;
        private string mSubscriptionConfirmationUrl;
        private string mUnsubscribeFromEmailFeedUrl;


        #region Properties

        [RegisterColumn]
        public string SubscriptionConfirmationUrl
        {
            get
            {
                if (mSubscriptionConfirmationUrl == null)
                {
                    mSubscriptionConfirmationUrl = (subscription != null && Newsletter.NewsletterEnableOptIn) ?
                        urlService.CreateActivationUrl(Newsletter, subscriber, subscription) : String.Empty;
                }

                return mSubscriptionConfirmationUrl;
            }
        }


        [RegisterColumn]
        public string UnsubscribeFromEmailFeedUrl
        {
            get
            {
                if (String.IsNullOrEmpty(mUnsubscribeFromEmailFeedUrl))
                {
                    mUnsubscribeFromEmailFeedUrl = urlService.GetUnsubscriptionUrl(Newsletter, issue, subscriber);
                }

                return mUnsubscribeFromEmailFeedUrl;
            }
        }


        [RegisterColumn]
        public string UnsubscribeFromAllEmailFeedsUrl => $"{UnsubscribeFromEmailFeedUrl}&amp;unsubscribeFromAll=true";

        #endregion


        public EmailFeedwithSubscriber(IIssueUrlService urlService, NewsletterInfo newsletter, IssueInfo issue, SubscriberNewsletterInfo subscription, SubscriberInfo subscriber)
            : base(newsletter)
        {
            this.urlService = urlService;
            this.issue = issue;
            this.subscription = subscription;
            this.subscriber = subscriber;
        }  
    }
}
