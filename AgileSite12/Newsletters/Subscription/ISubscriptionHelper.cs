using System;

using CMS.ContactManagement;

namespace CMS.Newsletters
{
    /// <summary>
    /// Handles all work with subscriptions.
    /// </summary>
    internal interface ISubscriptionHelper
    {
        /// <summary>
        /// Checks existence of a subscription for the subscriber and
        /// newsletter. No advanced checks (like 
        /// if subscription is approved etc.) are performed. 
        /// </summary>
        /// <param name="subscriberID">Subscriber ID</param>
        /// <param name="newsletterID">Newsletter ID</param>
        /// <returns>Returns <c>true</c> if subscriber is subscribed to newsletter.</returns>
        bool SubscriptionExists(int subscriberID, int newsletterID);


        /// <summary>
        /// Returns <c>true</c> if subscription exceeded OPT-IN interval.
        /// </summary>
        /// <param name="subscription">Subscription.</param>
        bool IsExceeded(SubscriberNewsletterInfo subscription);


        /// <summary>
        /// Subscribes the subscriber to the newsletter.
        /// Sets the subscription approve column. If false, double opt-in email must be send after this method.
        /// </summary>
        /// <param name="subscriberID">Subscriber ID</param>
        /// <param name="newsletterID">Newsletter ID</param>
        /// <param name="isApproved">Sets the subscription approve column. If false, double opt-in email must be send after this method.</param>
        void Subscribe(int subscriberID, int newsletterID, bool isApproved);


        /// <summary>
        /// Creates subscriber from contact object and saves to the database.
        /// </summary>
        /// <param name="contact">ContactInfo object</param>
        /// <param name="siteId">Site ID</param>
        /// <param name="sourceSubscriber">Subscriber info with initial data used for subscriber creation.</param>
        /// <returns>Subscriber object</returns>
        SubscriberInfo CreateSubscriber(ContactInfo contact, int siteId, SubscriberInfo sourceSubscriber = null);


        /// <summary>
        /// Creates subscriber from contact group object and saves to the database.
        /// </summary>
        /// <param name="contactGroup">ContactGroupInfo object</param>
        /// <param name="siteId">Site ID</param>
        /// <returns>Subscriber object</returns>
        SubscriberInfo CreateSubscriber(ContactGroupInfo contactGroup, int siteId);
    }
}