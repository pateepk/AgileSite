using System;
using System.Linq;
using System.Text;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Newsletters;

[assembly: RegisterImplementation(typeof(ISubscriptionHashValidator), typeof(SubscriptionHashValidator), Priority = CMS.Core.RegistrationPriority.SystemDefault)]

namespace CMS.Newsletters
{
    /// <summary>
    /// Validates subscription hash.
    /// </summary>
    internal class SubscriptionHashValidator : ISubscriptionHashValidator
    {
        /// <summary>
        /// Validates the subscription and unsubscription hash.
        /// </summary>
        /// <param name="requestHash">Hash to validate</param>
        /// <param name="siteName">Site name</param>
        /// <param name="datetime">Date time</param>
        public HashValidationResult Validate(string requestHash, string siteName, DateTime datetime)
        {
            var subscription = SubscriberNewsletterInfoProvider.GetSubscriberNewsletterInfo(requestHash);

            if (subscription == null)
            {
                return HashValidationResult.NotFound;
            }

            // Get newsletter and subscriber
            NewsletterInfo newsletter = NewsletterInfoProvider.GetNewsletterInfo(subscription.NewsletterID);
            SubscriberInfo subscriber = SubscriberInfoProvider.GetSubscriberInfo(subscription.SubscriberID);

            if ((newsletter == null) || (subscriber == null))
            {
                return HashValidationResult.NotFound;
            }

            // Validate hash
            if (!SecurityHelper.ValidateConfirmationEmailHash(requestHash, newsletter.NewsletterGUID + "|" + subscriber.SubscriberGUID, datetime))
            {
                return HashValidationResult.Failed;
            }

            DateTime now = DateTime.Now;
            TimeSpan span = now.Subtract(datetime);

            // Get interval from settings
            double interval = SettingsKeyInfoProvider.GetDoubleValue(siteName + ".CMSOptInInterval");

            // Check interval
            if ((interval > 0) && (span.TotalHours > interval))
            {
                return HashValidationResult.TimeExceeded;
            }

            return HashValidationResult.Success;
        }
    }
}