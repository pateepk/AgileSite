using System;
using System.Linq;

using CMS;
using CMS.ContactManagement;
using CMS.Helpers;
using CMS.Membership;
using CMS.Newsletters;

[assembly: RegisterImplementation(typeof(IContactProvider), typeof(ContactProvider), Priority = CMS.Core.RegistrationPriority.SystemDefault)]

namespace CMS.Newsletters
{
    /// <summary>
    /// Provides methods for retrieving and preparing <see cref="ContactInfo" /> objects so it can be subscribed to the newsletter.
    /// </summary>
    internal class ContactProvider : IContactProvider
    {
        private readonly IContactCreator mContactCreator;
        private readonly IContactMergeService mContactMergeService;

        
        public ContactProvider(IContactCreator contactCreator, IContactMergeService contactMergeService)
        {
            mContactCreator = contactCreator;
            mContactMergeService = contactMergeService;
        }


        /// <summary>
        /// Gets contact associated with <paramref name="email"/>.
        /// When contact doesn't exist, contact is created and inserted to database.
        /// </summary>
        /// <param name="email">Email used to get or update contact.</param>
        /// <param name="firstName">First name used to update contact.</param>
        /// <param name="lastName">Last name used to update contact.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="email"/> is <c>null</c></exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="email" /> is not valid email address.</exception>
        /// <returns>Retrieved contact.</returns>
        public ContactInfo GetContactForSubscribing(string email, string firstName = null, string lastName = null)
        {
            if (email == null)
            {
                throw new ArgumentNullException("email");
            }

            if (string.IsNullOrWhiteSpace(email) || !ValidationHelper.IsEmail(email))
            {
                throw new ArgumentException("Email is not valid.");
            }

            var contact = GetContactFromContextOrByEmail(email) ?? mContactCreator.CreateAnonymousContact();
            contact.ContactEmail = email;

            // Contact with the same email can be already present in the database and its data can be different from the one retrieved from the context.
            mContactMergeService.MergeContactByEmail(contact);

            if (!SubscriberExistsForContact(contact))
            {
                UpdateContactNames(contact, firstName, lastName);
            }

            ContactInfoProvider.SetContactInfo(contact);

            return contact;
        }


        /// <summary>
        /// Gets contact associated with <paramref name="user"/>.
        /// When contact doesn't exist, contact is created and inserted to database
        /// </summary>
        /// <param name="user">User used to get and update contact.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="user"/> is <c>null</c></exception>
        /// <exception cref="InvalidOperationException">Thrown when user's email address is not valid.</exception>
        /// <returns>Retrieved contact.</returns>
        public ContactInfo GetContactForSubscribing(UserInfo user)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (string.IsNullOrWhiteSpace(user.Email) || !ValidationHelper.IsEmail(user.Email))
            {
                throw new InvalidOperationException("User email is not valid.");
            }

            var contact = GetContactFromContextOrByEmail(user.Email) ?? mContactCreator.CreateAnonymousContact();
            ContactInfoProvider.UpdateContactFromExternalData(user, true, contact);

            // Contact with the same email can be already present in the database and its data can be different from the one retrieved from the context.
            mContactMergeService.MergeContactByEmail(contact);

            return contact;
        }


        /// <summary>
        /// Get contact from context. If it's not possible (wrong license, disabled OM or current contact contains different email etc.)
        /// try to get contact from the DB with the specified email.
        /// </summary>
        private ContactInfo GetContactFromContextOrByEmail(string email)
        {
            var contact = TryGetCurrentContact(email) ?? ContactInfoProvider.GetContactInfo(email);

            return contact;
        }


        /// <summary>
        /// Get contact from context if possible (EMS license is used, OM is enabled etc.) 
        /// but only when this contact doesn't contain email or contains the same email as specified in <paramref name="email"/> parameter.
        /// </summary>
        private ContactInfo TryGetCurrentContact(string email)
        {
            var contact = ContactManagementContext.CurrentContact;
            if (contact == null || (contact.ContactEmail != email.Trim() && !string.IsNullOrWhiteSpace(contact.ContactEmail)))
            {
                return null;
            }

            return contact;
        }


        private bool SubscriberExistsForContact(ContactInfo contact)
        {
            return SubscriberInfoProvider.GetSubscribers()
                                         .WhereEquals("SubscriberType", ContactInfo.OBJECT_TYPE)
                                         .WhereEquals("SubscriberRelatedID", contact.ContactID)
                                         .Any();
        }


        private void UpdateContactNames(ContactInfo contact, string firstName, string lastName)
        {
            if (!string.IsNullOrWhiteSpace(firstName))
            {
                contact.ContactFirstName = firstName;
            }
            if (!string.IsNullOrWhiteSpace(lastName))
            {
                contact.ContactLastName = lastName;
            }
        }
    }
}
