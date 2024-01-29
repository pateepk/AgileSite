using System;

using CMS.Base;
using CMS.ContactManagement.Internal;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Service for recognizing and storing information about a contact currently browsing the live site. 
    /// This implementation uses all known methods to recognize the contact.
    /// </summary>
    internal class DefaultCurrentContactProvider : ICurrentContactProvider
    {
        private readonly IContactValidator mContactValidator;
        private readonly IContactPersistentStorage mContactPersistentStorage;
        private readonly ICurrentUserContactProvider mCurrentUserContactProvider;
        private readonly IContactCreator mContactCreator;
        private readonly IContactRelationAssigner mContactRelationAssigner;
        

        public DefaultCurrentContactProvider(IContactValidator contactValidator, IContactPersistentStorage contactPersistentStorage, ICurrentUserContactProvider currentUserContactProvider, IContactCreator contactCreator, IContactRelationAssigner contactRelationAssigner)
        {
            mContactValidator = contactValidator;
            mContactPersistentStorage = contactPersistentStorage;
            mCurrentUserContactProvider = currentUserContactProvider;
            mContactCreator = contactCreator;
            mContactRelationAssigner = contactRelationAssigner;
        }


        /// <summary>
        /// Recognizes a contact currently browsing the live site. A returned contact is always 
        /// valid (exists in database, is not merged, etc.) and is automatically assigned to <paramref name="currentUser"/> User (if it isn't a public user).
        /// </summary>
        /// <param name="currentUser">Currently signed in user or public user</param>
        /// <param name="forceUserMatching">If true, current contact is tried to be determined by the user even if it can be found by other ways</param>
        /// <exception cref="ArgumentNullException"> <paramref name="currentUser"/> is <c>null</c> </exception>
        /// <returns>Recognized contact</returns>
        public ContactInfo GetExistingContact(IUserInfo currentUser, bool forceUserMatching)
        {
            if (currentUser == null)
            {
                throw new ArgumentNullException(nameof(currentUser));
            }

            // Try to find current contact in a cookie
            ContactInfo currentContact = mContactPersistentStorage.GetPersistentContact();

            // Check if current contact is still valid
            currentContact = mContactValidator.ValidateContact(currentContact);

            // User matching might be forced for example when user was authenticated in the current request, so contact might have changed
            // and it is needed to recognize the new contact based on its user
            if (currentContact == null || forceUserMatching)
            {
                if (currentContact == null)
                {
                    currentContact = mCurrentUserContactProvider.GetContactForCurrentUser(currentUser);
                }
                else
                {
                    currentContact = mCurrentUserContactProvider.GetContactForCurrentUserAndContact(currentUser, currentContact);
                }
            
                // Check if current contact is still valid
                currentContact = mContactValidator.ValidateContact(currentContact);
            }

            return currentContact;
        }


        /// <summary>
        /// Recognizes a contact currently browsing the live site. If the contact cannot be recognized a new one is created. A returned contact is always 
        /// valid (exists in database, is not merged, etc.) and is automatically assigned to <paramref name="currentUser"/> User (if it isn't a public
        /// user).
        /// </summary>
        /// <param name="currentUser">Currently signed in user or public user</param>
        /// <param name="forceUserMatching">If true, current contact is tried to be determined by the user even if it can be found by other ways</param>
        /// <exception cref="ArgumentNullException"> <paramref name="currentUser"/> is <c>null</c></exception>
        /// <returns>Recognized contact or new contact, if it was not possible to recognize contact.</returns>
        public ContactInfo GetCurrentContact(IUserInfo currentUser, bool forceUserMatching)
        {
            if (currentUser == null)
            {
                throw new ArgumentNullException(nameof(currentUser));
            }

            var currentContact = GetExistingContact(currentUser, forceUserMatching);

            // If no contact was found nor recognized, create the anonymous one
            if (currentContact == null)
            {
                currentContact = mContactCreator.CreateAnonymousContact();

                // When creating new contact, it is possible it has to be assigned to some user
                AssignContactToUser(currentUser, currentContact);
                SetCurrentContact(currentContact);
            }

            return currentContact;
        }


        /// <summary>
        /// Stores information about the current contact into the persistent storage (<see cref="IContactPersistentStorage"/>), so that the next time 
        /// (possibly in another request in the same session) <see cref="ICurrentContactProvider.GetCurrentContact(IUserInfo, bool)"/> is called, the stored 
        /// contact is returned.
        /// </summary>
        /// <param name="contact">The contact who performed the request</param>
        public void SetCurrentContact(ContactInfo contact)
        {
            if (contact == null)
            {
                throw new ArgumentNullException(nameof(contact));
            }

            mContactPersistentStorage.SetPersistentContact(contact);
        }


        private void AssignContactToUser(IUserInfo user, ContactInfo contact)
        {
            if (user != null && contact != null && !user.IsPublic())
            {
                mContactRelationAssigner.Assign(user, contact, new UserContactDataPropagationChecker());
            }
        }
    }
}