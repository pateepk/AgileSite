using CMS;
using CMS.Base;
using CMS.ContactManagement;
using CMS.Core;
using CMS.DataEngine;
using CMS.Newsletters;

[assembly: RegisterImplementation(typeof(IBounceDetection), typeof(BounceDetection), Priority = RegistrationPriority.SystemDefault)]

namespace CMS.Newsletters
{
    /// <summary>
    /// Detects bounced subscriber.
    /// </summary>
    internal class BounceDetection : IBounceDetection
    {
        /// <summary>
        /// Returns true if given <paramref name="subscriber"/> is bounced.
        /// </summary>
        /// <param name="subscriber">Subscriber to check.</param>
        public bool IsBounced(SubscriberInfo subscriber)
        {
            var siteName = new SiteInfoIdentifier(subscriber.SubscriberSiteID).ObjectCodeName;

            var bounceLimit = Service.Resolve<ISettingsService>()[siteName + ".CMSBouncedEmailsLimit"].ToInteger(0);

            if (bounceLimit == 0)
            {
                return false;
            }

            var bounced = subscriber.SubscriberBounces != 0 && subscriber.SubscriberBounces > bounceLimit;
            if (bounced)
            {
                return true;
            }

            var contact = ContactInfoProvider.GetContactInfo(subscriber.SubscriberRelatedID);
            if (contact != null)
            {
                return contact.ContactBounces != 0 && contact.ContactBounces > bounceLimit;
            }

            return false;
        }
    }
}
