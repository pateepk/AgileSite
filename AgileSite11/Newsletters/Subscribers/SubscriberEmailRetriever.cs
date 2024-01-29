using System;

using CMS;
using CMS.Base;
using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.Newsletters;

[assembly: RegisterImplementation(typeof(ISubscriberEmailRetriever), typeof(SubscriberEmailRetriever), Priority = CMS.Core.RegistrationPriority.SystemDefault)]

namespace CMS.Newsletters
{
    /// <summary>
    /// Class for retrieving subscriber email address.
    /// </summary>
    internal class SubscriberEmailRetriever : ISubscriberEmailRetriever
    {
        /// <summary>
        /// Returns email address for given contact subscriber.
        /// Returns null for contact group subscriber.
        /// </summary>
        /// <param name="subscriberID">Subscriber ID</param>
        public string GetSubscriberEmail(int subscriberID)
        {
            var subscriber = SubscriberInfoProvider.GetSubscriberInfo(subscriberID);
            if (subscriber == null)
            {
                throw new ArgumentException("[SubscriberEmailRetriever.GetSubscriberEmail]: Subscriber does not exist");
            }

            return IsContactSubscriber(subscriber) ? GetEmailForContact(subscriber.SubscriberRelatedID) : null;
        }


        /// <summary>
        /// Get email for contact.
        /// </summary>
        /// <param name="contactID">ID of contact that email is retrieved for</param>
        /// <returns>Null when contact was not found</returns>
        public string GetEmailForContact(int contactID)
        {
            ContactInfo contact = ContactInfoProvider.GetContactInfo(contactID);

            return contact != null ? contact.ContactEmail : null;
        }


        /// <summary>
        /// Returns true if subscriber is contact.
        /// </summary>
        /// <param name="subscriber">Subscriber</param>
        private bool IsContactSubscriber(SubscriberInfo subscriber)
        {
            return subscriber.SubscriberType.EqualsCSafe(PredefinedObjectType.CONTACT, true);
        }
    }
}
