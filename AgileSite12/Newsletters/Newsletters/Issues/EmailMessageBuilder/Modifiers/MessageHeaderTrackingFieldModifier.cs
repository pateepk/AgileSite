using CMS.Core;
using CMS.EmailEngine;
using CMS.SiteProvider;

namespace CMS.Newsletters
{
    /// <summary>
    /// Modifies the <see cref="EmailMessage"/> to include tracking fields in the message header.
    /// </summary>
    internal sealed class MessageHeaderTrackingFieldModifier : IEmailMessageModifier
    {
        private readonly SubscriberInfo subscriber;
        private readonly IssueInfo issue;


        /// <summary>
        /// Creates an instance of <see cref="MessageHeaderTrackingFieldModifier"/> class.
        /// </summary>
        /// <param name="issue">Issue.</param>
        /// <param name="subscriber">Subscriber.</param>
        public MessageHeaderTrackingFieldModifier(IssueInfo issue, SubscriberInfo subscriber)
        {
            this.issue = issue;
            this.subscriber = subscriber;
        }


        /// <summary>
        /// Applies the modification.
        /// </summary>
        /// <param name="message">Email message to modify.</param>
        public void Apply(EmailMessage message)
        {
            if (!FeatureIsAvailable())
            {
                return;
            }

            // When sending email to role or contact group subscribers it's necessary to clear headers to avoid duplication
            message.Headers.Clear();

            // Add IssueID (if a variant of A/B test is sent add ID of its parent issue)
            message.Headers.Add(BounceChecker.IssueIDField, issue.IssueIsVariant ? issue.IssueVariantOfIssueID.ToString() : issue.IssueID.ToString());

            if (subscriber.SubscriberID > 0)
            {
                message.Headers.Add(BounceChecker.SubscriberIDField, subscriber.SubscriberID.ToString());
            }
            else
            {
                message.Headers.Add(BounceChecker.ContactIDField, subscriber.SubscriberRelatedID.ToString());
            }
        }


        private bool FeatureIsAvailable()
        {
            var site = SiteInfoProvider.GetSiteInfo(issue.IssueSiteID);

            return NewsletterHelper.MonitorBouncedEmailsEnabled(site.SiteName) && Service.Resolve<INewsletterLicenseCheckerService>().IsTrackingAvailable();
        }
    }
}
