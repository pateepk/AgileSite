using System;

using CMS;
using CMS.Base;
using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Newsletters;

[assembly: RegisterImplementation(typeof(ISubscriptionService), typeof(SubscriptionService), Priority = CMS.Core.RegistrationPriority.SystemDefault)]

namespace CMS.Newsletters
{
    /// <summary>
    /// Handles all work with subscriptions and unsubscriptions.
    /// </summary>
    internal class SubscriptionService : ISubscriptionService
    {
        private const int CONTACT_GROUP_MEMBER_CONTACT = 0;
        private readonly ISubscriptionProvider mSubscriptionProvider;
        private readonly IUnsubscriptionProvider mUnsubscriptionProvider;
        private readonly ISubscriberEmailRetriever mSubscriberEmailRetriever;
        private readonly IConfirmationSender mConfirmationSender;
        private readonly NewslettersActivityLogger mNewslettersActivityLogger;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="subscriptionProvider">Implementation of <see cref="ISubscriptionProvider"/></param>
        /// <param name="unsubscriptionProvider">Implementation of <see cref="IUnsubscriptionProvider"/></param>
        /// <param name="subscriberEmailRetriever">Implementation of <see cref="ISubscriberEmailRetriever"/></param>
        /// <param name="confirmationSender">Implementation of <see cref="IConfirmationSender"/></param>
        public SubscriptionService(ISubscriptionProvider subscriptionProvider, IUnsubscriptionProvider unsubscriptionProvider, ISubscriberEmailRetriever subscriberEmailRetriever, IConfirmationSender confirmationSender)
        {
            mSubscriptionProvider = subscriptionProvider;
            mUnsubscriptionProvider = unsubscriptionProvider;
            mSubscriberEmailRetriever = subscriberEmailRetriever;
            mConfirmationSender = confirmationSender;
            mNewslettersActivityLogger = new NewslettersActivityLogger();
        }


        #region "Public methods"

        /// <summary>
        /// Returns true if subscriber is subscribed to given newsletter and at the same time is not in the unsubscription list for the given newsletter.
        /// </summary>
        /// <param name="subscriberID">Subscriber ID</param>
        /// <param name="newsletterID">Newsletter ID</param>
        /// <exception cref="ArgumentException">Subscriber -or- newsletter does not exist</exception>
        /// <returns>True if subscriber is not unsubscribed</returns>
        public bool IsSubscribed(int subscriberID, int newsletterID)
        {
            var subscriber = SubscriberInfoProvider.GetSubscriberInfo(subscriberID);
            var newsletter = NewsletterInfoProvider.GetNewsletterInfo(newsletterID);

            if (subscriber == null)
            {
                throw new ArgumentException("[SubscriptionService.IsSubscribed]: Subscriber does not exist", "subscriberID");
            }
            if (newsletter == null)
            {
                throw new ArgumentException("[SubscriptionService.IsSubscribed]: Newsletter does not exist", "newsletterID");
            }

            var subscribed = mSubscriptionProvider.IsSubscribed(subscriberID, newsletterID);

            // If group subscriber, no need to look at email as there is no one.
            if (IsContactGroupSubscriber(subscriber))
            {
                return subscribed;
            }

            var email = mSubscriberEmailRetriever.GetSubscriberEmail(subscriberID);

            var unsubscribed = !String.IsNullOrEmpty(email) && mUnsubscriptionProvider.IsUnsubscribedFromSingleNewsletter(email, newsletterID);
            return subscribed && !unsubscribed;
        }


        /// <summary>
        /// Returns true if given <paramref name="contact"/> is subscribed to given and at the same time is not in the unsubscription list for the given <paramref name="newsletter"/>.
        /// </summary>
        /// <param name="contact">Contact.</param>
        /// <param name="newsletter">Newsletter which the contact is about to subscribe to.</param>
        /// <exception cref="ArgumentNullException"><paramref name="contact"/> or <paramref name="newsletter"/> is <c>null</c>.</exception>
        /// <returns>True if contact is not unsubscribed</returns>
        public bool IsSubscribed(ContactInfo contact, NewsletterInfo newsletter)
        {
            if (contact == null)
            {
                throw new ArgumentNullException("contact");
            }

            var subscriber = SubscriberInfoProvider.GetSubscriberInfo(PredefinedObjectType.CONTACT, contact.ContactID, newsletter.NewsletterSiteID);

            if (subscriber == null)
            {
                return false;
            }

            if (newsletter == null)
            {
                throw new ArgumentNullException("newsletter");
            }

            bool subscribed = mSubscriptionProvider.IsSubscribed(subscriber.SubscriberID, newsletter.NewsletterID);

            var unsubscribed = !String.IsNullOrEmpty(contact.ContactEmail) && mUnsubscriptionProvider.IsUnsubscribedFromSingleNewsletter(contact.ContactEmail, newsletter.NewsletterID);
            return subscribed && !unsubscribed;
        }


