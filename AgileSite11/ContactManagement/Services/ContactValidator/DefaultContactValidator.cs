using System;

using CMS.Base;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Provides method for validating contact against the database.
    /// </summary>
    internal class DefaultContactValidator : IContactValidator
    {
        /// <summary>
        /// Checks whether contact passed in <paramref name="contact"/> is valid and can be used as a current contact.
        /// Updates contact from hashtables/DB. If contact is merged, returns parent contact if available.
        /// </summary>
        /// <param name="contact">Contact to be validated</param>
        /// <returns>Returns updated and parent contact or null if passed contact is not valid</returns>
        public ContactInfo ValidateContact(ContactInfo contact)
        {
            // Check if contact exists
            if (contact == null)
            {
                return null;
            }

            // Get up-to-date representation of contact from hashtables or DB
            contact = ContactInfoProvider.GetContactInfo(contact.ContactID);

            return contact;
        }
    }
}
