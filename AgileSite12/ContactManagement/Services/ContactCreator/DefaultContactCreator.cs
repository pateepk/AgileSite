using System;
using System.Linq;
using System.Text;

using CMS.Core;
using CMS.Core.Internal;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Provides method for creating new contacts.
    /// </summary>
    internal class DefaultContactCreator : IContactCreator
    {
        private readonly IDateTimeNowService mDateTimeNowService;


        /// <summary>
        /// Instantiates new instance of <see cref="DefaultContactCreator"/>.
        /// </summary>
        /// <param name="dateTimeNowService">Service for obtaining current date time</param>
        /// <param name="eventLogService">Event log service interface</param>
        /// <exception cref="ArgumentNullException"><paramref name="dateTimeNowService" /> is null -or- <paramref name="dateTimeNowService"/> is null</exception>
        public DefaultContactCreator(IDateTimeNowService dateTimeNowService, IEventLogService eventLogService)
        {
            if (dateTimeNowService == null)
            {
                throw new ArgumentNullException("dateTimeNowService");
            }
            if (eventLogService == null)
            {
                throw new ArgumentNullException("eventLogService");
            }

            mDateTimeNowService = dateTimeNowService;
        }


        /// <summary>
        /// Creates new anonymous contact. Created instance of <see cref="ContactInfo"/> is saved to the database.
        /// </summary>
        /// <returns>Created anonymous contact.</returns>
        public ContactInfo CreateAnonymousContact()
        {
            return CreateContact(ContactHelper.ANONYMOUS);
        }


        /// <summary>
        /// Creates new instance of <see cref="ContactInfo"/> with given <paramref name="namePrefix"/> in <see cref="ContactInfo.ContactLastName"/>.
        /// Created instance of <see cref="ContactInfo"/> is saved to the database.
        /// </summary>
        /// <remarks>
        /// Current date time will be used as default value of <see cref="ContactInfo.ContactLastName"/>.
        /// </remarks>
        /// <param name="namePrefix">Prefix that will be prepended to the created <see cref="ContactInfo.ContactLastName"/>. If null is passed, no prefix will be used</param>
        public ContactInfo CreateContact(string namePrefix)
        {
            var currentContact = new ContactInfo
            {
                ContactLastName = namePrefix + mDateTimeNowService.GetDateTimeNow().ToString(ContactHelper.ANONYMOUS_CONTACT_LASTNAME_DATE_PATTERN),
                ContactMonitored = true
            };

            ContactInfoProvider.SetContactInfo(currentContact);
            return currentContact;
        }
    }
}
