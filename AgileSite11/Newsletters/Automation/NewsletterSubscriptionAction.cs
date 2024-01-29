using System;

using CMS.Automation;
using CMS.ContactManagement;
using CMS.Core;
using CMS.Helpers;
using CMS.EventLog;
using CMS.DataEngine;
using CMS.SiteProvider;

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
        public virtual string NewsletterName
        {
            get
            {
                return GetResolvedParameter("NewsletterName", String.Empty);
            }
        }


        /// <summary>
        /// Gets current action - 0 for SUBSCRIBE, 1 for UNSUBSCRIBE contact.
        /// </summary>
        public virtual int Action
        {
            get
            {
                return GetResolvedParameter("Action", SUBSCRIBE_ACTION);
            }
        }


        /// <summary>
        /// Site name.
        /// </summary>
        public virtual string SiteName
        {
            get
            {
                return GetResolvedParameter("Site", String.Empty);
            }
        }

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
                    var subscriptionProvider = Service.Resolve<ISubscriptionProvider>();
                    subscriber = subscriptionProvider.CreateSubscriber(contact, siteId);
                }

                // Add to current newsletter
                subscriptionService.Subscribe(subscriber.SubscriberID, newsletter.NewsletterID, new SubscribeSettings
                {
                    SendConfirmationEmail = false,
                    RemoveAlsoUnsubscriptionFromAllNewsletters = false,
                    AllowOptIn = false,
                });
            }
            // Unsubscribe action
            else
            {
                string email = GetEmailToUnsubscribe(subscriptionService, newsletter, subscriber, contact);
                if (string.IsNullOrEmpty(email))
                {
                    return;
                }
                subscriptionService.UnsubscribeFromSingleNewsletter(email, newsletter.NewsletterID, null, false);
            }
        }


        /// <summary>
        /// Gets a value indicating whether the contact has active subscription.
        /// Contact can be subscribed directly or using contact group.
        /// Checks only active subscriptions (not approved one are omitted, unsubscriptions are reflected).
        /// </summary>
        private static bool HasActiveSubscription(ISubscriptionService subscriptionService, NewsletterInfo newsletter, ContactInfo contact)
        {
            var numberOfSubscriptions = subscriptionService.GetAllActiveSubscriptions(contact.ContactID)
                                                            .WhereEquals("NewsletterID", newsletter.NewsletterID)
                                                            .Count;
            return numberOfSubscriptions > 0;
        }


        private static string GetEmailToUnsubscribe(ISubscriptionService subscriptionService, NewsletterInfo newsletter, SubscriberInfo subscriber, ContactInfo contact)
        {
            if (subscriber == null || !subscriptionService.IsSubscribed(subscriber.SubscriberID, newsletter.NewsletterID))
            {
                if (string.IsNullOrEmpty(contact.ContactEmail) || !HasActiveSubscription(subscriptionService, newsletter, contact))
                {
                    return null;
                }

                return contact.ContactEmail;
            }

            return subscriber.SubscriberEmail;
        }
    }
}