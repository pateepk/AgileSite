using System;

using CMS.Activities;
using CMS.ContactManagement;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Membership;
using CMS.WebAnalytics;
using CMS.DataEngine;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Helper methods for logging activities in administration.
    /// </summary>
    public class ActivityTrackingHelper
    {
        /// <summary>
        /// Returns contact ID for particular customer.
        /// </summary>
        /// <param name="cust">Customer</param>
        /// <param name="siteId">Site ID</param>
        public static int GetContactID(CustomerInfo cust, int siteId)
        {
            if (cust == null)
            {
                return 0;
            }

            // Check license
            if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, "") != "")
            {
                if (!LicenseHelper.CheckFeature(RequestContext.CurrentDomain, FeatureEnum.FullContactManagement))
                {
                    return 0;
                }
            }

            // Try to retrieve contact IDs for particular customer from membership relations
            string firstName = null;
            string lastName = null;
            string email = null;

            // Is it a user customer? => check if user has allowed tracking activities
            if (cust.CustomerIsRegistered)
            {
                UserInfo ui = UserInfoProvider.GetUserInfo(cust.CustomerUserID);
                if (ui != null)
                {
                    if (!ActivitySettingsHelper.ActivitiesEnabledForThisUser(ui))
                    {
                        // Tracking activities for this user is disabled => exit
                        return 0;
                    }
                    firstName = ui.FirstName;
                    lastName = ui.LastName;
                    email = ui.Email;
                }
            }

            // Under what contacts this customer belongs to?
            int customerId = cust.CustomerID;
            int contactId = ContactMembershipInfoProvider.GetContactIDByMembership(customerId, MemberTypeEnum.EcommerceCustomer);

            // No contacts found? => try to get contact ID using e-mail address
            if (contactId <= 0)
            {
                if (!cust.CustomerIsRegistered)
                {
                    // Ordinary customer? => use e-mail address directly from customer record
                    firstName = cust.CustomerFirstName;
                    lastName = cust.CustomerLastName;
                    email = cust.CustomerEmail;
                }

                if ((email != null) && !String.IsNullOrEmpty(email.Trim()))
                {
                    contactId = ContactInfoProvider.GetContactIDByEmail(email);
                }
            }

            // Create relation between contact and customer (only for monitored contact)
            bool isMonitored = true;
            if ((contactId > 0) && (isMonitored = ModuleCommands.OnlineMarketingContactIsMonitored(contactId)))
            {
                ModuleCommands.OnlineMarketingCreateRelation(cust.CustomerID, MembershipType.ECOMMERCE_CUSTOMER, contactId);
            }

            // No contacts found? => create new contact and relation
            if (contactId <= 0)
            {
                contactId = ModuleCommands.OnlineMarketingCreateNewContact(firstName, lastName, email,
                                                                           cust.CustomerID, MembershipType.ECOMMERCE_CUSTOMER);
            }
            else
            {
                if (!isMonitored)
                {
                    // Contact found but it is not being monitored
                    contactId = 0;
                }
            }

            return contactId;
        }
    }
}