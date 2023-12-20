using System;

using CMS.Base;
using CMS.ContactManagement.Internal;
using CMS.Core;
using CMS.Membership;
using CMS.SiteProvider;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Contact context method and variables.
    /// </summary>
    public class ContactManagementContext : AbstractContext<ContactManagementContext>
    {
        #region "Context data"
        
        private ContactInfo mCurrentContact;
        private SiteInfo mCurrentSite;
        
        internal SiteInfo CurrentSite
        {
            get
            {
                return mCurrentSite ?? (mCurrentSite = SiteContext.CurrentSite);
            }
            set
            {
                mCurrentSite = value;
            }
        }

        #endregion


        #region "Services"

        private ICurrentContactProvider mCurrentContactProvider;
        private IContactValidator mContactValidator;
        private IContactRelationAssigner mContactRelationAssigner;
        private IContactProcessingChecker mContactProcessingChecker;


        internal ICurrentContactProvider CurrentContactProvider
        {
            get
            {
                return mCurrentContactProvider ?? (mCurrentContactProvider = Service.Resolve<ICurrentContactProvider>());
            }
            set
            {
                mCurrentContactProvider = value;
            }
        }


        internal IContactValidator ContactValidator
        {
            get
            {
                return mContactValidator ?? (mContactValidator = Service.Resolve<IContactValidator>());
            }
            set
            {
                mContactValidator = value;
            }
        }
        

        internal IContactRelationAssigner ContactRelationAssigner
        {
            get
            {
                return mContactRelationAssigner ?? (mContactRelationAssigner = Service.Resolve<IContactRelationAssigner>());
            }
            set
            {
                mContactRelationAssigner = value;
            }
        }


        internal IContactProcessingChecker ContactProcessingChecker
        {
            get
            {
                return mContactProcessingChecker ?? (mContactProcessingChecker = Service.Resolve<IContactProcessingChecker>());
            }
            set
            {
                mContactProcessingChecker = value;
            }
        }


        #endregion


        #region "Public static properties"

        /// <summary>
        /// Current contact info.
        /// </summary>
        [RegisterProperty]
        public static ContactInfo CurrentContact => GetCurrentContact();


        /// <summary>
        /// Current contact ID.
        /// </summary>
        [RegisterColumn]
        public static int CurrentContactID
        {
            get
            {
                ContactInfo ci = CurrentContact;
                return ci?.ContactID ?? 0;
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Returns current contact. Tries to find contact in persistent storage, obtains it from the users or tries to recognize
        /// it according to known information. If no contact can be found and <paramref name="createAnonymous"/> is true (default), it creates a new anonymous contact. If Online marketing is 
        /// not enabled or there is no license for ContactManagement found, null is returned. Also, null is returned if request
        /// is made by crawler (googlebot, etc.). Currently authenticated user is used to find contacts.
        /// </summary>
        /// <param name="createAnonymous">Indicates whether anonymous contact should be created if no contact information can be found.</param>
        /// <returns>Current contact</returns>
        public static ContactInfo GetCurrentContact(bool createAnonymous = true)
        {
            return Current.GetCurrentContactInternal(MembershipContext.AuthenticatedUser, false, createAnonymous);
        }


        /// <summary>
        /// Returns current contact. Tries to find contact in persistent storage, obtains it from the users or tries to recognize
        /// it according to known information. If no contact can be found, it creates a new anonymous contact. If Online marketing is 
        /// not enabled or there is no license for ContactManagement found, null is returned. Also, null is returned if request
        /// is made by crawler (googlebot, etc.).
        /// </summary>
        /// <param name="currentUser">Contact assigned to this user will be returned, if contact is not already known</param>
        /// <param name="forceUserMatching">If true, contact contact will be taken from user in <paramref name="currentUser"/> parameter even if it is already known from cookie for example.</param>
        /// <returns>Current contact</returns>
        [Obsolete("Use method ContactManagementContext.UpdateUserLoginContact(string) instead.")]
        public static ContactInfo GetCurrentContact(CurrentUserInfo currentUser, bool forceUserMatching)
        {
            return Current.GetCurrentContactInternal(currentUser, forceUserMatching);
        }


        /// <summary>
        /// Returns the contact assigned to the specified user. The contact is retrieved from persistent storage and is compared with all the contacts related to the given user.
        /// Applies contact merging to resolve the conflicts when the selected contact is already in relationship with another user.
        /// </summary>
        /// <remarks>
        /// Use only for login and registration purposes.
        /// The method has no effect and returns null if the ContactManagement feature is disabled or unavailable in the license.
        /// </remarks>      
        /// <param name="userName">Username of user the contact must be assigned to.</param>
        public static ContactInfo UpdateUserLoginContact(string userName)
        {
            var user = UserInfoProvider.GetUserInfo(userName);

            if (user == null)
            {
                return null;
            }

            return Current.GetCurrentContactInternal(user, true);
        }

        #endregion


        #region "Internal methods"

        internal ContactInfo GetCurrentContactInternal(IUserInfo currentUser, bool forceUserMatching, bool createAnonymous = true)
        {
            // Online marketing must be enabled, including license check and request must not be done by a crawler
            if (!ContactProcessingChecker.CanProcessContactInCurrentContext())
            {
                return null;
            }

            ContactInfo currentContact = Current.mCurrentContact;

            int originalContactID = currentContact?.ContactID ?? 0;
            bool isOriginalContactValid = currentContact != null && currentContact.Generalized.IsObjectValid;

            // Check if current contact is still valid, return parent otherwise
            currentContact = ContactValidator.ValidateContact(currentContact);

            if ((currentContact == null) || forceUserMatching)
            {
                currentContact = createAnonymous 
                    ? CurrentContactProvider.GetCurrentContact(currentUser, forceUserMatching)
                    : CurrentContactProvider.GetExistingContact(currentUser, forceUserMatching);
            }

            // Set contact only if it differs from the original value
            if ((currentContact != null) && ((originalContactID != currentContact.ContactID) || !isOriginalContactValid))
            {
                SetCurrentContactInternal(currentContact);
            }

            return currentContact;
        }


        /// <summary>
        /// Stores contact in request items, session or cookies.
        /// </summary>
        /// <param name="contact">Contact to be set</param>
        internal static void SetCurrentContact(ContactInfo contact)
        {
            Current.SetCurrentContactInternal(contact);
        }


        /// <summary>
        /// Stores contact in request items, session or cookies.
        /// </summary>
        /// <param name="contact">Contact to be set</param>
        internal void SetCurrentContactInternal(ContactInfo contact)
        {
            if (contact == null)
            {
                throw new ArgumentNullException(nameof(contact));
            }
            
            // Online marketing must be enabled, including license check and request must not be done by a crawler
            if (!ContactProcessingChecker.CanProcessContactInCurrentContext())
            {
                return;
            }

            // Store current contact in request items
            Current.mCurrentContact = contact;

            // Store contact in cookie or in session
            CurrentContactProvider.SetCurrentContact(contact);
        }

        #endregion
    }
}