        /// <summary>
        /// Method returns all subscriptions for given contact. Contact can be subscribed directly or using contact group.
        /// Only active subscriptions are retrieved (not approved ones are omitted, unsubscriptions are reflected).
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
        public ObjectQuery<SubscriberNewsletterInfo> GetAllActiveSubscriptions(int contactId)
        {
            var subscriberCondition = new WhereCondition().WhereIn("SubscriberID",
                                                             // Get subscriptions when contact is subscribed directly
                                                             SubscriberInfoProvider.GetSubscribers()
                                                                                   .Column("SubscriberID")
                                                                                   .WhereEquals("SubscriberType", PredefinedObjectType.CONTACT)
                                                                                   .WhereEquals("SubscriberRelatedID", contactId)
                                                          )
                                                          .Or()
                                                          .WhereIn("SubscriberID",
                                                             // Get subscriptions when contact is subscribed using contact group
                                                             SubscriberInfoProvider.GetSubscribers()
                                                                                   .Column("SubscriberID")
                                                                                   .WhereEquals("SubscriberType", PredefinedObjectType.CONTACTGROUP)
                                                                                   .WhereIn("SubscriberRelatedID",
                                                                                      new ObjectQuery(PredefinedObjectType.CONTACTGROUPMEMBERCONTACT)
                                                                                         .Column("ContactGroupMemberContactGroupID")
                                                                                         .WhereEquals("ContactGroupMemberType", CONTACT_GROUP_MEMBER_CONTACT)
                                                                                         .WhereEquals("ContactGroupMemberRelatedID", contactId)
                                                                                   )
                                                          );

            var activeSubscriptions = SubscriberNewsletterInfoProvider.GetSubscriberNewsletters()
                                                                      .Where(new WhereCondition().WhereTrue("SubscriptionApproved")
                                                                                                 .Or()
                                                                                                 .WhereNull("SubscriptionApproved"))
                                                                      .Where(subscriberCondition);

            // Retrieve email for given contact. Must be materialized because of possibility of separated DB
            string contactEmail = new ObjectQuery(PredefinedObjectType.CONTACT).Column("ContactEmail")
                                                                               .TopN(1)
                                                                               .WhereEquals("ContactID", contactId)
                                                                               .GetScalarResult("");

            // Filter out newsletter that contact unsubscribed from. Filter both unsubscriptions from single and all newsletters.
            if (!string.IsNullOrEmpty(contactEmail))
            {
                activeSubscriptions.WhereNotIn("NewsletterID",
                    UnsubscriptionInfoProvider.GetUnsubscriptions()
                                              .Column("UnsubscriptionNewsletterID")
                                              .WhereEquals("UnsubscriptionEmail", contactEmail)
                    );
            }

            return activeSubscriptions;
        }


        /// <summary>
        /// Returns true if specified email address is unsubscribed from newsletter.
        /// </summary>
        /// <param name="email">Email address</param>
        /// <param name="newsletterID">Newsletter ID</param>
        /// <exception cref="ArgumentException">Email is not specified</exception>
        public bool IsUnsubscribed(string email, int newsletterID)
        {
            if (String.IsNullOrEmpty(email))
            {
                throw new ArgumentException("[SubscriptionService.IsUnsubscribed]: Email must be specified", "email");
            }

            return mUnsubscriptionProvider.IsUnsubscribedFromSingleNewsletter(email, newsletterID);
        }


