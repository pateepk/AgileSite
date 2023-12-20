using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.ContactManagement;
using CMS.Core;
using CMS.DataEngine;
using CMS.SiteProvider;
using CMS.UIControls;

namespace CMS.Newsletters.Web.UI.Internal
{
    /// <summary>
    /// Provides email marketing status calculation for newsletter recipients.
    /// </summary>
    public class RecipientStatusCalculator
    {
        /// <summary>
        /// Key under which is stored email recipient status in result <see cref="IDataContainer"/>.
        /// </summary>
        public const string EMAIL_RECIPIENT_STATUS = "EmailRecipientStatus";


        /// <summary>
        /// Key under which is stored subscription status in result <see cref="IDataContainer"/>.
        /// </summary>
        public const string SUBSCRIPTION_STATUS = "SubscriptionStatus";


        internal const string SUBSCRIPTION_STATUS_NO_EMAIL = "noemail";
        internal const string SUBSCRIPTION_STATUS_WAITING_FOR_CONFIRMATION = "waitingforconfirmation";
        internal const string SUBSCRIPTION_STATUS_UNSUBSCRIBED = "unsubscribed";
        internal const string SUBSCRIPTION_STATUS_SUBSCRIBED = "subscribed";
        private readonly SafeDictionary<int, IDataContainer> mStatusDictionary = new SafeDictionary<int, IDataContainer>();
        private readonly int mNewsletterId;


        /// <summary>
        /// Initializes new instance of <see cref="RecipientStatusCalculator"/>.
        /// </summary>
        /// <param name="newsletterNewsletterId">ID of newsletter for which status calculation should be performed.</param>
        public RecipientStatusCalculator(int newsletterNewsletterId)
        {
            mNewsletterId = newsletterNewsletterId;
        }


        /// <summary>
        /// Returns dictionary of subscriber ids filled with data for <see cref="ObjectTransformation"/>. Key of the dictionary is the ID of subscriber.
        /// </summary>
        /// <param name="subscriberIds">IDs of subscribers which the dictionary is to be filled with.</param>
        public SafeDictionary<int, IDataContainer> GetStatuses(IEnumerable<int> subscriberIds)
        {
            var subscriberIdsList = subscriberIds.ToList();
            var subscribersWhere = new WhereCondition().WhereIn("SubscriberID", subscriberIdsList);
            var contactIds = GetContactIds(subscribersWhere);

            MarkAllAsMarketableAndSubscribed(subscriberIdsList);
            MarkUnsubscribedFromCurentNewsletter(subscribersWhere, contactIds);
            MarkGloballyUnsubscribed(subscribersWhere, contactIds);
            MarkMissingEmail(contactIds, subscribersWhere);
            MarkWaitingForConfirmation(subscribersWhere);
            MarkBounced(subscribersWhere, contactIds);

            return mStatusDictionary;
        }


        private static IList<int> GetContactIds(WhereCondition subscribersWhere)
        {
            var contactIds = SubscriberInfoProvider.GetSubscribers()
                                                   .Where(subscribersWhere)
                                                   .Column("SubscriberRelatedID")
                                                   .GetListResult<int>();
            return contactIds;
        }


        private void MarkAllAsMarketableAndSubscribed(List<int> subscriberIdsList)
        {
            foreach (var id in subscriberIdsList)
            {
                SaveOrUpdateStatusDictionary(id, EmailRecipientStatusEnum.Marketable, SUBSCRIPTION_STATUS_SUBSCRIBED);
            }
        }


        private void MarkUnsubscribedFromCurentNewsletter(WhereCondition subscribersWhere, IList<int> contactIds)
        {
            var service = new IssueRecipientsListService();

            var unsubscribed = SubscriberInfoProvider.GetSubscribers()
                                                     .Where(subscribersWhere)
                                                     .WhereIn("SubscriberRelatedID",
                                                         service.GetUnsubscribedContactsIds(contactIds, mNewsletterId))
                                                     .Column("SubscriberID")
                                                     .GetListResult<int>();

            foreach (var id in unsubscribed)
            {
                SaveOrUpdateStatusDictionary(id, EmailRecipientStatusEnum.Marketable, SUBSCRIPTION_STATUS_UNSUBSCRIBED);
            }
        }


