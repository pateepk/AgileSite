using System;

using CMS.Base;
using CMS.ContactManagement.Internal;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Decorates the implementation of <see cref="ICurrentContactProvider"/> given in constructor to perform <see cref="IContactProcessingChecker.CanProcessContactInCurrentContext"/> check before invoking every method.
    /// </summary>
    public class CurrentContactProviderContextDecorator : ICurrentContactProvider
    {
        private const string CONTACT_PROCESSING_CANNOT_CONTINUE_EXCEPTION_MESSAGE = "Processing of contacts cannot continue. This can be due to the insufficient license, disabled online marketing or incorrect request state.";

        private readonly ICurrentContactProvider mDefaultCurrentContactProvider;
        private readonly IContactProcessingChecker mContactProcessingChecker;

        /// <summary>
        /// Instantiates new instance of <see cref="CurrentContactProviderContextDecorator"/>.
        /// </summary>
        /// <param name="defaultCurrentContactProvider">Implementation of <see cref="ICurrentContactProvider"/> to be decorated</param>
        /// <param name="contactProcessingChecker">Provides method for checking whether the contact processing can continue</param>
        public CurrentContactProviderContextDecorator(ICurrentContactProvider defaultCurrentContactProvider, IContactProcessingChecker contactProcessingChecker)
        {
            mDefaultCurrentContactProvider = defaultCurrentContactProvider;
            mContactProcessingChecker = contactProcessingChecker;
        }


        /// <summary>
        /// Checks whether processing of contact can continue. If so, recognizes a contact currently browsing the live site. If the contact cannot be recognized a new one is created. A returned contact is always 
        /// valid (exists in database, is not merged, etc.) and is automatically assigned to <paramref name="currentUser"/> User (if it's not a public
        /// user). 
        /// </summary>
        /// <param name="currentUser">Currently signed in user or public user</param>
        /// <param name="forceUserMatching">If true, the current contact is tried to be determined by the user even if it can be found by other ways</param>
        /// <exception cref="ArgumentNullException"> <paramref name="currentUser"/> is <c>null</c> </exception>
        /// <exception cref="InvalidOperationException">Processing of contacts cannot continue. This can be due to the insufficient license, disabled online marketing or incorrect request state.</exception>
        /// <returns>The recognized contact or a new contact, if it was not possible to recognize the contact.</returns>
        public ContactInfo GetCurrentContact(IUserInfo currentUser, bool forceUserMatching)
        {
            CheckIfContactProcessingCanContinue();
            return mDefaultCurrentContactProvider.GetCurrentContact(currentUser, forceUserMatching);
        }


        /// <summary>
        /// Checks whether processing of contact can continue. If so, gets a contact assigned to the visitor currently browsing the live site (contact can only be recognized during web request).
        /// A returned contact is always valid (exists in database, is not merged, etc.) and is automatically assigned to <paramref name="currentUser"/> (if it's not a public user).
        /// If no contact is currently assigned to the visitor <c>null</c> is returned.
        /// </summary>
        /// <param name="currentUser">Currently signed in user or public user</param>
        /// <param name="forceUserMatching">If true, the current contact is tried to be determined by the user even if it can be found by other ways</param>
        /// <exception cref="ArgumentNullException"> <paramref name="currentUser"/> is <c>null</c> </exception>
        /// <exception cref="InvalidOperationException">Processing of contacts cannot continue. This can be due to the insufficient license, disabled online marketing or incorrect request state.</exception>
        /// <returns>The recognized contact or <c>null</c> when there is no contact assigned to current live site visitor</returns>
        public ContactInfo GetExistingContact(IUserInfo currentUser, bool forceUserMatching)
        {
            CheckIfContactProcessingCanContinue();
            return mDefaultCurrentContactProvider.GetExistingContact(currentUser, forceUserMatching);
        }


        /// <summary>
        /// Checks whether processing of contact can continue. If so, stores information about the current contact into the persistent storage (<see cref="IContactPersistentStorage"/>), so that the next time 
        /// (possibly in another request in the same session) <see cref="GetCurrentContact(IUserInfo, bool)"/> is called, the stored contact is returned.
        /// </summary>
        /// <param name="contact">The contact who performed the request</param>
        /// <exception cref="InvalidOperationException">Processing of contacts cannot continue. This can be due to the insufficient license, disabled online marketing or incorrect request state.</exception>
        public void SetCurrentContact(ContactInfo contact)
        {
            CheckIfContactProcessingCanContinue();
            mDefaultCurrentContactProvider.SetCurrentContact(contact);
        }


        private void CheckIfContactProcessingCanContinue()
        {
            if (!mContactProcessingChecker.CanProcessContactInCurrentContext())
            {
                throw new InvalidOperationException(CONTACT_PROCESSING_CANNOT_CONTINUE_EXCEPTION_MESSAGE);
            }
        }
    }
}