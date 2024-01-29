using System;

using CMS.ContactManagement.Internal;
using CMS.Helpers;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Provides methods for get and set contact from/to the cookie.
    /// </summary>
    internal class DefaultContactPersistentStorage : IContactPersistentStorage
    {
        private readonly ICookieService mCookieService;
        private readonly IContactProcessingChecker mContactProcessingChecker;


        /// <summary>
        /// Instantiates new instance of <see cref="DefaultContactPersistentStorage"/>.
        /// </summary>
        /// <param name="cookieService">Provides method for getting or setting the cookie from/to the request/response.</param>
        /// <param name="contactProcessingChecker">Provides method for checking whether the contact processing can continue.</param>
        /// <exception cref="ArgumentNullException"><paramref name="cookieService"/> is <c>null</c></exception>
        public DefaultContactPersistentStorage(ICookieService cookieService, IContactProcessingChecker contactProcessingChecker)
        {
            if (cookieService == null)
            {
                throw new ArgumentNullException(nameof(cookieService));
            }

            if (contactProcessingChecker == null)
            {
                throw new ArgumentNullException(nameof(contactProcessingChecker));
            }

            mCookieService = cookieService;
            mContactProcessingChecker = contactProcessingChecker;
        }


        /// <summary>
        /// Gets contact from the cookie.
        /// </summary>
        /// <returns>Contact retrieved from the persistent storage</returns>
        public ContactInfo GetPersistentContact()
        {
            if (!mContactProcessingChecker.CanProcessContactInCurrentContext())
            {
                return null;
            }

            var guid = ValidationHelper.GetGuid(mCookieService.GetValue(CookieName.CurrentContact), Guid.Empty);

            var contact = ContactInfoProvider.GetContactInfo(guid);

            return contact ?? VisitorToContact(guid);
        }


        private ContactInfo VisitorToContact(Guid visitor)
        {
            return VisitorToContactInfoProvider.GetContactForVisitor(visitor);
        }


        /// <summary>
        /// Sets given contact to the cookie.
        /// </summary>
        /// <param name="contact">Contact to be set</param>
        public void SetPersistentContact(ContactInfo contact)
        {
            if (!mContactProcessingChecker.CanProcessContactInCurrentContext())
            {
                return;
            }

            // Don't set cookie again if cookie has already the desired value
            if (mCookieService.GetValue(CookieName.CurrentContact) != contact.ContactGUID.ToString())
            {
                mCookieService.SetValue(CookieName.CurrentContact, contact.ContactGUID.ToString(), TimeSpan.FromDays(50 * 365));
            }
        }
    }
}