using System;

using CMS;
using CMS.Base;
using CMS.ContactManagement;
using CMS.Core;
using CMS.Core.Internal;
using CMS.DataEngine;
using CMS.Newsletters;
using CMS.SiteProvider;

[assembly: RegisterImplementation(typeof(ISubscriptionHelper), typeof(SubscriptionHelper), Priority = RegistrationPriority.SystemDefault)]

namespace CMS.Newsletters
{
    /// <summary>
    /// Handles all work with subscriptions.
    /// Works with newsletter subscriber binding table.
    /// </summary>
    internal class SubscriptionHelper : ISubscriptionHelper
    {
        /// <summary>
        /// Returns true if subscriber is subscribed to newsletter.
        /// Looks only in the binding table.
        /// </summary>
        /// <param name="subscriberID">Subscriber ID</param>
        /// <param name="newsletterID">Newsletter ID</param>
        public bool SubscriptionExists(int subscriberID, int newsletterID)
        {
            var subscription = SubscriberNewsletterInfoProvider.GetSubscriberNewsletterInfo(subscriberID, newsletterID);
            return subscription != null;
        }


        /// <summary>
        /// Returns true if given subscription exceeded the opt-in interval (Interval is defined in settings).
        /// </summary>
        /// <param name="subscription">Subscription</param>
        public bool IsExceeded(SubscriberNewsletterInfo subscription)
        {
            if (subscription == null || subscription.SubscriptionApproved)
            {
                return false;
            }

            var elapsedTime = DateTime.Now.Subtract(subscription.SubscribedWhen);
            var settingsService = Service.Resolve<ISettingsService>();
            var interval = settingsService[SiteContext.CurrentSiteName + ".CMSOptInInterval"].ToDouble(0, null);

            return interval > 0 && elapsedTime.TotalHours > interval;
        }


        /// <summary>
        /// Subscribes the subscriber to the newsletter.
        /// Sets the subscription approve column. If false, double opt-in email must be send after this method.
        /// </summary>
        /// <param name="subscriberID">Subscriber ID</param>
        /// <param name="newsletterID">Newsletter ID</param>
        /// <param name="isApproved">Sets the subscription approve column. If false, double opt-in email must be send after this method.</param>
        public void Subscribe(int subscriberID, int newsletterID, bool isApproved)
        {
            var subscription = SubscriberNewsletterInfoProvider.GetSubscriberNewsletterInfo(subscriberID, newsletterID);
            if (subscription != null)
            {
                if (IsExceeded(subscription))
                {
                    UpdateSubscriptionDate(subscription);
                }

                SubscriberNewsletterInfoProvider.SetApprovalStatus(subscriberID, newsletterID, isApproved);
            }
            else
            {
                SubscriberNewsletterInfoProvider.AddSubscriberToNewsletter(subscriberID, newsletterID, DateTime.Now, isApproved);
            }
        }


        private static void UpdateSubscriptionDate(SubscriberNewsletterInfo subscription)
        {
            var dateTimeNowService = Service.Resolve<IDateTimeNowService>();
            subscription.SubscribedWhen = dateTimeNowService.GetDateTimeNow();
            SubscriberNewsletterInfoProvider.SetSubscriberNewsletterInfo(subscription);
        }


        /// <summary>
        /// Creates subscriber from contact object and saves to the database.
        /// </summary>
        /// <param name="contact">ContactInfo object</param>
        /// <param name="siteId">Site ID</param>
        /// <param name="sourceSubscriber">Subscriber info with initial data used for subscriber creation.</param>
        /// <returns>Subscriber object</returns>
        public SubscriberInfo CreateSubscriber(ContactInfo contact, int siteId, SubscriberInfo sourceSubscriber = null)
        {
            var subscriber = sourceSubscriber != null ? new SubscriberInfo(sourceSubscriber, false) : new SubscriberInfo();

            subscriber.SubscriberID = 0;
            subscriber.SubscriberGUID = Guid.Empty;
            subscriber.SubscriberEmail = contact.ContactEmail;
            subscriber.SubscriberFirstName = contact.ContactFirstName;
            subscriber.SubscriberLastName = contact.ContactLastName;
            subscriber.SubscriberFullName = new SubscriberFullNameFormater().GetContactSubscriberName(contact.ContactFirstName, contact.ContactMiddleName, contact.ContactLastName);
            subscriber.SubscriberSiteID = siteId;
            subscriber.SubscriberType = PredefinedObjectType.CONTACT;
            subscriber.SubscriberRelatedID = contact.ContactID;


            SubscriberInfoProvider.SetSubscriberInfo(subscriber);

            return subscriber;
        }


        /// <summary>
        /// Creates subscriber from contact group object and saves to the database.
        /// </summary>
        /// <param name="contactGroup">ContactGroupInfo object</param>
        /// <param name="siteId">Site ID</param>
        /// <returns>Subscriber object</returns>
        public SubscriberInfo CreateSubscriber(ContactGroupInfo contactGroup, int siteId)
        {
            var subscriber = new SubscriberInfo
            {
                SubscriberFirstName = contactGroup.ContactGroupDisplayName,
                SubscriberFullName = new SubscriberFullNameFormater().GetContactGroupSubscriberName(contactGroup.ContactGroupDisplayName),
                SubscriberSiteID = siteId,
                SubscriberType = PredefinedObjectType.CONTACTGROUP,
                SubscriberRelatedID = contactGroup.ContactGroupID
            };

            SubscriberInfoProvider.SetSubscriberInfo(subscriber);

            return subscriber;
        }
    }
}