using System;

using CMS.Automation;
using CMS.ContactManagement;
using CMS.Core;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.SiteProvider;
using CMS.Helpers;

namespace CMS.Newsletters
{
    /// <summary>
    /// Class for subscribe/unsubscribe contact to newsletter
    /// </summary>
    public class NewsletterSubscriptionAction : AutomationAction
    {
        #region "Constants"

        private const int SUBSCRIBE_ACTION = 0;

        #endregion


        #region "Parameters"

        /// <summary>
        /// Newsletter identifier.
        /// </summary>
        public virtual string NewsletterName => GetResolvedParameter("NewsletterName", String.Empty);


        /// <summary>
        /// Gets current action - 0 for SUBSCRIBE, 1 for UNSUBSCRIBE contact.
        /// </summary>
        public virtual int Action => GetResolvedParameter("Action", SUBSCRIBE_ACTION);

        /// <summary>
        /// If true, action uses subscribe settings from it's newsletter.
        /// </summary>
        public virtual bool InheritDoubleOptIn => GetResolvedParameter("InheritDoubleOptIn", false);

        /// <summary>
        /// Site name.
        /// </summary>
        public virtual string SiteName => GetResolvedParameter("Site", String.Empty);

        #endregion


        /// <summary>
        /// Executes current action
        /// </summary>
        public override void Execute()
        {
            if (InfoObject == null)
            {
                return;
            }

            int siteId = SiteInfoProvider.GetSiteID(SiteName);
            if (siteId <= 0)
            {
                LogMessage(EventType.WARNING, "SUBSCRIBECONTACT", ResHelper.GetAPIString("ma.action.missingSite", "The selected site was not found."), InfoObject);
                return;
            }

            var contact = (ContactInfo)InfoObject;

            NewsletterInfo newsletter = NewsletterInfoProvider.GetNewsletterInfo(NewsletterName, siteId);
            if (newsletter == null)
            {
                LogMessage(EventType.ERROR, "SUBSCRIBECONTACT", ResHelper.GetAPIString("ma.action.subscribenewsletter.nonewsletter", "Selected newsletter was not found."), InfoObject);
                return;
            }

            // Try get existing subscriber
            SubscriberInfo subscriber = SubscriberInfoProvider.GetSubscriberInfo(PredefinedObjectType.CONTACT, contact.ContactID, siteId);
            var subscriptionService = Service.Resolve<ISubscriptionService>();

            if (Action == SUBSCRIBE_ACTION)
            {
                if (String.IsNullOrEmpty(contact.ContactEmail))
                {
                    LogMessage(EventType.WARNING, "SUBSCRIBECONTACT", ResHelper.GetAPIString("ma.action.sendnewsletterissue.noemail", "Contact e-mail address is missing."), InfoObject);
                    return;
                }

                // Do not subscribe if contact is unsubscribed from newsletter
                if (subscriptionService.IsUnsubscribed(contact.ContactEmail, newsletter.NewsletterID))
                {
                    return;
                }

                // Create new subscriber
                if (subscriber == null)
                {
                    var subscriptionProvider = Service.Resolve<ISubscriptionHelper>();
                    subscriber = subscriptionProvider.CreateSubscriber(contact, siteId);
                }

                Subscribe(subscriber, newsletter, subscriptionService);
            }
            else
            {
                string email = GetEmailToUnsubscribe(contact, subscriber, newsletter, subscriptionService);
                if (string.IsNullOrEmpty(email))
                {
                    return;
                }
                Unsubscribe(email, newsletter, subscriptionService);
            }
        }


        private void Unsubscribe(string email, NewsletterInfo newsletter, ISubscriptionService subscriptionService)
        {
            var originalSiteName = SiteContext.CurrentSiteName;

            try
            {
                // Propagate site to use correct site context for macro resolving within emails
                SiteContext.CurrentSiteName = SiteName;
                var sendConfirmation = InheritDoubleOptIn ? newsletter.NewsletterSendOptInConfirmation : false;
                subscriptionService.UnsubscribeFromSingleNewsletter(email, newsletter.NewsletterID, null, sendConfirmation);
            }   
            finally
            {
                SiteContext.CurrentSiteName = originalSiteName;
            }
        }


        private void Subscribe(SubscriberInfo subscriber, NewsletterInfo newsletter, ISubscriptionService subscriptionService)
        {
            var settings = new SubscribeSettings
            {
                SendConfirmationEmail = InheritDoubleOptIn ? newsletter.NewsletterSendOptInConfirmation : false,
                RemoveUnsubscriptionFromNewsletter = false,
                RemoveAlsoUnsubscriptionFromAllNewsletters = false,
                AllowOptIn = InheritDoubleOptIn ? newsletter.NewsletterEnableOptIn : false,
            };

            var originalSiteName = SiteContext.CurrentSiteName;

            try
            {
                // Propagate site to use correct site context for macro resolving within emails
                SiteContext.CurrentSiteName = SiteName;
                subscriptionService.Subscribe(subscriber.SubscriberID, newsletter.NewsletterID, settings);
            }
            finally
            {
                SiteContext.CurrentSiteName = originalSiteName;
            }
        }


        /// <summary>
        /// Gets a value indicating whether the contact has active subscription.
        /// Contact can be subscribed directly or using contact group.
        /// Checks only active subscriptions (not approved one are omitted, unsubscriptions are reflected).
        /// </summary>
        private static bool HasActiveSubscription(ContactInfo contact, NewsletterInfo newsletter, ISubscriptionService subscriptionService)
        {
            var numberOfSubscriptions = subscriptionService.GetAllActiveSubscriptions(contact.ContactID)
                                                           .WhereEquals("NewsletterID", newsletter.NewsletterID)
                                                           .Count;
            return numberOfSubscriptions > 0;
        }


        private static string GetEmailToUnsubscribe(ContactInfo contact, SubscriberInfo subscriber, NewsletterInfo newsletter, ISubscriptionService subscriptionService)
        {
            if (subscriber == null || !subscriptionService.IsMarketable(subscriber.SubscriberID, newsletter.NewsletterID))
            {
                if (string.IsNullOrEmpty(contact.ContactEmail) || !HasActiveSubscription(contact, newsletter, subscriptionService))
                {
                    return null;
                }

                return contact.ContactEmail;
            }

            return subscriber.SubscriberEmail;
        }
    }
}