        /// <summary>
        /// Unsubscribes the email from newsletter. <see cref="NewsletterEvents.SubscriberUnsubscribes"/> event is invoked after unsubscribing.
        /// Binding in the newsletter-subscription table will stay. 
        /// Email will be added into the unsubscription list.
        /// Optionally, you can decide whether to send confirmation email.
        /// </summary>
        /// <param name="email">Email address</param>
        /// <param name="newsletterID">Newsletter to unsubscribe email from</param>
        /// <param name="issueID">ID of issue that visitor used for unsubscription. Use only when issue ID is known. When issue ID is present, number of unsubscriptions for this issue is increased</param>
        /// <param name="sendConfirmationEmail">If true, confirmation email will be sent after successful unsubscription</param>
        /// <exception cref="ArgumentException">Newsletter does not exist or email is not specified</exception>
        /// <exception cref="InvalidOperationException"><paramref name="email"/> is already unsubscribed for given <paramref name="newsletterID"/>.</exception>
        public void UnsubscribeFromSingleNewsletter(string email, int newsletterID, int? issueID = null, bool sendConfirmationEmail = true)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentException("Email must be specified", nameof(email));
            }

            var newsletter = NewsletterInfoProvider.GetNewsletterInfo(newsletterID);
            if (newsletter == null)
            {
                throw new ArgumentException("Newsletter does not exist", nameof(newsletterID));
            }

            if (mUnsubscriptionProvider.IsUnsubscribedFromSingleNewsletter(email, newsletter.NewsletterID))
            {
                throw new InvalidOperationException("Email is already unsubscribed");
            }

            if (sendConfirmationEmail)
            {
                var subscriber = SubscriberInfoProvider.GetFirstSubscriberWithSpecifiedEmail(newsletterID, email);
                mConfirmationSender.SendUnsubscriptionConfirmation(subscriber, newsletterID);
            }

            if (issueID.HasValue)
            {
                IssueInfoProvider.IncreaseUnsubscribeCount(issueID.Value);
            }

            mUnsubscriptionProvider.UnsubscribeFromSingleNewsletter(email, newsletter.NewsletterID, issueID);

