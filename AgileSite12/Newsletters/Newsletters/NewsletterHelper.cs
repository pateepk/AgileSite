using System;

using CMS.Core;
using CMS.Base;
using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.Modules;

namespace CMS.Newsletters
{
    /// <summary>
    /// Newsletter helper class.
    /// </summary>
    public static class NewsletterHelper
    {

        #region "License helper methods"

        /// <summary>
        /// License version check.
        /// </summary>
        /// <param name="domain">Domain name</param>
        /// <param name="feature">Feature to check</param>
        /// <param name="action">Action, if action is Insert limitations are not checked under administration interface</param>
        public static bool LicenseVersionCheck(string domain, FeatureEnum feature, ObjectActionEnum action)
        {
            return Service.Resolve<INewsletterLicenseCheckerService>().LicenseVersionCheck(domain, feature, action);
        }


        /// <summary>
        /// License version check.
        /// </summary>
        /// <param name="domain">Domain name</param>
        /// <param name="feature">Feature to check</param>
        /// <param name="action">Action</param>
        /// <param name="siteCheck">If true limitations are not applied under URLs in Site manager, CMS Desk, CMSModules and CMSPages/Logon</param>
        public static bool LicenseVersionCheck(string domain, FeatureEnum feature, ObjectActionEnum action, bool siteCheck)
        {
            return Service.Resolve<INewsletterLicenseCheckerService>().LicenseVersionCheck(domain, feature, action, siteCheck);
        }


        /// <summary>
        /// Checks the license.
        /// </summary>
        /// <param name="action">Object action</param>
        /// <param name="domainName">Domain name, if not set, current domain name is used</param>
        public static bool CheckLicense(ObjectActionEnum action = ObjectActionEnum.Edit, string domainName = null)
        {
            return Service.Resolve<INewsletterLicenseCheckerService>().CheckLicense(action, domainName);
        }


        /// <summary>
        /// Checks the license for insert for a new newsletter or for edit in other cases.
        /// </summary>
        /// <param name="newsletter">Newsletter</param>
        public static void CheckLicense(NewsletterInfo newsletter)
        {
            Service.Resolve<INewsletterLicenseCheckerService>().CheckLicense(newsletter);
        }


        /// <summary>
        /// Clear license newsletter hashtable.
        /// </summary>
        /// <param name="logTasks">If true, web farm tasks are logged</param>
        internal static void ClearLicNewsletter(bool logTasks)
        {
            Service.Resolve<INewsletterLicenseCheckerService>().ClearLicNewsletter(logTasks);
        }


        /// <summary>
        /// Checks if newsletter tracking (open e-mail, click through and bounces) is available for current URL.
        /// </summary>
        public static bool IsTrackingAvailable()
        {
            return Service.Resolve<INewsletterLicenseCheckerService>().IsTrackingAvailable();
        }


        /// <summary>
        /// Checks if newsletter A/B testing is available for current URL.
        /// </summary>
        public static bool IsABTestingAvailable()
        {
            return Service.Resolve<INewsletterLicenseCheckerService>().IsABTestingAvailable();
        }


        #endregion


        #region "Settings keys"

        /// <summary>
        /// Gets whether Online marketing is available and enabled for the site.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <returns>TRUE if online marketing module is loaded, available and enabled for the site, otherwise false</returns>
        public static bool OnlineMarketingAvailable(string siteName)
        {
            if (OnlineMarketingEnabled(siteName) && ModuleEntryManager.IsModuleLoaded(ModuleName.ONLINEMARKETING))
            {
                return ResourceSiteInfoProvider.IsResourceOnSite(ModuleName.ONLINEMARKETING, siteName);
            }

            return false;
        }


        /// <summary>
        /// Gets whether Online marketing is enabled for the site.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <returns>TRUE if Open email tracking is enabled, otherwise false</returns>
        public static bool OnlineMarketingEnabled(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSEnableOnlineMarketing");
        }


        /// <summary>
        /// Indicates if scheduled task for dynamic newsletters should run in windows service
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool UseExternalServiceForDynamicNewsletters(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSNewsletterUseExternalService");
        }


