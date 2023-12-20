using System;
using CMS.Base;
using CMS.ContactManagement.Internal;
using CMS.Membership;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Provides method for merging of a current <see cref="ContactInfo"/> into another one.
    /// </summary>
    internal class CurrentContactMergeService : ICurrentContactMergeService
    {
        private readonly IContactMergeService mContactMergeService;
        private readonly IContactProcessingChecker mContactProcessingChecker;
        private readonly ICurrentContactProvider mCurrentContactProvider;


        public CurrentContactMergeService(IContactMergeService contactMergeService, ICurrentContactProvider currentContactProvider, IContactProcessingChecker contactProcessingChecker)
        {
            mContactMergeService = contactMergeService;
            mContactProcessingChecker = contactProcessingChecker;

            mCurrentContactProvider = new CurrentContactProviderContextDecorator(currentContactProvider, contactProcessingChecker);
        }


        /// <summary>
        /// Merges current contact with the given <paramref name="targetContact"/> in case the current contact is anonymous (does not have any email). Moves activities, 
        /// memberships, relations and copies contact's data.
        /// </summary>
        /// <remarks>
        /// Sets the result of merge to the response cookie, so the result will be persistent within the next requests.
        /// </remarks>
        /// <param name="targetContact">Contact the current contact should be merged with</param>
        /// <exception cref="ArgumentNullException"><paramref name="targetContact"/> is <c>null</c></exception>
        public void MergeCurrentContactWithContact(ContactInfo targetContact)
        {
            if (targetContact == null)
            {
                throw new ArgumentNullException(nameof(targetContact));
            }
            if (!mContactProcessingChecker.CanProcessContactInCurrentContext())
            {
                return;
            }

            var currentContact = mCurrentContactProvider.GetCurrentContact(MembershipContext.AuthenticatedUser, false);

            if (currentContact != null &&
                currentContact.ContactGUID != targetContact.ContactGUID &&
                string.IsNullOrEmpty(currentContact.ContactEmail))
            {
                mContactMergeService.MergeContacts(currentContact, targetContact);
                mCurrentContactProvider.SetCurrentContact(targetContact);
            }
        }


        /// <summary>
        /// Updates email of current contact.
        /// </summary>
        /// <param name="email">Contacts new email</param>
        /// <param name="currentUser">Current user</param>
        public void UpdateCurrentContactEmail(string email, IUserInfo currentUser)
        {
            if (string.IsNullOrEmpty(email))
            {
                return;
            }
            if (!mContactProcessingChecker.CanProcessContactInCurrentContext())
            {
                return;
            }

            var contact = mCurrentContactProvider.GetCurrentContact(currentUser, false);
            if (contact != null)
            {
                contact.ContactEmail = email;
                ContactInfoProvider.SetContactInfo(contact);
            }
        }
    }
}