            NewsletterEvents.SubscriberUnsubscribes.StartEvent(new UnsubscriptionEventArgs
            {
                Newsletter = newsletter,
                Email = email,
                IssueID = issueID
            });
        }


        /// <summary>
        /// Unsubscribes the email from all newsletters. <see cref="NewsletterEvents.SubscriberUnsubscribes"/> event is invoked after unsubscribing.
        /// Binding in the newsletter-subscription table will stay.
        /// Email will be added into the unsubscription list.
        /// </summary>
        /// <param name="email">Email address</param>
        /// <param name="issueID">ID of issue that visitor used for unsubscription. Use only when issue ID is known.</param>
        /// <param name="sendConfirmationEmail">If true, confirmation email will be sent after successful unsubscription</param>
        /// <exception cref="InvalidOperationException"><paramref name="email"/> is already unsubscribed.</exception>
        /// <exception cref="ArgumentException">Email is not specified</exception>
        public void UnsubscribeFromAllNewsletters(string email, int? issueID = null, bool sendConfirmationEmail = true)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentException("[SubscriptionService.UnsubscribeFromAllNewsletters]: Email must be specified", "email");
            }

            if (mUnsubscriptionProvider.IsUnsubscribedFromAllNewsletters(email))
            {
                throw new InvalidOperationException("[SubscriptionService.UnsubscribeFromAllNewsletters]: Email is already unsubscribed");
            }


            if (issueID.HasValue)
            {
                if (sendConfirmationEmail)
                {
                    var issue = IssueInfoProvider.GetIssueInfo(issueID.Value);
                    if (issue != null)
                    {
                        var subscriber = SubscriberInfoProvider.GetFirstSubscriberWithSpecifiedEmail(issue.IssueNewsletterID, email);
                        mConfirmationSender.SendUnsubscriptionConfirmation(subscriber, issue.IssueNewsletterID);
                    }
                }

                IssueInfoProvider.IncreaseUnsubscribeCount(issueID.Value);
            }

            mUnsubscriptionProvider.UnsubscribeFromAllNewsletters(email, issueID);

            NewsletterEvents.SubscriberUnsubscribes.StartEvent(new UnsubscriptionEventArgs
            {
                Email = email,
                IssueID = issueID
            });
        }


        /// <summary>
        /// Subscribes subscriber to given newsletter.
        /// Removes the email from unsubscription list for given newsletter. Whether or not email is removed from "unsubscribe from all" list can be specified in <paramref name="subscribeSettings"/> parameter.
        /// <paramref name="subscribeSettings"/> parameter specifies whether confirmation email will be sent and if subscription should be approved immediately.
        /// </summary>
        /// <remarks>
        /// If the subscription is approved, performs logging of the subscription logging activity.
        /// </remarks>
        /// <param name="subscriberID">Subscriber ID</param>
        /// <param name="newsletterID">Newsletter ID</param>
        /// <param name="subscribeSettings">Options defining additional behavior of this method. Cannot be null</param>
        /// <exception cref="ArgumentNullException"><paramref name="subscribeSettings"/> is null</exception>
        /// <exception cref="ArgumentException">Newsletter does not exist or email is not specified</exception>
        public void Subscribe(int subscriberID, int newsletterID, SubscribeSettings subscribeSettings)
        {
            if (subscribeSettings == null)
            {
                throw new ArgumentNullException("subscribeSettings");
            }

            var subscriber = SubscriberInfoProvider.GetSubscriberInfo(subscriberID);
            if (subscriber == null)
            {
                throw new ArgumentException("[SubscriptionService.Subscribe]: Subscriber does not exist", "subscriberID");
            }

            var newsletter = NewsletterInfoProvider.GetNewsletterInfo(newsletterID);
            if (newsletter == null)
            {
                throw new ArgumentException("[SubscriptionService.Subscribe]: Newsletter does not exist", "newsletterID");
            }

            if (IsContactGroupSubscriber(subscriber))
            {
                Subscribe(subscriber, newsletter, subscribeSettings);
            }
            else
            {
                var email = mSubscriberEmailRetriever.GetSubscriberEmail(subscriber.SubscriberID);
                var subscription = SubscriberNewsletterInfoProvider.GetSubscriberNewsletterInfo(subscriber.SubscriberID, newsletter.NewsletterID);

                if (SubscriptionDoesNotExist(subscription)
                    || ExpiredOptInInterval(newsletter, subscription)
                    || NewsletterWithoutDoubleOptIn(newsletter)
                    || IsUnsubscribed(newsletter, email))
                {
                    Subscribe(subscriber, newsletter, subscribeSettings);
                }
            }
        }


        private bool IsContactGroupSubscriber(SubscriberInfo subscriber)
        {
            return string.Equals(subscriber.SubscriberType, PredefinedObjectType.CONTACTGROUP, StringComparison.OrdinalIgnoreCase);
        }


        /// <summary>
        /// Subscribes contact to given newsletter.
        /// Removes the email from unsubscription list for given newsletter. Whether or not email is removed from "unsubscribe from all" list can be specified in <paramref name="subscribeSettings"/> parameter.
        /// <paramref name="subscribeSettings"/> parameter specifies whether confirmation email will be sent and if subscription should be approved immediately.
        /// </summary>
        /// <remarks>
        /// If the subscription is approved, performs logging of the subscription logging activity.
        /// </remarks>
        /// <param name="contact">ContactInfo object</param>
        /// <param name="newsletter">NewsletterInfo object</param>
        /// <param name="subscribeSettings">Options defining additional behavior of this method. Cannot be null</param>
        /// <exception cref="ArgumentNullException">Either <paramref name="contact"/>, <paramref name="newsletter"/>, or <paramref name="subscribeSettings"/> is <c>null</c>.</exception>
        public void Subscribe(ContactInfo contact, NewsletterInfo newsletter, SubscribeSettings subscribeSettings)
        {
            if (subscribeSettings == null)
            {
                throw new ArgumentNullException("subscribeSettings");
            }

            if (contact == null)
            {
                throw new ArgumentNullException("contact");
            }

            if (newsletter == null)
            {
                throw new ArgumentNullException("newsletter");
            }

            // Try to get existing subscriber using ContactId
            var subscriber = SubscriberInfoProvider.GetSubscriberInfo(PredefinedObjectType.CONTACT, contact.ContactID, newsletter.NewsletterSiteID);

            // Create new subscriber
            if (subscriber == null)
            {
                subscriber = mSubscriptionProvider.CreateSubscriber(contact, newsletter.NewsletterSiteID, subscribeSettings.SourceSubscriber);
            }


            if (HasApprovedSubscription(contact, newsletter))
            {
                return;
            }

            var subscription = SubscriberNewsletterInfoProvider.GetSubscriberNewsletterInfo(subscriber.SubscriberID, newsletter.NewsletterID);
            var email = mSubscriberEmailRetriever.GetSubscriberEmail(subscriber.SubscriberID);

            if (SubscriptionDoesNotExist(subscription)
               || ExpiredOptInInterval(newsletter, subscription)
               || NewsletterWithoutDoubleOptIn(newsletter)
               || IsUnsubscribed(newsletter, email))
            {
                Subscribe(subscriber, newsletter, subscribeSettings);
                UpdateContactDataWhenDoubleOptInDisabled(subscriber, newsletter);
            }
        }


        private static bool NewsletterWithoutDoubleOptIn(NewsletterInfo newsletter)
        {
            return !newsletter.NewsletterEnableOptIn;
        }


        private static bool SubscriptionDoesNotExist(SubscriberNewsletterInfo subscription)
        {
            return subscription == null;
        }


        private bool ExpiredOptInInterval(NewsletterInfo newsletter, SubscriberNewsletterInfo subscription)
        {
            return newsletter.NewsletterEnableOptIn && mSubscriptionProvider.IsExceeded(subscription);
        }


        private bool HasApprovedSubscription(ContactInfo contact, NewsletterInfo newsletter)
        {
            var subscriber = SubscriberInfoProvider.GetSubscriberInfo(PredefinedObjectType.CONTACT, contact.ContactID, newsletter.NewsletterSiteID);

            if (subscriber == null)
            {
                return true;
            }

            var subscription = SubscriberNewsletterInfoProvider.GetSubscriberNewsletterInfo(subscriber.SubscriberID, newsletter.NewsletterID);

            return IsApproved(subscription) && !IsUnsubscribed(newsletter, contact.ContactEmail);
        }


        private static bool IsApproved(SubscriberNewsletterInfo subscription)
        {
            return subscription != null && subscription.SubscriptionApproved;
        }


        private bool IsUnsubscribed(NewsletterInfo newsletter, string email)
        {
            if (String.IsNullOrEmpty(email))
            {
                return false;
            }
            return mUnsubscriptionProvider.IsUnsubscribedFromSingleNewsletter(email, newsletter.NewsletterID)
                   || mUnsubscriptionProvider.IsUnsubscribedFromAllNewsletters(email);
        }


        private static void UpdateContactDataWhenDoubleOptInDisabled(SubscriberInfo subscriber, NewsletterInfo newsletter)
        {
            if (newsletter.NewsletterEnableOptIn)
            {
                return;
            }

            DataClassInfo classInfo = DataClassInfoProvider.GetDataClassInfo(subscriber.TypeInfo.ObjectClassName);
            ContactInfoProvider.UpdateContactFromExternalData(subscriber, classInfo.ClassContactOverwriteEnabled, subscriber.SubscriberRelatedID);
        }


        /// <summary>
        /// Subscribes contact group to given newsletter.
        /// </summary>
        /// <param name="contactGroup">ContactGroupInfo object</param>
        /// <param name="newsletter">NewsletterInfo object</param>
        /// <exception cref="ArgumentNullException">Either <paramref name="contactGroup"/> or <paramref name="newsletter"/> is <c>null</c>.</exception>
        public void Subscribe(ContactGroupInfo contactGroup, NewsletterInfo newsletter)
        {
            if (contactGroup == null)
            {
                throw new ArgumentNullException("contactGroup");
            }

            if (newsletter == null)
            {
                throw new ArgumentNullException("newsletter");
            }

            // Try to get existing subscriber using ContactId
            SubscriberInfo subscriber = SubscriberInfoProvider.GetSubscriberInfo(PredefinedObjectType.CONTACTGROUP, contactGroup.ContactGroupID, newsletter.NewsletterSiteID);

            // Create new subscriber
            if (subscriber == null)
            {
                subscriber = mSubscriptionProvider.CreateSubscriber(contactGroup, newsletter.NewsletterSiteID);
            }

            if (!IsSubscribed(subscriber.SubscriberID, newsletter.NewsletterID))
            {
                mSubscriptionProvider.Subscribe(subscriber.SubscriberID, newsletter.NewsletterID, true);
            }
        }


        /// <summary>
        /// Unsubscribe the subscriber from the newsletter.
        /// Removes the binding from newsletter subscriber table.
        /// </summary>
        /// <param name="subscriberID">Subscriber ID</param>
        /// <param name="newsletterID">Newsletter ID</param>
        /// <param name="sendConfirmationEmail">Indicates whether to send confirmation email or not</param>
        /// <exception cref="ArgumentException">Newsletter or subscriber does not exist or subscription does not exist.</exception>
        public void RemoveSubscription(int subscriberID, int newsletterID, bool sendConfirmationEmail = true)
        {
            NewsletterInfo newsletter = NewsletterInfoProvider.GetNewsletterInfo(newsletterID);
            SubscriberInfo subscriber = SubscriberInfoProvider.GetSubscriberInfo(subscriberID);

            if (subscriber == null)
            {
                throw new ArgumentException("[SubscriptionService.RemoveSubscription]: Subscriber does not exist.", "subscriberID");
            }
            if (newsletter == null)
            {
                throw new ArgumentException("[SubscriptionService.RemoveSubscription]: Newsletter does not exist.", "newsletterID");
            }

            var subscription = SubscriberNewsletterInfoProvider.GetSubscriberNewsletterInfo(subscriber.SubscriberID, newsletter.NewsletterID);
            if (subscription == null)
            {
                throw new ArgumentException("[SubscriptionService.RemoveSubscription]: Subscriber newsletter binding does not exist.");
            }

            SubscriberNewsletterInfoProvider.DeleteSubscriberNewsletterInfo(subscription);

            if (sendConfirmationEmail)
            {
                mConfirmationSender.SendConfirmationEmail(false, subscription.SubscriberID, subscription.NewsletterID);
            }
        }

        #endregion


        #region "Private methods"

        private void LogSubscriptionActivityIfCalledFromRequest(NewsletterInfo newsletter, SubscriberInfo subscriber)
        {
            if (CMSHttpContext.Current != null)
            {
                mNewslettersActivityLogger.LogNewsletterSubscribingActivity(newsletter, subscriber.SubscriberID);
            }
        }


        /// <summary>
        /// Processes given combination of subscription and newsletter accordingly to given settings
        /// If email address can be retrieved, this email is removed from unsubscription
        /// </summary>
        private void ProcessSubscriptions(SubscribeSettings subscribeSettings, int subscriberID, NewsletterInfo newsletter)
        {
            var email = mSubscriberEmailRetriever.GetSubscriberEmail(subscriberID);
            if (string.IsNullOrEmpty(email))
            {
                return;
            }

            if (subscribeSettings.RemoveAlsoUnsubscriptionFromAllNewsletters)
            {
                mUnsubscriptionProvider.RemoveUnsubscriptionFromSingleNewsletter(email, newsletter.NewsletterID);
            }
        }


        /// <summary>
        /// Subscribes subscriber to given newsletter.
        /// Removes the email from unsubscription list for given newsletter. Whether or not email is removed from "unsubscribe form all" list can be specified in <paramref name="subscribeSettings"/> parameter.
        /// <paramref name="subscribeSettings"/> parameter specifies whether confirmation email will be send and if subscription should be approved immediately.
        /// </summary>
        /// <remarks>
        /// If the subscription is approved, performs logging of the subscription logging activity.
        /// </remarks>
        /// <param name="subscriber">SubscriberInfo object</param>
        /// <param name="newsletter">NewsletterInfo object</param>
        /// <param name="subscribeSettings">Options defining additional behavior of this method. Cannot be null.</param>
        /// <exception cref="ArgumentNullException">Either <paramref name="subscriber"/>, <paramref name="newsletter"/>, or <paramref name="subscribeSettings"/> is null.</exception>
        private void Subscribe(SubscriberInfo subscriber, NewsletterInfo newsletter, SubscribeSettings subscribeSettings)
        {
            if (subscriber == null)
            {
                throw new ArgumentNullException("subscriber");
            }

            if (newsletter == null)
            {
                throw new ArgumentNullException("newsletter");
            }

            if (subscribeSettings == null)
            {
                throw new ArgumentNullException("subscribeSettings");
            }

            bool isApproved = !(subscribeSettings.AllowOptIn && newsletter.NewsletterEnableOptIn);

            mSubscriptionProvider.Subscribe(subscriber.SubscriberID, newsletter.NewsletterID, isApproved);

            if (isApproved)
            {
                ProcessSubscriptions(subscribeSettings, subscriber.SubscriberID, newsletter);

                if (subscribeSettings.SendConfirmationEmail)
                {
                    mConfirmationSender.SendConfirmationEmail(true, subscriber.SubscriberID, newsletter.NewsletterID);
                }

                LogSubscriptionActivityIfCalledFromRequest(newsletter, subscriber);
            }
            else
            {
                // Remove from unsubscription list is done after confirmation in approval web part
                mConfirmationSender.SendDoubleOptInEmail(subscriber.SubscriberID, newsletter.NewsletterID);
            }
        }

        #endregion
    }
}
