using System;
using System.Collections.Generic;

using CMS.Core;
using CMS.EmailEngine;
using CMS.Helpers;
using CMS.Newsletters.Filters;
using CMS.SiteProvider;

namespace CMS.Newsletters
{
    /// <summary>
    /// Class for sending newsletter issues confirmation.
    /// </summary>
    internal class ConfirmationSender : IConfirmationSender
    {
        /// <summary>
        /// Sends e-mail to confirm subscription/unsubscription of subscriber to newsletter.
        /// </summary>
        /// <param name="isSubscriptionEmail">True if the message is subscription confirmation, false if unsubscription confirmation</param>
        /// <param name="subscriberId">Subscriber ID</param>
        /// <param name="newsletterId">Newsletter ID</param>
        public void SendConfirmationEmail(bool isSubscriptionEmail, int subscriberId, int newsletterId)
        {
            SendConfirmation(subscriberId, newsletterId, false, isSubscriptionEmail);
        }


        /// <summary>
        /// Sends double opt-in e-mail to confirm subscription.
        /// </summary>
        /// <param name="subscriberId">Subscriber ID</param>
        /// <param name="newsletterId">Newsletter ID</param>
        public void SendDoubleOptInEmail(int subscriberId, int newsletterId)
        {
            SendConfirmation(subscriberId, newsletterId, true, false);
        }


        /// <summary>
        /// Sends unsubscription confirmation e-mail.
        /// </summary>
        /// <param name="subscriber">Subscriber to send email to</param>
        /// <param name="newsletterId">ID of newsletter to be unsubscribed from</param>
        public void SendUnsubscriptionConfirmation(SubscriberInfo subscriber, int newsletterId)
        {
            if (subscriber == null)
            {
                return;
            }

            var newsletter = NewsletterInfoProvider.GetNewsletterInfo(newsletterId);
            if (newsletter == null)
            {
                return;
            }

            CreateAndSendConfirmationToSubscribers(new [] { subscriber }, newsletter, null, false, false);
        }


        /// <summary>
        /// Sends either double opt-in e-mail or subscription/unsubscription confirmation e-mail.
        /// </summary>
        /// <param name="subscriberId">Subscriber ID</param>
        /// <param name="newsletterId">Newsletter ID</param>
        /// <param name="isOptIn">True if double opt-in e-mail should be sent, false for subscription/unsubscription e-mails</param>
        /// <param name="isSubscription">True if subscription e-mail should be sent, false, false is unsubscription e-mail should be sent</param>
        private void SendConfirmation(int subscriberId, int newsletterId, bool isOptIn, bool isSubscription)
        {
            // Get the subscriber and newsletter
            var newsletter = NewsletterInfoProvider.GetNewsletterInfo(newsletterId);
            var subscriber = SubscriberInfoProvider.GetSubscriberInfo(subscriberId);
            if (newsletter == null || subscriber == null)
            {
                return;
            }

            // Get subscription info for the subscriber if this is a double opt-in message
            SubscriberNewsletterInfo subscription = null;
            if (isOptIn)
            {
                subscription = SubscriberNewsletterInfoProvider.GetSubscriberNewsletterInfo(subscriberId, newsletterId);
            }

            string siteName = SiteInfoProvider.GetSiteName(newsletter.NewsletterSiteID);
            if (String.IsNullOrEmpty(siteName))
            {
                throw new Exception("[IssueInfoProvider.SendNewsletterEmail]: Newsletter site not found.");
            }

            var subscribers = SubscriberInfoProvider.GetSubscribers(subscriber);

            CreateAndSendConfirmationToSubscribers(subscribers, newsletter, subscription, isOptIn, isSubscription);
        }


