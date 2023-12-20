using System;

using CMS;
using CMS.Activities;
using CMS.Base;
using CMS.ContactManagement;
using CMS.Core;
using CMS.DataEngine;
using CMS.Newsletters;

[assembly: RegisterImplementation(typeof(ISubscriptionApprovalService), typeof(SubscriptionApprovalService), Priority = CMS.Core.RegistrationPriority.SystemDefault)]

namespace CMS.Newsletters
{
    /// <summary>
    /// Approves subscription by provided hash.
    /// </summary>
    internal class SubscriptionApprovalService : ISubscriptionApprovalService
    {
        private readonly IUnsubscriptionProvider mUnsubscriptionProvider;
        private readonly ISubscriptionHashValidator mSubscriptionHashValidator;
        private readonly ISubscriberEmailRetriever mSubscriberEmailRetriever;
        private readonly IConfirmationSender mConfirmationSender;
        private readonly INewslettersActivityLogger mNewslettersActivityLogger;
        private readonly IHttpContextAccessor mHttpContextAccessor = Service.Resolve<IHttpContextAccessor>();


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="unsubscriptionProvider">Implementation of <see cref="IUnsubscriptionProvider"/> interface.</param>
        /// <param name="subscriptionHashValidator">Implementation of <see cref="ISubscriptionHashValidator"/> interface.</param>
        /// <param name="subscriberEmailRetriever">Implementation of <see cref="ISubscriberEmailRetriever"/> interface.</param>
        /// <param name="confirmationSender">Implementation of <see cref="IConfirmationSender"/> interface.</param>
        /// <param name="newslettersActivityLogger">Implementation of <see cref="INewslettersActivityLogger"/> interface.</param>
        public SubscriptionApprovalService(IUnsubscriptionProvider unsubscriptionProvider, ISubscriptionHashValidator subscriptionHashValidator, ISubscriberEmailRetriever subscriberEmailRetriever, IConfirmationSender confirmationSender, INewslettersActivityLogger newslettersActivityLogger)
        {
            mUnsubscriptionProvider = unsubscriptionProvider;
            mSubscriptionHashValidator = subscriptionHashValidator;
            mSubscriberEmailRetriever = subscriberEmailRetriever;
            mConfirmationSender = confirmationSender;
            mNewslettersActivityLogger = newslettersActivityLogger;
        }


        /// <summary>
        /// Approves existing subscription - sets SubscriptionApproved to true and SubscriptionApprovedWhen to current time. 
        /// Checks if subscription wasn't already approved. Confirmation email may be sent optionally.
        /// </summary>
        /// <remarks>
        /// If the subscription is approved, performs logging of the subscription logging activity.
        /// </remarks>
        /// <param name="subscriptionHash">Hash parameter representing specific subscription</param>
        /// <param name="sendConfirmationEmail">Indicates if confirmation email should be sent. Confirmation email may also be sent if newsletter settings requires so.</param>
        /// <param name="siteName">Site name</param>
        /// <param name="datetime">Date and time of request</param>
        public ApprovalResult ApproveSubscription(string subscriptionHash, bool sendConfirmationEmail, string siteName, DateTime datetime)
        {
            // Validate hash 
            HashValidationResult hashValidationResult = mSubscriptionHashValidator.Validate(subscriptionHash, siteName, datetime);

            switch (hashValidationResult)
            {
                case HashValidationResult.Success:
                    return ApproveSubscription(SubscriberNewsletterInfoProvider.GetSubscriberNewsletterInfo(subscriptionHash), sendConfirmationEmail);

                case HashValidationResult.NotFound:
                    return ApprovalResult.NotFound;
                   
                case HashValidationResult.TimeExceeded:
                    return ApprovalResult.TimeExceeded;

                default:
                    return ApprovalResult.Failed;  
            }
        }


