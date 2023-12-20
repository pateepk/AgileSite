using CMS;
using CMS.Base;
using CMS.ContactManagement;
using CMS.Membership;

[assembly: RegisterImplementation(typeof(ICurrentUserContactProvider), typeof(DefaultCurrentUserContactProvider), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.ContactManagement
{
    /// <summary>
    /// Provides methods for getting <see cref="ContactInfo"/> for <see cref="UserInfo"/>.
    /// </summary>
    public interface ICurrentUserContactProvider
    {
        /// <summary>
        /// Gets a <see cref="ContactInfo"/> for the given <paramref name="currentUser"/> when no information about the possible current contact is available.
        /// </summary>
        /// <param name="currentUser">The user the contacts will be obtained for</param>
        /// <returns>
        /// Contact selected as the current one for the <paramref name="currentUser"/>. 
        /// </returns>
        ContactInfo GetContactForCurrentUser(IUserInfo currentUser);


        /// <summary>
        /// Gets a <see cref="ContactInfo"/> for the given <paramref name="currentUser"/> when there is a priori information about the possible current contact available.
        /// </summary>
        /// <param name="currentUser">The user the contact will be obtained for</param>
        /// <param name="currentContact">A possible candidate to be selected as the current contact</param>
        /// <returns>
        /// The contact selected as the current one for the <paramref name="currentUser"/>. 
        /// </returns>
        ContactInfo GetContactForCurrentUserAndContact(IUserInfo currentUser, ContactInfo currentContact);
    }
}