using CMS;
using CMS.Base;
using CMS.ContactManagement;

[assembly: RegisterImplementation(typeof(ICurrentContactMergeService), typeof(CurrentContactMergeService), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.ContactManagement
{
    /// <summary>
    /// Provides method for merging of a current <see cref="ContactInfo"/> into another one.
    /// </summary>
    public interface ICurrentContactMergeService
    {
        /// <summary>
        /// Merges current contact with the given <paramref name="targetContact"/> in case the current contact is anonymous (does not have any email). Moves activities, 
        /// memberships, relations and copies contact's data.
        /// </summary>
        /// <remarks>
        /// Sets the result of merge to the response cookie, so the result will be persistent within the next requests.
        /// </remarks>
        /// <param name="targetContact">Contact the current contact should be merged with</param>
        void MergeCurrentContactWithContact(ContactInfo targetContact);


        /// <summary>
        /// Updates email of current contact.
        /// </summary>
        /// <param name="email">Contacts new email</param>
        /// <param name="currentUser">Current user</param>
        void UpdateCurrentContactEmail(string email, IUserInfo currentUser);
    }
}