        private void MarkGloballyUnsubscribed(WhereCondition subscribersWhere, IList<int> contactIds)
        {
            var unsubscribeProvider = Service.Resolve<IUnsubscriptionProvider>();

            var globalUnsubscribedContactsIds = ContactInfoProvider.GetContacts()
                                                                   .WhereIn("ContactID", contactIds)
                                                                   .WhereIn("ContactEmail", unsubscribeProvider.GetUnsubscriptionsFromAllNewsletters().Column("UnsubscriptionEmail"))
                                                                   .Column("ContactID")
                                                                   .GetListResult<int>();

            var globalUnsubscribedSubscribers = SubscriberInfoProvider.GetSubscribers()
                                                                      .Where(subscribersWhere)
                                                                      .WhereIn("SubscriberRelatedID", globalUnsubscribedContactsIds)
                                                                      .Column("SubscriberID")
                                                                      .GetListResult<int>();

            foreach (var id in globalUnsubscribedSubscribers)
            {
                SaveOrUpdateStatusDictionary(id, EmailRecipientStatusEnum.OptedOut, SUBSCRIPTION_STATUS_UNSUBSCRIBED);
            }
        }


        private void MarkMissingEmail(IList<int> contactIds, WhereCondition subscribersWhere)
        {
            var contactsWithoutEmail = ContactInfoProvider.GetContacts()
                                                          .WhereIn("ContactID", contactIds)
                                                          .WhereEmpty("ContactEmail")
                                                          .Column("ContactID");


            var withMissingEmail = SubscriberInfoProvider.GetSubscribers()
                                                         .Where(subscribersWhere)
                                                         .WhereIn("SubscriberRelatedId", contactsWithoutEmail)
                                                         .Column("SubscriberID")
                                                         .GetListResult<int>();

            foreach (var id in withMissingEmail)
            {
                SaveOrUpdateStatusDictionary(id, EmailRecipientStatusEnum.MissingEmail, SUBSCRIPTION_STATUS_NO_EMAIL);
            }
        }


        private void MarkWaitingForConfirmation(WhereCondition subscribersWhere)
        {
            var waiting = SubscriberNewsletterInfoProvider.GetSubscriberNewsletters()
                                                          .Where(subscribersWhere)
                                                          .WhereEquals("NewsletterID", mNewsletterId)
                                                          .WhereEquals("SubscriptionApproved", false)
                                                          .Column("SubscriberID")
                                                          .GetListResult<int>();

            foreach (var id in waiting)
            {
                SaveOrUpdateStatusDictionary(id, null, SUBSCRIPTION_STATUS_WAITING_FOR_CONFIRMATION);
            }
        }


        private void MarkBounced(WhereCondition subscribersWhere, IList<int> contactIds)
        {
            var bounceLimit = NewsletterHelper.BouncedEmailsLimit(SiteContext.CurrentSiteName);
            var withinBounceLimit = ContactInfoProvider.GetContacts()
                                                       .WhereIn("ContactID", contactIds)
                                                       .WithoutBounces(bounceLimit)
                                                       .Column("ContactID");
            var bounced = ContactInfoProvider.GetContacts()
                                             .WhereIn("ContactID", contactIds)
                                             .WhereNotIn("ContactID", withinBounceLimit)
                                             .Column("ContactID")
                                             .GetListResult<int>();

            var bouncedSubscribers = SubscriberInfoProvider.GetSubscribers()
                                                           .Where(subscribersWhere)
                                                           .WhereIn("SubscriberRelatedId", bounced)
                                                           .Column("SubscriberID")
                                                           .GetListResult<int>();


            foreach (var id in bouncedSubscribers)
            {
                SaveOrUpdateStatusDictionary(id, EmailRecipientStatusEnum.Bounced, null);
            }
        }


        private void SaveOrUpdateStatusDictionary(int subscriberId, EmailRecipientStatusEnum? recipientStatusEnum, string subscriptionStatus)
        {
            var container = new DataContainer();

            container.SetValue(EMAIL_RECIPIENT_STATUS, recipientStatusEnum ?? mStatusDictionary[subscriberId].GetValue(EMAIL_RECIPIENT_STATUS));
            container.SetValue(SUBSCRIPTION_STATUS, subscriptionStatus ?? mStatusDictionary[subscriberId].GetValue(SUBSCRIPTION_STATUS));

            mStatusDictionary[subscriberId] = container;
        }
    }
}