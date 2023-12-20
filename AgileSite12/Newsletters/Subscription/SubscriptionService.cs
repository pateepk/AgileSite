using System;

using CMS;
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
        private readonly ISubscriptionHelper mSubscriptionHelper;
        private readonly IUnsubscriptionProvider mUnsubscriptionProvider;
        private readonly ISubscriberEmailRetriever mSubscriberEmailRetriever;
        private readonly IConfirmationSender mConfirmationSender;
        private readonly INewslettersActivityLogger mNewslettersActivityLogger;
        private readonly IBounceDetection mBounceDetection;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="subscriptionHelper">Implementation of <see cref="ISubscriptionHelper"/>.</param>
        /// <param name="unsubscriptionProvider">Implementation of <see cref="IUnsubscriptionProvider"/>.</param>
        /// <param name="subscriberEmailRetriever">Implementation of <see cref="ISubscriberEmailRetriever"/>.</param>
        /// <param name="confirmationSender">Implementation of <see cref="IConfirmationSender"/>.</param>
        /// <param name="newslettersActivityLogger">Implementation of <see cref="INewslettersActivityLogger"/>.</param>
        /// <param name="bounceDetection">Implementation of <see cref="IBounceDetection"/>.</param>
        public SubscriptionService(ISubscriptionHelper subscriptionHelper, IUnsubscriptionProvider unsubscriptionProvider, ISubscriberEmailRetriever subscriberEmailRetriever, IConfirmationSender confirmationSender, INewslettersActivityLogger newslettersActivityLogger, IBounceDetection bounceDetection)
        {
            mSubscriptionHelper = subscriptionHelper;
            mUnsubscriptionProvider = unsubscriptionProvider;
            mSubscriberEmailRetriever = subscriberEmailRetriever;
            mConfirmationSender = confirmationSender;
            mNewslettersActivityLogger = newslettersActivityLogger;
            mBounceDetection = bounceDetection;
        }


        #region "Public methods"

        /// <summary>
        /// Returns <c>true</c> if subscriber receives marketing emails.
        /// </summary>
        /// <param name="subscriberID">Subscriber ID.</param>
        /// <param name="newsletterID">Newsletter ID.</param>
        /// <exception cref="ArgumentException">Subscriber -or- newsletter does not exist</exception>
        /// <remarks>If subscriber represents contact group, method only checks the contact 
        /// group is subscribed (does not check particular contacts).</remarks>
        public bool IsMarketable(int subscriberID, int newsletterID)
        {
            var subscriber = SubscriberInfoProvider.GetSubscriberInfo(subscriberID);
            var newsletter = NewsletterInfoProvider.GetNewsletterInfo(newsletterID);

            if (subscriber == null)
            {
                throw new ArgumentException("Subscriber does not exist", nameof(subscriberID));
            }
            if (newsletter == null)
            {
                throw new ArgumentException("Newsletter does not exist", nameof(newsletterID));
            }

            var subscription = SubscriberNewsletterInfoProvider.GetSubscriberNewsletterInfo(subscriberID, newsletterID);

            // If group subscriber, no need to look at email as there is no one.
            if (IsContactGroupSubscriber(subscriber))
            {
                return subscription != null;
            }

            var email = mSubscriberEmailRetriever.GetSubscriberEmail(subscriberID);
            return IsApproved(subscription) && !IsUnsubscribed(newsletter, email) && !mBounceDetection.IsBounced(subscriber);
        }


        /// <summary>
        /// Returns <c>true</c> if given <paramref name="contact"/> receives marketing emails of the given <paramref name="newsletter"/>.
        /// </summary>
        /// <param name="contact">Contact.</param>
        /// <param name="newsletter">Newsletter.</param>
        /// <exception cref="ArgumentNullException"><paramref name="contact"/> or <paramref name="newsletter"/> is <c>null</c>.</exception>
        public bool IsMarketable(ContactInfo contact, NewsletterInfo newsletter)
        {
            if (contact == null)
            {
                throw new ArgumentNullException(nameof(contact));
            }

            if (newsletter == null)
            {
                throw new ArgumentNullException(nameof(newsletter));
            }

            var subscriber = SubscriberInfoProvider.GetSubscriberInfo(PredefinedObjectType.CONTACT, contact.ContactID, newsletter.NewsletterSiteID);

            if (subscriber == null)
            {
                return false;
            }

            var subscription = SubscriberNewsletterInfoProvider.GetSubscriberNewsletterInfo(subscriber.SubscriberID, newsletter.NewsletterID);

            return IsApproved(subscription) && !IsUnsubscribed(newsletter, contact.ContactEmail) && !mBounceDetection.IsBounced(subscriber);
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
        /// <remarks>Takes global unsubscription into account.</remarks>
        public bool IsUnsubscribed(string email, int newsletterID)
        {
            if (String.IsNullOrEmpty(email))
            {
                throw new ArgumentException("[SubscriptionService.IsUnsubscribed]: Email must be specified", nameof(email));
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
                SubscriberInfo subscriber;
                if (newsletter.NewsletterType == EmailCommunicationTypeEnum.EmailCampaign)
                {
                    var contactId = ContactInfoProvider.GetContactIDByEmail(email);
                    subscriber = SubscriberInfoProvider.CreateSubscriberFromContact(contactId, new SubscriberInfo());
                }
                else
                {
                    subscriber = SubscriberInfoProvider.GetFirstSubscriberWithSpecifiedEmail(newsletterID, email);
                }

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
        /// <param name="sendConfirmationEmail">The parameter for backward compatibility - it is not used anymore.</param>
        /// <exception cref="InvalidOperationException"><paramref name="email"/> is already unsubscribed.</exception>
        /// <exception cref="ArgumentException">Email is not specified</exception>
        public void UnsubscribeFromAllNewsletters(string email, int? issueID = null, bool sendConfirmationEmail = true)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new ArgumentException("[SubscriptionService.UnsubscribeFromAllNewsletters]: Email must be specified", nameof(email));
            }

            if (mUnsubscriptionProvider.IsUnsubscribedFromAllNewsletters(email))
            {
                throw new InvalidOperationException("[SubscriptionService.UnsubscribeFromAllNewsletters]: Email is already unsubscribed");
            }

            if (issueID.HasValue)
            {
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
        public void Subscribe(int subscriberID, int newsletterID, SubscribeSettings subscribeSettings)
        {
            if (subscribeSettings == null)
            {
                throw new ArgumentNullException(nameof(subscribeSettings));
            }

            var subscriber = SubscriberInfoProvider.GetSubscriberInfo(subscriberID);
            if (subscriber == null)
            {
                throw new ArgumentException("[SubscriptionService.Subscribe]: Subscriber does not exist", nameof(subscriberID));
            }

            var newsletter = NewsletterInfoProvider.GetNewsletterInfo(newsletterID);
            if (newsletter == null)
            {
                throw new ArgumentException("[SubscriptionService.Subscribe]: Newsletter does not exist", nameof(newsletterID));
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
        public void Subscribe(ContactInfo contact, NewsletterInfo newsletter, SubscribeSettings subscribeSettings)
        {
            if (subscribeSettings == null)
            {
                throw new ArgumentNullException(nameof(subscribeSettings));
            }

            if (contact == null)
            {
                throw new ArgumentNullException(nameof(contact));
            }

            if (newsletter == null)
            {
                throw new ArgumentNullException(nameof(newsletter));
            }

            // Try to get existing subscriber using ContactId
            var subscriber = GetOrCreateSubscriberForContact(contact, newsletter, subscribeSettings);

            if (IsMarketable(subscriber.SubscriberID, newsletter.NewsletterID))
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


        private SubscriberInfo GetOrCreateSubscriberForContact(ContactInfo contact, NewsletterInfo newsletter, SubscribeSettings subscribeSettings)
        {
            var subscriber = SubscriberInfoProvider.GetSubscriberInfo(PredefinedObjectType.CONTACT, contact.ContactID, newsletter.NewsletterSiteID);

            // Create new subscriber
            if (subscriber == null)
            {
                subscriber = mSubscriptionHelper.CreateSubscriber(contact, newsletter.NewsletterSiteID, subscribeSettings.SourceSubscriber);
            }

            return subscriber;
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
            return newsletter.NewsletterEnableOptIn && mSubscriptionHelper.IsExceeded(subscription);
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
                throw new ArgumentNullException(nameof(contactGroup));
            }

            if (newsletter == null)
            {
                throw new ArgumentNullException(nameof(newsletter));
            }

            // Try to get existing subscriber using ContactId
            SubscriberInfo subscriber = SubscriberInfoProvider.GetSubscriberInfo(PredefinedObjectType.CONTACTGROUP, contactGroup.ContactGroupID, newsletter.NewsletterSiteID);

            // Create new subscriber
            if (subscriber == null)
            {
                subscriber = mSubscriptionHelper.CreateSubscriber(contactGroup, newsletter.NewsletterSiteID);
            }

            if (mSubscriptionHelper.SubscriptionExists(subscriber.SubscriberID, newsletter.NewsletterID))
            {
                return;
            }

            mSubscriptionHelper.Subscribe(subscriber.SubscriberID, newsletter.NewsletterID, true);
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
                throw new ArgumentException("[SubscriptionService.RemoveSubscription]: Subscriber does not exist.", nameof(subscriberID));
            }
            if (newsletter == null)
            {
                throw new ArgumentException("[SubscriptionService.RemoveSubscription]: Newsletter does not exist.", nameof(newsletterID));
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


        private bool IsContactGroupSubscriber(SubscriberInfo subscriber)
        {
            return string.Equals(subscriber.SubscriberType, PredefinedObjectType.CONTACTGROUP, StringComparison.OrdinalIgnoreCase);
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
                // removes newsletter specific unsubscription and "unsubscription from all"
                mUnsubscriptionProvider.RemoveUnsubscriptionFromSingleNewsletter(email, newsletter.NewsletterID);
                return;
            }

            if (subscribeSettings.RemoveUnsubscriptionFromNewsletter)
            {
                UnsubscriptionInfoProvider.GetUnsubscriptions()
                    .WhereEquals("UnsubscriptionNewsletterID", newsletter.NewsletterID)
                    .WhereEquals("UnsubscriptionEmail", email)
                    .ForEachObject(unsubscription => unsubscription.Delete());
            }
        }


        /// <summary>
        /// Subscribes subscriber to the given newsletter.
        /// Whether or not email is removed from newsletter unsubscription or "unsubscribe from all" list can be specified in <paramref name="subscribeSettings"/> parameter.
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
                throw new ArgumentNullException(nameof(subscriber));
            }

            if (newsletter == null)
            {
                throw new ArgumentNullException(nameof(newsletter));
            }

            if (subscribeSettings == null)
            {
                throw new ArgumentNullException(nameof(subscribeSettings));
            }

            bool isApproved = !(subscribeSettings.AllowOptIn && newsletter.NewsletterEnableOptIn);

            mSubscriptionHelper.Subscribe(subscriber.SubscriberID, newsletter.NewsletterID, isApproved);

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
