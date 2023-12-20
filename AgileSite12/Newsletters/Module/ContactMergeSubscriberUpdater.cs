using System.Collections.Generic;
using System.Linq;

using CMS.ContactManagement;
using CMS.DataEngine;

namespace CMS.Newsletters
{
    internal class ContactMergeSubscriberUpdater
    {
        private readonly Dictionary<string, string> mFieldsToBeUpdated = new Dictionary<string, string>
        {
            { "ContactFirstName", "SubscriberFirstName" },
            { "ContactLastName", "SubscriberLastName"},
            { "ContactEmail", "SubscriberEmail" }
        };


        private readonly ContactInfo mSourceContact;
        private readonly ContactInfo mTargetContact;

        private IList<SubscriberInfo> mSubscribersToBeChanged;
        private IList<SubscriberNewsletterInfo> mAllSubscriptions;

        private List<SubscriberInfo> mSourceSubscribers;
        private List<SubscriberInfo> mTargetSubscribers;
       

        public ContactMergeSubscriberUpdater(ContactInfo sourceContact, ContactInfo targetContact)
        {
            mSourceContact = sourceContact;
            mTargetContact = targetContact;
        }


        private IEnumerable<SubscriberInfo> SubscribersToBeChanged 
            => mSubscribersToBeChanged ?? (mSubscribersToBeChanged = GetSubscribersToBeChanged());


        private IEnumerable<SubscriberNewsletterInfo> AllSubscriptions
            => mAllSubscriptions ?? (mAllSubscriptions = GetAllSubscriptions());


        private IEnumerable<SubscriberNewsletterInfo> SourceSubscriptions 
            => AllSubscriptions.Where(subscription => SourceSubscribers
                    .Select(s => s.SubscriberID)
                    .Contains(subscription.SubscriberID))
                .ToList();


        private IEnumerable<SubscriberNewsletterInfo> TargetSubscriptions
            =>  AllSubscriptions.Where(subscription => TargetSubscribers
                    .Select(s => s.SubscriberID)
                    .Contains(subscription.SubscriberID))
                .ToList();


        private List<SubscriberInfo> SourceSubscribers 
            => mSourceSubscribers ?? (mSourceSubscribers = SubscribersToBeChanged
                .Where(s => s.SubscriberRelatedID == mSourceContact.ContactID)
                .ToList());


        private IEnumerable<SubscriberInfo> TargetSubscribers 
            => mTargetSubscribers ?? (mTargetSubscribers = SubscribersToBeChanged
                .Where(s => s.SubscriberRelatedID == mTargetContact.ContactID)
                .ToList());


        public void Update()
        {
            if (!SourceSubscribers.Any())
            {
                return;
            }

            if (!TargetSubscribers.Any())
            {
                MoveSourceSubscribers(SourceSubscribers);
            }
            else
            {
                var sourceSubscribersWithMatchingTargetSubscriber = SourceSubscribers
                    .Where(HasMatchingSubscriberOnTargetContact)
                    .ToList();

                foreach (var sourceSubscriber in sourceSubscribersWithMatchingTargetSubscriber)
                {
                    var matchingTargetSubscriber = GetMatchingTargetSubscriber(sourceSubscriber);

                    MoveSubscriptions(sourceSubscriber, matchingTargetSubscriber);
                    UpdateSubscriberFieldsIfNotEmpty(matchingTargetSubscriber);
                }

                DeleteSubscribers(sourceSubscribersWithMatchingTargetSubscriber);

                var sourceSubscribersWithoutMatchingTargetSubscriber = SourceSubscribers
                    .Where(s => !HasMatchingSubscriberOnTargetContact(s))
                    .ToList();

                MoveSourceSubscribers(sourceSubscribersWithoutMatchingTargetSubscriber);
            }
        }


        private static void DeleteSubscribers(IEnumerable<SubscriberInfo> subscribers)
        {
            var sourceSubscriberIds = subscribers.Select(s => s.SubscriberID).ToList();
            var deleteCondition = new WhereCondition().WhereIn("SubscriberID", sourceSubscriberIds);
            SubscriberInfoProvider.BulkDeleteInternal(deleteCondition);
        }


