using System;

using CMS.ContactManagement;
using CMS.DataEngine;

namespace CMS.Newsletters
{
    internal class NewsletterRecipientsProvider : IRecipientsProvider
    {
        private readonly NewsletterInfo mNewsletter;


        /// <summary>
        /// Creates an instance of the <see cref="NewsletterRecipientsProvider"/> class.
        /// </summary>
        /// <param name="newsletter">Newsletter recipients are assigned to.</param>
        /// <exception cref="ArgumentNullException">Is thrown when parameter <paramref name="newsletter"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Is thrown when parameter <paramref name="newsletter"/> is not type of <see cref="EmailCommunicationTypeEnum.Newsletter"/>.</exception>
        public NewsletterRecipientsProvider(NewsletterInfo newsletter)
        {
            if (newsletter == null)
            {
                throw new ArgumentNullException("newsletter");
            }

            if (newsletter.NewsletterType != EmailCommunicationTypeEnum.Newsletter)
            {
                throw new ArgumentException("Invalid type of newsletter. Allowed type is only 'newsletter'", "newsletter");
            }

            mNewsletter = newsletter;
        }


        /// <summary>
        /// Return all contacts which are subscribed to an newsletter, includes opted out contacts and bounced.
        /// </summary>
        public ObjectQuery<ContactInfo> GetAllRecipients()
        {
            var contacts = GetContactsWithEmail();
            AddSubscriptionWhere(contacts);
            return contacts;
        }


        /// <summary>
        /// Returns query for marketable recipients subscribed for a specified newsletter.
        /// </summary>
        public ObjectQuery<ContactInfo> GetMarketableRecipients()
        {
            var bounceLimit = NewsletterHelper.BouncedEmailsLimit(new SiteInfoIdentifier(mNewsletter.NewsletterSiteID));

            return GetAllRecipients().WithoutUnsubscribed(mNewsletter.NewsletterID).WithoutBounces(bounceLimit);
        }


        private static ObjectQuery<ContactInfo> GetContactsWithEmail()
        {
            return ContactInfoProvider.GetContacts().WhereNotEmpty("ContactEmail");
        }


        private void AddSubscriptionWhere(ObjectQuery<ContactInfo> contacts)
        {
            var contactSubscriberIds = GetContactSubscriberIds();
            var contactGroupMemberIds = GetContactGroupMemberIds();

            contacts.Where(where => where
                .WhereIn("ContactID", contactSubscriberIds)
                .Or()
                .WhereIn("ContactID", contactGroupMemberIds)
            );
        }


        private ObjectQuery<ContactGroupMemberInfo> GetContactGroupMemberIds()
        {
            var groupSubscriberIds = GetNewsletterSubscriberIds(PredefinedObjectType.CONTACTGROUP);

            var contactGroupsIds = ContactGroupInfoProvider.GetContactGroups()
                .Column("ContactGroupID")
                .WhereIn("ContactGroupID", groupSubscriberIds);

            return ContactGroupMemberInfoProvider.GetRelationships()
                .Column("ContactGroupMemberRelatedID")
                .WhereEquals("ContactGroupMemberType", (int)ContactGroupMemberTypeEnum.Contact)
                .WhereIn("ContactGroupMemberContactGroupID", contactGroupsIds);
        }


        private ObjectQuery<SubscriberInfo> GetContactSubscriberIds()
        {
            return GetNewsletterSubscriberIds(PredefinedObjectType.CONTACT);
        }


        private ObjectQuery<SubscriberInfo> GetNewsletterSubscriberIds(string subscriberType)
        {
            return SubscriberInfoProvider.GetSubscribers()
                                         .Column("SubscriberRelatedID")
                                         .WhereEquals("SubscriberType", subscriberType)
                                         .WhereIn(
                                             "SubscriberID",
                                             SubscriberNewsletterInfoProvider.GetApprovedSubscriberNewsletters()
                                                                             .Column("SubscriberID")
                                                                             .WhereEquals("NewsletterID", mNewsletter.NewsletterID));
        }
    }
}