        /// <summary>
        /// Approves existing subscription - sets SubscriptionApproved to true and SubscriptionApprovedWhen to current time. 
        /// Checks if subscription wasn't already approved. Confirmation email may be sent optionally.
        /// When email of subscriber is blocked for all newsletters or for newsletter that is being approved, this email is deleted from blacklist when approval is successful.
        /// </summary>
        /// <param name="subscription">SubscriberNewsletterInfo object</param>
        /// <param name="sendConfirmationEmail">Indicates if confirmation email should be sent. Confirmation email may also be sent if newsletter settings requires so</param>
        private ApprovalResult ApproveSubscription(SubscriberNewsletterInfo subscription, bool sendConfirmationEmail)
        {
            // Check if subscription exists
            if (subscription == null)
            {
                return ApprovalResult.NotFound;
            }

            // Stop approving when subscriber is already approved
            if (subscription.SubscriptionApproved)
            {
                return ApprovalResult.AlreadyApproved;
            }

            // Invoke registered extensions
            foreach (var definition in DoubleOptInExtensionDefinitionRegister.Instance.Items)
            {
                definition.ApprovalAction?.Invoke(mHttpContextAccessor.HttpContext?.Request.QueryString);
            }

            SubscriberNewsletterInfoProvider.ApproveSubscription(subscription.SubscriberID, subscription.NewsletterID);

            var newsletter = NewsletterInfoProvider.GetNewsletterInfo(subscription.NewsletterID);
            if (newsletter == null)
            {
                throw new Exception("[SubscriptionApprovalService.Approvesubscription]: Newsletter that given subscription is connected with was not found.");
            }

            // Check if newsletter requires sending confirmation message
            if (sendConfirmationEmail || newsletter.NewsletterSendOptInConfirmation)
            {
                mConfirmationSender.SendConfirmationEmail(true, subscription.SubscriberID, subscription.NewsletterID);
            }

            var subscriber = SubscriberInfoProvider.GetSubscriberInfo(subscription.SubscriberID);

            // When email of subscriber is blocked for all newsletters or for newsletter that is being approved -> unblock it
            var email = mSubscriberEmailRetriever.GetSubscriberEmail(subscriber.SubscriberID);
            if (!string.IsNullOrEmpty(email))
            {
                mUnsubscriptionProvider.RemoveUnsubscriptionFromSingleNewsletter(email, newsletter.NewsletterID);
            }

            TryToLogSubscribingActivity(newsletter, subscriber, email);
            UpdateContactData(subscriber);
            return ApprovalResult.Success;
        }


        /// <summary>
        /// Tries to log <see cref="PredefinedActivityType.NEWSLETTER_SUBSCRIBING"/> activity for the currently approved subscription.
        /// </summary>
        /// <param name="newsletter">Newsletter the subscription was approved for</param>
        /// <param name="subscriber">Subscriber the subscription was approved for</param>
        /// <param name="email">Email the subscription was approved for</param>
        private void TryToLogSubscribingActivity(NewsletterInfo newsletter, SubscriberInfo subscriber, string email)
        {
            if (!ModuleEntryManager.IsModuleLoaded(ModuleName.ONLINEMARKETING))
            {
                return;
            }

            var contactId = new ObjectQuery(PredefinedObjectType.CONTACT).WhereEquals("ContactEmail", email)
                                                                         .Column("ContactID")
                                                                         .TopN(1)
                                                                         .GetScalarResult(0);

            if (contactId > 0)
            {
                mNewslettersActivityLogger.LogNewsletterSubscribingActivity(newsletter, subscriber.SubscriberID, contactId);
            }
        }


        private static void UpdateContactData(SubscriberInfo subscriber)
        {
            DataClassInfo classInfo = DataClassInfoProvider.GetDataClassInfo(subscriber.TypeInfo.ObjectClassName);
            ContactInfoProvider.UpdateContactFromExternalData(subscriber, classInfo.ClassContactOverwriteEnabled, subscriber.SubscriberRelatedID);
        }
    }
}