        private bool HasMatchingSubscriberOnTargetContact(SubscriberInfo sourceSubscriber)
        {
            return TargetSubscribers.Any(s => s.SubscriberSiteID == sourceSubscriber.SubscriberSiteID);
        }


        private SubscriberInfo GetMatchingTargetSubscriber(SubscriberInfo sourceSubscriber)
        {
            return TargetSubscribers.FirstOrDefault(s => s.SubscriberSiteID == sourceSubscriber.SubscriberSiteID);
        }


        private void MoveSourceSubscribers(IEnumerable<SubscriberInfo> subscribers)
        {
            var updateDictionary = new Dictionary<string, object>
            {
                { "SubscriberRelatedID", mTargetContact.ContactID }
            };

            var subscriberIds = subscribers.Select(s => s.SubscriberID).ToList();
            var updateCondition = new WhereCondition().WhereIn("SubscriberID", subscriberIds);
            SubscriberInfoProvider.UpdateDataInternal(updateCondition, updateDictionary);
        }


        private void MoveSubscriptions(SubscriberInfo source, SubscriberInfo target)
        {
            var subscriptionsToBeMoved = SourceSubscriptions.Where(s => s.SubscriberID == source.SubscriberID);

            var updateDictionary = new Dictionary<string, object>
            {
                { "SubscriberID", target.SubscriberID }
            };

            var subscriptionsToUpdate = subscriptionsToBeMoved
                .Where(s => !TargetSubscriberHasTheSameSubscription(s))
                .Select(s => s.SubscriberNewsletterID)
                .ToList();

            if (!subscriptionsToUpdate.Any())
            {
                return;
            }

            var updateCondition = new WhereCondition().WhereIn("SubscriberNewsletterID", subscriptionsToUpdate);
            SubscriberNewsletterInfoProvider.UpdateDataInternal(updateCondition, updateDictionary);
        }


        private bool TargetSubscriberHasTheSameSubscription(SubscriberNewsletterInfo subscription)
        {
            return TargetSubscriptions.Any(s => s.NewsletterID == subscription.NewsletterID);
        }


        private void UpdateSubscriberFieldsIfNotEmpty(SubscriberInfo target)
        {
            foreach (var field in mFieldsToBeUpdated)
            {
                UpdateSubscriberFieldIfSourceIsNotEmpty(target, field.Key, field.Value);
            }

            SubscriberInfoProvider.SetSubscriberInfo(target);
        }


        private void UpdateSubscriberFieldIfSourceIsNotEmpty(SubscriberInfo target, string contactFieldName, string subscriberFieldName)
        {
            var fieldValue = mSourceContact.GetStringValue(contactFieldName, null);
            if (!string.IsNullOrEmpty(fieldValue))
            {
                target.SetValue(subscriberFieldName, fieldValue);
            }
        }


        private List<SubscriberInfo> GetSubscribersToBeChanged()
        {
            return SubscriberInfoProvider.GetSubscribers()
                                         .WhereIn("SubscriberRelatedID", new[]
                                         {
                                             mSourceContact.ContactID,
                                             mTargetContact.ContactID
                                         })
                                         .WhereEquals("SubscriberType", ContactInfo.OBJECT_TYPE)
                                         .ToList();
        }


        private List<SubscriberNewsletterInfo> GetAllSubscriptions()
        {
            var sourceSubscriberIds = SourceSubscribers.Select(s => s.SubscriberID);
            var targetSubscriberIds = TargetSubscribers.Select(s => s.SubscriberID);

            var allSubscribersIds = sourceSubscriberIds.Concat(targetSubscriberIds).ToList();

            return SubscriberNewsletterInfoProvider.GetSubscriberNewsletters()
                                                   .WhereIn("SubscriberID", allSubscribersIds)
                                                   .ToList();
        }
    }
}