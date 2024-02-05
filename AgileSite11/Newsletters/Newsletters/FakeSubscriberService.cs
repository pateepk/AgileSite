using System;

using CMS;
using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.Membership;
using CMS.Newsletters;

[assembly: RegisterImplementation(typeof(IFakeSubscriberService), typeof(FakeSubscriberService), Priority = CMS.Core.RegistrationPriority.SystemDefault)]

namespace CMS.Newsletters
{
    /// <summary>
    /// Provides access to the fake subscriber data.
    /// </summary>
    internal class FakeSubscriberService : IFakeSubscriberService
    {
        /// <summary>
        /// Returns fake subscriber.
        /// </summary>
        public SubscriberInfo GetFakeSubscriber()
        {
            const string EMAIL = "tony@starkindustries.local";
            const string FIRST_NAME = "Anthony";
            const string MIDDLE_NAME = "Edward";
            const string LAST_NAME = "Stark";

            return new SubscriberInfo
            {
                SubscriberID = 0,
                SubscriberEmail = EMAIL,
                SubscriberFirstName = FIRST_NAME,
                SubscriberLastName = LAST_NAME,
                SubscriberType = PredefinedObjectType.CONTACT,
                SubscriberRelated = new ContactInfo
                {
                    ContactEmail = EMAIL,
                    ContactFirstName = FIRST_NAME,
                    ContactMiddleName = MIDDLE_NAME,
                    ContactLastName = LAST_NAME,
                    ContactGender = (int)UserGenderEnum.Male,
                    ContactCompanyName = "Stark Industries",
                    ContactJobTitle = "CEO",
                    ContactCity = "Malibu",
                    ContactAddress1 = "10880 Malibu Point",
                    ContactBirthday = new DateTime(1970, 5, 29)
                }
            };
        }
    }
}