        /// <summary>
        /// Gets whether bounced e-mails monitoring is available for the given site and current URL.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool MonitorBouncedEmails(string siteName)
        {
            return MonitorBouncedEmailsEnabled(siteName) && IsTrackingAvailable();
        }


        /// <summary>
        /// Indicates if bounced e-mails monitoring is enabled for the given site.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool MonitorBouncedEmailsEnabled(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSMonitorBouncedEmails");
        }


        /// <summary>
        /// Gets limit for bounced e-mails.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static int BouncedEmailsLimit(string siteName)
        {
            return Service.Resolve<ISettingsService>()[siteName + ".CMSBouncedEmailsLimit"].ToInteger(0);
        }


        /// <summary>
        /// Gets e-mail address where bounced back e-mails should be sent.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static string BouncedEmailAddress(string siteName)
        {
            return SettingsKeyInfoProvider.GetValue(siteName + ".CMSBouncedEmailAddress");
        }


        /// <summary>
        /// Gets if newsletter e-mail generation is enabled.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <returns>true, if newsletter e-mail generation is enabled, otherwise false</returns>
        public static bool GenerateEmailsEnabled(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSGenerateNewsletters");
        }

        #endregion

        /// <summary>
        /// Returns number of recipients subscribed for specified newsletter. Takes unsubscription and bounces into account (see <see cref="IRecipientsProvider.GetMarketableRecipients"/>).
        /// </summary>
        /// <param name="issue">Count is calculated for this issue</param>
        /// <exception cref="ArgumentNullException"><paramref name="issue"/> is null</exception>
        /// <returns>Total amount of emails subscribed to the given <paramref name="issue"/> without the unsubscribed ones.</returns>
        public static int GetEmailAddressCount(IssueInfo issue)
        {
            if (issue == null)
            {
                throw new ArgumentNullException("issue");
            }

            return issue.GetRecipientsProvider().GetMarketableRecipients().Count;
        }


        /// <summary>
        /// Returns contact related with subscriber. If there is no known relation between contact and subscriber, 
        /// returns contact with the same email. In this case, if there is more contacts with the same email, selects the one with last logged activity.
        /// </summary>
        /// <param name="subscriber">Subscriber info</param>
        /// <returns>Contact info related to subscriber</returns>
        /// <exception cref="ArgumentNullException"><paramref name="subscriber"/> is null</exception>
        public static ContactInfo GetContactInfo(SubscriberInfo subscriber)
        {
            if (subscriber == null)
            {
                throw new ArgumentNullException("subscriber");
            }

            if (subscriber.SubscriberType == PredefinedObjectType.CONTACT)
            {
                // In this case contact is already linked with subscriber
                return subscriber.SubscriberRelated as ContactInfo;
            }

            return null;
        }


        /// <summary>
        /// Returns number of recipients subscribed for specified newsletter. Takes unsubscription table in account.
        /// </summary>
        /// <param name="newsletter">Newsletter.</param>
        /// <exception cref="ArgumentNullException">Is thrown when parameter <paramref name="newsletter"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Is thrown when parameter <paramref name="newsletter"/> is not type of <see cref="EmailCommunicationTypeEnum.Newsletter"/>.</exception>
        /// <remarks>The method counts recipients only for <see cref="EmailCommunicationTypeEnum.Newsletter">newsletters</see>.</remarks>
        public static int GetNewsletterMarketableRecipientsCount(NewsletterInfo newsletter)
        {
            return newsletter.GetRecipientsProvider()
                             .GetMarketableRecipients()
                             .Count;
        }


        /// <summary>
        /// Creates a subscriber from a contact.
        /// </summary>
        /// <param name="contact">Contact to create subscriber from.</param>
        public static SubscriberInfo ToContactSubscriber(this ContactInfo contact)
        {
            return new SubscriberInfo
            {
                SubscriberType = PredefinedObjectType.CONTACT,
                SubscriberRelatedID = contact.ContactID,
                SubscriberFirstName = contact.ContactFirstName,
                SubscriberLastName = contact.ContactLastName,
                SubscriberEmail = contact.ContactEmail
            };
        }
    }
}
