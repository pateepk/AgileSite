using System;
using System.Collections.Generic;
using System.Linq;

using CMS.Base;
using CMS.ContactManagement;
using CMS.Core;
using CMS.DataEngine;

namespace CMS.Newsletters.Web.UI
{
    /// <summary>
    /// Provides service methods to obtain <see cref="ContactMarketingEmailStatusViewModel"/>.
    /// </summary>
    internal class ContactMarketingEmailStatusControllerService : IContactMarketingEmailStatusService
    {
        private class StatusResolver
        {
            public Func<ContactInfo, bool> Condition
            {
                get;
                set;
            }


            public ContactMarketingEmailStatusEnum Status
            {
                get;
                set;
            }
        }

        private readonly ISiteService mSiteService;
        private readonly ISettingsService mSettingsService;
        private readonly List<StatusResolver> mStatusResolvers;


        /// <summary>
        /// Instantiates new instance of <see cref="ContactMarketingEmailStatusControllerService"/>.
        /// </summary>
        /// <param name="unsubscriptionProvider">Handles unsubscriptions</param>
        /// <param name="siteService">Provides properties for obtaining current site</param>
        /// <param name="settingsService">Provides methods for obtaining the site settings</param>
        public ContactMarketingEmailStatusControllerService(IUnsubscriptionProvider unsubscriptionProvider, ISiteService siteService, ISettingsService settingsService)
        {
            mSiteService = siteService;
            mSettingsService = settingsService;

            mStatusResolvers = new List<StatusResolver>
            {
                new StatusResolver
                {
                    Condition = c => c == null,
                    Status = ContactMarketingEmailStatusEnum.Undeliverable
                },
                new StatusResolver
                {
                    Condition = c => string.IsNullOrEmpty(c.ContactEmail),
                    Status = ContactMarketingEmailStatusEnum.NoEmailAddress
                },
                new StatusResolver
                {
                    Condition = c => unsubscriptionProvider.IsUnsubscribedFromAllNewsletters(c.ContactEmail),
                    Status = ContactMarketingEmailStatusEnum.OptedOut
                },
                new StatusResolver
                {
                    Condition = ExceedsBounceLimit,
                    Status = ContactMarketingEmailStatusEnum.Undeliverable
                },
                new StatusResolver
                {
                    Condition = c => true,
                    Status = ContactMarketingEmailStatusEnum.ReceivingMarketingEmails
                }
            };
        }


        /// <summary>
        /// Gets <see cref="ContactMarketingEmailStatusViewModel"/> for the given <paramref name="contactID"/>. 
        /// </summary>
        /// <param name="contactID">ID of contact <see cref="ContactMarketingEmailStatusViewModel"/> is obtained for</param>
        /// <returns><see cref="ContactMarketingEmailStatusViewModel"/> for the given <paramref name="contactID"/></returns>
        public ContactMarketingEmailStatusViewModel GetContactMarketingEmailStatus(int contactID)
        {
            var contact = ContactInfoProvider.GetContactInfo(contactID);

            return new ContactMarketingEmailStatusViewModel
            {
                MarketingEmailStatus = mStatusResolvers.First(r => r.Condition(contact)).Status
            };
        }

        
        private bool ExceedsBounceLimit(ContactInfo contact)
        {
            int contactBounces = contact.ContactBounces;
            int subscriberBounces = GetSubscriberBounces(contact);

            int bounces = Math.Max(contactBounces, subscriberBounces);

            var bounceLimit = mSettingsService[mSiteService.CurrentSite.SiteName + ".CMSBouncedEmailsLimit"].ToInteger(0);
            return (bounceLimit > 0) && (bounces >= bounceLimit);
        }


        private static int GetSubscriberBounces(ContactInfo contact)
        {
            return SubscriberInfoProvider.GetSubscribers()
                                         .WhereEquals("SubscriberType", "om.contact")
                                         .WhereEquals("SubscriberRelatedID", contact.ContactID)
                                         .Column(new AggregatedColumn(AggregationType.Max, "SubscriberBounces"))
                                         .GetScalarResult<int>();
        }
    }
}
