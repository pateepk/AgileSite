using System;

namespace CMS.Newsletters
{
    internal class EmailWithSubscriber : Email
    {
        private readonly IIssueUrlService urlService;
        private readonly SubscriberInfo subscriber;
        private string mViewInBrowserUrl;

        #region Properties
       

        [RegisterColumn]
        public string ViewInBrowserUrl
        {
            get
            {
                if (String.IsNullOrEmpty(mViewInBrowserUrl))
                {
                    mViewInBrowserUrl = urlService.GetViewInBrowserUrl(newsletter, issue, subscriber);
                }

                return mViewInBrowserUrl;
            }
        }

        #endregion


        public EmailWithSubscriber(IIssueUrlService urlService, NewsletterInfo newsletter, IssueInfo issue, SubscriberInfo subscriber)
            : base (newsletter, issue)
        {
            this.urlService = urlService;
            this.subscriber = subscriber;
        }
    }
}
