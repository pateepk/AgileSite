﻿using System;

using CMS.ContactManagement;
using CMS.DataEngine;

namespace CMS.Newsletters
{
    /// <summary>
    /// Handles all work with subscriptions and unsubscriptions.
    /// </summary>
    public interface ISubscriptionService
    {
        /// <summary>
        /// Returns <c>true</c> if subscriber receives marketing emails.
        /// </summary>
        /// <param name="subscriberID">Subscriber ID.</param>
        /// <param name="newsletterID">Newsletter ID.</param>
        /// <exception cref="ArgumentException">Subscriber or newsletter does not exist</exception>
        /// <remarks>If subscriber represents contact group, method only checks the contact 
        /// group is subscribed (does not check particular contacts).</remarks>
        bool IsMarketable(int subscriberID, int newsletterID);


        /// <summary>
        /// Returns <c>true</c> if given <paramref name="contact"/> receives marketing emails of the given <paramref name="newsletter"/>.
        /// </summary>
        /// <param name="contact">Contact.</param>
        /// <param name="newsletter">Newsletter.</param>
        /// <exception cref="ArgumentNullException"><paramref name="contact"/> or <paramref name="newsletter"/> is <c>null</c>.</exception>
        bool IsMarketable(ContactInfo contact, NewsletterInfo newsletter);


        /// <summary>
        /// Method returns all subscriptions for given contact. Contact can be subscribed directly or using contact group.
        /// Only active subscriptions are retrieved (not approved one are omitted, unsubscriptions are reflected).
        /// Does not filter out subscriptions blocked using bounces.
        /// </summary>
        /// <example>
        /// Example shows how to get all newsletters the current contact is subscribed to
        /// <code>
        /// var newsletterIds = Service&lt;ISubscriptionService&gt;.Entry().GetAllActiveSubscriptions(currentContact);
        ///                                                          .Column("NewsletterID")
        ///                                                          .Distinct()
        /// </code>
        /// </example>
        /// <param name="contactId">ContactId that subscriptions are returned for</param>
        /// <returns>Returns <see cref="ObjectQuery"/> with all <see cref="SubscriberNewsletterInfo"/> objects that are valid for the given contact</returns>
        ObjectQuery<SubscriberNewsletterInfo> GetAllActiveSubscriptions(int contactId);


        /// <summary>
        /// Returns true if specified email address is unsubscribed from newsletter.
        /// </summary>
        /// <param name="email">Email address</param>
        /// <param name="newsletterID">Newsletter ID</param>
        /// <exception cref="ArgumentException">Email is not specified</exception>
        /// <remarks>Takes global unsubscription into account.</remarks>
        bool IsUnsubscribed(string email, int newsletterID);


        /// <summary>
        /// Unsubscribes the email from newsletter. <see cref="NewsletterEvents.SubscriberUnsubscribes"/> event is invoked after unsubscribing.
        /// Binding in the newsletter-subscription table will stay. 
        /// Email will be added into the unsubscription list.
        /// Optionally, you can decide whether to send confirmation email.
        /// </summary>
        /// <param name="email">Email address</param>
        /// <param name="newsletterID">Newsletter to unsubscribe email from</param>
        /// <param name="issueID">ID of issue that visitor used for unsubscription. Use only when issue ID is known. When issue ID is present, number of unsubscriptions for this issue is increased</param>
        /// <param name="sendConfirmationEmail">If true, confirmation email will be send after successful unsubscription</param>
        /// <exception cref="ArgumentException">Newsletter does not exist or email is not specified</exception>
        void UnsubscribeFromSingleNewsletter(string email, int newsletterID, int? issueID = null, bool sendConfirmationEmail = true);


        /// <summary>
        /// Unsubscribes the email from all newsletters. <see cref="NewsletterEvents.SubscriberUnsubscribes"/> event is invoked after unsubscribing.
        /// Binding in the newsletter-subscription table will stay.
        /// Email will be added into the unsubscription list.
        /// </summary>
        /// <param name="email">Email address</param>
        /// <param name="issueID">ID of issue that visitor used for unsubscription. Use only when issue ID is known.</param>
        /// <param name="sendConfirmationEmail">The parameter for backward compatibility - it is not used anymore.</param>
        /// <exception cref="ArgumentException">Email is not specified</exception>
        void UnsubscribeFromAllNewsletters(string email, int? issueID = null, bool sendConfirmationEmail = true);


        /// <summary>
        /// Subscribes subscriber to given newsletter.
        /// </summary>
        /// <remarks>      
        /// If the subscription is approved, performs logging of the subscription logging activity.
        /// Whether or not email is removed from newsletter unsubscription or "unsubscribe from all" list can be specified in <paramref name="subscribeSettings"/> parameter.
        /// </remarks>
        /// <param name="subscriberID">Subscriber ID.</param>
        /// <param name="newsletterID">Newsletter ID.</param>
        /// <param name="subscribeSettings">Options defining additional behavior of this method. Cannot be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="subscribeSettings"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Newsletter does not exist or email is not specified.</exception>
        void Subscribe(int subscriberID, int newsletterID, SubscribeSettings subscribeSettings);


        /// <summary>
        /// Subscribes contact to given newsletter.
        /// </summary>
        /// <remarks>
        /// If the subscription is approved, performs logging of the subscription logging activity.
        /// Whether or not email is removed from newsletter unsubscription or "unsubscribe from all" list can be specified in <paramref name="subscribeSettings"/> parameter.
        /// </remarks>
        /// <param name="contact"><see cref="ContactInfo"/> object.</param>
        /// <param name="newsletter"><see cref="NewsletterInfo"/> object.</param>
        /// <param name="subscribeSettings">Options defining additional behavior of this method. Cannot be <c>null</c>.</param>
        /// <exception cref="ArgumentNullException">Either <paramref name="contact"/>, <paramref name="newsletter"/>, or <paramref name="subscribeSettings"/> is <c>null</c>.</exception>
        void Subscribe(ContactInfo contact, NewsletterInfo newsletter, SubscribeSettings subscribeSettings);


        /// <summary>
        /// Subscribes contact group to given newsletter.
        /// </summary>
        /// <param name="contactGroup">ContactGroupInfo object</param>
        /// <param name="newsletter">NewsletterInfo object</param>
        /// <exception cref="ArgumentNullException">Either <paramref name="contactGroup"/> or <paramref name="newsletter"/> is <c>null</c>.</exception>
        void Subscribe(ContactGroupInfo contactGroup, NewsletterInfo newsletter);


        /// <summary>
        /// Unsubscribe the subscriber from the newsletter.
        /// Removes the binding from newsletter subscriber table.
        /// </summary>
        /// <param name="subscriberID">Subscriber ID</param>
        /// <param name="newsletterID">Newsletter ID</param>
        /// <param name="sendConfirmationEmail">Indicates whether to send confirmation email or not</param>
        /// <exception cref="ArgumentException">Newsletter or subscriber does not exist or subscription does not exist.</exception>
        void RemoveSubscription(int subscriberID, int newsletterID, bool sendConfirmationEmail = true);
    }
}