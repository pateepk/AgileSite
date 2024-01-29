using System;

using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.Newsletters;
using CMS.SiteProvider;

namespace CMS.DancingGoat.Samples
{
    /// <summary>
    /// Contains methods for generating sample data for the Contact groups module.
    /// </summary>
    internal class ContactGroupSubscribersDataGenerator
    {
        private readonly SiteInfo mSite;

        private const string CONTACT_GROUP_ALL_CONTACTS_WITH_EMAIL = "AllContactsWithEmail";
        private const string CONTACT_GROUP_ALL_CHICAGO_CONTACTS_WITH_EMAIL = "AllChicagoContactsWithEmail";

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="site">Site the newsletters data will be generated for</param>
        public ContactGroupSubscribersDataGenerator(SiteInfo site)
        {
            mSite = site;
        }



        /// <summary>
        /// Performs contact group sample data generation.
        /// </summary>
        public void Generate()
        {
            AddContactGroupSubscribers();
        }


        private void AddContactGroupSubscribers()
        {
            AddContactGroupSubscriber(CONTACT_GROUP_ALL_CONTACTS_WITH_EMAIL, NewslettersDataGenerator.NEWSLETTER_COLOMBIA_COFFEE_PROMOTION, mSite.SiteName);
            AddContactGroupSubscriber(CONTACT_GROUP_ALL_CHICAGO_CONTACTS_WITH_EMAIL, NewslettersDataGenerator.NEWSLETTER_COLOMBIA_COFFEE_PROMOTION_SAMPLE, mSite.SiteName);
        }


        private void AddContactGroupSubscriber(string contactGroupName, string newsletterName, string siteName)
        {
            var contactGroup = ContactGroupInfoProvider.GetContactGroupInfo(contactGroupName);
            if (contactGroup == null)
            {
                return;
            }
            
            var site = SiteInfoProvider.GetSiteInfo(siteName);
            var newsletter = NewsletterInfoProvider.GetNewsletterInfo(newsletterName, site.SiteID);
            if (newsletter == null)
            {
                return;
            }

            var subscriber = SubscriberInfoProvider.GetSubscriberInfo(PredefinedObjectType.CONTACTGROUP,
                contactGroup.ContactGroupID, site.SiteID);

            if (subscriber != null)
            {
                return;
            }

            subscriber = new SubscriberInfo
            {
                SubscriberType = PredefinedObjectType.CONTACTGROUP,
                SubscriberRelatedID = contactGroup.ContactGroupID,
                SubscriberSiteID = site.SiteID,
                SubscriberFirstName = contactGroup.ContactGroupDisplayName,
                SubscriberFullName = string.Format("Contact group '{0}'", contactGroup.ContactGroupDisplayName)
            };

            SubscriberInfoProvider.SetSubscriberInfo(subscriber);

            SubscriberNewsletterInfoProvider.AddSubscriberToNewsletter(subscriber.SubscriberID, newsletter.NewsletterID, DateTime.Now);
            SubscriberNewsletterInfoProvider.AddSubscriberToNewsletter(subscriber.SubscriberID, newsletter.NewsletterID, DateTime.Now);
        }
    }
}
