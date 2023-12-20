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
        private readonly ICookieService cookieService;
        private readonly IContactProcessingChecker contactProcessingChecker;


        /// <summary>
        /// Instantiates new instance of <see cref="DefaultContactPersistentStorage"/>.
        /// </summary>
        /// <param name="cookieService">Provides method for getting or setting the cookie from/to the request/response</param>
        /// <param name="contactProcessingChecker">Provides method for checking whether the contact processing can continue.</param>
        /// <exception cref="ArgumentNullException"><paramref name="cookieService"/> or <paramref name="contactProcessingChecker"/> is <c>null</c></exception>
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

            this.cookieService = cookieService;
            this.contactProcessingChecker = contactProcessingChecker;
        }


        /// <summary>
        /// Gets contact from the cookie.
        /// </summary>
        /// <returns>Contact retrieved from the persistent storage</returns>
        public ContactInfo GetPersistentContact()
        {
            if (!contactProcessingChecker.CanProcessContactInCurrentContext())
            {
                return null;
            }

            var guid = ValidationHelper.GetGuid(cookieService.GetValue(CookieName.CurrentContact), Guid.Empty);

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
            if (!contactProcessingChecker.CanProcessContactInCurrentContext())
            {
                return;
            }

            // Don't set cookie again if cookie has already the desired value
            if (cookieService.GetValue(CookieName.CurrentContact) != contact.ContactGUID.ToString())
            {
                cookieService.SetValue(CookieName.CurrentContact, contact.ContactGUID.ToString(), TimeSpan.FromDays(50 * 365));
            }
        }
    }
}