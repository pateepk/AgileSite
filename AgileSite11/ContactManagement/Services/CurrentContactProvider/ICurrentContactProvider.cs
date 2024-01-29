using System;

using CMS;
using CMS.ContactManagement;
using CMS.Base;

[assembly: RegisterImplementation(typeof(ICurrentContactProvider), typeof(DefaultCurrentContactProvider), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.ContactManagement
{
    /// <summary>
    /// Service for recognizing and storing information about a contact currently browsing the live site. 
    /// </summary>
    public interface ICurrentContactProvider
    {
        /// <summary>
        /// Recognizes a contact currently browsing the live site. If the contact cannot be recognized a new one is created. A returned contact is always 
        /// valid (exists in database, is not merged, etc.) and is automatically assigned to <paramref name="currentUser"/> User (if it's not a public
        /// user).
        /// </summary>
        /// <param name="currentUser">Currently signed in user or public user</param>
        /// <param name="forceUserMatching">If true, the current contact is tried to be determined by the user even if it can be found by other ways</param>
        /// <exception cref="ArgumentNullException"> <paramref name="currentUser"/> is <c>null</c> </exception>
        /// <exception cref="InvalidOperationException">Processing of contacts cannot continue. This can be due to the insufficient license, disabled online marketing or incorrect request state.</exception>
        /// <returns>The recognized contact or a new contact, if it was not possible to recognize the contact.</returns>
        ContactInfo GetCurrentContact(IUserInfo currentUser, bool forceUserMatching);


        /// <summary>
        /// Gets a contact assigned to the visitor currently browsing the live site (contact can only be recognized during web request).
        /// A returned contact is always valid (exists in database, is not merged, etc.) and is automatically assigned to <paramref name="currentUser"/> (if it's not a public user).
        /// If no contact is currently assigned to the visitor <c>null</c> is returned.
        /// </summary>
        /// <param name="currentUser">Currently signed in user or public user</param>
        /// <param name="forceUserMatching">If true, the current contact is tried to be determined by the user even if it can be found by other ways</param>
        /// <exception cref="ArgumentNullException"> <paramref name="currentUser"/> is <c>null</c> </exception>
        /// <exception cref="InvalidOperationException">Processing of contacts cannot continue. This can be due to the insufficient license, disabled online marketing or incorrect request state.</exception>
        /// <returns>The recognized contact or <c>null</c> when there is no contact assigned to current live site visitor</returns>
        ContactInfo GetExistingContact(IUserInfo currentUser, bool forceUserMatching);


        /// <summary>
        /// Stores information about the current contact into the persistent storage (<see cref="IContactPersistentStorage"/>), so that the next time 
        /// (possibly in another request in the same session) <see cref="GetCurrentContact(IUserInfo, bool)"/> is called, the stored contact is returned.
        /// </summary>
        /// <param name="contact">The contact who performed the request</param>
        /// <exception cref="InvalidOperationException">Processing of contacts cannot continue. This can be due to the insufficient license, disabled online marketing or incorrect request state.</exception>
        void SetCurrentContact(ContactInfo contact);
    }
}