        /// <summary>
        /// Creates double opt-in e-mail or subscription/unsubscription confirmation email and sends it to subscribers.
        /// </summary>
        /// <param name="subscribers">Newsletter subscriber's members</param>
        /// <param name="newsletter">Newsletter to send email for</param>
        /// <param name="subscription">Subscription info for double-opt-in message</param>
        /// <param name="isOptIn">True if double-opt-in message</param>
        /// <param name="isSubscription">True if subscription message</param>
        private void CreateAndSendConfirmationToSubscribers(IEnumerable<SubscriberInfo> subscribers, NewsletterInfo newsletter, SubscriberNewsletterInfo subscription, bool isOptIn, bool isSubscription)
        {
            var site = SiteInfoProvider.GetSiteInfo(newsletter.NewsletterSiteID);

            // Get e-mail template depending on the type of e-mail message
            var template = GetEmailTemplateInfo(newsletter, site.SiteName, isOptIn, isSubscription);
            var baseUrl = Service.Resolve<IIssueUrlService>().GetBaseUrl(newsletter);

            // Iterate through all subscribed members
            foreach (var subscriber in subscribers)
            {
                var resolver = ContentFilterResolvers.GetConfirmationResolver(newsletter, subscriber, subscription);
                var subjectFilter = new MacroResolverContentFilter(resolver);
                var bodyFilter = new ConfirmationMessageContentFilter(resolver, baseUrl, subscriber);

                var message = new ConfirmationMessageBuilder(template, newsletter, bodyFilter, subjectFilter, new ConfirmationMessageModifier(template, subscriber)).Build();

                EmailSender.SendEmail(site.SiteName, message);
            }
        }


        /// <summary>
        /// Gets a template for a specified type of confirmation e-mail message.
        /// </summary>
        /// <param name="newsletter">Newsletter to get email template for</param>
        /// <param name="siteName">Newsletter site</param>
        /// <param name="isOptIn">True if double opt-in e-mail should be sent, false for subscription/unsubscription e-mails</param>
        /// <param name="isSubscription">True if subscription e-mail should be sent, false, false is unsubscription e-mail should be sent</param>
        private EmailTemplateInfo GetEmailTemplateInfo(NewsletterInfo newsletter, string siteName, bool isOptIn, bool isSubscription)
        {
            string culture = CultureHelper.GetDefaultCultureCode(siteName);

            // Get e-mail template depending on the type of e-mail message
            EmailTemplateInfo template = GetTemplate(isOptIn, isSubscription, newsletter, culture);
            if (template == null)
            {
                throw new Exception("Email template not found.");
            }

            return template;
        }


        /// <summary>
        /// Gets a template for a specified type of confirmation e-mail message.
        /// </summary>        
        /// <param name="isOptIn">True if double opt-in e-mail should be sent, false for subscription/unsubscription e-mails</param>
        /// <param name="isSubscription">True if subscription e-mail should be sent, false, false is unsubscription e-mail should be sent</param>
        /// <param name="newsletter">Newsletter</param>
        /// <param name="culture">Culture string</param>
        /// <returns>Confirmation e-mail template</returns>
        private EmailTemplateInfo GetTemplate(bool isOptIn, bool isSubscription, NewsletterInfo newsletter, string culture)
        {
            EmailTemplateInfo template;
            string defaultSubject;

            // Get predefined templates and fill boilerplate subject
            if (isOptIn)
            {
                defaultSubject = ResHelper.GetAPIString("Newsletters.DoubleOptInConfirmation", culture, "Newsletter subscription activation");
                // Get double opt-in template
                template = EmailTemplateInfoProvider.GetEmailTemplateInfo(newsletter.NewsletterOptInTemplateID);
            }
            else if (isSubscription)
            {
                defaultSubject = ResHelper.GetAPIString("Newsletters.SubscriptionConfirmation", culture, "Subscription confirmation");
                // Get subscription template
                template = EmailTemplateInfoProvider.GetEmailTemplateInfo(newsletter.NewsletterSubscriptionTemplateID);
            }
            else
            {
                defaultSubject = ResHelper.GetAPIString("Newsletters.UnsubscriptionConfirmation", culture, "Unsubscription confirmation");
                // Get unsubscription template
                template = EmailTemplateInfoProvider.GetEmailTemplateInfo(newsletter.NewsletterUnsubscriptionTemplateID);
            }

            // When template does not specify subject text, use default
            if (String.IsNullOrEmpty(template.TemplateSubject))
            {
                // Create clone so the source object in hash is not changed
                EmailTemplateInfo clone = template.Clone();
                clone.TemplateSubject = defaultSubject;

                return clone;
            }

            return template;
        }
    }
}
