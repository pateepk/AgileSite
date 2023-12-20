using System;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;

namespace CMS.ContactManagement
{
    /// <summary>
	/// Provides methods for getting <see cref="ContactInfo"/> for <see cref="IUserInfo"/>.
    /// </summary>
    internal class DefaultCurrentUserContactProvider : ICurrentUserContactProvider
    {
        private readonly IContactMergeService mMergeService;
        private readonly IContactRelationAssigner mContactRelationAssigner;


        public DefaultCurrentUserContactProvider(IContactMergeService mergeService, IContactRelationAssigner contactRelationAssigner)
        {
            mMergeService = mergeService;
            mContactRelationAssigner = contactRelationAssigner;
        }


        /// <summary>
        /// Gets a <see cref="ContactInfo"/> for the given <paramref name="currentUser"/> when no information about the possible current contact is available.
        /// </summary>
        /// <param name="currentUser">The user the contacts will be obtained for</param>
        /// <exception cref="ArgumentNullException"><paramref name="currentUser"/>Thrown when <paramref name="currentUser"/> is <c>null</c>.</exception>
        /// <returns>
        /// Contact selected as the current one for the <paramref name="currentUser"/>. If the user is public, returns null.
        /// </returns>
        public ContactInfo GetContactForCurrentUser(IUserInfo currentUser)
        {
            if (currentUser == null)
            {
                throw new ArgumentNullException("currentUser");
            }

            if (currentUser.IsPublic())
            {
                return null;
            }

            return GetRelatedContact(currentUser);
        }


        /// <summary>
        /// Gets a <see cref="ContactInfo"/> for the given <paramref name="currentUser"/> when there is a priori information about the possible current contact available.
        /// </summary>
        /// <param name="currentUser">The user the contact will be obtained for</param>
        /// <param name="currentContact">A possible candidate to be selected as the current contact</param>
        /// <exception cref="ArgumentNullException"><paramref name="currentUser"/> or <paramref name="currentContact"/></exception>
        /// <returns>
        /// The contact selected as the current one for the <paramref name="currentUser"/>. Note that all other contacts related to the <paramref name="currentUser"/> are merged into this one.
        /// If the user is public, returns null.
        /// </returns>
        public ContactInfo GetContactForCurrentUserAndContact(IUserInfo currentUser, ContactInfo currentContact)
        {
            if (currentUser == null)
            {
                throw new ArgumentNullException("currentUser");
            }
            if (currentContact == null)
            {
                throw new ArgumentNullException("currentContact");
            }

            if (currentUser.IsPublic())
            {
                return null;
            }

            var relatedContact = GetRelatedContact(currentUser);
            if (relatedContact != null)
            {
                if (!ContactHasUserAssigned(currentContact))
                {
                    mMergeService.MergeContacts(relatedContact, currentContact);
                    return currentContact;
                }

                return relatedContact;
            }

            mContactRelationAssigner.Assign(currentUser, currentContact, new UserContactDataPropagationChecker());
            return currentContact;
        }


        /// <summary>
        /// Gets related contact to the given <paramref name="user"/>.
        /// </summary>
        /// <param name="user">The user the related contacts are searched for</param>
        private ContactInfo GetRelatedContact(IUserInfo user)
        {
            var id = GetContactIdForUser(user);
            return ContactInfoProvider.GetContactInfo(id);
        }


        /// <summary>
        /// Determines whether the given <paramref name="contact"/> has at least one user assigned.
        /// </summary>
        /// <param name="contact">The contact the user assignment will be searched for</param>
        /// <returns>True, if the contact has at least one user assigned; otherwise, false.</returns>
        private bool ContactHasUserAssigned(ContactInfo contact)
        {
            return new IDQuery(ContactMembershipInfo.OBJECT_TYPE_USER, "ContactID").WhereEquals("ContactID", contact.ContactID).Any();
        }


        private int GetContactIdForUser(IUserInfo user)
        {
            return new IDQuery(ContactMembershipInfo.OBJECT_TYPE_USER, "ContactID").WhereEquals("RelatedID", user.UserID)
                                                                                   .GetScalarResult(0);
        }
    }
}
